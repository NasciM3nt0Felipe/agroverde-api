using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AgroVerde.Application.Common;
using AgroVerde.Application.DTOs;
using AgroVerde.Domain.Entities;
using AgroVerde.Domain.Repositories;

namespace AgroVerde.Application.Services
{
    public class VeiculoService : IVeiculoService
    {
        private readonly IVeiculoRepository _veiculoRepository;

        public VeiculoService(IVeiculoRepository veiculoRepository)
        {
            _veiculoRepository = veiculoRepository;
        }

        public async Task<AppResponse<IEnumerable<VeiculoResponse>>> ObterTodosAsync()
        {
            var lista = await _veiculoRepository.GetAllAsync();
            var dtos = lista.Select(MapearParaResponse);
            return AppResponse<IEnumerable<VeiculoResponse>>.Ok("Veículos recuperados com sucesso.", dtos);
        }

        public async Task<AppResponse<VeiculoResponse>> ObterPorIdAsync(int id)
        {
            if (id <= 0)
                return AppResponse<VeiculoResponse>.Fail("Identificador inválido.");

            var veiculo = await _veiculoRepository.GetByIdAsync(id);
            if (veiculo == null)
                return AppResponse<VeiculoResponse>.Fail("Veículo não encontrado.");

            return AppResponse<VeiculoResponse>.Ok("Veículo recuperado com sucesso.", MapearParaResponse(veiculo));
        }

        public async Task<AppResponse<IEnumerable<VeiculoResponse>>> ObterPorStatusAsync(StatusVeiculo status)
        {
            var lista = await _veiculoRepository.ObterPorStatusAsync(status);
            var dtos = lista.Select(MapearParaResponse);
            return AppResponse<IEnumerable<VeiculoResponse>>.Ok("Veículos filtrados com sucesso.", dtos);
        }

        public async Task<AppResponse<VeiculoResponse>> CriarAsync(VeiculoRequest req)
        {
            // Regra 1: Nome/Identificação obrigatória com tamanho mínimo
            if (string.IsNullOrWhiteSpace(req.Nome) || req.Nome.Trim().Length < 3)
                return AppResponse<VeiculoResponse>.Fail("O nome/identificação do veículo deve conter pelo menos 3 caracteres.");

            // Regra 2: Ano de fabricação consistente
            int anoLimite = DateTime.UtcNow.Year + 1;
            if (req.AnoFabricacao.HasValue && (req.AnoFabricacao < 1950 || req.AnoFabricacao > anoLimite))
                return AppResponse<VeiculoResponse>.Fail($"Ano de fabricação inválido. Deve estar entre 1950 e {anoLimite}.");

            // Regra 3: Data de aquisição não pode ser futura
            if (req.DataAquisicao > DateTime.UtcNow.AddDays(1))
                return AppResponse<VeiculoResponse>.Fail("A data de aquisição não pode ser uma data futura.");

            // Regra 4: Valores iniciais de medidores não negativos
            if (req.HorimetroAtual < 0 || req.QuilometragemAtual < 0)
                return AppResponse<VeiculoResponse>.Fail("O horímetro e a quilometragem inicial não podem ser negativos.");

            // Regra 5: Normalização e Unicidade de Placa ou Chassi
            string? identificadorLimpo = null;
            if (!string.IsNullOrWhiteSpace(req.PlacaOuChassi))
            {
                identificadorLimpo = Regex.Replace(req.PlacaOuChassi.Trim().ToUpper(), @"[^A-Z0-9]", "");
                var existente = await _veiculoRepository.ObterPorPlacaOuChassiAsync(identificadorLimpo);
                if (existente != null)
                    return AppResponse<VeiculoResponse>.Fail("Já existe um veículo cadastrado com esta placa ou chassi.");
            }

            var veiculo = new Veiculo
            {
                Nome = req.Nome.Trim(),
                PlacaOuChassi = identificadorLimpo,
                Tipo = req.Tipo,
                Modelo = req.Modelo?.Trim(),
                AnoFabricacao = req.AnoFabricacao,
                HorimetroAtual = req.HorimetroAtual,
                QuilometragemAtual = req.QuilometragemAtual,
                Status = req.Status == 0 ? StatusVeiculo.Ativo : req.Status,
                DataAquisicao = req.DataAquisicao == default ? DateTime.UtcNow : req.DataAquisicao,
                Observacoes = req.Observacoes?.Trim()
            };

            await _veiculoRepository.AddAsync(veiculo);
            await _veiculoRepository.SaveChangesAsync();

            return AppResponse<VeiculoResponse>.Ok("Veículo cadastrado com sucesso.", MapearParaResponse(veiculo));
        }

        public async Task<AppResponse<VeiculoResponse>> AtualizarAsync(int id, VeiculoRequest req)
        {
            if (id <= 0)
                return AppResponse<VeiculoResponse>.Fail("Identificador inválido.");

            var veiculo = await _veiculoRepository.GetByIdAsync(id);
            if (veiculo == null)
                return AppResponse<VeiculoResponse>.Fail("Veículo não encontrado.");

            if (string.IsNullOrWhiteSpace(req.Nome) || req.Nome.Trim().Length < 3)
                return AppResponse<VeiculoResponse>.Fail("O nome/identificação do veículo deve conter pelo menos 3 caracteres.");

            int anoLimite = DateTime.UtcNow.Year + 1;
            if (req.AnoFabricacao.HasValue && (req.AnoFabricacao < 1950 || req.AnoFabricacao > anoLimite))
                return AppResponse<VeiculoResponse>.Fail($"Ano de fabricação inválido. Deve estar entre 1950 e {anoLimite}.");

            if (req.DataAquisicao > DateTime.UtcNow.AddDays(1))
                return AppResponse<VeiculoResponse>.Fail("A data de aquisição não pode ser uma data futura.");

            // Regra: Bloqueio de redução de medidores
            if (req.HorimetroAtual < veiculo.HorimetroAtual)
                return AppResponse<VeiculoResponse>.Fail($"O horímetro ({req.HorimetroAtual}h) não pode ser reduzido. Valor atual: {veiculo.HorimetroAtual}h.");

            if (req.QuilometragemAtual < veiculo.QuilometragemAtual)
                return AppResponse<VeiculoResponse>.Fail($"A quilometragem ({req.QuilometragemAtual}km) não pode ser reduzida. Valor atual: {veiculo.QuilometragemAtual}km.");

            // Validação de placa única se modificada
            string? identificadorLimpo = null;
            if (!string.IsNullOrWhiteSpace(req.PlacaOuChassi))
            {
                identificadorLimpo = Regex.Replace(req.PlacaOuChassi.Trim().ToUpper(), @"[^A-Z0-9]", "");
                var existente = await _veiculoRepository.ObterPorPlacaOuChassiAsync(identificadorLimpo);
                if (existente != null && existente.Id != id)
                    return AppResponse<VeiculoResponse>.Fail("Esta placa ou chassi já está em uso por outro veículo.");
            }

            veiculo.Nome = req.Nome.Trim();
            veiculo.PlacaOuChassi = identificadorLimpo;
            veiculo.Tipo = req.Tipo;
            veiculo.Modelo = req.Modelo?.Trim();
            veiculo.AnoFabricacao = req.AnoFabricacao;
            veiculo.HorimetroAtual = req.HorimetroAtual;
            veiculo.QuilometragemAtual = req.QuilometragemAtual;
            veiculo.Status = req.Status;
            veiculo.DataAquisicao = req.DataAquisicao;
            veiculo.Observacoes = req.Observacoes?.Trim();

            _veiculoRepository.Update(veiculo);
            await _veiculoRepository.SaveChangesAsync();

            return AppResponse<VeiculoResponse>.Ok("Veículo atualizado com sucesso.", MapearParaResponse(veiculo));
        }

        public async Task<AppResponse<bool>> AtualizarHodometroAsync(int id, AtualizarHodometroRequest req)
        {
            if (id <= 0)
                return AppResponse<bool>.Fail("Identificador inválido.");

            var veiculo = await _veiculoRepository.GetByIdAsync(id);
            if (veiculo == null)
                return AppResponse<bool>.Fail("Veículo não encontrado.");

            if (req.NovoHorimetro <= 0 && req.NovaQuilometragem <= 0)
                return AppResponse<bool>.Fail("Informe ao menos um valor positivo para novo horímetro ou nova quilometragem.");

            if (req.NovoHorimetro > 0 && req.NovoHorimetro < veiculo.HorimetroAtual)
                return AppResponse<bool>.Fail($"O novo horímetro ({req.NovoHorimetro}h) não pode ser menor que o registro anterior ({veiculo.HorimetroAtual}h).");

            if (req.NovaQuilometragem > 0 && req.NovaQuilometragem < veiculo.QuilometragemAtual)
                return AppResponse<bool>.Fail($"A nova quilometragem ({req.NovaQuilometragem}km) não pode ser menor que o registro anterior ({veiculo.QuilometragemAtual}km).");

            if (req.NovoHorimetro > 0) veiculo.HorimetroAtual = req.NovoHorimetro;
            if (req.NovaQuilometragem > 0) veiculo.QuilometragemAtual = req.NovaQuilometragem;

            _veiculoRepository.Update(veiculo);
            await _veiculoRepository.SaveChangesAsync();

            return AppResponse<bool>.Ok("Horímetro e quilometragem atualizados com sucesso.", true);
        }

        public async Task<AppResponse<bool>> RemoverAsync(int id)
        {
            if (id <= 0)
                return AppResponse<bool>.Fail("Identificador inválido.");

            var veiculo = await _veiculoRepository.GetByIdAsync(id);
            if (veiculo == null)
                return AppResponse<bool>.Fail("Veículo não encontrado.");

            _veiculoRepository.Delete(veiculo);
            await _veiculoRepository.SaveChangesAsync();

            return AppResponse<bool>.Ok("Veículo removido com sucesso.", true);
        }

        private static VeiculoResponse MapearParaResponse(Veiculo v) => new()
        {
            Id = v.Id,
            Nome = v.Nome,
            PlacaOuChassi = v.PlacaOuChassi,
            Tipo = v.Tipo,
            TipoDescricao = v.Tipo.ToString(),
            Modelo = v.Modelo,
            AnoFabricacao = v.AnoFabricacao,
            HorimetroAtual = v.HorimetroAtual,
            QuilometragemAtual = v.QuilometragemAtual,
            Status = v.Status,
            StatusDescricao = v.Status.ToString(),
            DataAquisicao = v.DataAquisicao,
            Observacoes = v.Observacoes
        };
    }
}