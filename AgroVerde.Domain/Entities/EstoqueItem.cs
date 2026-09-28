namespace AgroVerde.Domain.Entities;

public class EstoqueItem
{
    public int Id { get; set; }
    public int PropriedadeId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Categoria { get; set; } = string.Empty;
    public double QuantidadeInicial { get; set; }
    public double QuantidadeAtual { get; set; }
    public string UnidadeMedida { get; set; } = string.Empty;
    public double PrecoMedioUnitario { get; set; }
    public double EstoqueMinimo { get; set; }
    public string? Fornecedor { get; set; }
    public string? Observacao { get; set; }

    public double ValorTotal => QuantidadeAtual * PrecoMedioUnitario;

    public bool EstoqueZerado => QuantidadeAtual == 0;

    public bool EstoqueBaixo =>
        QuantidadeAtual > 0 && QuantidadeAtual <= EstoqueMinimo;
}