using System.Net;

namespace SFA.DAS.RAA.Vacancy.AI.Api.Core.Http;

public static class HttpStatusCodeExtensions
{
    public static bool IsSuccessStatusCode(this HttpStatusCode statusCode) => (int)statusCode >= 200 && (int)statusCode <= 299;
}