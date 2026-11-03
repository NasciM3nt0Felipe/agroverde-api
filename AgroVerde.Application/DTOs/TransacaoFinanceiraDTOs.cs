
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
    int? SafraId = null,
    // Somente na inclusão: módulo do app que gerou o lançamento
    // (ver OrigemFinanceira). Ignorado na atualização.
    string? OrigemTipo = null
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
    int? SafraId,
    // null = lançamento manual.
    string? OrigemTipo = null,
    // Ações permitidas pela API, para a interface não oferecer as demais.
    bool PermiteEditar = true,
    bool PermiteExcluir = true
);

public record ResumoFinanceiroResponse(
    decimal TotalReceitas,
    decimal TotalDespesas,
    decimal Saldo
);
