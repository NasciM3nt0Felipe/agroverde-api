using Microsoft.EntityFrameworkCore;
using AgroVerde.Domain.Entities;
using AgroVerde.Domain.Repositories;
using AgroVerde.Infrastructure.Data;

namespace AgroVerde.Infrastructure.Repositories;

public class TalhaoRepository : ITalhaoRepository
{
    private readonly AgroVerdeDbContext _context;

    public TalhaoRepository(AgroVerdeDbContext context)
    {
        _context = context;
    }

    public async Task<Talhao> InserirAsync(Talhao talhao)
    {
        await _context.Talhoes.AddAsync(talhao);
        await _context.SaveChangesAsync();

        return talhao;
    }

    public async Task<Talhao?> BuscarPorIdAsync(int id)
    {
        return await _context.Talhoes
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<List<Talhao>> ListarTodosAsync()
    {
        return await _context.Talhoes
            .ToListAsync();
    }

    public async Task AtualizarAsync(Talhao talhao)
    {
        _context.Talhoes.Update(talhao);
        await _context.SaveChangesAsync();
    }

    public async Task ExcluirAsync(int id)
    {
        var talhao = await BuscarPorIdAsync(id);

        if (talhao != null)
        {
            _context.Talhoes.Remove(talhao);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<List<Talhao>> ListarPorPropriedadeIdAsync(int propriedadeId)
    {
        return await _context.Talhoes
            .Where(t => t.PropriedadeId == propriedadeId)
            .OrderBy(t => t.Nome)
            .ToListAsync();
    }
}