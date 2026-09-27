namespace AgroVerde.Domain.Repositories;

public interface IRepository<T>
{
    Task<T> InserirAsync(T entidade);

    Task<T?> BuscarPorIdAsync(int id);

    Task<List<T>> ListarTodosAsync();

    Task AtualizarAsync(T entidade);

    Task ExcluirAsync(int id);
}