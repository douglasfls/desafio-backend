using System.Net.Http.Json;

using Aspire.Hosting;

using FluentAssertions;


namespace DesafioBackend.Tests.ApiService.Tests;

public class IntegrationTest1 : IAsyncLifetime
{
    private DistributedApplication? _app;
    private ResourceNotificationService _resourceNotificationService;

    /// <summary>
    /// Load Aspire Environment
    /// </summary>
    public async Task InitializeAsync()
    {
        var appHost =
            await DistributedApplicationTestingBuilder.CreateAsync<Projects.DesafioBackend_AppHost>(["unit-tests"]);
        appHost.Services.ConfigureHttpClientDefaults(clientBuilder =>
        {
            clientBuilder.AddStandardResilienceHandler();
        });

        _app = await appHost.BuildAsync();
        _resourceNotificationService = _app.Services.GetRequiredService<ResourceNotificationService>();
        await _app.StartAsync();
    }

    /// <summary>
    /// Stop Services
    /// </summary>
    public async Task DisposeAsync()
    {
        if (_app != null) await _app.DisposeAsync();
    }

    private async Task<HttpClient> GetHttpClient()
    {
        var httpClient = _app.CreateHttpClient("apiservice");
        await _resourceNotificationService.WaitForResourceAsync("apiservice", KnownResourceStates.Running)
            .WaitAsync(TimeSpan.FromSeconds(30));
        return httpClient;
    }

    record LoginRequest(string Login = "letscode", string Password = "lets@123");
    record ApiAuthenticationResponse(string AccessToken);
    record Card(int Id, string Title, string Content, string List);

    [Fact]
    public async Task LoginRequestShouldReturnStatusCode200()
    {
        // Act
        HttpClient httpClient = await GetHttpClient();
        var response = await httpClient.PostAsJsonAsync("/login", new LoginRequest());
        var token = await response.Content.ReadFromJsonAsync<ApiAuthenticationResponse>()!;

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        token.AccessToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task CreateCardRequestShouldReturnStatusCode200()
    {
        var client = await GetHttpClient();
        var response = await client.PostAsJsonAsync("/login", new LoginRequest());
        var token = await response.Content.ReadFromJsonAsync<ApiAuthenticationResponse>()!;
        client.DefaultRequestHeaders.Authorization = new("Bearer", token.AccessToken);
        var newCard = await client.PostAsJsonAsync("/cards",
            new { Title = "Desafio", Content = "Conteudo do desafio", List = "Algum valor" });

        var card = await newCard.Content.ReadFromJsonAsync<Card>();
        newCard.StatusCode.Should().Be(HttpStatusCode.OK);
        card.Title.Should().Be("Desafio");
        card.Content.Should().Be("Conteudo do desafio");
        card.List.Should().Be("Algum valor");
        card.Id.Should().Be(1);
    }
}