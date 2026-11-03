namespace AgroVerde.Application.DTOs;

// Consumo de insumo em uma safra, com os dados do item utilizado.
public class ConsumoSafraResponse
{
    public int Id { get; set; }
    public int SafraId { get; set; }
    public int EstoqueItemId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Categoria { get; set; } = string.Empty;
    public string UnidadeMedida { get; set; } = string.Empty;
    public double QuantidadeUtilizada { get; set; }
    public double ValorTotal { get; set; }
    public string DataMovimentacao { get; set; } = string.Empty;
    public string? Observacao { get; set; }
}
