namespace PharmaSearch.Application.Search;

/// <summary>
/// Contrato de resposta. É o mesmo para todos os motores: o teste de contrato
/// garante que o JSON tem o mesmo formato venha de onde vier.
/// </summary>
public sealed record ProductHit(
    string Sku,
    string Name,
    string ActiveIngredient,
    string Brand,
    string Category,
    decimal Price,
    double Score);
