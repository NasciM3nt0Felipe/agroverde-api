namespace AgroVerde.Domain.Entities;

public class ControleSanitario
{
    public int Id { get; set; }
    public int AnimalId { get; set; }
    public Animal Animal { get; set; } = null!;
    public string DiagnosticoTratamento { get; set; } = string.Empty;
    public string Medicamento { get; set; } = string.Empty;
    public DateTime DataInicio { get; set; }
    public DateTime? DataFim { get; set; }
    public string? Observacoes { get; set; }
}