using AgroVerde.Domain.Entities;

namespace AgroVerde.Domain.Repositories;

// O módulo de Talhão pode injetar este repositório e chamar
// BuscarPorIdAsync(propriedadeId) para ler a AreaTotal.
public interface IPropriedadeRepository : IRepository<Propriedade>
{
    Task<List<Propriedade>> ListarPorUsuarioIdAsync(int usuarioId);
}
