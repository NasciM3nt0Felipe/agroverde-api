using System.Collections.Generic;
using System.Threading.Tasks;
using AgroVerde.Domain.Entities;

namespace AgroVerde.Domain.Repositories
{
    public interface IVeiculoRepository
    {
        Task<IEnumerable<Veiculo>> GetAllAsync();
        Task<Veiculo?> GetByIdAsync(int id);
        Task<Veiculo?> ObterPorPlacaOuChassiAsync(string placaOuChassi);
        Task<IEnumerable<Veiculo>> ObterPorStatusAsync(StatusVeiculo status);
        Task<IEnumerable<Veiculo>> ObterPorTipoAsync(TipoVeiculo tipo);
        Task AddAsync(Veiculo entity);
        void Update(Veiculo entity);
        void Delete(Veiculo entity);
        Task SaveChangesAsync();
    }
}