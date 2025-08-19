using System.Text.Json.Serialization;
using hackathon.Application.Dtos;
using hackathon.Domain.Entities;
using hackathon.Domain.ValueObjects;

namespace hackathon.Api.Serialization;

[JsonSerializable(typeof(decimal))]
[JsonSerializable(typeof(DateTime))]
[JsonSerializable(typeof(Simulacao))]
[JsonSerializable(typeof(SimulacaoRequest))]
[JsonSerializable(typeof(SimulacaoResponse))]
[JsonSerializable(typeof(ResultadoSimulacao))]
[JsonSerializable(typeof(Parcela))]
[JsonSerializable(typeof(List<Parcela>))]
internal partial class AppJsonSerializerContext : JsonSerializerContext
{
}