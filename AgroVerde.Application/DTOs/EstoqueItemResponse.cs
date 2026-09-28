namespace AgroVerde.Application.DTOs;

public class EstoqueItemResponse
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

    public double ValorTotal { get; set; }
    public bool EstoqueZerado { get; set; }
    public bool EstoqueBaixo { get; set; }
}