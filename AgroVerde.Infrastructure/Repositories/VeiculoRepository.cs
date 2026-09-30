using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AgroVerde.Domain.Entities;
using AgroVerde.Domain.Repositories;
using AgroVerde.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AgroVerde.Infrastructure.Repositories
{
    public class VeiculoRepository : IVeiculoRepository
    {
        private readonly AgroVerdeDbContext _context;

        public VeiculoRepository(AgroVerdeDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Veiculo>> GetAllAsync()
        {
            return await _context.Veiculos.AsNoTracking().ToListAsync();
        }

        public async Task<Veiculo?> GetByIdAsync(int id)
        {
            return await _context.Veiculos.FindAsync(id);
        }

        public async Task<Veiculo?> ObterPorPlacaOuChassiAsync(string placaOuChassi)
        {
            return await _context.Veiculos.AsNoTracking()
                .FirstOrDefaultAsync(v => v.PlacaOuChassi != null && v.PlacaOuChassi.ToUpper() == placaOuChassi.ToUpper());
        }

        public async Task<IEnumerable<Veiculo>> ObterPorStatusAsync(StatusVeiculo status)
        {
            return await _context.Veiculos.AsNoTracking().Where(v => v.Status == status).ToListAsync();
        }

        public async Task<IEnumerable<Veiculo>> ObterPorTipoAsync(TipoVeiculo tipo)
        {
            return await _context.Veiculos.AsNoTracking().Where(v => v.Tipo == tipo).ToListAsync();
        }

        public async Task AddAsync(Veiculo entity)
        {
            await _context.Veiculos.AddAsync(entity);
        }

        public void Update(Veiculo entity)
        {
            _context.Veiculos.Update(entity);
        }

        public void Delete(Veiculo entity)
        {
            _context.Veiculos.Remove(entity);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}