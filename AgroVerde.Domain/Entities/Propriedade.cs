namespace AgroVerde.Domain.Entities;

public class Propriedade
{
    public int Id { get; set; }

    public int UsuarioId { get; set; }

    public string Nome { get; set; } = string.Empty;

    public string Localizacao { get; set; } = string.Empty;

    // Campo consultado pelo módulo de Talhão (regra: soma dos talhões <= 90% da AreaTotal).
    public double AreaTotal { get; set; }

    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
}
