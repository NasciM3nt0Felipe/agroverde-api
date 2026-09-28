using AgroVerde.Domain.Entities;

namespace AgroVerde.Domain.Repositories;

public interface IUsuarioRepository : IRepository<Usuario>
{
    Task<Usuario?> BuscarPorEmailAsync(string email);

    Task<bool> EmailExisteAsync(string email);
}
