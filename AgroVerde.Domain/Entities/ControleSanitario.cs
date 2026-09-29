namespace AgroVerde.Domain.Entities;

public class ControleSanitario
{
    public int Id { get; set; }

    public int AnimalId { get; set; }
    public Animal Animal { get; set; } = null!;

    public string Procedimento { get; set; } = string.Empty;

    public DateTime Data { get; set; }

    public string? Medicamento { get; set; }

    public string? Observacao { get; set; }
}