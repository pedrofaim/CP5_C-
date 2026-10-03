using System.ComponentModel.DataAnnotations;

namespace EstoqueFacil.Api.Dtos;

public class ProdutoRequest
{
    [Required(ErrorMessage = "Informe o nome.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "O nome deve ter entre 2 e 100 caracteres.")]
    [RegularExpression(@"^\S(?:.*\S)?$", ErrorMessage = "Remova espaços do início e do fim do nome.")]
    public string Nome { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe a categoria.")]
    [StringLength(60, MinimumLength = 2)]
    [RegularExpression(@"^\S(?:.*\S)?$", ErrorMessage = "Remova espaços do início e do fim da categoria.")]
    public string Categoria { get; set; } = string.Empty;

    [Required]
    [Range(typeof(decimal), "0.01", "999999.99", ErrorMessage = "O preço deve estar entre 0,01 e 999999,99.")]
    public decimal? Preco { get; set; }

    [Required]
    [Range(0, 1000000, ErrorMessage = "A quantidade deve estar entre 0 e 1000000.")]
    public int? Quantidade { get; set; }
}
