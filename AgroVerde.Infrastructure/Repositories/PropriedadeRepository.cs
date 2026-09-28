using AgroVerde.Domain.Entities;
using AgroVerde.Domain.Repositories;
using AgroVerde.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AgroVerde.Infrastructure.Repositories;

public class PropriedadeRepository : IPropriedadeRepository
{
    private readonly AgroVerdeDbContext _context;

    public PropriedadeRepository(AgroVerdeDbContext context)
    {
        _context = context;
    }

    public async Task<Propriedade> InserirAsync(Propriedade propriedade)
    {
        await _context.Set<Propriedade>().AddAsync(propriedade);
        await _context.SaveChangesAsync();

        return propriedade;
    }

    public async Task<Propriedade?> BuscarPorIdAsync(int id)
    {
        return await _context.Set<Propriedade>()
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<List<Propriedade>> ListarTodosAsync()
    {
        return await _context.Set<Propriedade>()
            .ToListAsync();
    }

    public async Task AtualizarAsync(Propriedade propriedade)
    {
        _context.Set<Propriedade>().Update(propriedade);
        await _context.SaveChangesAsync();
    }

    public async Task ExcluirAsync(int id)
    {
        var propriedade = await BuscarPorIdAsync(id);

        if (propriedade != null)
        {
            _context.Set<Propriedade>().Remove(propriedade);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<List<Propriedade>> ListarPorUsuarioIdAsync(int usuarioId)
    {
        return await _context.Set<Propriedade>()
            .Where(p => p.UsuarioId == usuarioId)
            .OrderByDescending(p => p.DataCriacao)
            .ToListAsync();
    }
}
