using AgroVerde.Domain.Entities;

namespace AgroVerde.Domain.Repositories;

public interface ISafraRepository : IRepository<Safra>
{
    Task<List<Safra>> ListarPorTalhaoIdAsync(int talhaoId);

    Task<bool> ExisteSafraAtivaAsync(
        int talhaoId,
        int? ignorarSafraId = null
    );

    Task<List<Safra>> ListarDisponiveisParaColheitaPorPropriedadeAsync(
        int propriedadeId
    );
}