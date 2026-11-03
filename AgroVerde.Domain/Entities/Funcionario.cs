namespace AgroVerde.Domain.Entities;

// Vínculo de uma Pessoa com uma Propriedade (módulo Funcionários).
// A propriedade pertence a um usuário; é por ela que se sabe de quem é o funcionário.
public class Funcionario
{
    public int Id { get; set; }

    public int PessoaId { get; set; }

    public Pessoa? Pessoa { get; set; }

    public int PropriedadeId { get; set; }

    public string Cargo { get; set; } = string.Empty;

    public decimal? Salario { get; set; }

    public DateTime? DataContratacao { get; set; }

    public DateTime? DataDesligamento { get; set; }

    // "Ativo" ou "Desligado" (ver StatusFuncionario).
    public string Status { get; set; } = StatusFuncionario.Ativo;

    public string? Observacao { get; set; }
}

public static class StatusFuncionario
{
    public const string Ativo = "Ativo";
    public const string Desligado = "Desligado";
}
