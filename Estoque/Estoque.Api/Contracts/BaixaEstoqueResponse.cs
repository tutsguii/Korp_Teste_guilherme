namespace Estoque.Api.Contracts;

public class BaixaEstoqueResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}
