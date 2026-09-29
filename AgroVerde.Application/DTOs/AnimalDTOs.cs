namespace AgroVerde.Application.DTOs;

public record AnimalRequest(
    int PropriedadeId,
    string Identificacao,
    string Especie,
    string? Raca,
    string Sexo,
    string? DataNascimento,
    double? Peso,
    string Status,
    string? Observacao
);

public record AnimalResponse(
    int Id,
    int PropriedadeId,
    string Identificacao,
    string Especie,
    string? Raca,
    string Sexo,
    string? DataNascimento,
    double? Peso,
    string Status,
    string? Observacao
);

public record PesagemRequest(
    int AnimalId,
    decimal PesoKg,
    DateTime DataPesagem,
    string? Observacao
);

public record PesagemResponse(
    int Id,
    int AnimalId,
    decimal PesoKg,
    DateTime DataPesagem,
    string? Observacao
);

public record VacinacaoRequest(
    int AnimalId,
    string Vacina,
    string DataAplicacao,
    string? ProximaDose,
    string? Observacao
);

public record VacinacaoResponse(
    int Id,
    int AnimalId,
    string Vacina,
    string DataAplicacao,
    string? ProximaDose,
    string? Observacao
);

public record ControleSanitarioRequest(
    int AnimalId,
    string Procedimento,
    string Data,
    string? Medicamento,
    string? Observacao
);

public record ControleSanitarioResponse(
    int Id,
    int AnimalId,
    string Procedimento,
    string Data,
    string? Medicamento,
    string? Observacao
);

public record RegistroReproducaoRequest(
    int AnimalId,
    string Tipo,
    string Data,
    string? Observacao
);

public record RegistroReproducaoResponse(
    int Id,
    int AnimalId,
    string Tipo,
    string Data,
    string? Observacao
);

public record AnimalDetalhesResponse(
    int Id,
    int PropriedadeId,
    string Identificacao,
    string Especie,
    string? Raca,
    string Sexo,
    string? DataNascimento,
    double? Peso,
    string Status,
    string? Observacao,
    IEnumerable<PesagemResponse> Pesagens,
    IEnumerable<VacinacaoResponse> Vacinacoes,
    IEnumerable<ControleSanitarioResponse> ControlesSanitarios,
    IEnumerable<RegistroReproducaoResponse> Reproducoes
);