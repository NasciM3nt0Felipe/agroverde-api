using AgroVerde.Application.Common;
using AgroVerde.Application.DTOs;
using AgroVerde.Domain.Entities;
using AgroVerde.Domain.Repositories;

namespace AgroVerde.Application.Services;

public class EstoqueService : IEstoqueService
{
    private readonly IEstoqueRepository _estoqueRepository;

    public EstoqueService(IEstoqueRepository estoqueRepository)
    {
        _estoqueRepository = estoqueRepository;
    }

    public async Task<AppResponse<EstoqueItemResponse>> CriarAsync(
        EstoqueItemRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Nome))
                return AppResponse<EstoqueItemResponse>.Fail(
                    "Informe o nome do item.");

            if (string.IsNullOrWhiteSpace(request.Categoria))
                return AppResponse<EstoqueItemResponse>.Fail(
                    "Informe a categoria.");

            if (request.QuantidadeInicial < 0)
                return AppResponse<EstoqueItemResponse>.Fail(
                    "A quantidade inicial não pode ser negativa.");

            if (request.QuantidadeAtual < 0)
                return AppResponse<EstoqueItemResponse>.Fail(
                    "A quantidade atual não pode ser negativa.");

            if (request.PrecoMedioUnitario < 0)
                return AppResponse<EstoqueItemResponse>.Fail(
                    "O preço médio unitário não pode ser negativo.");

            if (request.EstoqueMinimo < 0)
                return AppResponse<EstoqueItemResponse>.Fail(
                    "O estoque mínimo não pode ser negativo.");

            var item = new EstoqueItem
            {
                PropriedadeId = request.PropriedadeId,
                Nome = request.Nome.Trim(),
                Categoria = request.Categoria.Trim(),
                QuantidadeInicial = request.QuantidadeInicial,
                QuantidadeAtual = request.QuantidadeAtual,
                UnidadeMedida = request.UnidadeMedida.Trim(),
                PrecoMedioUnitario = request.PrecoMedioUnitario,
                EstoqueMinimo = request.EstoqueMinimo,
                Fornecedor = request.Fornecedor?.Trim(),
                Observacao = request.Observacao?.Trim()
            };

            var itemCriado = await _estoqueRepository.InserirAsync(item);

            return AppResponse<EstoqueItemResponse>.Ok(
                "Item de estoque cadastrado com sucesso.",
                MapearParaResponse(itemCriado));
        }
        catch (Exception ex)
        {
            return AppResponse<EstoqueItemResponse>.Fail(
                "Erro ao cadastrar o item de estoque.",
                ex);
        }
    }

    public async Task<AppResponse<EstoqueItemResponse>> BuscarPorIdAsync(int id)
    {
        try
        {
            var item = await _estoqueRepository.BuscarPorIdAsync(id);

            if (item == null)
                return AppResponse<EstoqueItemResponse>.Fail(
                    "Item de estoque não encontrado.");

            return AppResponse<EstoqueItemResponse>.Ok(
                "Item de estoque encontrado com sucesso.",
                MapearParaResponse(item));
        }
        catch (Exception ex)
        {
            return AppResponse<EstoqueItemResponse>.Fail(
                "Erro ao buscar o item de estoque.",
                ex);
        }
    }

    public async Task<AppResponse<List<EstoqueItemResponse>>> ListarPorPropriedadeIdAsync(
        int propriedadeId)
    {
        try
        {
            var itens = await _estoqueRepository
                .ListarPorPropriedadeIdAsync(propriedadeId);

            var response = itens
                .Select(MapearParaResponse)
                .ToList();

            return AppResponse<List<EstoqueItemResponse>>.Ok(
                "Itens de estoque listados com sucesso.",
                response);
        }
        catch (Exception ex)
        {
            return AppResponse<List<EstoqueItemResponse>>.Fail(
                "Erro ao listar os itens de estoque.",
                ex);
        }
    }

    public async Task<AppResponse<List<EstoqueItemResponse>>> ListarVacinasDisponiveisAsync(
        int propriedadeId)
    {
        try
        {
            var itens = await _estoqueRepository
                .ListarVacinasDisponiveisPorPropriedadeAsync(propriedadeId);

            var response = itens
                .Select(MapearParaResponse)
                .ToList();

            return AppResponse<List<EstoqueItemResponse>>.Ok(
                "Vacinas disponíveis listadas com sucesso.",
                response);
        }
        catch (Exception ex)
        {
            return AppResponse<List<EstoqueItemResponse>>.Fail(
                "Erro ao listar as vacinas disponíveis.",
                ex);
        }
    }

    public async Task<AppResponse<EstoqueItemResponse>> AtualizarAsync(
        int id,
        EstoqueItemRequest request)
    {
        try
        {
            var item = await _estoqueRepository.BuscarPorIdAsync(id);

            if (item == null)
                return AppResponse<EstoqueItemResponse>.Fail(
                    "Item de estoque não encontrado.");

            if (string.IsNullOrWhiteSpace(request.Nome))
                return AppResponse<EstoqueItemResponse>.Fail(
                    "Informe o nome do item.");

            if (string.IsNullOrWhiteSpace(request.Categoria))
                return AppResponse<EstoqueItemResponse>.Fail(
                    "Informe a categoria.");

            if (request.QuantidadeInicial < 0)
                return AppResponse<EstoqueItemResponse>.Fail(
                    "A quantidade inicial não pode ser negativa.");

            if (request.QuantidadeAtual < 0)
                return AppResponse<EstoqueItemResponse>.Fail(
                    "A quantidade atual não pode ser negativa.");

            if (request.PrecoMedioUnitario < 0)
                return AppResponse<EstoqueItemResponse>.Fail(
                    "O preço médio unitário não pode ser negativo.");

            if (request.EstoqueMinimo < 0)
                return AppResponse<EstoqueItemResponse>.Fail(
                    "O estoque mínimo não pode ser negativo.");

            item.PropriedadeId = request.PropriedadeId;
            item.Nome = request.Nome.Trim();
            item.Categoria = request.Categoria.Trim();
            item.QuantidadeInicial = request.QuantidadeInicial;
            item.QuantidadeAtual = request.QuantidadeAtual;
            item.UnidadeMedida = request.UnidadeMedida.Trim();
            item.PrecoMedioUnitario = request.PrecoMedioUnitario;
            item.EstoqueMinimo = request.EstoqueMinimo;
            item.Fornecedor = request.Fornecedor?.Trim();
            item.Observacao = request.Observacao?.Trim();

            await _estoqueRepository.AtualizarAsync(item);

            return AppResponse<EstoqueItemResponse>.Ok(
                "Item de estoque atualizado com sucesso.",
                MapearParaResponse(item));
        }
        catch (Exception ex)
        {
            return AppResponse<EstoqueItemResponse>.Fail(
                "Erro ao atualizar o item de estoque.",
                ex);
        }
    }

    public async Task<AppResponse<EstoqueItemResponse>> ExcluirAsync(int id)
    {
        try
        {
            var item = await _estoqueRepository.BuscarPorIdAsync(id);

            if (item == null)
                return AppResponse<EstoqueItemResponse>.Fail(
                    "Item de estoque não encontrado.");

            await _estoqueRepository.ExcluirAsync(id);

            return AppResponse<EstoqueItemResponse>.Ok(
                "Item de estoque excluído com sucesso.",
                MapearParaResponse(item));
        }
        catch (Exception ex)
        {
            return AppResponse<EstoqueItemResponse>.Fail(
                "Erro ao excluir o item de estoque.",
                ex);
        }
    }

    public async Task<AppResponse<EstoqueItemResponse>> ConsumirEstoqueAsync(
        int estoqueItemId,
        double quantidade)
    {
        try
        {
            if (quantidade <= 0)
                return AppResponse<EstoqueItemResponse>.Fail(
                    "A quantidade consumida deve ser maior que zero.");

            var item = await _estoqueRepository.BuscarPorIdAsync(estoqueItemId);

            if (item == null)
                return AppResponse<EstoqueItemResponse>.Fail(
                    "Item de estoque não encontrado.");

            if (item.QuantidadeAtual < quantidade)
                return AppResponse<EstoqueItemResponse>.Fail(
                    $"Estoque insuficiente para {item.Nome}. " +
                    $"Disponível: {item.QuantidadeAtual} {item.UnidadeMedida}.");

            item.QuantidadeAtual -= quantidade;

            await _estoqueRepository.AtualizarAsync(item);

            return AppResponse<EstoqueItemResponse>.Ok(
                "Estoque atualizado com sucesso.",
                MapearParaResponse(item));
        }
        catch (Exception ex)
        {
            return AppResponse<EstoqueItemResponse>.Fail(
                "Erro ao consumir o item de estoque.",
                ex);
        }
    }

    public async Task<AppResponse<EstoqueItemResponse>> RegistrarConsumoSafraAsync(
        ConsumoEstoqueRequest request)
    {
        try
        {
            if (request.Quantidade <= 0)
                return AppResponse<EstoqueItemResponse>.Fail(
                    "A quantidade consumida deve ser maior que zero.");

            var item = await _estoqueRepository
                .BuscarPorIdAsync(request.EstoqueItemId);

            if (item == null)
                return AppResponse<EstoqueItemResponse>.Fail(
                    "Item de estoque não encontrado.");

            if (item.QuantidadeAtual < request.Quantidade)
                return AppResponse<EstoqueItemResponse>.Fail(
                    $"Estoque insuficiente para {item.Nome}. " +
                    $"Disponível: {item.QuantidadeAtual} {item.UnidadeMedida}.");

            item.QuantidadeAtual -= request.Quantidade;

            var consumo = new EstoqueInsumo
            {
                SafraId = request.SafraId,
                EstoqueItemId = item.Id,
                QuantidadeUtilizada = request.Quantidade,
                ValorTotal = request.Quantidade * item.PrecoMedioUnitario,
                DataMovimentacao = DateTime.Now.ToString("o"),
                Observacao = request.Observacao?.Trim()
            };

            await _estoqueRepository.RegistrarConsumoSafraAsync(
                item,
                consumo);

            return AppResponse<EstoqueItemResponse>.Ok(
                "Consumo da safra registrado com sucesso.",
                MapearParaResponse(item));
        }
        catch (Exception ex)
        {
            return AppResponse<EstoqueItemResponse>.Fail(
                "Erro ao registrar o consumo da safra.",
                ex);
        }
    }

    private static EstoqueItemResponse MapearParaResponse(EstoqueItem item)
    {
        return new EstoqueItemResponse
        {
            Id = item.Id,
            PropriedadeId = item.PropriedadeId,
            Nome = item.Nome,
            Categoria = item.Categoria,
            QuantidadeInicial = item.QuantidadeInicial,
            QuantidadeAtual = item.QuantidadeAtual,
            UnidadeMedida = item.UnidadeMedida,
            PrecoMedioUnitario = item.PrecoMedioUnitario,
            EstoqueMinimo = item.EstoqueMinimo,
            Fornecedor = item.Fornecedor,
            Observacao = item.Observacao,
            ValorTotal = item.ValorTotal,
            EstoqueZerado = item.EstoqueZerado,
            EstoqueBaixo = item.EstoqueBaixo
        };
    }
}