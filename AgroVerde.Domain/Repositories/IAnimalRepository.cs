using AgroVerde.Domain.Entities;

namespace AgroVerde.Domain.Repositories;

public interface IAnimalRepository : IRepository<Animal>
{
    Task<Animal?> BuscarPorIdComDetalhesAsync(int id);
    Task<IEnumerable<Animal>> BuscarPorStatusAsync(string status);
    Task<IEnumerable<Animal>> ListarPorPropriedadeIdAsync(int propriedadeId);
    
}