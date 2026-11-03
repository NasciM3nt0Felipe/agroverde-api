using System;
using System.Collections.Generic;
using System.Globalization;
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
            // Regra 1: Nome/Identificação obrigatória
            if (string.IsNullOrWhiteSpace(req.Nome))
                return AppResponse<VeiculoResponse>.Fail("Informe o nome/identificação do veículo.");

            if (req.PropriedadeId <= 0)
                return AppResponse<VeiculoResponse>.Fail("A propriedade do veículo é obrigatória.");

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
                var existente = await _veiculoRepository.ObterPorPlacaNaPropriedadeAsync(req.PropriedadeId, identificadorLimpo);
                if (existente != null)
                    return AppResponse<VeiculoResponse>.Fail("Já existe um veículo cadastrado com esta placa ou chassi.");
            }

            var veiculo = new Veiculo
            {
                PropriedadeId = req.PropriedadeId,
                Marca = req.Marca?.Trim(),
                ValorVenda = req.ValorVenda,
                DataVenda = req.DataVenda,
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

            if (string.IsNullOrWhiteSpace(req.Nome))
                return AppResponse<VeiculoResponse>.Fail("Informe o nome/identificação do veículo.");

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
                var existente = await _veiculoRepository.ObterPorPlacaNaPropriedadeAsync(veiculo.PropriedadeId, identificadorLimpo);
                if (existente != null && existente.Id != id)
                    return AppResponse<VeiculoResponse>.Fail("Esta placa ou chassi já está em uso por outro veículo.");
            }

            veiculo.Nome = req.Nome.Trim();
            veiculo.Marca = req.Marca?.Trim();
            veiculo.ValorVenda = req.ValorVenda;
            veiculo.DataVenda = req.DataVenda;
            veiculo.PlacaOuChassi = identificadorLimpo;
            veiculo.Tipo = req.Tipo;
            veiculo.Modelo = req.Modelo?.Trim();
            veiculo.AnoFabricacao = req.AnoFabricacao;
            veiculo.HorimetroAtual = req.HorimetroAtual;
            veiculo.QuilometragemAtual = req.QuilometragemAtual;
            veiculo.Status = req.Status;
            veiculo.DataAquisicao = req.DataAquisicao == default ? veiculo.DataAquisicao : req.DataAquisicao;
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

            if (await _veiculoRepository.PossuiRegistrosAsync(id))
                return AppResponse<bool>.Fail(
                    $"Não é possível excluir o veículo \"{veiculo.Nome}\" porque ele possui abastecimentos ou manutenções registrados. Exclua esses registros antes.");

            _veiculoRepository.Delete(veiculo);
            await _veiculoRepository.SaveChangesAsync();

            return AppResponse<bool>.Ok("Veículo removido com sucesso.", true);
        }

        // ---------- Por propriedade ----------

        public async Task<AppResponse<List<VeiculoResponse>>> ObterPorPropriedadeAsync(int propriedadeId)
        {
            var lista = await _veiculoRepository.ListarPorPropriedadeAsync(propriedadeId);
            var totais = await _veiculoRepository.ObterTotaisAsync(lista.Select(v => v.Id));

            var dtos = lista.Select(v =>
            {
                var dto = MapearParaResponse(v);
                if (totais.TryGetValue(v.Id, out var t))
                {
                    dto.TotalAbastecimentos = t.Abastecimento;
                    dto.TotalLitros = t.Litros;
                    dto.TotalManutencoes = t.Manutencao;
                    dto.QuantidadeManutencoes = t.QtdManutencoes;
                }
                return dto;
            }).ToList();

            return AppResponse<List<VeiculoResponse>>.Ok("Veículos recuperados com sucesso.", dtos);
        }

        public async Task<int?> ObterPropriedadeDoVeiculoAsync(int veiculoId) =>
            (await _veiculoRepository.GetByIdAsync(veiculoId))?.PropriedadeId;

        public async Task<int?> ObterPropriedadeDoAbastecimentoAsync(int id) =>
            (await _veiculoRepository.ObterAbastecimentoAsync(id))?.Veiculo.PropriedadeId;

        public async Task<int?> ObterPropriedadeDaManutencaoAsync(int id) =>
            (await _veiculoRepository.ObterManutencaoAsync(id))?.Veiculo.PropriedadeId;

        // ---------- Datas ----------

        private static readonly string[] FormatosData =
        [
            "dd/MM/yyyy",
            "d/M/yyyy",
            "yyyy-MM-dd",
            "yyyy-MM-ddTHH:mm:ss",
            "yyyy-MM-ddTHH:mm:ss.FFFFFFF"
        ];

        private static bool TentarLerData(string? texto, out DateTime data) =>
            DateTime.TryParseExact(
                texto?.Trim(),
                FormatosData,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out data);

        // ---------- Abastecimentos ----------

        private static AbastecimentoResponse MapearAbastecimento(AbastecimentoVeiculo a, int? lancamentoId = null) => new()
        {
            Id = a.Id,
            VeiculoId = a.VeiculoId,
            Data = a.Data.ToString("yyyy-MM-dd"),
            Litros = a.Litros,
            ValorTotal = a.ValorTotal,
            HorimetroOdometro = a.HorimetroOdometro,
            Observacao = a.Observacao,
            LancamentoId = lancamentoId
        };

        private static string? ValidarAbastecimento(AbastecimentoRequest req, out DateTime data)
        {
            if (!TentarLerData(req.Data, out data))
                return "Informe uma data válida para o abastecimento.";

            if (req.Litros <= 0 || req.ValorTotal <= 0)
                return "Informe litros e valor total.";

            if (req.HorimetroOdometro.HasValue && req.HorimetroOdometro < 0)
                return "O horímetro/odômetro não pode ser negativo.";

            return null;
        }

        public async Task<AppResponse<List<AbastecimentoResponse>>> ListarAbastecimentosAsync(int veiculoId)
        {
            var lista = await _veiculoRepository.ListarAbastecimentosAsync(veiculoId);

            return AppResponse<List<AbastecimentoResponse>>.Ok(
                "Abastecimentos recuperados com sucesso.",
                lista.Select(a => MapearAbastecimento(a)).ToList());
        }

        public async Task<AppResponse<AbastecimentoResponse>> CriarAbastecimentoAsync(AbastecimentoRequest req)
        {
            var veiculo = await _veiculoRepository.GetByIdAsync(req.VeiculoId);
            if (veiculo == null)
                return AppResponse<AbastecimentoResponse>.Fail("Veículo não encontrado.");

            var erro = ValidarAbastecimento(req, out var data);
            if (erro != null)
                return AppResponse<AbastecimentoResponse>.Fail(erro, [erro]);

            var item = new AbastecimentoVeiculo
            {
                VeiculoId = veiculo.Id,
                Data = data,
                Litros = req.Litros,
                ValorTotal = Math.Round(req.ValorTotal, 2, MidpointRounding.AwayFromZero),
                HorimetroOdometro = req.HorimetroOdometro,
                Observacao = req.Observacao?.Trim()
            };

            var despesa = NovaDespesa(veiculo.PropriedadeId, $"Abastecimento - {veiculo.Nome}", item.ValorTotal, data);
            var lancamentoId = await _veiculoRepository.InserirAbastecimentoAsync(item, despesa);

            return AppResponse<AbastecimentoResponse>.Ok(
                "Abastecimento registrado com sucesso.",
                MapearAbastecimento(item, lancamentoId));
        }

        public async Task<AppResponse<AbastecimentoResponse>> AtualizarAbastecimentoAsync(int id, AbastecimentoRequest req)
        {
            var item = await _veiculoRepository.ObterAbastecimentoAsync(id);
            if (item == null)
                return AppResponse<AbastecimentoResponse>.Fail("Abastecimento não encontrado.");

            var erro = ValidarAbastecimento(req, out var data);
            if (erro != null)
                return AppResponse<AbastecimentoResponse>.Fail(erro, [erro]);

            item.Data = data;
            item.Litros = req.Litros;
            item.ValorTotal = Math.Round(req.ValorTotal, 2, MidpointRounding.AwayFromZero);
            item.HorimetroOdometro = req.HorimetroOdometro;
            item.Observacao = req.Observacao?.Trim();

            var lancamentoId = await _veiculoRepository.AtualizarAbastecimentoAsync(
                item, item.Veiculo.PropriedadeId, $"Abastecimento - {item.Veiculo.Nome}");

            return AppResponse<AbastecimentoResponse>.Ok(
                "Abastecimento atualizado com sucesso.",
                MapearAbastecimento(item, lancamentoId));
        }

        public async Task<AppResponse<bool>> ExcluirAbastecimentoAsync(int id)
        {
            var item = await _veiculoRepository.ObterAbastecimentoAsync(id);
            if (item == null)
                return AppResponse<bool>.Fail("Abastecimento não encontrado.");

            await _veiculoRepository.ExcluirAbastecimentoAsync(item);

            return AppResponse<bool>.Ok("Abastecimento excluído com sucesso.", true);
        }

        // ---------- Manutenções ----------

        private static ManutencaoResponse MapearManutencao(ManutencaoVeiculo m, int? lancamentoId = null) => new()
        {
            Id = m.Id,
            VeiculoId = m.VeiculoId,
            Data = m.Data.ToString("yyyy-MM-dd"),
            Tipo = m.Tipo,
            Descricao = m.Descricao,
            Valor = m.Valor,
            HorimetroOdometro = m.HorimetroOdometro,
            Observacao = m.Observacao,
            LancamentoId = lancamentoId
        };

        private static string? ValidarManutencao(ManutencaoRequest req, out DateTime data)
        {
            if (!TentarLerData(req.Data, out data))
                return "Informe uma data válida para a manutenção.";

            if (string.IsNullOrWhiteSpace(req.Tipo) || req.Valor <= 0)
                return "Informe o tipo e o valor da manutenção.";

            if (req.HorimetroOdometro.HasValue && req.HorimetroOdometro < 0)
                return "O horímetro/odômetro não pode ser negativo.";

            return null;
        }

        public async Task<AppResponse<List<ManutencaoResponse>>> ListarManutencoesAsync(int veiculoId)
        {
            var lista = await _veiculoRepository.ListarManutencoesAsync(veiculoId);

            return AppResponse<List<ManutencaoResponse>>.Ok(
                "Manutenções recuperadas com sucesso.",
                lista.Select(m => MapearManutencao(m)).ToList());
        }

        public async Task<AppResponse<ManutencaoResponse>> CriarManutencaoAsync(ManutencaoRequest req)
        {
            var veiculo = await _veiculoRepository.GetByIdAsync(req.VeiculoId);
            if (veiculo == null)
                return AppResponse<ManutencaoResponse>.Fail("Veículo não encontrado.");

            var erro = ValidarManutencao(req, out var data);
            if (erro != null)
                return AppResponse<ManutencaoResponse>.Fail(erro, [erro]);

            var item = new ManutencaoVeiculo
            {
                VeiculoId = veiculo.Id,
                Data = data,
                Tipo = req.Tipo.Trim(),
                Descricao = req.Descricao?.Trim(),
                Valor = Math.Round(req.Valor, 2, MidpointRounding.AwayFromZero),
                HorimetroOdometro = req.HorimetroOdometro,
                Observacao = req.Observacao?.Trim()
            };

            var despesa = NovaDespesa(veiculo.PropriedadeId, $"Manutenção - {veiculo.Nome}", item.Valor, data);
            var lancamentoId = await _veiculoRepository.InserirManutencaoAsync(item, despesa);

            return AppResponse<ManutencaoResponse>.Ok(
                "Manutenção registrada com sucesso.",
                MapearManutencao(item, lancamentoId));
        }

        public async Task<AppResponse<ManutencaoResponse>> AtualizarManutencaoAsync(int id, ManutencaoRequest req)
        {
            var item = await _veiculoRepository.ObterManutencaoAsync(id);
            if (item == null)
                return AppResponse<ManutencaoResponse>.Fail("Manutenção não encontrada.");

            var erro = ValidarManutencao(req, out var data);
            if (erro != null)
                return AppResponse<ManutencaoResponse>.Fail(erro, [erro]);

            item.Data = data;
            item.Tipo = req.Tipo.Trim();
            item.Descricao = req.Descricao?.Trim();
            item.Valor = Math.Round(req.Valor, 2, MidpointRounding.AwayFromZero);
            item.HorimetroOdometro = req.HorimetroOdometro;
            item.Observacao = req.Observacao?.Trim();

            var lancamentoId = await _veiculoRepository.AtualizarManutencaoAsync(
                item, item.Veiculo.PropriedadeId, $"Manutenção - {item.Veiculo.Nome}");

            return AppResponse<ManutencaoResponse>.Ok(
                "Manutenção atualizada com sucesso.",
                MapearManutencao(item, lancamentoId));
        }

        public async Task<AppResponse<bool>> ExcluirManutencaoAsync(int id)
        {
            var item = await _veiculoRepository.ObterManutencaoAsync(id);
            if (item == null)
                return AppResponse<bool>.Fail("Manutenção não encontrada.");

            await _veiculoRepository.ExcluirManutencaoAsync(item);

            return AppResponse<bool>.Ok("Manutenção excluída com sucesso.", true);
        }

        // Despesa paga na data do registro, gerada pela API.
        private static TransacaoFinanceira NovaDespesa(
            int propriedadeId, string descricao, decimal valor, DateTime data) => new()
        {
            PropriedadeId = propriedadeId,
            Descricao = descricao.Length > 200 ? descricao.Substring(0, 200) : descricao,
            Valor = valor,
            Tipo = TipoTransacao.Despesa,
            Categoria = "Veículos",
            Data = data,
            DataVencimento = data,
            DataPagamento = data,
            Status = StatusTransacao.Pago
        };

        private static VeiculoResponse MapearParaResponse(Veiculo v) => new()
        {
            Id = v.Id,
            PropriedadeId = v.PropriedadeId,
            Marca = v.Marca,
            ValorVenda = v.ValorVenda,
            DataVenda = v.DataVenda,
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