namespace hackathon.Domain.ValueObjects;

public record Parcela(
    int Numero,
    decimal ValorAmortizacao,
    decimal ValorJuros,
    decimal ValorPrestacao
);
