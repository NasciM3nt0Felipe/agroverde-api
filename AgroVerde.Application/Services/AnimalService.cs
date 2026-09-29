using AgroVerde.Application.Common;
using AgroVerde.Application.DTOs;
using AgroVerde.Domain.Entities;
using AgroVerde.Domain.Repositories;

namespace AgroVerde.Application.Services;

public class AnimalService : IAnimalService
{
    private readonly IAnimalRepository _repository;

    public AnimalService(IAnimalRepository repository)
    {
        _repository = repository;
    }

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

                animal.Pesagens.Select(p =>
                    new PesagemResponse(
                        p.Id,
                        p.AnimalId,
                        p.PesoKg,
                        p.DataPesagem,
                        p.Observacao
                    )
                ),

                animal.Vacinacoes.Select(v =>
                    new VacinacaoResponse(
                        v.Id,
                        v.AnimalId,
                        v.NomeVacina,
                        v.DataAplicacao.ToString("yyyy-MM-dd"),
                        v.ProximaDose?.ToString("yyyy-MM-dd"),
                        v.Observacao
                    )
                ),

                animal.ControlesSanitarios.Select(c =>
                    new ControleSanitarioResponse(
                        c.Id,
                        c.AnimalId,
                        c.Procedimento,
                        c.Data.ToString("yyyy-MM-dd"),
                        c.Medicamento,
                        c.Observacao
                    )
                ),

                animal.Reproducoes.Select(r =>
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

            if (!DateTime.TryParse(request.DataAplicacao, out var dataAplicacao))
            {
                return AppResponse<VacinacaoResponse>.Fail(
                    "A data de aplicação é inválida.",
                    ["A data de aplicação é inválida."]
                );
            }

            DateTime? proximaDose = null;

            if (!string.IsNullOrWhiteSpace(request.ProximaDose))
            {
                if (!DateTime.TryParse(
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

            var vacinacao = new Vacinacao
            {
                AnimalId = request.AnimalId,
                NomeVacina = request.Vacina.Trim(),
                DataAplicacao = dataAplicacao,
                ProximaDose = proximaDose,
                Observacao = request.Observacao?.Trim()
            };

            animal.Vacinacoes.Add(vacinacao);

            await _repository.AtualizarAsync(animal);

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

            if (!DateTime.TryParse(request.Data, out var data))
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

            if (!DateTime.TryParse(request.Data, out var data))
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