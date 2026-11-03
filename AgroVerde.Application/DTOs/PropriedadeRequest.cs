namespace AgroVerde.Application.DTOs;

// O usuário dono da propriedade NÃO vem aqui: é sempre o usuário do token JWT.
public class PropriedadeRequest
{
    public string Nome { get; set; } = string.Empty;

    // Opcional: se vier vazia, a API monta "Cidade - Estado" (ou "Não informada").
    public string Localizacao { get; set; } = string.Empty;

    public string? Cidade { get; set; }

    public string? Estado { get; set; }

    public string? Descricao { get; set; }

    public double AreaTotal { get; set; }
}
