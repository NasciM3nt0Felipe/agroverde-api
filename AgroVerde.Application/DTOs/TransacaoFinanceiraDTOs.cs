
using AgroVerde.Domain.Entities;

namespace AgroVerde.Application.DTOs;

public record TransacaoFinanceiraRequest(
    string Descricao,
    decimal Valor,
    TipoTransacao Tipo,
    string? Categoria,
    DateTime? DataVencimento,
    DateTime? DataPagamento,
    StatusTransacao? Status,
    int? AnimalId,
    int PropriedadeId = 0,
    DateTime? Data = null,
    int? SafraId = null
);

public record TransacaoFinanceiraResponse(
    int Id,
    string Descricao,
    decimal Valor,
    string Tipo,
    string Categoria,
    DateTime DataVencimento,
    DateTime? DataPagamento,
    string Status,
    int? AnimalId,
    int PropriedadeId,
    DateTime Data,
    int? SafraId
);

public record ResumoFinanceiroResponse(
    decimal TotalReceitas,
    decimal TotalDespesas,
    decimal Saldo
);
