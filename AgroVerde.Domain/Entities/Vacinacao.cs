namespace AgroVerde.Domain.Entities;

public class Vacinacao
{
    public int Id { get; set; }

    public int AnimalId { get; set; }
    public Animal Animal { get; set; } = null!;

    public string NomeVacina { get; set; } = string.Empty;

    public DateTime DataAplicacao { get; set; }

    public DateTime? ProximaDose { get; set; }

    public string? Observacao { get; set; }
}