using System.Text.Json;

namespace SFA.DAS.RAA.Vacancy.AI.Api.IntegrationTests;

public static class HttpResponseMessageExtensions
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new() { PropertyNameCaseInsensitive = true };
    
    public static async Task<TEntity?> ReadAsAsync<TEntity>(this HttpContent? content)
    {
        ArgumentNullException.ThrowIfNull(content);

        var json = await content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<TEntity>(json, JsonSerializerOptions);
    }
}