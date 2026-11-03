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

    public async Task<List<Pesagem>> ListarPesagensPorPropriedadeAsync(
        int propriedadeId) =>
        await _context.Pesagens
            .AsNoTracking()
            .Where(p => p.Animal.PropriedadeId == propriedadeId)
            .OrderByDescending(p => p.DataPesagem)
            .ThenByDescending(p => p.Id)
            .ToListAsync();

    public async Task<List<Vacinacao>> ListarVacinacoesPorPropriedadeAsync(
        int propriedadeId) =>
        await _context.Vacinacoes
            .AsNoTracking()
            .Where(v => v.Animal.PropriedadeId == propriedadeId)
            .OrderByDescending(v => v.DataAplicacao)
            .ThenByDescending(v => v.Id)
            .ToListAsync();

    public async Task<List<ControleSanitario>>
        ListarControlesSanitariosPorPropriedadeAsync(int propriedadeId) =>
        await _context.ControlesSanitarios
            .AsNoTracking()
            .Where(c => c.Animal.PropriedadeId == propriedadeId)
            .OrderByDescending(c => c.Data)
            .ThenByDescending(c => c.Id)
            .ToListAsync();

    public async Task<List<RegistroReproducao>>
        ListarReproducoesPorPropriedadeAsync(int propriedadeId) =>
        await _context.RegistrosReproducao
            .AsNoTracking()
            .Where(r => r.Animal.PropriedadeId == propriedadeId)
            .OrderByDescending(r => r.Data)
            .ThenByDescending(r => r.Id)
            .ToListAsync();

    public async Task<int?> ObterPropriedadeDoRegistroAsync(
        TipoRegistroAnimal tipo,
        int id)
    {
        return tipo switch
        {
            TipoRegistroAnimal.Pesagem => await _context.Pesagens
                .Where(p => p.Id == id)
                .Select(p => (int?)p.Animal.PropriedadeId)
                .FirstOrDefaultAsync(),
            TipoRegistroAnimal.Vacinacao => await _context.Vacinacoes
                .Where(v => v.Id == id)
                .Select(v => (int?)v.Animal.PropriedadeId)
                .FirstOrDefaultAsync(),
            TipoRegistroAnimal.Sanitario => await _context.ControlesSanitarios
                .Where(c => c.Id == id)
                .Select(c => (int?)c.Animal.PropriedadeId)
                .FirstOrDefaultAsync(),
            TipoRegistroAnimal.Reproducao => await _context.RegistrosReproducao
                .Where(r => r.Id == id)
                .Select(r => (int?)r.Animal.PropriedadeId)
                .FirstOrDefaultAsync(),
            _ => null
        };
    }

    public async Task<bool> ExcluirRegistroAsync(TipoRegistroAnimal tipo, int id)
    {
        var excluidos = tipo switch
        {
            TipoRegistroAnimal.Pesagem => await _context.Pesagens
                .Where(p => p.Id == id).ExecuteDeleteAsync(),
            TipoRegistroAnimal.Vacinacao => await _context.Vacinacoes
                .Where(v => v.Id == id).ExecuteDeleteAsync(),
            TipoRegistroAnimal.Sanitario => await _context.ControlesSanitarios
                .Where(c => c.Id == id).ExecuteDeleteAsync(),
            TipoRegistroAnimal.Reproducao => await _context.RegistrosReproducao
                .Where(r => r.Id == id).ExecuteDeleteAsync(),
            _ => 0
        };

        return excluidos > 0;
    }

    public async Task<bool> ExisteVacinacaoAsync(
        int animalId,
        string nomeVacina,
        DateTime dataAplicacao)
    {
        var nome = nomeVacina.Trim().ToLower();
        var dia = dataAplicacao.Date;

        return await _context.Vacinacoes.AnyAsync(v =>
            v.AnimalId == animalId &&
            v.NomeVacina.ToLower() == nome &&
            v.DataAplicacao >= dia &&
            v.DataAplicacao < dia.AddDays(1));
    }

    public async Task<bool> RegistrarVacinacaoComBaixaAsync(
        Vacinacao vacinacao,
        int estoqueItemId)
    {
        await using var transaction =
            await _context.Database.BeginTransactionAsync();

        try
        {
            // Baixa condicional: só desconta se ainda houver ao menos 1 dose.
            var baixados = await _context.EstoqueItens
                .Where(e => e.Id == estoqueItemId && e.QuantidadeAtual >= 1)
                .ExecuteUpdateAsync(s => s.SetProperty(
                    e => e.QuantidadeAtual,
                    e => e.QuantidadeAtual - 1));

            if (baixados == 0)
            {
                await transaction.RollbackAsync();
                return false;
            }

            await _context.Vacinacoes.AddAsync(vacinacao);
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();
            return true;
        }
        catch
        {
            await transaction.RollbackAsync();
            _context.ChangeTracker.Clear();
            throw;
        }
    }
}