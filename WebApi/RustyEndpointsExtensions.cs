namespace WebApi;

public static class RustyEndpointsExtensions
{
    public static IEndpointRouteBuilder UseRustyEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/rusty", async (IConfiguration config, IHttpClientFactory httpClientFactory) =>
        {
            var httpClient = httpClientFactory.CreateClient();
            httpClient.BaseAddress = new Uri(config["services:rusty:http:0"] ?? throw new InvalidOperationException("Rusty service URL not set"));
            var response = await httpClient.GetFromJsonAsync<RustyResponseMessage>("/hello");
            return Results.Ok(response);
        }).WithTags("Rusty").WithName("RustyHello");

        return app;
    }
}

public record RustyResponseMessage(string Message);
