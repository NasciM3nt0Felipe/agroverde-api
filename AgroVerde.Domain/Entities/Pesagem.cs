namespace AgroVerde.Domain.Entities;

public class Pesagem
{
    public int Id { get; set; }
    public int AnimalId { get; set; }
    public Animal Animal { get; set; } = null!;
    public decimal PesoKg { get; set; }
    public DateTime DataPesagem { get; set; } = DateTime.UtcNow;
    public string? Observacao { get; set; }
}