using hackathon.Application.Dto;
using hackathon.Application.Interfaces;
using hackathon.Domain.Entities;
using hackathon.Domain.ValueObjects;

namespace hackathon.Application.UseCases;

public class SimularEmprestimoUseCase
{
    private readonly IProdutoRepository _produtoRepository;

    public SimularEmprestimoUseCase(IProdutoRepository produtoRepository)
    {
        _produtoRepository = produtoRepository;
    }

    public async Task<SimulacaoResponse?> ExecutarAsync(SimulacaoRequest request)
    {
        var produtos = await _produtoRepository.ObterProdutosCompativeisAsync(request.ValorDesejado, request.PrazoMeses);
        var produtoIdeal = produtos.OrderBy(p => p.TaxaJuros).FirstOrDefault();

        if (produtoIdeal is null) return null;

        var sac = SimularSAC(request.ValorDesejado, request.PrazoMeses, produtoIdeal.TaxaJuros);
        var price = SimularPRICE(request.ValorDesejado, request.PrazoMeses, produtoIdeal.TaxaJuros);

        return new SimulacaoResponse(
            IdSimulacao: Guid.NewGuid().GetHashCode(),
            CodigoProduto: produtoIdeal.Codigo,
            DescricaoProduto: produtoIdeal.Nome,
            TaxaJuros: produtoIdeal.TaxaJuros,
            Sac: new("SAC", sac.Select(p => p.ToDto()).ToList()),
            Price: new("PRICE", price.Select(p => p.ToDto()).ToList())
        );
    }

    private List<Parcela> SimularSAC(decimal valor, int prazo, decimal taxa)
    {
        var amortizacao = valor / prazo;
        var parcelas = new List<Parcela>();

        for (int i = 1; i <= prazo; i++)
        {
            var saldoDevedor = valor - amortizacao * (i - 1);
            var juros = saldoDevedor * taxa;
            var prestacao = amortizacao + juros;

            parcelas.Add(new Parcela(i, Math.Round(amortizacao, 2), Math.Round(juros, 2), Math.Round(prestacao, 2)));
        }

        return parcelas;
    }

    private List<Parcela> SimularPRICE(decimal valor, int prazo, decimal taxa)
    {
        var fator = (decimal)Math.Pow(1 + (double)taxa, prazo);
        var parcela = valor * taxa * fator / (fator - 1);
        var saldoDevedor = valor;
        var parcelas = new List<Parcela>();

        for (int i = 1; i <= prazo; i++)
        {
            var juros = saldoDevedor * taxa;
            var amortizacao = parcela - juros;
            saldoDevedor -= amortizacao;

            parcelas.Add(new Parcela(i, Math.Round(amortizacao, 2), Math.Round(juros, 2), Math.Round(parcela, 2)));
        }

        return parcelas;
    }
}
