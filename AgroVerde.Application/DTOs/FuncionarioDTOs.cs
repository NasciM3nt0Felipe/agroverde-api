namespace AgroVerde.Application.DTOs;

// O usuário NUNCA vem no corpo: é o do token JWT.
// A Pessoa (nome/cpf/telefone) é gravada junto com o vínculo, na mesma operação.
public class FuncionarioRequest
{
    // Obrigatório na criação. Na edição, se informado, deve ser a propriedade atual.
    public int PropriedadeId { get; set; }

    public string Nome { get; set; } = string.Empty;

    public string? Cpf { get; set; }

    public string? Telefone { get; set; }

    public string Cargo { get; set; } = string.Empty;

    public decimal? Salario { get; set; }

    public DateTime? DataContratacao { get; set; }

    public DateTime? DataDesligamento { get; set; }

    // "Ativo" ou "Desligado".
    public string Status { get; set; } = string.Empty;

    public string? Observacao { get; set; }
}

public class DesligarFuncionarioRequest
{
    public DateTime? DataDesligamento { get; set; }

    // Se omitida, mantém a observação atual.
    public string? Observacao { get; set; }
}

public class FuncionarioResponse
{
    public int Id { get; set; }

    public int PessoaId { get; set; }

    public int PropriedadeId { get; set; }

    public string Nome { get; set; } = string.Empty;

    public string? Cpf { get; set; }

    public string? Telefone { get; set; }

    public string Cargo { get; set; } = string.Empty;

    public decimal? Salario { get; set; }

    public DateTime? DataContratacao { get; set; }

    public DateTime? DataDesligamento { get; set; }

    public string Status { get; set; } = string.Empty;

    public string? Observacao { get; set; }
}
