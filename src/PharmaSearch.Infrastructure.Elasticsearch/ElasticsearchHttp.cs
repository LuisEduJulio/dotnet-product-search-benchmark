namespace PharmaSearch.Infrastructure.Elasticsearch;

internal static class ElasticsearchHttp
{
    public static async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
            return;

        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        throw new InvalidOperationException(
            $"Elasticsearch respondeu {(int)response.StatusCode} em {response.RequestMessage?.RequestUri}: {body}");
    }
}
