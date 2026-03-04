using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.JsonPatch;
using Newtonsoft.Json;

namespace SFA.DAS.RAA.Vacancy.AI.Api.IntegrationTests;

public static class HttpClientExtensions
{
    public static Task<HttpResponseMessage> PatchAsync<T>(this HttpClient client, [StringSyntax(StringSyntaxAttribute.Uri)] string? requestUri, JsonPatchDocument<T> patchDocument) where T : class
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentNullException.ThrowIfNull(patchDocument);

        // IMPORTANT: System.Json.Text.JsonSerializer currently does not serialise JsonPatchDocument correctly 
        var stringContent = JsonConvert.SerializeObject(patchDocument);
        var requestContent = new StringContent(stringContent, System.Text.Encoding.UTF8, "application/json-patch+json");
        return client.PatchAsync(requestUri, requestContent);
    }
    
    public static Task<HttpResponseMessage> PatchAsync(this HttpClient client, [StringSyntax(StringSyntaxAttribute.Uri)] string? requestUri, JsonPatchDocument patchDocument)
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentNullException.ThrowIfNull(patchDocument);

        // IMPORTANT: System.Json.Text.JsonSerializer currently does not serialise JsonPatchDocument correctly 
        var stringContent = JsonConvert.SerializeObject(patchDocument);
        var requestContent = new StringContent(stringContent, System.Text.Encoding.UTF8, "application/json-patch+json");
        return client.PatchAsync(requestUri, requestContent);
    }
}