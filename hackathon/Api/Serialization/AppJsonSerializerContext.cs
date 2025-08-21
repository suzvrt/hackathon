using System.Text.Json.Serialization;
using hackathon.Application.Dtos;
using hackathon.Domain.Entities;
using hackathon.Domain.ValueObjects;
using hackathon.Infrastructure.Persistence;

namespace hackathon.Api.Serialization;

[JsonSerializable(typeof(decimal))]
[JsonSerializable(typeof(DateTime))]
[JsonSerializable(typeof(Simulacao))]
[JsonSerializable(typeof(SimulacaoRequest))]
[JsonSerializable(typeof(SimulacaoResponse))]
[JsonSerializable(typeof(ListarRequest))]
[JsonSerializable(typeof(SimulacaoResumo))]
[JsonSerializable(typeof(PaginacaoResultado<SimulacaoResumo>))]
[JsonSerializable(typeof(ResultadoSimulacao))]
[JsonSerializable(typeof(Parcela))]
[JsonSerializable(typeof(List<Parcela>))]
[JsonSerializable(typeof(VolumeSimuladoDiario))]
[JsonSerializable(typeof(TelemetriaResponse))]
internal partial class AppJsonSerializerContext : JsonSerializerContext
{
}