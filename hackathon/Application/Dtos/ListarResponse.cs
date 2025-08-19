namespace hackathon.Application.Dtos;

public sealed class SimulacaoResumo
{
    public Guid IdSimulacao { get; init; }
    public decimal ValorDesejado { get; init; }
    public int Prazo { get; init; }
    public decimal ValorTotalParcelas { get; init; }
}

public sealed class PaginacaoResultado<T>
{
    public int Pagina { get; init; }
    public int QtdRegistros { get; init; }
    public int QtdRegistrosPagina { get; init; }
    public IReadOnlyList<T> Registros { get; init; } = [];
}
