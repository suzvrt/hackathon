using hackathon.Application.Dto;
using hackathon.Application.Interfaces;
using hackathon.Domain.Entities;
using hackathon.Domain.ValueObjects;

namespace hackathon.Application.UseCases;

public class RetornarSimulacoesUseCase
{
    private readonly IProdutoRepository _produtoRepository;
    private readonly ISimulacaoPersistenceService _persistenceService;

    public RetornarSimulacoesUseCase(
        IProdutoRepository produtoRepository,
        ISimulacaoPersistenceService persistenceService)
    {
        _produtoRepository = produtoRepository;
        _persistenceService = persistenceService;
    }

    public async Task<SimulacaoResponse?> ExecutarAsync(SimulacaoRequest request)
    {
        var produtos = await _produtoRepository.ObterProdutosCompativeisAsync(request.ValorDesejado, request.Prazo);
        var produtoIdeal = produtos.OrderBy(p => p.TaxaJuros).FirstOrDefault();

        if (produtoIdeal is null) return null;

        var simulacao = new Simulacao
        {
            CodigoProduto = produtoIdeal.Codigo,
            DescricaoProduto = produtoIdeal.Nome,
            TaxaJuros = produtoIdeal.TaxaJuros / 1.0000000000000000000000000000m
        };

        // Fire and forget
        _ = _persistenceService.EnqueueAsync(simulacao);

        return new SimulacaoResponse(
            IdSimulacao: simulacao.Id.GetHashCode(),
            CodigoProduto: simulacao.CodigoProduto,
            DescricaoProduto: simulacao.DescricaoProduto,
            TaxaJuros: simulacao.TaxaJuros,
            Sac: simulacao.Sac,
            Price: simulacao.Price
        );
    }
}
