using MinimumApiExample.Extensions;

namespace MinimumApiExample;

internal static class WeatherEndpoints
{
    internal static RouteGroupBuilder AddWeatherEndpoints(this RouteGroupBuilder routeGroup)
    {
        routeGroup.WithOpenApi()
            .RequireAuthorization()
            .AddOpenApiSecurityRequirement();

        routeGroup.MapGet("/weatherforecast", () =>
            {
                var forecast = Enumerable.Range(1, 5).Select(index =>
                        new WeatherForecast
                        (
                            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                            Random.Shared.Next(-20, 55),
                            Summaries[Random.Shared.Next(Summaries.Length)]
                        ))
                    .ToArray();
                return forecast;
            })
            .WithName("GetWeatherForecast")
            .Produces<WeatherForecast>();

        return routeGroup;
    }
    
    private static readonly string[] Summaries = {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };
    
    
}

public record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}