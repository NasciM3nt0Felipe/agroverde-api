using AgroVerde.Domain.Entities;
using AgroVerde.Domain.Repositories;
using AgroVerde.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AgroVerde.Infrastructure.Repositories;

public class UsuarioRepository : IUsuarioRepository
{
    private readonly AgroVerdeDbContext _context;

    public UsuarioRepository(AgroVerdeDbContext context)
    {
        _context = context;
    }

    public async Task<Usuario> InserirAsync(Usuario usuario)
    {
        await _context.Set<Usuario>().AddAsync(usuario);
        await _context.SaveChangesAsync();

        return usuario;
    }

    public async Task<Usuario?> BuscarPorIdAsync(int id)
    {
        return await _context.Set<Usuario>()
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<List<Usuario>> ListarTodosAsync()
    {
        return await _context.Set<Usuario>()
            .ToListAsync();
    }

    public async Task AtualizarAsync(Usuario usuario)
    {
        _context.Set<Usuario>().Update(usuario);
        await _context.SaveChangesAsync();
    }

    public async Task ExcluirAsync(int id)
    {
        var usuario = await BuscarPorIdAsync(id);

        if (usuario != null)
        {
            _context.Set<Usuario>().Remove(usuario);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<Usuario?> BuscarPorEmailAsync(string email)
    {
        return await _context.Set<Usuario>()
            .FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<bool> EmailExisteAsync(string email)
    {
        return await _context.Set<Usuario>()
            .AnyAsync(u => u.Email == email);
    }
}
