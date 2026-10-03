# EstoqueFácil API

API RESTful de controle de produtos e estoque, desenvolvida em **C# / .NET 10 com Entity Framework Core 10** para o CP5 de C# Software Development — FIAP.

## Integrantes

| Nome | RM |
| --- | --- |
| Pedro Henrique Faim dos Santos | 557440 |
| Nicolas Lorenzo Ferreira da Silva | 557962 |
| Luiz Felipe Motta da Silva | 559126 |

## Contexto do projeto

Pequenos comércios precisam consultar quais produtos vendem, seus preços e quantidades disponíveis. Anotações dispersas dificultam a atualização dessas informações. O EstoqueFácil centraliza esse cadastro em uma API que pode ser consumida por um sistema de vendas, aplicativo ou painel administrativo.

O escopo é o cadastro de produtos: criar, listar, consultar por ID, atualizar e excluir. Não inclui autenticação, vendas, movimentações de estoque ou pagamentos. A aplicação é uma demonstração acadêmica para execução local.

## Tecnologias e banco de dados

- C# com ASP.NET Core, destino `net10.0`.
- Entity Framework Core 10.0.12: consultas e persistência via `DbContext`.
- **SQLite**, banco em arquivo `estoquefacil.db`, sem servidor ou senha.
- Swagger UI via Swashbuckle 10.2.3.
- Validação com Data Annotations e respostas de erro com Problem Details.

O arquivo do banco é criado na pasta da aplicação ao iniciar pelo perfil `http`. Ele não é versionado. A primeira execução começa sem produtos. Não é usado `EnsureCreated`: o esquema é criado pela Migration incluída.

## Como executar no Windows

### Requisitos

Instale o **SDK .NET 10** (não somente o Runtime):
https://dotnet.microsoft.com/download/dotnet/10.0

Feche e reabra o terminal após instalar. Confira:

```powershell
dotnet --list-sdks
```

A lista deve conter uma versão `10.0.xxx`. O primeiro restore exige acesso à internet para baixar os pacotes NuGet.

### Opção rápida

1. Extraia o ZIP inteiro.
2. Entre na pasta `EstoqueFacil`.
3. Abra `Iniciar-Windows.bat` e mantenha a janela aberta.
4. Acesse **http://localhost:5080/swagger** no navegador.

### Pelo terminal / VS Code

Abra o terminal na pasta que contém este README:

```powershell
dotnet restore src/EstoqueFacil.Api
dotnet build src/EstoqueFacil.Api --no-restore
dotnet run --project src/EstoqueFacil.Api --launch-profile http
```

O perfil `http` define o ambiente Development e a porta 5080. Nesse ambiente a aplicação aplica as migrations pendentes automaticamente. Os mesmos comandos funcionam em Linux e macOS com o SDK instalado. Use `Ctrl+C` para parar.

Se a porta estiver ocupada, pare outra execução ou use:

```powershell
dotnet run --project src/EstoqueFacil.Api --launch-profile http -- --urls http://localhost:5081
```

Nesse caso abra `http://localhost:5081/swagger`.

## Endpoints disponíveis

Base local: `http://localhost:5080`

| Método | Rota | Descrição | Respostas |
| --- | --- | --- | --- |
| GET | `/api/v1/produtos` | Lista produtos por ID; retorna `[]` quando vazio | 200 |
| GET | `/api/v1/produtos/{id}` | Busca um produto | 200, 404 |
| POST | `/api/v1/produtos` | Cadastra produto, gera ID e retorna cabeçalho Location | 201, 400 |
| PUT | `/api/v1/produtos/{id}` | Substitui todos os campos editáveis do produto | 204, 400, 404 |
| DELETE | `/api/v1/produtos/{id}` | Exclui produto | 204, 404 |

O versionamento está implementado **no caminho da rota**, com `[Route("api/v1/produtos")]` no controller. Apenas v1 está disponível. Uma rota `/api/v2/produtos` não está implementada. Não há negociação de versão por header ou query string.

### Exemplo de POST e PUT

```json
{
  "nome": "Caderno universitário",
  "categoria": "Papelaria",
  "preco": 24.90,
  "quantidade": 30
}
```

Resposta de criação, com ID ilustrativo:

```json
{
  "id": 1,
  "nome": "Caderno universitário",
  "categoria": "Papelaria",
  "preco": 24.90,
  "quantidade": 30
}
```

Use o ID retornado pelo POST nas demais requisições. PUT e DELETE bem-sucedidos retornam `204` **sem corpo**. DELETE repetido retorna `404` porque o recurso já não existe.

### Regras e erros

- Nome obrigatório, de 2 a 100 caracteres; categoria obrigatória, de 2 a 60 caracteres.
- Nome e categoria não aceitam espaços no início/fim ou conteúdo formado somente por espaços.
- Preço obrigatório entre 0,01 e 999999,99; arredondado para duas casas decimais.
- Quantidade obrigatória, inteira, entre 0 e 1000000.
- O corpo do PUT deve conter todos os campos editáveis.
- `[ApiController]` retorna `400` em JSON inválido, campos ausentes ou validação reprovada.
- Um ID inteiro sem registro correspondente retorna `404` com `ProblemDetails`.
- Exceções inesperadas passam pelo tratamento global e retornam erro 500 sem expor a stack trace na resposta.
- IDs são gerados pelo banco. DTOs impedem alteração do ID pelo corpo da requisição.

## EF Core e Migration

O `AppDbContext` define `DbSet<Produto>` e mapeia chave, campos obrigatórios, limites e precisão. As operações usam `ToListAsync`, `FirstOrDefaultAsync`, `FindAsync` e `SaveChangesAsync`. Leituras usam `AsNoTracking`.

A Migration **CriacaoInicial**, em `src/EstoqueFacil.Api/Migrations`, cria a tabela `Produtos`. Seu método `Up` cria a tabela e `Down` a remove. O projeto inclui também o designer e o snapshot do modelo, gerados pelo EF CLI.

Restaurar a ferramenta local e aplicar manualmente, a partir da raiz:

```powershell
dotnet tool restore
dotnet ef migrations list --project src/EstoqueFacil.Api
dotnet ef database update --project src/EstoqueFacil.Api
```

O comando abaixo documenta como a Migration inicial foi criada; **não é preciso executá-lo novamente**:

```powershell
dotnet ef migrations add CriacaoInicial --project src/EstoqueFacil.Api
```

Depois de uma futura alteração no modelo:

```powershell
dotnet ef migrations add NomeDaAlteracao --project src/EstoqueFacil.Api
dotnet ef database update --project src/EstoqueFacil.Api
```

Para conferir se o modelo está sincronizado com o snapshot:

```powershell
dotnet ef migrations has-pending-model-changes --project src/EstoqueFacil.Api
```

Em produção, a atualização do banco deve ocorrer no processo de implantação; a aplicação só aplica migrations automaticamente em Development.

## Organização

```text
src/EstoqueFacil.Api/
  Controllers/     Endpoints e respostas HTTP
  Data/            DbContext e mapeamento
  Dtos/            Contratos de entrada e saída
  Models/          Entidade Produto
  Migrations/      Migration inicial, designer e snapshot
  Properties/      Perfil de execução local
  Program.cs       Serviços, tratamento de erros e inicialização
scripts/           Testes reproduzíveis
docs/             Evidências e resultado dos testes HTTP
```

A estrutura mantém a aplicação pequena: o controller utiliza diretamente o DbContext, sem acrescentar uma camada de repositório que apenas repetiria os métodos do EF Core.

## Testes e evidências

Os prints abaixo foram capturados **no Swagger UI com a API executando**, realizando chamadas HTTP reais contra SQLite. Os IDs dependem da sequência de testes, por isso não devem ser tratados como dados iniciais obrigatórios.

| Evidência | Resultado |
| --- | --- |
| [POST — criar](docs/evidencias/01-post-201.png) | 201, JSON e Location |
| [GET — listar](docs/evidencias/02-get-lista-200.png) | 200, lista de produtos |
| [GET — buscar](docs/evidencias/03-get-id-200.png) | 200, produto pelo ID |
| [PUT — atualizar](docs/evidencias/04-put-204.png) | 204, sem corpo |
| [POST — dados inválidos](docs/evidencias/05-post-400.png) | 400, erros de validação |
| [DELETE — excluir](docs/evidencias/06-delete-204.png) | 204, sem corpo |
| [GET — após exclusão](docs/evidencias/07-get-404.png) | 404, produto não encontrado |

### Repetir manualmente no Swagger

1. Abra `/swagger` e expanda POST; clique em **Try it out**.
2. Cole o JSON válido acima e clique em **Execute**. Anote o ID retornado e confira 201/Location.
3. Execute GET da coleção e GET por ID; confira 200.
4. Execute PUT com o mesmo ID e quantidade 12; confira 204. Faça GET novamente para verificar o valor persistido.
5. Execute POST com nome vazio, preço -1 e quantidade -1; confira 400.
6. Execute DELETE com o ID criado; confira 204.
7. Repita GET por ID; confira 404.

Também é possível importar [a coleção do Postman](docs/EstoqueFacil.postman_collection.json). Execute primeiro **Criar produto**; a coleção salva automaticamente o ID retornado.

### Teste automatizado opcional

Com a API rodando na porta 5080 e Python 3 instalado, em outro terminal:

```powershell
python scripts/testar-api.py
```

O script utiliza somente a biblioteca padrão, cria e remove seu próprio produto e verifica códigos HTTP, Location e persistência da atualização. Ele grava as respostas reais em [docs/testes-http.json](docs/testes-http.json). Testa também PUT inválido, campos numéricos ausentes e operações sobre produto excluído. Não remove outros produtos existentes.

## Publicar no GitHub e entregar

1. Crie um repositório no GitHub, por exemplo `estoquefacil-api`.
2. Envie **todo o conteúdo desta pasta**, mantendo `README.md` na raiz, incluindo `src`, `.config`, `docs` e os scripts. Não envie somente o ZIP.
3. Se usar Git, a partir desta pasta, substitua a URL abaixo pelo endereço do seu repositório:

```powershell
git init
git add .
git commit -m "Implementa API de estoque com EF Core e versionamento v1"
git branch -M main
git remote add origin https://github.com/SEU-USUARIO/estoquefacil-api.git
git push -u origin main
```

4. Confira se o README e os sete prints abrem no GitHub.
5. Mantenha o repositório público ou conceda acesso ao professor caso seja privado.
6. Entregue o link do repositório na atividade. O grupo deve usar um único repositório e listar todos os integrantes acima.

## Referências

- [EF Core com SQLite](https://learn.microsoft.com/ef/core/get-started/overview/first-app)
- [Migrations do EF Core](https://learn.microsoft.com/ef/core/managing-schemas/migrations/)
- [APIs com controllers no ASP.NET Core](https://learn.microsoft.com/aspnet/core/web-api/?view=aspnetcore-10.0)
