using AgroVerde.Domain.Entities;

namespace AgroVerde.Application.DTOs;

public record AnimalRequest(
    string Brinco,
    string? Nome,
    string Raca,
    SexoAnimal Sexo,
    DateTime DataNascimento,
    StatusAnimal Status
);

public record AnimalResponse(
    int Id,
    string Brinco,
    string? Nome,
    string Raca,
    string Sexo,
    DateTime DataNascimento,
    string Status
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
    string NomeVacina,
    string? Lote,
    string? Dose,
    DateTime DataAplicacao,
    DateTime? ProximaDose
);

public record VacinacaoResponse(
    int Id,
    int AnimalId,
    string NomeVacina,
    string? Lote,
    string? Dose,
    DateTime DataAplicacao,
    DateTime? ProximaDose
);

public record ControleSanitarioRequest(
    int AnimalId,
    string DiagnosticoTratamento,
    string Medicamento,
    DateTime DataInicio,
    DateTime? DataFim,
    string? Observacoes
);

public record ControleSanitarioResponse(
    int Id,
    int AnimalId,
    string DiagnosticoTratamento,
    string Medicamento,
    DateTime DataInicio,
    DateTime? DataFim,
    string? Observacoes
);

public record RegistroReproducaoRequest(
    int AnimalId,
    TipoEventoReproducao TipoEvento,
    DateTime DataEvento,
    string? IdentificacaoMacho,
    DateTime? PrevisaoParto,
    string? Observacoes
);

public record RegistroReproducaoResponse(
    int Id,
    int AnimalId,
    string TipoEvento,
    DateTime DataEvento,
    string? IdentificacaoMacho,
    DateTime? PrevisaoParto,
    string? Observacoes
);

public record AnimalDetalhesResponse(
    int Id,
    string Brinco,
    string? Nome,
    string Raca,
    string Sexo,
    DateTime DataNascimento,
    string Status,
    IEnumerable<PesagemResponse> Pesagens,
    IEnumerable<VacinacaoResponse> Vacinacoes,
    IEnumerable<ControleSanitarioResponse> ControlesSanitarios,
    IEnumerable<RegistroReproducaoResponse> Reproducoes
);