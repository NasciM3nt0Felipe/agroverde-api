using AgroVerde.Domain.Entities;
using AgroVerde.Domain.Repositories;
using AgroVerde.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AgroVerde.Infrastructure.Repositories;

public class AnimalRepository : IAnimalRepository
{
    private readonly AgroVerdeDbContext _context;

    public AnimalRepository(AgroVerdeDbContext context)
    {
        _context = context;
    }

    public async Task<Animal?> BuscarPorIdAsync(int id) =>
        await _context.Animais.FindAsync(id);

    public async Task<Animal?> BuscarPorIdComDetalhesAsync(int id) =>
        await _context.Animais
            .Include(a => a.Pesagens)
            .Include(a => a.Vacinacoes)
            .Include(a => a.ControlesSanitarios)
            .Include(a => a.Reproducoes)
            .FirstOrDefaultAsync(a => a.Id == id);

    public async Task<List<Animal>> ListarTodosAsync() =>
        await _context.Animais
            .AsNoTracking()
            .ToListAsync();

    public async Task<IEnumerable<Animal>> BuscarPorStatusAsync(string status) =>
        await _context.Animais
            .AsNoTracking()
            .Where(a => a.Status == status)
            .ToListAsync();

    public async Task<IEnumerable<Animal>> ListarPorPropriedadeIdAsync(
        int propriedadeId) =>
        await _context.Animais
            .AsNoTracking()
            .Where(a => a.PropriedadeId == propriedadeId)
            .ToListAsync();

    public async Task<Animal> InserirAsync(Animal entity)
    {
        await _context.Animais.AddAsync(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task AtualizarAsync(Animal entity)
    {
        _context.Animais.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task ExcluirAsync(int id)
    {
        var animal = await _context.Animais.FindAsync(id);

        if (animal != null)
        {
            _context.Animais.Remove(animal);
            await _context.SaveChangesAsync();
        }
    }
}