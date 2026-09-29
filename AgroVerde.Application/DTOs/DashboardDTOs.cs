namespace AgroVerde.Application.DTOs;

public record DashboardResponse(
    int TotalTalhoes,
    int TotalSafras,
    int TotalAnimais,
    decimal Receitas,
    decimal Despesas,
    decimal Saldo
);