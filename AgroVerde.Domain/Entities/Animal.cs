namespace AgroVerde.Domain.Entities;

public enum SexoAnimal
{
    Macho = 1,
    Femea = 2
}

public enum StatusAnimal
{
    Ativo = 1,
    Vendido = 2,
    Morto = 3,
    Isolado = 4
}

public class Animal
{
    public int Id { get; set; }
    public string Brinco { get; set; } = string.Empty;
    public string? Nome { get; set; }
    public string Raca { get; set; } = string.Empty;
    public SexoAnimal Sexo { get; set; }
    public DateTime DataNascimento { get; set; }
    public StatusAnimal Status { get; set; } = StatusAnimal.Ativo;

    public ICollection<Pesagem> Pesagens { get; set; } = new List<Pesagem>();
    public ICollection<Vacinacao> Vacinacoes { get; set; } = new List<Vacinacao>();
    public ICollection<ControleSanitario> ControlesSanitarios { get; set; } = new List<ControleSanitario>();
    public ICollection<RegistroReproducao> Reproducoes { get; set; } = new List<RegistroReproducao>();
}