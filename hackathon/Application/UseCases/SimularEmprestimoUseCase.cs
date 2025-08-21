using System.Text.Json;
using hackathon.Api.Serialization;
using hackathon.Application.Dtos;
using hackathon.Application.Interfaces;
using hackathon.Domain.Entities;
using hackathon.Domain.ValueObjects;
using hackathon.Infrastructure.Threading;

namespace hackathon.Application.UseCases;

public class SimularEmprestimoUseCase : ISimularEmprestimoUseCase
{
    private readonly IProdutoRepository _produtoRepository;
    private readonly ISimulacaoPersistenceService _persistenceService;
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<SimularEmprestimoUseCase> _logger;

    public SimularEmprestimoUseCase(
        IProdutoRepository produtoRepository,
        ISimulacaoPersistenceService persistenceService,
        IEventPublisher eventPublisher,
        ILogger<SimularEmprestimoUseCase> logger)
    {
        _produtoRepository = produtoRepository;
        _persistenceService = persistenceService;
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    public async Task<SimulacaoResponse?> ExecutarAsync(SimulacaoRequest request)
    {
        if (request.ValorDesejado <= 0 || request.Prazo <= 0)
            throw new ArgumentOutOfRangeException(nameof(request), "Valor e prazo devem ser positivos.");

        try
        {
            // === Caminho crítico (pode falhar o request) ===
            var produtos = await _produtoRepository
                .ObterProdutosCompativeisAsync(request.ValorDesejado, request.Prazo);

            var produtoIdeal = produtos?.OrderBy(p => p.TaxaJuros).FirstOrDefault();
            if (produtoIdeal is null) return null;

            var sac = SimularSAC(request.ValorDesejado, request.Prazo, produtoIdeal.TaxaJuros);
            var price = SimularPRICE(request.ValorDesejado, request.Prazo, produtoIdeal.TaxaJuros);

            var simulacao = new Simulacao
            {
                ValorDesejado = request.ValorDesejado,
                CodigoProduto = produtoIdeal.Codigo,
                DescricaoProduto = produtoIdeal.Nome,
                TaxaJuros = produtoIdeal.TaxaJuros,
                Sac = sac,
                Price = price
            };

            var response = new SimulacaoResponse(
                IdSimulacao: simulacao.Id.GetHashCode(),
                CodigoProduto: simulacao.CodigoProduto,
                DescricaoProduto: simulacao.DescricaoProduto,
                TaxaJuros: simulacao.TaxaJuros,
                Sac: new("SAC", simulacao.Sac.Select(p => p.ToDto()).ToList()),
                Price: new("PRICE", simulacao.Price.Select(p => p.ToDto()).ToList())
            );

            // === Não-crítico (best effort): nunca propaga exceção ===
            _persistenceService.EnqueueAsync(simulacao)
                .SafeFireAndForget(_logger, "Persistência de simulação");
            _eventPublisher.PublishAsync(
                    JsonSerializer.Serialize(response, AppJsonSerializerContext.Default.SimulacaoResponse))
                .SafeFireAndForget(_logger, "Publicação de evento de simulação");

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao simular empréstimo.");
            throw; // o pipeline HTTP converte em ProblemDetails
        }
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
