namespace AgroVerde.Domain.Entities;

public enum TipoEventoReproducao
{
    Inseminacao = 1,
    Cobertura = 2,
    DiagnosticoGestacaoPositivo = 3,
    DiagnosticoGestacaoNegativo = 4,
    Parto = 5,
    Aborto = 6
}

public class RegistroReproducao
{
    public int Id { get; set; }
    public int AnimalId { get; set; }
    public Animal Animal { get; set; } = null!;
    public TipoEventoReproducao TipoEvento { get; set; }
    public DateTime DataEvento { get; set; }
    public string? IdentificacaoMacho { get; set; }
    public DateTime? PrevisaoParto { get; set; }
    public string? Observacoes { get; set; }
}