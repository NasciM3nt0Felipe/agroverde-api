using AgroVerde.Domain.Entities;

namespace AgroVerde.Domain.Repositories;

public enum TipoRegistroAnimal
{
    Pesagem,
    Vacinacao,
    Sanitario,
    Reproducao
}

public interface IAnimalRepository : IRepository<Animal>
{
    Task<Animal?> BuscarPorIdComDetalhesAsync(int id);
    Task<IEnumerable<Animal>> BuscarPorStatusAsync(string status);
    Task<IEnumerable<Animal>> ListarPorPropriedadeIdAsync(int propriedadeId);

    Task<List<Pesagem>> ListarPesagensPorPropriedadeAsync(int propriedadeId);
    Task<List<Vacinacao>> ListarVacinacoesPorPropriedadeAsync(int propriedadeId);
    Task<List<ControleSanitario>> ListarControlesSanitariosPorPropriedadeAsync(int propriedadeId);
    Task<List<RegistroReproducao>> ListarReproducoesPorPropriedadeAsync(int propriedadeId);

    // Propriedade do animal dono do registro (null se o registro não existir).
    Task<int?> ObterPropriedadeDoRegistroAsync(TipoRegistroAnimal tipo, int id);
    Task<bool> ExcluirRegistroAsync(TipoRegistroAnimal tipo, int id);

    Task<bool> ExisteVacinacaoAsync(int animalId, string nomeVacina, DateTime dataAplicacao);

    // Baixa 1 dose do item e grava a vacinação na mesma transação.
    // Retorna false (sem gravar nada) se o item não tiver saldo.
    Task<bool> RegistrarVacinacaoComBaixaAsync(Vacinacao vacinacao, int estoqueItemId);
}