using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Channels;
using Microsoft.Extensions.DependencyInjection;
using hackathon.Infrastructure.Services;
using hackathon.Application.Interfaces;
using hackathon.Domain.Entities;
using hackathon.Infrastructure.Telemetria;

namespace hackathon.Tests.Services;

public class TelemetriaServiceTests
{
    private class TelemetriaRepositoryFake : ITelemetriaRepository
    {
        public List<TelemetriaRecord> Persistidos { get; } = new();

        public Task SalvarTelemetriaAsync(List<TelemetriaRecord> records)
        {
            Persistidos.AddRange(records);
            return Task.CompletedTask;
        }

        public Task<IEnumerable<TelemetriaRecord>> ObterTelemetriaPorDataAsync(DateOnly dataReferencia)
        {
            return Task.FromResult(Persistidos.Where(r => r.DataReferencia.Date == dataReferencia.ToDateTime(TimeOnly.MinValue).Date));
        }
    }

    private class ServiceProviderFake : IServiceProvider
    {
        private readonly ITelemetriaRepository _repository;

        public ServiceProviderFake(ITelemetriaRepository repository)
        {
            _repository = repository;
        }

        public object GetService(Type serviceType)
        {
            if (serviceType == typeof(ITelemetriaRepository))
                return _repository;
            return null!;
        }

        public IServiceScope CreateScope()
        {
            return new ServiceScopeFake(_repository);
        }
    }

    private class ServiceScopeFake : IServiceScope
    {
        private readonly ITelemetriaRepository _repository;

        public ServiceScopeFake(ITelemetriaRepository repository)
        {
            _repository = repository;
        }

        public IServiceProvider ServiceProvider => new ServiceProviderFake(_repository);

        public void Dispose() { }
    }

    [Fact]
    public async Task RegistrarRequisicao_DeveAcumularMetricasCorretamente()
    {
        // Arrange
        var repo = new TelemetriaRepositoryFake();
        var serviceProvider = new ServiceProviderFake(repo);
        var channel = Channel.CreateUnbounded<TelemetriaMessage>();
        var service = new TelemetriaService(channel, serviceProvider);

        // Act
        service.RegistrarRequisicao("GET simulacoes", TimeSpan.FromMilliseconds(100), true);
        service.RegistrarRequisicao("GET simulacoes", TimeSpan.FromMilliseconds(200), false);
        await service.ObterTelemetriaAsync(DateOnly.FromDateTime(DateTime.Today)); // força flush

        // Assert
        Assert.Single(repo.Persistidos);
        var record = repo.Persistidos[0];
        Assert.Equal("GET simulacoes", record.NomeApi);
        Assert.Equal(2, record.QtdRequisicoes);
        Assert.Equal(150, record.TempoMedio);
        Assert.Equal(100, record.TempoMinimo);
        Assert.Equal(200, record.TempoMaximo);
        Assert.Equal(0.5m, record.PercentualSucesso);
    }

    [Fact]
    public async Task ObterTelemetriaAsync_DeveRetornarTelemetriaResponseComDados()
    {
        // Arrange
        var repo = new TelemetriaRepositoryFake();
        var hoje = DateOnly.FromDateTime(DateTime.Today);
        repo.Persistidos.Add(new TelemetriaRecord
        {
            DataReferencia = hoje.ToDateTime(TimeOnly.MinValue),
            NomeApi = "POST simulacoes",
            QtdRequisicoes = 10,
            TempoMedio = 120,
            TempoMinimo = 100,
            TempoMaximo = 150,
            PercentualSucesso = 0.9m
        });

        var serviceProvider = new ServiceProviderFake(repo);
        var channel = Channel.CreateUnbounded<TelemetriaMessage>();
        var service = new TelemetriaService(channel, serviceProvider);

        // Act
        var response = await service.ObterTelemetriaAsync(hoje);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(hoje.ToString("yyyy-MM-dd"), response.DataReferencia);
        Assert.Single(response.ListaEndpoints);
        var endpoint = response.ListaEndpoints[0];
        Assert.Equal("POST simulacoes", endpoint.NomeApi);
        Assert.Equal(10, endpoint.QtdRequisicoes);
        Assert.Equal(120, endpoint.TempoMedio);
        Assert.Equal(100, endpoint.TempoMinimo);
        Assert.Equal(150, endpoint.TempoMaximo);
        Assert.Equal(0.9m, endpoint.PercentualSucesso);
    }
}
