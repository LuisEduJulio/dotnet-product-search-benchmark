using PharmaSearch.Application.Search;
using PharmaSearch.Domain.Products;
using PharmaSearch.Infrastructure.Elasticsearch;

namespace PharmaSearch.UnitTests.Infrastructure;

public class ElasticsearchQueryTests
{
    [Fact]
    public void Consulta_tem_fuzziness_autocomplete_e_nao_conta_total()
    {
        var json = ElasticsearchProductSearchEngine
            .BuildQuery(new ProductSearchCriteria(SearchTerm.Create("dipirna")))
            .ToJsonString();

        Assert.Contains("\"fuzziness\":\"AUTO\"", json);
        Assert.Contains("name.autocomplete^2", json);
        Assert.Contains("\"track_total_hits\":false", json);
        Assert.DoesNotContain("\"filter\"", json);
    }

    [Fact]
    public void Categoria_vai_para_o_filter_context()
    {
        var json = ElasticsearchProductSearchEngine
            .BuildQuery(new ProductSearchCriteria(SearchTerm.Create("vitamina c"), "Vitaminas e Suplementos", 10))
            .ToJsonString();

        Assert.Contains("\"filter\":[{\"term\":{\"category\":\"Vitaminas e Suplementos\"}}]", json);
        Assert.Contains("\"size\":10", json);
    }
}
