using PortfolioApplicationAPI.Application.Interfaces;
using PortfolioApplicationAPI.Models;

namespace PortfolioApplicationAPI.Infrastructure.ExternalClients;

public class ExternalAccountsClient(HttpClient httpClient, IConfiguration configuration) : IExternalAccountsClient
{
    private const string AccountsUrl = "api/accounts";

    /// <summary>
    /// Retrieves a specific account by its identifier asynchronously.
    /// </summary>
    /// <param name="id">The identifier of the account to retrieve.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an <see cref="Account"/> object if found, or null if no account is found.</returns>
    /// <exception cref="HttpRequestException">Thrown when the request to retrieve the account fails.</exception>
    /// <exception cref="JsonException">Thrown when the response content cannot be deserialized into an <see cref="Account"/> object.</exception>
    public async Task<Account?> GetAccountAsync(string id)
    {
        var request = SetUpRequest($"{AccountsUrl}/{id}");
        var response = await httpClient.SendAsync(request);
        return await response.Content.ReadFromJsonAsync<Account>();
    }
    /// <summary>
    /// Retrieves a collection of accounts asynchronously.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains an enumerable collection of <see cref="Account"/> objects, or null if no accounts are found.</returns>
    /// <exception cref="HttpRequestException">Thrown when the request to retrieve accounts fails.</exception>
    /// <exception cref="Exception">Thrown if the response content cannot be deserialized into an enumerable collection of accounts.</exception>
    public async Task<IEnumerable<Account>?> GetAccountsAsync()
    {
        var request = SetUpRequest(AccountsUrl);
        var response = await httpClient.SendAsync(request);
        return await response.Content.ReadFromJsonAsync<IEnumerable<Account>>();
    }
    /// <summary>
    /// Configures and returns an HTTP request message to retrieve a specific account by its identifier.
    /// </summary>
    /// <param name="id">The identifier of the account to retrieve.</param>
    /// <returns>An <see cref="HttpRequestMessage"/> configured with the appropriate HTTP method, URL, and headers needed to request the account details.</returns>
    private HttpRequestMessage SetUpRequest(string folderStructure)
    {
        var request = new HttpRequestMessage(HttpMethod.Get,
            $"{httpClient.BaseAddress}{folderStructure}");
        AddApiKeyHeader(request);
        return request;
    }


    /// <summary>
    /// Adds the API key header to the specified HTTP request.
    /// </summary>
    /// <param name="request">The HTTP request to which the API key header will be added.</param>
    private void AddApiKeyHeader(HttpRequestMessage request)
    {
        request.Headers.Add("x-api-key", GetOutgoingKey());
    }



    /// <summary>
    /// Retrieves the outgoing key from the configuration.
    /// </summary>
    /// <returns>A string representing the outgoing key.</returns>
    /// <exception cref="Exception">Thrown if the outgoing key is not found or is null/empty.</exception>
    private string GetOutgoingKey()
    {
        var outgoingKey = configuration["OutgoingAccountsAPIKey"] ?? throw new Exception("Outgoing Key not found");
        if (string.IsNullOrEmpty(outgoingKey)) throw new Exception("Outgoing Key not found");
        return outgoingKey;
    }
}