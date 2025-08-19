using System.Text.Json.Serialization;
using hackathon.Application.Dto;
using hackathon.Domain.Entities;

namespace hackathon.Api.Serialization;

[JsonSerializable(typeof(decimal))]
[JsonSerializable(typeof(DateTime))]
[JsonSerializable(typeof(Simulacao))]
[JsonSerializable(typeof(SimulacaoRequest))]
[JsonSerializable(typeof(SimulacaoResponse))]
[JsonSerializable(typeof(ResultadoSimulacao))]
internal partial class AppJsonSerializerContext : JsonSerializerContext
{
}