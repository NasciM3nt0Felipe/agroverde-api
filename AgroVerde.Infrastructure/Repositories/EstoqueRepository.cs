using AgroVerde.Domain.Entities;
using AgroVerde.Domain.Repositories;
using AgroVerde.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AgroVerde.Infrastructure.Repositories;

public class EstoqueRepository : IEstoqueRepository
{
    private readonly AgroVerdeDbContext _context;

    public EstoqueRepository(AgroVerdeDbContext context)
    {
        _context = context;
    }
    public async Task<EstoqueItem> InserirAsync(EstoqueItem entidade)
    {
        _context.EstoqueItens.Add(entidade);

        await _context.SaveChangesAsync();

        return entidade;
    }
    public async Task<EstoqueItem?> BuscarPorIdAsync(int id)
    {
        return await _context.EstoqueItens
            .FirstOrDefaultAsync(e => e.Id == id);
    }
    public async Task<List<EstoqueItem>> ListarTodosAsync()
    {
        return await _context.EstoqueItens
            .OrderBy(e => e.Nome)
            .ToListAsync();
    }
    public async Task AtualizarAsync(EstoqueItem entidade)
    {
        _context.EstoqueItens.Update(entidade);

        await _context.SaveChangesAsync();
    }
    public async Task ExcluirAsync(int id)
    {
        var item = await _context.EstoqueItens
            .FirstOrDefaultAsync(e => e.Id == id);

        if (item == null)
            return;

        _context.EstoqueItens.Remove(item);

        await _context.SaveChangesAsync();
    }
    public async Task<List<EstoqueItem>> ListarPorPropriedadeIdAsync(
    int propriedadeId)
    {
        return await _context.EstoqueItens
            .Where(e => e.PropriedadeId == propriedadeId)
            .OrderBy(e => e.Nome)
            .ToListAsync();
    }
    public async Task<List<EstoqueItem>> ListarVacinasDisponiveisPorPropriedadeAsync(
    int propriedadeId)
    {
        return await _context.EstoqueItens
            .Where(e =>
                e.PropriedadeId == propriedadeId &&
                e.Categoria.ToLower() == "vacinas" &&
                e.QuantidadeAtual > 0)
            .OrderBy(e => e.Nome)
            .ToListAsync();
    }
    public async Task<EstoqueItem?> BuscarPorNomeCategoriaEPropriedadeAsync(
    int propriedadeId,
    string nome,
    string categoria)
    {
        return await _context.EstoqueItens
            .FirstOrDefaultAsync(e =>
                e.PropriedadeId == propriedadeId &&
                e.Nome.ToLower() == nome.ToLower() &&
                e.Categoria.ToLower() == categoria.ToLower());
    }
    public async Task<EstoqueInsumo> InserirConsumoInsumoAsync(
    EstoqueInsumo consumo)
    {
        _context.EstoqueInsumos.Add(consumo);

        await _context.SaveChangesAsync();

        return consumo;
    }
    public async Task<bool> ExisteConsumoPorSafraAsync(int safraId)
    {
        return await _context.EstoqueInsumos
            .AnyAsync(e => e.SafraId == safraId);
    }
    public async Task<bool> ExisteConsumoPorSafraECategoriaAsync(
    int safraId,
    string categoria)
    {
        return await _context.EstoqueInsumos
            .Join(
                _context.EstoqueItens,
                consumo => consumo.EstoqueItemId,
                item => item.Id,
                (consumo, item) => new { consumo, item })
            .AnyAsync(x =>
                x.consumo.SafraId == safraId &&
                x.item.Categoria.ToLower() == categoria.ToLower());
    }
    public async Task<EstoqueInsumo?> BuscarUltimoConsumoPorCategoriaAsync(
    int safraId,
    string categoria)
    {
        return await _context.EstoqueInsumos
            .Join(
                _context.EstoqueItens,
                consumo => consumo.EstoqueItemId,
                item => item.Id,
                (consumo, item) => new { consumo, item })
            .Where(x =>
                x.consumo.SafraId == safraId &&
                x.item.Categoria.ToLower() == categoria.ToLower())
            .OrderByDescending(x => x.consumo.Id)
            .Select(x => x.consumo)
            .FirstOrDefaultAsync();
    }
    public async Task RegistrarConsumoSafraAsync(
    EstoqueItem item,
    EstoqueInsumo consumo)
    {
        await using var transaction =
            await _context.Database.BeginTransactionAsync();

        try
        {
            _context.EstoqueItens.Update(item);

            await _context.EstoqueInsumos.AddAsync(consumo);

            await _context.SaveChangesAsync();

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}