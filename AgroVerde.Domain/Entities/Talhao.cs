namespace AgroVerde.Domain.Entities;

public class Talhao
{
    public int Id { get; set; }

    public int PropriedadeId { get; set; }

    public string Nome { get; set; } = string.Empty;

    public double Area { get; set; }

    public string? TipoSolo { get; set; }

    public string? Observacao { get; set; }

    public bool Ativo { get; set; } = true;
}
