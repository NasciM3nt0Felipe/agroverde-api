using AgroVerde.Domain.Entities;
using AgroVerde.Domain.Repositories;
using AgroVerde.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AgroVerde.Infrastructure.Repositories;

public class TransacaoFinanceiraRepository : ITransacaoFinanceiraRepository
{
    private readonly AgroVerdeDbContext _context;

    public TransacaoFinanceiraRepository(AgroVerdeDbContext context)
    {
        _context = context;
    }

    public async Task<TransacaoFinanceira?> BuscarPorIdAsync(int id) =>
        await _context.TransacoesFinanceiras.FindAsync(id);

    public async Task<List<TransacaoFinanceira>> ListarTodosAsync() =>
        await _context.TransacoesFinanceiras
            .AsNoTracking()
            .ToListAsync();

    public async Task<IEnumerable<TransacaoFinanceira>> BuscarPorPeriodoAsync(
        DateTime inicio,
        DateTime fim) =>
        await _context.TransacoesFinanceiras
            .AsNoTracking()
            .Where(t => t.DataVencimento >= inicio && t.DataVencimento <= fim)
            .ToListAsync();

    public async Task<IEnumerable<TransacaoFinanceira>> ListarPorPropriedadeIdAsync(
        int propriedadeId) =>
        await _context.TransacoesFinanceiras
            .AsNoTracking()
            .Where(t => t.PropriedadeId == propriedadeId)
            .OrderByDescending(t => t.Id)
            .ToListAsync();

    public async Task<TransacaoFinanceira> InserirAsync(
        TransacaoFinanceira entity)
    {
        await _context.TransacoesFinanceiras.AddAsync(entity);
        await _context.SaveChangesAsync();

        return entity;
    }

    public async Task AtualizarAsync(TransacaoFinanceira entity)
    {
        _context.TransacoesFinanceiras.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task ExcluirAsync(int id)
    {
        var transacao = await _context.TransacoesFinanceiras.FindAsync(id);

        if (transacao != null)
        {
            _context.TransacoesFinanceiras.Remove(transacao);
            await _context.SaveChangesAsync();
        }
    }
}