using hackathon.Application.Dto;
using hackathon.Application.Interfaces;
using hackathon.Domain.Entities;
using hackathon.Domain.ValueObjects;

namespace hackathon.Application.UseCases;

public class SimularEmprestimoUseCase
{
    private readonly IProdutoRepository _produtoRepository;
    private readonly ISimulacaoPersistenceService _persistenceService;

    public SimularEmprestimoUseCase(
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

        var sac = SimularSAC(request.ValorDesejado, request.Prazo, produtoIdeal.TaxaJuros);
        var price = SimularPRICE(request.ValorDesejado, request.Prazo, produtoIdeal.TaxaJuros);

        var simulacao = new Simulacao
        {
            CodigoProduto = produtoIdeal.Codigo,
            DescricaoProduto = produtoIdeal.Nome,
            TaxaJuros = produtoIdeal.TaxaJuros / 1.0000000000000000000000000000m,
            Sac = new("SAC", sac.Select(p => p.ToDto()).ToList()),
            Price = new("PRICE", price.Select(p => p.ToDto()).ToList())
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

    private List<Parcela> SimularSAC(decimal valor, int prazo, decimal taxa)
    {
        var amortizacao = valor / prazo;
        var parcelas = new List<Parcela>();

        for (int i = 1; i <= prazo; i++)
        {
            var saldoDevedor = valor - (amortizacao * (i - 1));
            var juros = saldoDevedor * taxa;
            var prestacao = amortizacao + juros;

            parcelas.Add(new Parcela(i, amortizacao, juros, prestacao));
        }

        return parcelas;
    }

    private List<Parcela> SimularPRICE(decimal valor, int prazo, decimal taxa)
    {
        var fator = decimal.One + taxa;
        var potencia = decimal.One;

        for (int i = 0; i < prazo; i++)
        {
            potencia *= fator;
        }

        var parcela = valor * taxa * potencia / (potencia - decimal.One);
        var saldoDevedor = valor;
        var parcelas = new List<Parcela>();

        for (int i = 1; i <= prazo; i++)
        {
            var juros = saldoDevedor * taxa;
            var amortizacao = parcela - juros;
            saldoDevedor -= amortizacao;

            parcelas.Add(new Parcela(i, amortizacao, juros, parcela));
        }

        return parcelas;
    }
}
