namespace AgroVerde.Domain.Entities;

public class Vacinacao
{
    public int Id { get; set; }
    public int AnimalId { get; set; }
    public Animal Animal { get; set; } = null!;
    public string NomeVacina { get; set; } = string.Empty;
    public string? Lote { get; set; }
    public string? Dose { get; set; }
    public DateTime DataAplicacao { get; set; } = DateTime.UtcNow;
    public DateTime? ProximaDose { get; set; }
}