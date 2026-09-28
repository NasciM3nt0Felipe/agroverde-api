namespace AgroVerde.Application.DTOs;

// O usuário dono da propriedade NÃO vem aqui: é sempre o usuário do token JWT.
public class PropriedadeRequest
{
    public string Nome { get; set; } = string.Empty;

    public string Localizacao { get; set; } = string.Empty;

    public double AreaTotal { get; set; }
}
