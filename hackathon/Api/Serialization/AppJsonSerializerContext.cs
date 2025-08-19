using System.Text.Json.Serialization;
using hackathon.Application.Dto;

namespace hackathon.Api.Serialization;

[JsonSerializable(typeof(SimulacaoRequest))]
[JsonSerializable(typeof(SimulacaoResponse))]
internal partial class AppJsonSerializerContext : JsonSerializerContext
{
}