using AgroVerde.Domain.Entities;
using AgroVerde.Domain.Repositories;
using AgroVerde.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AgroVerde.Infrastructure.Repositories;

public class SafraRepository : ISafraRepository
{
    private readonly AgroVerdeDbContext _context;

    public SafraRepository(AgroVerdeDbContext context)
    {
        _context = context;
    }

    public async Task<Safra> InserirAsync(Safra safra)
    {
        _context.Safras.Add(safra);
        await _context.SaveChangesAsync();

        return safra;
    }

    public async Task<Safra?> BuscarPorIdAsync(int id)
    {
        return await _context.Safras
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<List<Safra>> ListarTodosAsync()
    {
        return await _context.Safras
            .ToListAsync();
    }

    public async Task AtualizarAsync(Safra safra)
    {
        _context.Safras.Update(safra);
        await _context.SaveChangesAsync();
    }

    public async Task ExcluirAsync(int id)
    {
        var safra = await _context.Safras
            .FirstOrDefaultAsync(s => s.Id == id);

        if (safra != null)
        {
            _context.Safras.Remove(safra);
            await _context.SaveChangesAsync();
        }
    }
    public async Task<List<Safra>> ListarPorTalhaoIdAsync(int talhaoId)
    {
        return await _context.Safras
            .Where(s => s.TalhaoId == talhaoId)
            .OrderByDescending(s => s.Id)
            .ToListAsync();
    }
    public async Task<bool> ExisteSafraAtivaAsync(
    int talhaoId,
    int? ignorarSafraId = null)
    {
        var query = _context.Safras
            .Where(s =>
                s.TalhaoId == talhaoId &&
                (s.Status == "Planejada" || s.Status == "Em andamento"));

        if (ignorarSafraId.HasValue)
        {
            query = query.Where(s => s.Id != ignorarSafraId.Value);
        }

        return await query.AnyAsync();
    }
    public async Task<List<Safra>> ListarDisponiveisParaColheitaPorPropriedadeAsync(
    int propriedadeId)
    {
        var safras = await (
            from safra in _context.Safras
            join talhao in _context.Talhoes
                on safra.TalhaoId equals talhao.Id
            where talhao.PropriedadeId == propriedadeId
                && safra.DataColheitaPrevista != null
                && safra.DataColheitaPrevista != ""
                && safra.Status != "Colhida"
                && safra.Status != "Finalizada"
            select safra
        ).ToListAsync();

        var hoje = DateTime.Today;

        return safras
            .Where(s =>
                DateTime.TryParseExact(
                    s.DataColheitaPrevista,
                    "dd/MM/yyyy",
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None,
                    out var dataColheita)
                && dataColheita.Date <= hoje)
            .OrderBy(s =>
                DateTime.ParseExact(
                    s.DataColheitaPrevista!,
                    "dd/MM/yyyy",
                    System.Globalization.CultureInfo.InvariantCulture))
            .ToList();
    }
}