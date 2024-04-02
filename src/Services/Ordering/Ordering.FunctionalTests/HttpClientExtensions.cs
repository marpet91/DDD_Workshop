using Xunit.Abstractions;

namespace Ordering.FunctionalTests;

static class HttpClientExtensions
{
    public static HttpClient CreateIdempotentClient(this TestServer server)
    {
        var client = server.CreateClient();
        client.DefaultRequestHeaders.Add("x-requestid", Guid.NewGuid().ToString());
        return client;
    }   
    
    public static async Task EnsureSuccessResponseCodeAsync(this HttpResponseMessage response, ITestOutputHelper output)
    {
        try
        {
            response.EnsureSuccessStatusCode();
        }
        catch
        {
            output.WriteLine(await response.Content.ReadAsStringAsync());
            throw;
        }
    }
}
