using AgroVerde.Domain.Entities;

namespace AgroVerde.Domain.Repositories;

public interface ITalhaoRepository : IRepository<Talhao>
{
    Task<List<Talhao>> ListarPorPropriedadeIdAsync(int propriedadeId);
}