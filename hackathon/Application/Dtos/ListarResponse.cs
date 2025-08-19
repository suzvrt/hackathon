namespace hackathon.Application.Dtos;

public record ListarResponse(
    int Pagina,
    int QtdRegistros,
    int QtdRegistrosPagina,
    List<Registro> Registros
);

public record Registro(
    Guid IdSimulacao,
    decimal ValorDesejado,
    int Prazo,
    decimal ValorTotalParcelas
);