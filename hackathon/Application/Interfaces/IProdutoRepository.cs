using hackathon.Domain.Entities;

namespace hackathon.Application.Interfaces;

public interface IProdutoRepository
{
    Task<IEnumerable<Produto>> ObterProdutosCompativeisAsync(decimal valor, int prazo);
}