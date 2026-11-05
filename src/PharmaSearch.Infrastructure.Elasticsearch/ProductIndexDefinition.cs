namespace PharmaSearch.Infrastructure.Elasticsearch;

/// <summary>
/// Mapeamento do índice. As três decisões que fazem diferença na busca de produtos:
/// 1. pt_folded: minúsculas + remove acento + stemming leve em português ("ácido" = "acido").
/// 2. autocomplete: edge n-gram no índice, para "parace" achar "paracetamol".
/// 3. category como keyword, para filtrar sem afetar a nota (filter context).
/// </summary>
internal static class ProductIndexDefinition
{
    public const string Json = """
    {
      "settings": {
        "number_of_shards": 1,
        "number_of_replicas": 0,
        "refresh_interval": "1s",
        "analysis": {
          "filter": {
            "pt_light_stemmer": { "type": "stemmer", "language": "light_portuguese" },
            "prefix_2_15": { "type": "edge_ngram", "min_gram": 2, "max_gram": 15 }
          },
          "analyzer": {
            "pt_folded": {
              "type": "custom",
              "tokenizer": "standard",
              "filter": [ "lowercase", "asciifolding", "pt_light_stemmer" ]
            },
            "autocomplete_index": {
              "type": "custom",
              "tokenizer": "standard",
              "filter": [ "lowercase", "asciifolding", "prefix_2_15" ]
            },
            "autocomplete_search": {
              "type": "custom",
              "tokenizer": "standard",
              "filter": [ "lowercase", "asciifolding" ]
            }
          }
        }
      },
      "mappings": {
        "dynamic": "strict",
        "properties": {
          "sku": { "type": "keyword" },
          "name": {
            "type": "text",
            "analyzer": "pt_folded",
            "fields": {
              "autocomplete": { "type": "text", "analyzer": "autocomplete_index", "search_analyzer": "autocomplete_search" }
            }
          },
          "activeIngredient": {
            "type": "text",
            "analyzer": "pt_folded",
            "fields": {
              "autocomplete": { "type": "text", "analyzer": "autocomplete_index", "search_analyzer": "autocomplete_search" }
            }
          },
          "brand": { "type": "text", "analyzer": "pt_folded" },
          "category": { "type": "keyword" },
          "description": { "type": "text", "analyzer": "pt_folded" },
          "price": { "type": "scaled_float", "scaling_factor": 100 }
        }
      }
    }
    """;
}
