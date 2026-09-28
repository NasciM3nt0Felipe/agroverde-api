namespace AgroVerde.Application.DTOs;

public class ConsumoEstoqueRequest
{
    public int SafraId { get; set; }
    public int EstoqueItemId { get; set; }
    public double Quantidade { get; set; }
    public string? Observacao { get; set; }
}