using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AgroVerde.Domain.Entities;
using AgroVerde.Domain.Repositories;
using AgroVerde.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AgroVerde.Infrastructure.Repositories
{
    public class PessoaRepository : IPessoaRepository
    {
        private readonly AgroVerdeDbContext _context;

        public PessoaRepository(AgroVerdeDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Pessoa>> GetAllAsync()
        {
            return await _context.Pessoas.AsNoTracking().ToListAsync();
        }

        public async Task<Pessoa?> GetByIdAsync(int id)
        {
            return await _context.Pessoas.FindAsync(id);
        }

        public async Task<Pessoa?> ObterPorDocumentoAsync(string cpfCnpj)
        {
            return await _context.Pessoas.AsNoTracking()
                .FirstOrDefaultAsync(p => p.CpfCnpj == cpfCnpj);
        }

        public async Task<Pessoa?> ObterPorEmailAsync(string email)
        {
            return await _context.Pessoas.AsNoTracking()
                .FirstOrDefaultAsync(p => p.Email != null && p.Email.ToLower() == email.ToLower());
        }

        public async Task<IEnumerable<Pessoa>> ObterPorTipoAsync(TipoPessoa tipo)
        {
            return await _context.Pessoas.AsNoTracking().Where(p => p.Tipo == tipo).ToListAsync();
        }

        public async Task<IEnumerable<Pessoa>> ObterAtivosAsync()
        {
            return await _context.Pessoas.AsNoTracking().Where(p => p.Ativo).ToListAsync();
        }

        public async Task AddAsync(Pessoa entity)
        {
            await _context.Pessoas.AddAsync(entity);
        }

        public void Update(Pessoa entity)
        {
            _context.Pessoas.Update(entity);
        }

        public void Delete(Pessoa entity)
        {
            _context.Pessoas.Remove(entity);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}