using System.Security.Cryptography;
using System.Text;

namespace RecipeIngredients.Endpoints;

/// <summary>
/// Basic service-to-service auth: requests must send the configured key in the
/// X-Api-Key header. The key comes from configuration (Api:Key, e.g. the
/// Api__Key environment variable or Docker secret) and is never stored in the
/// repo. With no key configured every request is refused, so the API cannot be
/// left open by accident.
/// </summary>
public sealed class ApiKeyFilter(IConfiguration config) : IEndpointFilter
{
    public const string HeaderName = "X-Api-Key";

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var expected = config["Api:Key"];
        if (string.IsNullOrWhiteSpace(expected))
        {
            return Results.Problem("API key is not configured on the server.", statusCode: StatusCodes.Status503ServiceUnavailable);
        }

        var provided = context.HttpContext.Request.Headers[HeaderName].ToString();
        var ok = CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(provided), Encoding.UTF8.GetBytes(expected));
        return ok ? await next(context) : Results.Unauthorized();
    }
}
