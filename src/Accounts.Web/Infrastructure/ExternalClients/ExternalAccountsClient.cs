using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Options;
using PortfolioFe.Application.Interfaces;
using PortfolioFe.Domain.Bank;
using PortfolioFe.Infrastructure.Security;

namespace PortfolioFe.Infrastructure.ExternalClients;

public class ExternalAccountsClient(HttpClient httpClient, IOptions<ApiKeyOptions> options) : IExternalAccountsClient
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
        if (response.StatusCode == HttpStatusCode.NotFound) return null;
        await EnsureSuccessOrNotFound(response);
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
        if (response.StatusCode == HttpStatusCode.NotFound) return null;
        await EnsureSuccessOrNotFound(response);
        return await response.Content.ReadFromJsonAsync<IEnumerable<Account>>();
    }

    /// <summary>
    /// Throws an exception if the HTTP response indicates an unsuccessful status code.
    /// </summary>
    /// <param name="response">The <see cref="HttpResponseMessage"/> to evaluate.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <exception cref="Exception">
    /// Thrown when the HTTP response contains a non-success status code. The exception includes the
    /// status code, reason phrase, and response body for diagnostic purposes.
    /// </exception>
    private async Task EnsureSuccessOrNotFound(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.NotFound)
        {
            return;
        }

        var body = await response.Content.ReadAsStringAsync();
            throw new Exception(
                $"ExternalAccountsClient failed. Status: {(int)response.StatusCode} {response.ReasonPhrase}. Body: {body}");

    }


    /// <summary>
    /// Configures and creates an HTTP request message for a specified resource path.
    /// </summary>
    /// <param name="folderStructure">The relative path of the resource for which the request is being set up.</param>
    /// <returns>An <see cref="HttpRequestMessage"/> configured with the necessary headers and URI.</returns>
    private HttpRequestMessage SetUpRequest(string folderStructure)
    {

        var request = new HttpRequestMessage(HttpMethod.Get,
            folderStructure);
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
        if (options?.Value is null)
        {
            throw new InvalidOperationException("ApiKeyOptions not configured.");
        }
        var key = options?.Value?.AccountsApplicationApiKey;

        if (string.IsNullOrWhiteSpace(key))
        {
            throw new InvalidOperationException("Outgoing key not found.");
        }

        return key;

    }
}