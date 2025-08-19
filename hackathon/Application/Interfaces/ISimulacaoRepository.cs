using hackathon.Application.Dtos;
using hackathon.Domain.Entities;

namespace hackathon.Application.Interfaces;

public interface ISimulacaoRepository
{
    Task SalvarAsync(Simulacao simulacao);
    Task<PaginacaoResultado<SimulacaoResumo>> ObterPaginadoAsync(int pagina, int qtdPorPagina);
}