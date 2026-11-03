using AgroVerde.Domain.Entities;
using AgroVerde.Domain.Repositories;
using AgroVerde.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AgroVerde.Infrastructure.Repositories;

public class FuncionarioRepository : IFuncionarioRepository
{
    private readonly AgroVerdeDbContext _context;

    public FuncionarioRepository(AgroVerdeDbContext context)
    {
        _context = context;
    }

    public async Task<List<Funcionario>> ListarPorPropriedadesAsync(
        IReadOnlyCollection<int> propriedadeIds,
        bool somenteAtivos)
    {
        var ids = propriedadeIds.ToList();

        var consulta = _context.Funcionarios
            .AsNoTracking()
            .Include(f => f.Pessoa)
            .Where(f => ids.Contains(f.PropriedadeId));

        if (somenteAtivos)
        {
            consulta = consulta.Where(f => f.Status == StatusFuncionario.Ativo);
        }

        return await consulta
            .OrderBy(f => f.Status)
            .ThenBy(f => f.Cargo)
            .ToListAsync();
    }

    public async Task<Funcionario?> BuscarPorIdAsync(int id)
    {
        return await _context.Funcionarios
            .Include(f => f.Pessoa)
            .FirstOrDefaultAsync(f => f.Id == id);
    }

    public async Task<bool> ExisteVinculoAsync(
        int pessoaId,
        int propriedadeId,
        int? ignorarFuncionarioId = null)
    {
        return await _context.Funcionarios.AnyAsync(f =>
            f.PessoaId == pessoaId
            && f.PropriedadeId == propriedadeId
            && (ignorarFuncionarioId == null || f.Id != ignorarFuncionarioId));
    }

    public async Task<Pessoa?> BuscarPessoaPorDocumentoAsync(string cpf)
    {
        return await _context.Pessoas
            .FirstOrDefaultAsync(p => p.CpfCnpj == cpf);
    }

    public async Task<bool> PessoaVinculadaAOutroUsuarioAsync(int pessoaId, int usuarioId)
    {
        return await _context.Funcionarios.AnyAsync(f =>
            f.PessoaId == pessoaId
            && _context.Propriedades.Any(p =>
                p.Id == f.PropriedadeId && p.UsuarioId != usuarioId));
    }

    public async Task<int> ContarVinculosDaPessoaAsync(int pessoaId)
    {
        return await _context.Funcionarios.CountAsync(f => f.PessoaId == pessoaId);
    }

    public async Task AdicionarAsync(Funcionario funcionario)
    {
        await _context.Funcionarios.AddAsync(funcionario);
    }

    public void Remover(Funcionario funcionario)
    {
        _context.Funcionarios.Remove(funcionario);
    }

    public void RemoverPessoa(Pessoa pessoa)
    {
        _context.Pessoas.Remove(pessoa);
    }

    public async Task SalvarAsync()
    {
        await _context.SaveChangesAsync();
    }
}
