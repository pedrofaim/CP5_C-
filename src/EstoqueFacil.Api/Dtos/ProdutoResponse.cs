namespace EstoqueFacil.Api.Dtos;

public record ProdutoResponse(int Id, string Nome, string Categoria, decimal Preco, int Quantidade);
