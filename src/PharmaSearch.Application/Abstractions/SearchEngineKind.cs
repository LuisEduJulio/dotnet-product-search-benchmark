namespace PharmaSearch.Application.Abstractions;

/// <summary>Estratégias de busca comparadas no benchmark.</summary>
public enum SearchEngineKind
{
    /// <summary>Elasticsearch: índice invertido (Lucene), BM25, fuzziness e autocomplete.</summary>
    Elasticsearch,

    /// <summary>MongoDB com índice de texto ($text) e stemming em português.</summary>
    MongoText,

    /// <summary>MongoDB com $regex "contém" sobre um campo normalizado (o LIKE '%termo%').</summary>
    MongoRegex
}
