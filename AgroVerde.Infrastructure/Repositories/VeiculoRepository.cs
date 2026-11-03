using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AgroVerde.Domain.Entities;
using AgroVerde.Domain.Repositories;
using AgroVerde.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AgroVerde.Infrastructure.Repositories
{
    public class VeiculoRepository : IVeiculoRepository
    {
        private readonly AgroVerdeDbContext _context;

        public VeiculoRepository(AgroVerdeDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Veiculo>> GetAllAsync()
        {
            return await _context.Veiculos.AsNoTracking().ToListAsync();
        }

        public async Task<Veiculo?> GetByIdAsync(int id)
        {
            return await _context.Veiculos.FindAsync(id);
        }

        public async Task<Veiculo?> ObterPorPlacaOuChassiAsync(string placaOuChassi)
        {
            return await _context.Veiculos.AsNoTracking()
                .FirstOrDefaultAsync(v => v.PlacaOuChassi != null && v.PlacaOuChassi.ToUpper() == placaOuChassi.ToUpper());
        }

        public async Task<IEnumerable<Veiculo>> ObterPorStatusAsync(StatusVeiculo status)
        {
            return await _context.Veiculos.AsNoTracking().Where(v => v.Status == status).ToListAsync();
        }

        public async Task<IEnumerable<Veiculo>> ObterPorTipoAsync(TipoVeiculo tipo)
        {
            return await _context.Veiculos.AsNoTracking().Where(v => v.Tipo == tipo).ToListAsync();
        }

        public async Task AddAsync(Veiculo entity)
        {
            await _context.Veiculos.AddAsync(entity);
        }

        public void Update(Veiculo entity)
        {
            _context.Veiculos.Update(entity);
        }

        public void Delete(Veiculo entity)
        {
            _context.Veiculos.Remove(entity);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<List<Veiculo>> ListarPorPropriedadeAsync(int propriedadeId)
        {
            return await _context.Veiculos.AsNoTracking()
                .Where(v => v.PropriedadeId == propriedadeId)
                .OrderBy(v => v.Nome)
                .ToListAsync();
        }

        public async Task<Veiculo?> ObterPorPlacaNaPropriedadeAsync(int propriedadeId, string placaOuChassi)
        {
            return await _context.Veiculos.AsNoTracking()
                .FirstOrDefaultAsync(v =>
                    v.PropriedadeId == propriedadeId &&
                    v.PlacaOuChassi != null &&
                    v.PlacaOuChassi.ToUpper() == placaOuChassi.ToUpper());
        }

        public async Task<bool> PossuiRegistrosAsync(int veiculoId)
        {
            return await _context.AbastecimentosVeiculo.AnyAsync(a => a.VeiculoId == veiculoId) ||
                   await _context.ManutencoesVeiculo.AnyAsync(m => m.VeiculoId == veiculoId);
        }

        public async Task<Dictionary<int, (decimal Abastecimento, double Litros, decimal Manutencao, int QtdManutencoes)>>
            ObterTotaisAsync(IEnumerable<int> veiculoIds)
        {
            var ids = veiculoIds.ToList();

            // Somas feitas em memória: o SQLite não soma decimal no servidor.
            var abastecimentos = await _context.AbastecimentosVeiculo.AsNoTracking()
                .Where(a => ids.Contains(a.VeiculoId))
                .Select(a => new { a.VeiculoId, a.ValorTotal, a.Litros })
                .ToListAsync();

            var manutencoes = await _context.ManutencoesVeiculo.AsNoTracking()
                .Where(m => ids.Contains(m.VeiculoId))
                .Select(m => new { m.VeiculoId, m.Valor })
                .ToListAsync();

            return ids.ToDictionary(
                id => id,
                id => (
                    abastecimentos.Where(a => a.VeiculoId == id).Sum(a => a.ValorTotal),
                    abastecimentos.Where(a => a.VeiculoId == id).Sum(a => a.Litros),
                    manutencoes.Where(m => m.VeiculoId == id).Sum(m => m.Valor),
                    manutencoes.Count(m => m.VeiculoId == id)));
        }

        public async Task<List<AbastecimentoVeiculo>> ListarAbastecimentosAsync(int veiculoId)
        {
            return await _context.AbastecimentosVeiculo.AsNoTracking()
                .Where(a => a.VeiculoId == veiculoId)
                .OrderByDescending(a => a.Data)
                .ThenByDescending(a => a.Id)
                .ToListAsync();
        }

        public async Task<AbastecimentoVeiculo?> ObterAbastecimentoAsync(int id)
        {
            return await _context.AbastecimentosVeiculo
                .Include(a => a.Veiculo)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<List<ManutencaoVeiculo>> ListarManutencoesAsync(int veiculoId)
        {
            return await _context.ManutencoesVeiculo.AsNoTracking()
                .Where(m => m.VeiculoId == veiculoId)
                .OrderByDescending(m => m.Data)
                .ThenByDescending(m => m.Id)
                .ToListAsync();
        }

        public async Task<ManutencaoVeiculo?> ObterManutencaoAsync(int id)
        {
            return await _context.ManutencoesVeiculo
                .Include(m => m.Veiculo)
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<int?> ObterLancamentoIdAsync(string origemTipo, int origemId)
        {
            return await _context.TransacoesFinanceiras.AsNoTracking()
                .Where(t => t.OrigemTipo == origemTipo && t.OrigemId == origemId)
                .Select(t => (int?)t.Id)
                .FirstOrDefaultAsync();
        }

        // ---------- Gravação com despesa (transação única) ----------

        private async Task<int?> InserirComDespesaAsync<T>(
            T item,
            TransacaoFinanceira despesa,
            string origemTipo,
            System.Func<T, int> obterId) where T : class
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                _context.Add(item);
                await _context.SaveChangesAsync();

                despesa.OrigemTipo = origemTipo;
                despesa.OrigemId = obterId(item);
                _context.TransacoesFinanceiras.Add(despesa);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                return despesa.Id;
            }
            catch
            {
                await transaction.RollbackAsync();
                _context.ChangeTracker.Clear();
                throw;
            }
        }

        private async Task<int?> AtualizarComDespesaAsync(
            object item,
            string origemTipo,
            int origemId,
            int propriedadeId,
            string descricao,
            decimal valor,
            System.DateTime data)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                _context.Update(item);

                var despesa = await _context.TransacoesFinanceiras
                    .FirstOrDefaultAsync(t => t.OrigemTipo == origemTipo && t.OrigemId == origemId);

                if (despesa == null)
                {
                    despesa = NovaDespesa(propriedadeId, descricao, valor, data);
                    despesa.OrigemTipo = origemTipo;
                    despesa.OrigemId = origemId;
                    _context.TransacoesFinanceiras.Add(despesa);
                }
                else
                {
                    despesa.Descricao = descricao;
                    despesa.Valor = valor;
                    despesa.Data = data;
                    despesa.DataVencimento = data;
                    despesa.DataPagamento = data;
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return despesa.Id;
            }
            catch
            {
                await transaction.RollbackAsync();
                _context.ChangeTracker.Clear();
                throw;
            }
        }

        private async Task ExcluirComDespesaAsync(object item, string origemTipo, int origemId)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var despesas = await _context.TransacoesFinanceiras
                    .Where(t => t.OrigemTipo == origemTipo && t.OrigemId == origemId)
                    .ToListAsync();

                _context.TransacoesFinanceiras.RemoveRange(despesas);
                _context.Remove(item);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                _context.ChangeTracker.Clear();
                throw;
            }
        }

        // Despesa paga na data do registro (mesmo padrão do app).
        public static TransacaoFinanceira NovaDespesa(
            int propriedadeId, string descricao, decimal valor, System.DateTime data) => new()
        {
            PropriedadeId = propriedadeId,
            Descricao = descricao.Length > 200 ? descricao.Substring(0, 200) : descricao,
            Valor = valor,
            Tipo = TipoTransacao.Despesa,
            Categoria = "Veículos",
            Data = data,
            DataVencimento = data,
            DataPagamento = data,
            Status = StatusTransacao.Pago
        };

        public Task<int?> InserirAbastecimentoAsync(AbastecimentoVeiculo item, TransacaoFinanceira despesa) =>
            InserirComDespesaAsync(item, despesa, OrigemFinanceira.Abastecimento, i => i.Id);

        public Task<int?> AtualizarAbastecimentoAsync(AbastecimentoVeiculo item, int propriedadeId, string descricao) =>
            AtualizarComDespesaAsync(item, OrigemFinanceira.Abastecimento, item.Id,
                propriedadeId, descricao, item.ValorTotal, item.Data);

        public Task ExcluirAbastecimentoAsync(AbastecimentoVeiculo item) =>
            ExcluirComDespesaAsync(item, OrigemFinanceira.Abastecimento, item.Id);

        public Task<int?> InserirManutencaoAsync(ManutencaoVeiculo item, TransacaoFinanceira despesa) =>
            InserirComDespesaAsync(item, despesa, OrigemFinanceira.ManutencaoVeiculo, i => i.Id);

        public Task<int?> AtualizarManutencaoAsync(ManutencaoVeiculo item, int propriedadeId, string descricao) =>
            AtualizarComDespesaAsync(item, OrigemFinanceira.ManutencaoVeiculo, item.Id,
                propriedadeId, descricao, item.Valor, item.Data);

        public Task ExcluirManutencaoAsync(ManutencaoVeiculo item) =>
            ExcluirComDespesaAsync(item, OrigemFinanceira.ManutencaoVeiculo, item.Id);
    }
}
