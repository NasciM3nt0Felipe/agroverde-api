using AgroVerde.Application.Common;
using AgroVerde.Application.DTOs;
using AgroVerde.Domain.Entities;
using AgroVerde.Domain.Repositories;

namespace AgroVerde.Application.Services;

public class SafraService : ISafraService
{
    private readonly ISafraRepository _safraRepository;

    public SafraService(ISafraRepository safraRepository)
    {
        _safraRepository = safraRepository;
    }
    public async Task<AppResponse<SafraResponse>> CriarAsync(SafraRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Nome))
            {
                return AppResponse<SafraResponse>.Fail(
                    "Informe o nome da safra."
                );
            }

            if (string.IsNullOrWhiteSpace(request.Cultura))
            {
                return AppResponse<SafraResponse>.Fail(
                    "Informe a cultura."
                );
            }

            if (string.IsNullOrWhiteSpace(request.DataPlantio))
            {
                return AppResponse<SafraResponse>.Fail(
                    "Informe a data de plantio."
                );
            }

            if (request.Status == "Planejada" || request.Status == "Em andamento")
            {
                var existeSafraAtiva =
                    await _safraRepository.ExisteSafraAtivaAsync(request.TalhaoId);

                if (existeSafraAtiva)
                {
                    return AppResponse<SafraResponse>.Fail(
                        "Já existe uma safra ativa neste talhão."
                    );
                }
            }

            var safra = new Safra
            {
                TalhaoId = request.TalhaoId,
                Nome = request.Nome.Trim(),
                Cultura = request.Cultura.Trim(),
                Variedade = request.Variedade,
                DataPlantio = request.DataPlantio,
                DataColheitaPrevista = request.DataColheitaPrevista,
                DataColheitaReal = request.DataColheitaReal,
                ProducaoEstimada = request.ProducaoEstimada,
                ProducaoObtida = request.ProducaoObtida,
                Status = request.Status,
                Observacao = request.Observacao
            };

            var safraCriada = await _safraRepository.InserirAsync(safra);

            var response = new SafraResponse
            {
                Id = safraCriada.Id,
                TalhaoId = safraCriada.TalhaoId,
                Nome = safraCriada.Nome,
                Cultura = safraCriada.Cultura,
                Variedade = safraCriada.Variedade,
                DataPlantio = safraCriada.DataPlantio,
                DataColheitaPrevista = safraCriada.DataColheitaPrevista,
                DataColheitaReal = safraCriada.DataColheitaReal,
                ProducaoEstimada = safraCriada.ProducaoEstimada,
                ProducaoObtida = safraCriada.ProducaoObtida,
                Status = safraCriada.Status,
                Observacao = safraCriada.Observacao
            };

            return AppResponse<SafraResponse>.Ok(
                "Safra cadastrada com sucesso.",
                response
            );
        }
        catch (Exception ex)
        {
            return AppResponse<SafraResponse>.Fail(
                "Erro ao cadastrar a safra.",
                ex
            );
        }
    }
    public async Task<AppResponse<SafraResponse>> BuscarPorIdAsync(int id)
    {
        try
        {
            var safra = await _safraRepository.BuscarPorIdAsync(id);

            if (safra == null)
            {
                return AppResponse<SafraResponse>.Fail(
                    "Safra não encontrada."
                );
            }

            var response = new SafraResponse
            {
                Id = safra.Id,
                TalhaoId = safra.TalhaoId,
                Nome = safra.Nome,
                Cultura = safra.Cultura,
                Variedade = safra.Variedade,
                DataPlantio = safra.DataPlantio,
                DataColheitaPrevista = safra.DataColheitaPrevista,
                DataColheitaReal = safra.DataColheitaReal,
                ProducaoEstimada = safra.ProducaoEstimada,
                ProducaoObtida = safra.ProducaoObtida,
                Status = safra.Status,
                Observacao = safra.Observacao
            };

            return AppResponse<SafraResponse>.Ok(
                "Safra encontrada com sucesso.",
                response
            );
        }
        catch (Exception ex)
        {
            return AppResponse<SafraResponse>.Fail(
                "Erro ao buscar a safra.",
                ex
            );
        }
    }
    public async Task<AppResponse<List<SafraResponse>>> ListarPorTalhaoIdAsync(int talhaoId)
    {
        try
        {
            var safras = await _safraRepository.ListarPorTalhaoIdAsync(talhaoId);

            var response = safras.Select(safra => new SafraResponse
            {
                Id = safra.Id,
                TalhaoId = safra.TalhaoId,
                Nome = safra.Nome,
                Cultura = safra.Cultura,
                Variedade = safra.Variedade,
                DataPlantio = safra.DataPlantio,
                DataColheitaPrevista = safra.DataColheitaPrevista,
                DataColheitaReal = safra.DataColheitaReal,
                ProducaoEstimada = safra.ProducaoEstimada,
                ProducaoObtida = safra.ProducaoObtida,
                Status = safra.Status,
                Observacao = safra.Observacao
            }).ToList();

            return AppResponse<List<SafraResponse>>.Ok(
                "Safras listadas com sucesso.",
                response
            );
        }
        catch (Exception ex)
        {
            return AppResponse<List<SafraResponse>>.Fail(
                "Erro ao listar as safras.",
                ex
            );
        }
    }
    public async Task<AppResponse<SafraResponse>> AtualizarAsync(
    int id,
    SafraRequest request)
    {
        try
        {
            var safra = await _safraRepository.BuscarPorIdAsync(id);

            if (safra == null)
            {
                return AppResponse<SafraResponse>.Fail(
                    "Safra não encontrada."
                );
            }

            if (string.IsNullOrWhiteSpace(request.Nome))
            {
                return AppResponse<SafraResponse>.Fail(
                    "Informe o nome da safra."
                );
            }

            if (string.IsNullOrWhiteSpace(request.Cultura))
            {
                return AppResponse<SafraResponse>.Fail(
                    "Informe a cultura."
                );
            }

            if (string.IsNullOrWhiteSpace(request.DataPlantio))
            {
                return AppResponse<SafraResponse>.Fail(
                    "Informe a data de plantio."
                );
            }

            if (request.Status == "Planejada" || request.Status == "Em andamento")
            {
                var existeSafraAtiva =
                    await _safraRepository.ExisteSafraAtivaAsync(
                        request.TalhaoId,
                        id
                    );

                if (existeSafraAtiva)
                {
                    return AppResponse<SafraResponse>.Fail(
                        "Já existe uma safra ativa neste talhão."
                    );
                }
            }

            safra.TalhaoId = request.TalhaoId;
            safra.Nome = request.Nome.Trim();
            safra.Cultura = request.Cultura.Trim();
            safra.Variedade = request.Variedade;
            safra.DataPlantio = request.DataPlantio;
            safra.DataColheitaPrevista = request.DataColheitaPrevista;
            safra.DataColheitaReal = request.DataColheitaReal;
            safra.ProducaoEstimada = request.ProducaoEstimada;
            safra.ProducaoObtida = request.ProducaoObtida;
            safra.Status = request.Status;
            safra.Observacao = request.Observacao;

            await _safraRepository.AtualizarAsync(safra);

            var response = new SafraResponse
            {
                Id = safra.Id,
                TalhaoId = safra.TalhaoId,
                Nome = safra.Nome,
                Cultura = safra.Cultura,
                Variedade = safra.Variedade,
                DataPlantio = safra.DataPlantio,
                DataColheitaPrevista = safra.DataColheitaPrevista,
                DataColheitaReal = safra.DataColheitaReal,
                ProducaoEstimada = safra.ProducaoEstimada,
                ProducaoObtida = safra.ProducaoObtida,
                Status = safra.Status,
                Observacao = safra.Observacao
            };

            return AppResponse<SafraResponse>.Ok(
                "Safra atualizada com sucesso.",
                response
            );
        }
        catch (Exception ex)
        {
            return AppResponse<SafraResponse>.Fail(
                "Erro ao atualizar a safra.",
                ex
            );
        }
    }
    public async Task<AppResponse<SafraResponse>> ExcluirAsync(int id)
    {
        try
        {
            var safra = await _safraRepository.BuscarPorIdAsync(id);

            if (safra == null)
            {
                return AppResponse<SafraResponse>.Fail(
                    "Safra não encontrada."
                );
            }

            await _safraRepository.ExcluirAsync(id);

            var response = new SafraResponse
            {
                Id = safra.Id,
                TalhaoId = safra.TalhaoId,
                Nome = safra.Nome,
                Cultura = safra.Cultura,
                Variedade = safra.Variedade,
                DataPlantio = safra.DataPlantio,
                DataColheitaPrevista = safra.DataColheitaPrevista,
                DataColheitaReal = safra.DataColheitaReal,
                ProducaoEstimada = safra.ProducaoEstimada,
                ProducaoObtida = safra.ProducaoObtida,
                Status = safra.Status,
                Observacao = safra.Observacao
            };

            return AppResponse<SafraResponse>.Ok(
                "Safra excluída com sucesso.",
                response
            );
        }
        catch (Exception ex)
        {
            return AppResponse<SafraResponse>.Fail(
                "Erro ao excluir a safra.",
                ex
            );
        }
    }
    public async Task<AppResponse<List<SafraResponse>>> ListarDisponiveisParaColheitaPorPropriedadeAsync(
        int propriedadeId)
    {
        try
        {
            var safras = await _safraRepository
                .ListarDisponiveisParaColheitaPorPropriedadeAsync(propriedadeId);

            var response = safras
                .Select(s => new SafraResponse
                {
                    Id = s.Id,
                    TalhaoId = s.TalhaoId,
                    Nome = s.Nome,
                    Cultura = s.Cultura,
                    Variedade = s.Variedade,
                    DataPlantio = s.DataPlantio,
                    DataColheitaPrevista = s.DataColheitaPrevista,
                    DataColheitaReal = s.DataColheitaReal,
                    ProducaoEstimada = s.ProducaoEstimada,
                    ProducaoObtida = s.ProducaoObtida,
                    Status = s.Status,
                    Observacao = s.Observacao
                })
                .ToList();

            return AppResponse<List<SafraResponse>>.Ok(
                "Safras disponíveis para colheita listadas com sucesso.",
                response
            );
        }
        catch (Exception ex)
        {
            return AppResponse<List<SafraResponse>>.Fail(
                "Erro ao listar as safras disponíveis para colheita.",
                ex
            );
        }
    }
}