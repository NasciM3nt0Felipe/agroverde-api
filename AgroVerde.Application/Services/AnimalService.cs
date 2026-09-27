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
        var animais = await _repository.ListarTodosAsync();
        return new AppResponse<IEnumerable<AnimalResponse>>(true, "Animais listados com sucesso.", animais.Select(MapToResponse), []);
    }

    public async Task<AppResponse<AnimalDetalhesResponse>> BuscarPorIdAsync(int id)
    {
        var animal = await _repository.BuscarPorIdComDetalhesAsync(id);
        if (animal is null)
            return new AppResponse<AnimalDetalhesResponse>(false, "Animal não encontrado.", null, ["Animal não encontrado."]);

        var response = new AnimalDetalhesResponse(
            animal.Id,
            animal.Brinco,
            animal.Nome,
            animal.Raca,
            animal.Sexo.ToString(),
            animal.DataNascimento,
            animal.Status.ToString(),
            animal.Pesagens.Select(p => new PesagemResponse(p.Id, p.AnimalId, p.PesoKg, p.DataPesagem, p.Observacao)),
            animal.Vacinacoes.Select(v => new VacinacaoResponse(v.Id, v.AnimalId, v.NomeVacina, v.Lote, v.Dose, v.DataAplicacao, v.ProximaDose)),
            animal.ControlesSanitarios.Select(c => new ControleSanitarioResponse(c.Id, c.AnimalId, c.DiagnosticoTratamento, c.Medicamento, c.DataInicio, c.DataFim, c.Observacoes)),
            animal.Reproducoes.Select(r => new RegistroReproducaoResponse(r.Id, r.AnimalId, r.TipoEvento.ToString(), r.DataEvento, r.IdentificacaoMacho, r.PrevisaoParto, r.Observacoes))
        );

        return new AppResponse<AnimalDetalhesResponse>(true, "Animal recuperado com sucesso.", response, []);
    }

    public async Task<AppResponse<AnimalResponse>> InserirAsync(AnimalRequest request)
    {
        var animal = new Animal
        {
            Brinco = request.Brinco,
            Nome = request.Nome,
            Raca = request.Raca,
            Sexo = request.Sexo,
            DataNascimento = request.DataNascimento,
            Status = request.Status
        };

        var criado = await _repository.InserirAsync(animal);
        return new AppResponse<AnimalResponse>(true, "Animal cadastrado com sucesso.", MapToResponse(criado), []);
    }

    public async Task<AppResponse<bool>> AtualizarAsync(int id, AnimalRequest request)
    {
        var animal = await _repository.BuscarPorIdAsync(id);
        if (animal is null)
            return new AppResponse<bool>(false, "Animal não encontrado.", false, ["Animal não encontrado."]);

        animal.Brinco = request.Brinco;
        animal.Nome = request.Nome;
        animal.Raca = request.Raca;
        animal.Sexo = request.Sexo;
        animal.DataNascimento = request.DataNascimento;
        animal.Status = request.Status;

        await _repository.AtualizarAsync(animal);
        return new AppResponse<bool>(true, "Animal atualizado com sucesso.", true, []);
    }

    public async Task<AppResponse<bool>> ExcluirAsync(int id)
    {
        var animal = await _repository.BuscarPorIdAsync(id);
        if (animal is null)
            return new AppResponse<bool>(false, "Animal não encontrado.", false, ["Animal não encontrado."]);

        await _repository.ExcluirAsync(id);
        return new AppResponse<bool>(true, "Animal excluído com sucesso.", true, []);
    }

    public async Task<AppResponse<PesagemResponse>> AdicionarPesagemAsync(PesagemRequest request)
    {
        var animal = await _repository.BuscarPorIdComDetalhesAsync(request.AnimalId);
        if (animal is null)
            return new AppResponse<PesagemResponse>(false, "Animal não encontrado.", null, ["Animal não encontrado."]);

        var pesagem = new Pesagem
        {
            AnimalId = request.AnimalId,
            PesoKg = request.PesoKg,
            DataPesagem = request.DataPesagem,
            Observacao = request.Observacao
        };

        animal.Pesagens.Add(pesagem);
        await _repository.AtualizarAsync(animal);

        var response = new PesagemResponse(pesagem.Id, pesagem.AnimalId, pesagem.PesoKg, pesagem.DataPesagem, pesagem.Observacao);
        return new AppResponse<PesagemResponse>(true, "Pesagem registrada com sucesso.", response, []);
    }

    public async Task<AppResponse<VacinacaoResponse>> AdicionarVacinacaoAsync(VacinacaoRequest request)
    {
        var animal = await _repository.BuscarPorIdComDetalhesAsync(request.AnimalId);
        if (animal is null)
            return new AppResponse<VacinacaoResponse>(false, "Animal não encontrado.", null, ["Animal não encontrado."]);

        var vacinacao = new Vacinacao
        {
            AnimalId = request.AnimalId,
            NomeVacina = request.NomeVacina,
            Lote = request.Lote,
            Dose = request.Dose,
            DataAplicacao = request.DataAplicacao,
            ProximaDose = request.ProximaDose
        };

        animal.Vacinacoes.Add(vacinacao);
        await _repository.AtualizarAsync(animal);

        var response = new VacinacaoResponse(vacinacao.Id, vacinacao.AnimalId, vacinacao.NomeVacina, vacinacao.Lote, vacinacao.Dose, vacinacao.DataAplicacao, vacinacao.ProximaDose);
        return new AppResponse<VacinacaoResponse>(true, "Vacinação registrada com sucesso.", response, []);
    }

    public async Task<AppResponse<ControleSanitarioResponse>> AdicionarControleSanitarioAsync(ControleSanitarioRequest request)
    {
        var animal = await _repository.BuscarPorIdComDetalhesAsync(request.AnimalId);
        if (animal is null)
            return new AppResponse<ControleSanitarioResponse>(false, "Animal não encontrado.", null, ["Animal não encontrado."]);

        var controle = new ControleSanitario
        {
            AnimalId = request.AnimalId,
            DiagnosticoTratamento = request.DiagnosticoTratamento,
            Medicamento = request.Medicamento,
            DataInicio = request.DataInicio,
            DataFim = request.DataFim,
            Observacoes = request.Observacoes
        };

        animal.ControlesSanitarios.Add(controle);
        await _repository.AtualizarAsync(animal);

        var response = new ControleSanitarioResponse(controle.Id, controle.AnimalId, controle.DiagnosticoTratamento, controle.Medicamento, controle.DataInicio, controle.DataFim, controle.Observacoes);
        return new AppResponse<ControleSanitarioResponse>(true, "Tratamento sanitário registrado com sucesso.", response, []);
    }

    public async Task<AppResponse<RegistroReproducaoResponse>> AdicionarRegistroReproducaoAsync(RegistroReproducaoRequest request)
    {
        var animal = await _repository.BuscarPorIdComDetalhesAsync(request.AnimalId);
        if (animal is null)
            return new AppResponse<RegistroReproducaoResponse>(false, "Animal não encontrado.", null, ["Animal não encontrado."]);

        var reproducao = new RegistroReproducao
        {
            AnimalId = request.AnimalId,
            TipoEvento = request.TipoEvento,
            DataEvento = request.DataEvento,
            IdentificacaoMacho = request.IdentificacaoMacho,
            PrevisaoParto = request.PrevisaoParto,
            Observacoes = request.Observacoes
        };

        animal.Reproducoes.Add(reproducao);
        await _repository.AtualizarAsync(animal);

        var response = new RegistroReproducaoResponse(reproducao.Id, reproducao.AnimalId, reproducao.TipoEvento.ToString(), reproducao.DataEvento, reproducao.IdentificacaoMacho, reproducao.PrevisaoParto, reproducao.Observacoes);
        return new AppResponse<RegistroReproducaoResponse>(true, "Registro reprodutivo adicionado com sucesso.", response, []);
    }

    private static AnimalResponse MapToResponse(Animal animal) =>
        new(animal.Id, animal.Brinco, animal.Nome, animal.Raca, animal.Sexo.ToString(), animal.DataNascimento, animal.Status.ToString());
}