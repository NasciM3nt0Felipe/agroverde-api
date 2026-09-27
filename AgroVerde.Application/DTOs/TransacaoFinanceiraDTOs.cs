using AgroVerde.Domain.Entities;

namespace AgroVerde.Application.DTOs;

public record TransacaoFinanceiraRequest(
    string Descricao,
    decimal Valor,
    TipoTransacao Tipo,
    string Categoria,
    DateTime DataVencimento,
    DateTime? DataPagamento,
    StatusTransacao Status,
    int? AnimalId
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
    int? AnimalId
);

public record ResumoFinanceiroResponse(
    decimal TotalReceitas,
    decimal TotalDespesas,
    decimal Saldo
);