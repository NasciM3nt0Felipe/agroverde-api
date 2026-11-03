using System.Globalization;
using AgroVerde.Application.Common;
using AgroVerde.Application.DTOs;
using AgroVerde.Domain.Entities;
using AgroVerde.Domain.Repositories;

namespace AgroVerde.Application.Services;

public class AnimalService : IAnimalService
{
    private readonly IAnimalRepository _repository;
    private readonly IEstoqueRepository _estoqueRepository;

    private const string CategoriaVacinas = "Vacinas";

    public AnimalService(
        IAnimalRepository repository,
        IEstoqueRepository estoqueRepository)
    {
        _repository = repository;
        _estoqueRepository = estoqueRepository;
    }

    // Datas aceitas: dd/MM/yyyy (formato do app) ou yyyy-MM-dd (ISO).
    private static readonly string[] FormatosData =
    [
        "dd/MM/yyyy",
        "d/M/yyyy",
        "yyyy-MM-dd",
        "yyyy-MM-ddTHH:mm:ss",
        "yyyy-MM-ddTHH:mm:ss.FFFFFFF"
    ];

    private static bool TentarLerData(string? texto, out DateTime data) =>
        DateTime.TryParseExact(
            texto?.Trim(),
            FormatosData,
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out data);

    public async Task<AppResponse<IEnumerable<AnimalResponse>>>
        ListarPorPropriedadeAsync(int propriedadeId)
    {
        try
        {
            var animais = await _repository
                .ListarPorPropriedadeIdAsync(propriedadeId);

            return AppResponse<IEnumerable<AnimalResponse>>.Ok(
                "Animais listados com sucesso.",
                animais.OrderByDescending(a => a.Id).Select(MapToResponse).ToList()
            );
        }
        catch (Exception ex)
        {
            return AppResponse<IEnumerable<AnimalResponse>>.Fail(
                "Erro ao listar animais.",
                ex
            );
        }
    }

    public async Task<int?> ObterPropriedadeDoAnimalAsync(int animalId)
    {
        var animal = await _repository.BuscarPorIdAsync(animalId);

        return animal?.PropriedadeId;
    }

    public Task<int?> ObterPropriedadeDoRegistroAsync(
        TipoRegistroAnimal tipo,
        int id) =>
        _repository.ObterPropriedadeDoRegistroAsync(tipo, id);

    public async Task<AppResponse<bool>> ExcluirRegistroAsync(
        TipoRegistroAnimal tipo,
        int id)
    {
        try
        {
            var excluido = await _repository.ExcluirRegistroAsync(tipo, id);

            return excluido
                ? AppResponse<bool>.Ok("Registro excluído com sucesso.", true)
                : AppResponse<bool>.Fail(
                    "Registro não encontrado.",
                    ["Registro não encontrado."]);
        }
        catch (Exception ex)
        {
            return AppResponse<bool>.Fail("Erro ao excluir registro.", ex);
        }
    }

    public async Task<AppResponse<List<PesagemResponse>>>
        ListarPesagensPorPropriedadeAsync(int propriedadeId)
    {
        var lista = await _repository
            .ListarPesagensPorPropriedadeAsync(propriedadeId);

        return AppResponse<List<PesagemResponse>>.Ok(
            "Pesagens listadas com sucesso.",
            lista.Select(MapPesagem).ToList());
    }

    public async Task<AppResponse<List<VacinacaoResponse>>>
        ListarVacinacoesPorPropriedadeAsync(int propriedadeId)
    {
        var lista = await _repository
            .ListarVacinacoesPorPropriedadeAsync(propriedadeId);

        return AppResponse<List<VacinacaoResponse>>.Ok(
            "Vacinações listadas com sucesso.",
            lista.Select(MapVacinacao).ToList());
    }

    public async Task<AppResponse<List<ControleSanitarioResponse>>>
        ListarControlesSanitariosPorPropriedadeAsync(int propriedadeId)
    {
        var lista = await _repository
            .ListarControlesSanitariosPorPropriedadeAsync(propriedadeId);

        return AppResponse<List<ControleSanitarioResponse>>.Ok(
            "Controles sanitários listados com sucesso.",
            lista.Select(MapSanitario).ToList());
    }

    public async Task<AppResponse<List<RegistroReproducaoResponse>>>
        ListarReproducoesPorPropriedadeAsync(int propriedadeId)
    {
        var lista = await _repository
            .ListarReproducoesPorPropriedadeAsync(propriedadeId);

        return AppResponse<List<RegistroReproducaoResponse>>.Ok(
            "Registros de reprodução listados com sucesso.",
            lista.Select(MapReproducao).ToList());
    }

    private static PesagemResponse MapPesagem(Pesagem p) =>
        new(p.Id, p.AnimalId, p.PesoKg, p.DataPesagem, p.Observacao);

    private static VacinacaoResponse MapVacinacao(Vacinacao v) =>
        new(
            v.Id,
            v.AnimalId,
            v.NomeVacina,
            v.DataAplicacao.ToString("yyyy-MM-dd"),
            v.ProximaDose?.ToString("yyyy-MM-dd"),
            v.Observacao);

    private static ControleSanitarioResponse MapSanitario(ControleSanitario c) =>
        new(
            c.Id,
            c.AnimalId,
            c.Procedimento,
            c.Data.ToString("yyyy-MM-dd"),
            c.Medicamento,
            c.Observacao);

    private static RegistroReproducaoResponse MapReproducao(RegistroReproducao r) =>
        new(r.Id, r.AnimalId, r.Tipo, r.Data.ToString("yyyy-MM-dd"), r.Observacao);

    public async Task<AppResponse<IEnumerable<AnimalResponse>>> ListarTodosAsync()
    {
        try
        {
            var animais = await _repository.ListarTodosAsync();

            var response = animais.Select(MapToResponse);

            return AppResponse<IEnumerable<AnimalResponse>>.Ok(
                "Animais listados com sucesso.",
                response
            );
        }
        catch (Exception ex)
        {
            return AppResponse<IEnumerable<AnimalResponse>>.Fail(
                "Erro ao listar animais.",
                ex
            );
        }
    }

    public async Task<AppResponse<AnimalDetalhesResponse>> BuscarPorIdAsync(int id)
    {
        try
        {
            var animal = await _repository.BuscarPorIdComDetalhesAsync(id);

            if (animal is null)
            {
                return AppResponse<AnimalDetalhesResponse>.Fail(
                    "Animal não encontrado.",
                    ["Animal não encontrado."]
                );
            }

            var response = new AnimalDetalhesResponse(
                animal.Id,
                animal.PropriedadeId,
                animal.Identificacao,
                animal.Especie,
                animal.Raca,
                animal.Sexo,
                animal.DataNascimento,
                animal.Peso,
                animal.Status,
                animal.Observacao,

                animal.Pesagens
                    .OrderByDescending(p => p.DataPesagem)
                    .ThenByDescending(p => p.Id)
                    .Select(p =>
                    new PesagemResponse(
                        p.Id,
                        p.AnimalId,
                        p.PesoKg,
                        p.DataPesagem,
                        p.Observacao
                    )
                ),

                animal.Vacinacoes
                    .OrderByDescending(v => v.DataAplicacao)
                    .ThenByDescending(v => v.Id)
                    .Select(v =>
                    new VacinacaoResponse(
                        v.Id,
                        v.AnimalId,
                        v.NomeVacina,
                        v.DataAplicacao.ToString("yyyy-MM-dd"),
                        v.ProximaDose?.ToString("yyyy-MM-dd"),
                        v.Observacao
                    )
                ),

                animal.ControlesSanitarios
                    .OrderByDescending(c => c.Data)
                    .ThenByDescending(c => c.Id)
                    .Select(c =>
                    new ControleSanitarioResponse(
                        c.Id,
                        c.AnimalId,
                        c.Procedimento,
                        c.Data.ToString("yyyy-MM-dd"),
                        c.Medicamento,
                        c.Observacao
                    )
                ),

                animal.Reproducoes
                    .OrderByDescending(r => r.Data)
                    .ThenByDescending(r => r.Id)
                    .Select(r =>
                    new RegistroReproducaoResponse(
                        r.Id,
                        r.AnimalId,
                        r.Tipo,
                        r.Data.ToString("yyyy-MM-dd"),
                        r.Observacao
                    )
                )
            );

            return AppResponse<AnimalDetalhesResponse>.Ok(
                "Animal encontrado com sucesso.",
                response
            );
        }
        catch (Exception ex)
        {
            return AppResponse<AnimalDetalhesResponse>.Fail(
                "Erro ao buscar animal.",
                ex
            );
        }
    }

    public async Task<AppResponse<AnimalResponse>> InserirAsync(
        AnimalRequest request)
    {
        try
        {
            var erro = ValidarAnimal(request);

            if (erro is not null)
            {
                return AppResponse<AnimalResponse>.Fail(
                    erro,
                    [erro]
                );
            }

            var animal = new Animal
            {
                PropriedadeId = request.PropriedadeId,
                Identificacao = request.Identificacao.Trim(),
                Especie = request.Especie.Trim(),
                Raca = request.Raca?.Trim(),
                Sexo = request.Sexo.Trim(),
                DataNascimento = request.DataNascimento?.Trim(),
                Peso = request.Peso,
                Status = request.Status.Trim(),
                Observacao = request.Observacao?.Trim()
            };

            var inserido = await _repository.InserirAsync(animal);

            return AppResponse<AnimalResponse>.Ok(
                "Animal cadastrado com sucesso.",
                MapToResponse(inserido)
            );
        }
        catch (Exception ex)
        {
            return AppResponse<AnimalResponse>.Fail(
                "Erro ao cadastrar animal.",
                ex
            );
        }
    }

    public async Task<AppResponse<AnimalResponse>> AtualizarAsync(
        int id,
        AnimalRequest request)
    {
        try
        {
            var erro = ValidarAnimal(request);

            if (erro is not null)
            {
                return AppResponse<AnimalResponse>.Fail(
                    erro,
                    [erro]
                );
            }

            var animal = await _repository.BuscarPorIdAsync(id);

            if (animal is null)
            {
                return AppResponse<AnimalResponse>.Fail(
                    "Animal não encontrado.",
                    ["Animal não encontrado."]
                );
            }

            animal.PropriedadeId = request.PropriedadeId;
            animal.Identificacao = request.Identificacao.Trim();
            animal.Especie = request.Especie.Trim();
            animal.Raca = request.Raca?.Trim();
            animal.Sexo = request.Sexo.Trim();
            animal.DataNascimento = request.DataNascimento?.Trim();
            animal.Peso = request.Peso;
            animal.Status = request.Status.Trim();
            animal.Observacao = request.Observacao?.Trim();

            await _repository.AtualizarAsync(animal);

            return AppResponse<AnimalResponse>.Ok(
                "Animal atualizado com sucesso.",
                MapToResponse(animal)
            );
        }
        catch (Exception ex)
        {
            return AppResponse<AnimalResponse>.Fail(
                "Erro ao atualizar animal.",
                ex
            );
        }
    }

    public async Task<AppResponse<bool>> ExcluirAsync(int id)
    {
        try
        {
            var animal = await _repository.BuscarPorIdAsync(id);

            if (animal is null)
            {
                return AppResponse<bool>.Fail(
                    "Animal não encontrado.",
                    ["Animal não encontrado."]
                );
            }

            await _repository.ExcluirAsync(id);

            return AppResponse<bool>.Ok(
                "Animal excluído com sucesso.",
                true
            );
        }
        catch (Exception ex)
        {
            return AppResponse<bool>.Fail(
                "Erro ao excluir animal.",
                ex
            );
        }
    }

    public async Task<AppResponse<PesagemResponse>> AdicionarPesagemAsync(
        PesagemRequest request)
    {
        try
        {
            var animal =
                await _repository.BuscarPorIdComDetalhesAsync(request.AnimalId);

            if (animal is null)
            {
                return AppResponse<PesagemResponse>.Fail(
                    "Animal não encontrado.",
                    ["Animal não encontrado."]
                );
            }

            if (request.PesoKg <= 0)
            {
                return AppResponse<PesagemResponse>.Fail(
                    "O peso deve ser maior que zero.",
                    ["O peso deve ser maior que zero."]
                );
            }

            var pesagem = new Pesagem
            {
                AnimalId = request.AnimalId,
                PesoKg = request.PesoKg,
                DataPesagem = request.DataPesagem,
                Observacao = request.Observacao?.Trim()
            };

            animal.Pesagens.Add(pesagem);

            await _repository.AtualizarAsync(animal);

            var response = new PesagemResponse(
                pesagem.Id,
                pesagem.AnimalId,
                pesagem.PesoKg,
                pesagem.DataPesagem,
                pesagem.Observacao
            );

            return AppResponse<PesagemResponse>.Ok(
                "Pesagem registrada com sucesso.",
                response
            );
        }
        catch (Exception ex)
        {
            return AppResponse<PesagemResponse>.Fail(
                "Erro ao registrar pesagem.",
                ex
            );
        }
    }

    public async Task<AppResponse<VacinacaoResponse>> AdicionarVacinacaoAsync(
        VacinacaoRequest request)
    {
        try
        {
            var animal =
                await _repository.BuscarPorIdComDetalhesAsync(request.AnimalId);

            if (animal is null)
            {
                return AppResponse<VacinacaoResponse>.Fail(
                    "Animal não encontrado.",
                    ["Animal não encontrado."]
                );
            }

            if (string.IsNullOrWhiteSpace(request.Vacina))
            {
                return AppResponse<VacinacaoResponse>.Fail(
                    "A vacina é obrigatória.",
                    ["A vacina é obrigatória."]
                );
            }

            if (!TentarLerData(request.DataAplicacao, out var dataAplicacao))
            {
                return AppResponse<VacinacaoResponse>.Fail(
                    "A data de aplicação é inválida.",
                    ["A data de aplicação é inválida."]
                );
            }

            DateTime? proximaDose = null;

            if (!string.IsNullOrWhiteSpace(request.ProximaDose))
            {
                if (!TentarLerData(
                        request.ProximaDose,
                        out var dataProximaDose))
                {
                    return AppResponse<VacinacaoResponse>.Fail(
                        "A data da próxima dose é inválida.",
                        ["A data da próxima dose é inválida."]
                    );
                }

                proximaDose = dataProximaDose;
            }

            // A dose sai do estoque da mesma propriedade do animal.
            EstoqueItem? item;

            if (request.EstoqueItemId.HasValue)
            {
                item = await _estoqueRepository
                    .BuscarPorIdAsync(request.EstoqueItemId.Value);

                if (item is not null &&
                    (item.PropriedadeId != animal.PropriedadeId ||
                     !string.Equals(
                         item.Categoria.Trim(),
                         CategoriaVacinas,
                         StringComparison.OrdinalIgnoreCase)))
                {
                    item = null;
                }
            }
            else
            {
                item = await _estoqueRepository
                    .BuscarPorNomeCategoriaEPropriedadeAsync(
                        animal.PropriedadeId,
                        request.Vacina.Trim(),
                        CategoriaVacinas);
            }

            if (item is null)
            {
                return AppResponse<VacinacaoResponse>.Fail(
                    "Vacina não cadastrada no estoque desta propriedade.",
                    ["Vacina não cadastrada no estoque desta propriedade."]
                );
            }

            if (item.QuantidadeAtual < 1)
            {
                return AppResponse<VacinacaoResponse>.Fail(
                    "Não há estoque disponível para esta vacina.",
                    ["Não há estoque disponível para esta vacina."]
                );
            }

            // Evita registrar (e baixar) a mesma dose duas vezes.
            if (await _repository.ExisteVacinacaoAsync(
                    animal.Id, item.Nome, dataAplicacao))
            {
                return AppResponse<VacinacaoResponse>.Fail(
                    "Esta vacina já foi registrada para o animal nesta data.",
                    ["Esta vacina já foi registrada para o animal nesta data."]
                );
            }

            var vacinacao = new Vacinacao
            {
                AnimalId = request.AnimalId,
                NomeVacina = item.Nome.Trim(),
                DataAplicacao = dataAplicacao,
                ProximaDose = proximaDose,
                Observacao = request.Observacao?.Trim()
            };

            // Baixa da dose e histórico na mesma transação.
            var registrado = await _repository
                .RegistrarVacinacaoComBaixaAsync(vacinacao, item.Id);

            if (!registrado)
            {
                return AppResponse<VacinacaoResponse>.Fail(
                    "Não há estoque disponível para esta vacina.",
                    ["Não há estoque disponível para esta vacina."]
                );
            }

            var response = new VacinacaoResponse(
                vacinacao.Id,
                vacinacao.AnimalId,
                vacinacao.NomeVacina,
                vacinacao.DataAplicacao.ToString("yyyy-MM-dd"),
                vacinacao.ProximaDose?.ToString("yyyy-MM-dd"),
                vacinacao.Observacao
            );

            return AppResponse<VacinacaoResponse>.Ok(
                "Vacinação registrada com sucesso.",
                response
            );
        }
        catch (Exception ex)
        {
            return AppResponse<VacinacaoResponse>.Fail(
                "Erro ao registrar vacinação.",
                ex
            );
        }
    }

    public async Task<AppResponse<ControleSanitarioResponse>>
        AdicionarControleSanitarioAsync(
            ControleSanitarioRequest request)
    {
        try
        {
            var animal =
                await _repository.BuscarPorIdComDetalhesAsync(request.AnimalId);

            if (animal is null)
            {
                return AppResponse<ControleSanitarioResponse>.Fail(
                    "Animal não encontrado.",
                    ["Animal não encontrado."]
                );
            }

            if (string.IsNullOrWhiteSpace(request.Procedimento))
            {
                return AppResponse<ControleSanitarioResponse>.Fail(
                    "O procedimento é obrigatório.",
                    ["O procedimento é obrigatório."]
                );
            }

            if (!TentarLerData(request.Data, out var data))
            {
                return AppResponse<ControleSanitarioResponse>.Fail(
                    "A data é inválida.",
                    ["A data é inválida."]
                );
            }

            var controle = new ControleSanitario
            {
                AnimalId = request.AnimalId,
                Procedimento = request.Procedimento.Trim(),
                Data = data,
                Medicamento = request.Medicamento?.Trim(),
                Observacao = request.Observacao?.Trim()
            };

            animal.ControlesSanitarios.Add(controle);

            await _repository.AtualizarAsync(animal);

            var response = new ControleSanitarioResponse(
                controle.Id,
                controle.AnimalId,
                controle.Procedimento,
                controle.Data.ToString("yyyy-MM-dd"),
                controle.Medicamento,
                controle.Observacao
            );

            return AppResponse<ControleSanitarioResponse>.Ok(
                "Controle sanitário registrado com sucesso.",
                response
            );
        }
        catch (Exception ex)
        {
            return AppResponse<ControleSanitarioResponse>.Fail(
                "Erro ao registrar controle sanitário.",
                ex
            );
        }
    }

    public async Task<AppResponse<RegistroReproducaoResponse>>
        AdicionarRegistroReproducaoAsync(
            RegistroReproducaoRequest request)
    {
        try
        {
            var animal =
                await _repository.BuscarPorIdComDetalhesAsync(request.AnimalId);

            if (animal is null)
            {
                return AppResponse<RegistroReproducaoResponse>.Fail(
                    "Animal não encontrado.",
                    ["Animal não encontrado."]
                );
            }

            if (string.IsNullOrWhiteSpace(request.Tipo))
            {
                return AppResponse<RegistroReproducaoResponse>.Fail(
                    "O tipo do registro é obrigatório.",
                    ["O tipo do registro é obrigatório."]
                );
            }

            var tiposValidos = new[]
            {
                "Cobertura",
                "Inseminação",
                "Prenhez",
                "Parto"
            };

            var tipo = tiposValidos.FirstOrDefault(
                t => string.Equals(
                    t,
                    request.Tipo.Trim(),
                    StringComparison.OrdinalIgnoreCase)
            );

            if (tipo is null)
            {
                return AppResponse<RegistroReproducaoResponse>.Fail(
                    "O tipo do registro de reprodução é inválido.",
                    ["Os tipos válidos são: Cobertura, Inseminação, Prenhez e Parto."]
                );
            }

            if (!TentarLerData(request.Data, out var data))
            {
                return AppResponse<RegistroReproducaoResponse>.Fail(
                    "A data é inválida.",
                    ["A data é inválida."]
                );
            }

            var registro = new RegistroReproducao
            {
                AnimalId = request.AnimalId,
                Tipo = tipo,
                Data = data,
                Observacao = request.Observacao?.Trim()
            };

            animal.Reproducoes.Add(registro);

            await _repository.AtualizarAsync(animal);

            var response = new RegistroReproducaoResponse(
                registro.Id,
                registro.AnimalId,
                registro.Tipo,
                registro.Data.ToString("yyyy-MM-dd"),
                registro.Observacao
            );

            return AppResponse<RegistroReproducaoResponse>.Ok(
                "Registro de reprodução adicionado com sucesso.",
                response
            );
        }
        catch (Exception ex)
        {
            return AppResponse<RegistroReproducaoResponse>.Fail(
                "Erro ao registrar reprodução.",
                ex
            );
        }
    }

    private static string? ValidarAnimal(AnimalRequest request)
    {
        if (request.PropriedadeId <= 0)
            return "A propriedade é obrigatória.";

        if (string.IsNullOrWhiteSpace(request.Identificacao))
            return "A identificação do animal é obrigatória.";

        if (string.IsNullOrWhiteSpace(request.Especie))
            return "A espécie do animal é obrigatória.";

        if (string.IsNullOrWhiteSpace(request.Sexo))
            return "O sexo do animal é obrigatório.";

        if (string.IsNullOrWhiteSpace(request.Status))
            return "O status do animal é obrigatório.";

        if (request.Peso.HasValue && request.Peso.Value <= 0)
            return "O peso do animal deve ser maior que zero.";

        return null;
    }

    private static AnimalResponse MapToResponse(Animal animal)
    {
        return new AnimalResponse(
            animal.Id,
            animal.PropriedadeId,
            animal.Identificacao,
            animal.Especie,
            animal.Raca,
            animal.Sexo,
            animal.DataNascimento,
            animal.Peso,
            animal.Status,
            animal.Observacao
        );
    }
}