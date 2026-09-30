using System.Collections.Generic;
using System.Threading.Tasks;
using AgroVerde.Domain.Entities;

namespace AgroVerde.Domain.Repositories
{
    public interface IPessoaRepository
    {
        Task<IEnumerable<Pessoa>> GetAllAsync();
        Task<Pessoa?> GetByIdAsync(int id);
        Task<Pessoa?> ObterPorDocumentoAsync(string cpfCnpj);
        Task<Pessoa?> ObterPorEmailAsync(string email);
        Task<IEnumerable<Pessoa>> ObterPorTipoAsync(TipoPessoa tipo);
        Task<IEnumerable<Pessoa>> ObterAtivosAsync();
        Task AddAsync(Pessoa entity);
        void Update(Pessoa entity);
        void Delete(Pessoa entity);
        Task SaveChangesAsync();
    }
}