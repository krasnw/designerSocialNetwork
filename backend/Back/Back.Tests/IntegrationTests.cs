using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Back.Tests;

public class IntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public IntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
              
            });
        });
    }

    [Fact]
    public async Task Get_SwaggerEndpoint_ReturnsSuccessStatusCode()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/swagger/v1/swagger.json");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
