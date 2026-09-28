namespace AgroVerde.Application.DTOs;

public class PropriedadeResponse
{
    public int Id { get; set; }

    public int UsuarioId { get; set; }

    public string Nome { get; set; } = string.Empty;

    public string Localizacao { get; set; } = string.Empty;

    public double AreaTotal { get; set; }

    public DateTime DataCriacao { get; set; }
}
