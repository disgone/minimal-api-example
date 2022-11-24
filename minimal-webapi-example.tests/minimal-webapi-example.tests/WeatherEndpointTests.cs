using System.Net;
using System.Net.Http.Json;

namespace MinimumApiExample.Tests;

public class WeatherEndpointTests
{
    [Fact]
    public async Task GetWeather_RequiresAuthToken()
    {
        await using TestApplication application = new();
        HttpClient client = application.CreateClient();

        var response = await client.GetAsync("/weatherforecast");
        
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetWeather()
    {
        await using TestApplication application = new();
        HttpClient client = application.CreateAuthorizedClient(Guid.NewGuid().ToString());

        List<WeatherForecast>? weather = await client.GetFromJsonAsync<List<WeatherForecast>>("/weatherforecast");
        Assert.NotNull(weather);
    }
}