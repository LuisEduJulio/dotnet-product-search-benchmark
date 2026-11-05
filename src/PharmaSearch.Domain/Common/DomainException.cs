namespace PharmaSearch.Domain.Common;

/// <summary>Violação de uma regra de negócio do domínio.</summary>
public sealed class DomainException(string message) : Exception(message);
