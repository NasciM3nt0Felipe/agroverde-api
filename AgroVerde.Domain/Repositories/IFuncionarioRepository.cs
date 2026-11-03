using AgroVerde.Domain.Entities;

namespace AgroVerde.Domain.Repositories;

public interface IFuncionarioRepository
{
    // Somente leitura. Já traz a Pessoa. Ordem: status, cargo (como no app).
    Task<List<Funcionario>> ListarPorPropriedadesAsync(
        IReadOnlyCollection<int> propriedadeIds,
        bool somenteAtivos);

    // Rastreado (para atualizar/excluir). Já traz a Pessoa.
    Task<Funcionario?> BuscarPorIdAsync(int id);

    // Já existe vínculo desta pessoa com esta propriedade?
    Task<bool> ExisteVinculoAsync(int pessoaId, int propriedadeId, int? ignorarFuncionarioId = null);

    // Pessoa (rastreada) com este CPF, se houver.
    Task<Pessoa?> BuscarPessoaPorDocumentoAsync(string cpf);

    // A pessoa é funcionária de alguma propriedade de OUTRO usuário?
    Task<bool> PessoaVinculadaAOutroUsuarioAsync(int pessoaId, int usuarioId);

    // Quantos vínculos de funcionário a pessoa possui?
    Task<int> ContarVinculosDaPessoaAsync(int pessoaId);

    Task AdicionarAsync(Funcionario funcionario);

    void Remover(Funcionario funcionario);

    void RemoverPessoa(Pessoa pessoa);

    // Grava tudo numa única transação.
    Task SalvarAsync();
}
