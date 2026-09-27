namespace AgroVerde.Application.DTOs;

public class SafraResponse
{
    public int Id { get; set; }
    public int TalhaoId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Cultura { get; set; } = string.Empty;
    public string? Variedade { get; set; }
    public string DataPlantio { get; set; } = string.Empty;
    public string? DataColheitaPrevista { get; set; }
    public string? DataColheitaReal { get; set; }
    public double? ProducaoEstimada { get; set; }
    public double? ProducaoObtida { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Observacao { get; set; }
}