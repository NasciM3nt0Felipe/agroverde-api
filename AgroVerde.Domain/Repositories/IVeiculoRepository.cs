using System.Collections.Generic;
using System.Threading.Tasks;
using AgroVerde.Domain.Entities;

namespace AgroVerde.Domain.Repositories
{
    public interface IVeiculoRepository
    {
        Task<IEnumerable<Veiculo>> GetAllAsync();
        Task<Veiculo?> GetByIdAsync(int id);
        Task<Veiculo?> ObterPorPlacaOuChassiAsync(string placaOuChassi);
        Task<IEnumerable<Veiculo>> ObterPorStatusAsync(StatusVeiculo status);
        Task<IEnumerable<Veiculo>> ObterPorTipoAsync(TipoVeiculo tipo);
        Task AddAsync(Veiculo entity);
        void Update(Veiculo entity);
        void Delete(Veiculo entity);
        Task SaveChangesAsync();

        Task<List<Veiculo>> ListarPorPropriedadeAsync(int propriedadeId);
        Task<Veiculo?> ObterPorPlacaNaPropriedadeAsync(int propriedadeId, string placaOuChassi);
        Task<bool> PossuiRegistrosAsync(int veiculoId);

        // Totais por veículo: (gasto com abastecimento, litros, gasto com
        // manutenção, quantidade de manutenções).
        Task<Dictionary<int, (decimal Abastecimento, double Litros, decimal Manutencao, int QtdManutencoes)>>
            ObterTotaisAsync(IEnumerable<int> veiculoIds);

        Task<List<AbastecimentoVeiculo>> ListarAbastecimentosAsync(int veiculoId);
        Task<AbastecimentoVeiculo?> ObterAbastecimentoAsync(int id);
        Task<List<ManutencaoVeiculo>> ListarManutencoesAsync(int veiculoId);
        Task<ManutencaoVeiculo?> ObterManutencaoAsync(int id);

        // Gravam o registro e a despesa no Financeiro na mesma transação.
        Task<int?> InserirAbastecimentoAsync(AbastecimentoVeiculo item, TransacaoFinanceira despesa);
        Task<int?> AtualizarAbastecimentoAsync(AbastecimentoVeiculo item, int propriedadeId, string descricao);
        Task ExcluirAbastecimentoAsync(AbastecimentoVeiculo item);

        Task<int?> InserirManutencaoAsync(ManutencaoVeiculo item, TransacaoFinanceira despesa);
        Task<int?> AtualizarManutencaoAsync(ManutencaoVeiculo item, int propriedadeId, string descricao);
        Task ExcluirManutencaoAsync(ManutencaoVeiculo item);

        Task<int?> ObterLancamentoIdAsync(string origemTipo, int origemId);
    }
}
