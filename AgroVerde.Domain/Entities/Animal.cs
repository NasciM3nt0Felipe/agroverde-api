namespace AgroVerde.Domain.Entities;

public class Animal
{
    public int Id { get; set; }

    public int PropriedadeId { get; set; }

    public string Identificacao { get; set; } = string.Empty;

    public string Especie { get; set; } = string.Empty;

    public string? Raca { get; set; }

    public string Sexo { get; set; } = string.Empty;

    public string? DataNascimento { get; set; }

    public double? Peso { get; set; }

    public string Status { get; set; } = "Ativo";

    public string? Observacao { get; set; }

    public ICollection<Pesagem> Pesagens { get; set; } = new List<Pesagem>();

    public ICollection<Vacinacao> Vacinacoes { get; set; } = new List<Vacinacao>();

    public ICollection<ControleSanitario> ControlesSanitarios { get; set; }
        = new List<ControleSanitario>();

    public ICollection<RegistroReproducao> Reproducoes { get; set; }
        = new List<RegistroReproducao>();
}