using System.Net;
using System.Text.Json;
using FluentResults;
using Microsoft.Extensions.Options;
using PortfolioApplicationAPI.Application.Dtos.Accounts;
using PortfolioApplicationAPI.Application.Interfaces;
using PortfolioApplicationAPI.Domain.Entities;
using PortfolioApplicationAPI.Infrastructure.Security.ApiKeys;

namespace PortfolioApplicationAPI.Infrastructure.ExternalClients;

/// <summary>
/// ExternalAccountsClient is responsible for interacting with external account services.
/// It provides methods to retrieve account information by account identifier or to get a list of accounts.
/// </summary>
public class ExternalAccountsClient(
    HttpClient httpClient,
    IOptions<ApiKeyOptions> options) : IExternalAccountsClient
{
    private const string AccountsUrl = "api/accounts";

    /// <summary>
    /// Retrieves a specific account by its identifier asynchronously.
    /// </summary>
    /// <param name="id">The identifier of the account to retrieve.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an <see cref="Account"/> object if found, or null if no account is found.</returns>
    /// <exception cref="HttpRequestException">Thrown when the request to retrieve the account fails.</exception>
    /// <exception cref="JsonException">Thrown when the response content cannot be deserialized into an <see cref="Account"/> object.</exception>
    public async Task<Result<Account>> GetAccountAsync(int id, CancellationToken ct)
    {
        var request = SetUpRequest($"{AccountsUrl}/{id}");
        var response = await httpClient.SendAsync(request, ct);
        await EnsureSuccessOrNotFound(response, ct);
        var dto =  await response.Content.ReadFromJsonAsync<AccountDto>(ct);
        var account = new Account()
        {
            Id = AccountId.From(dto.Id),
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Balance = dto.Balance,
            OverdraftLimit = dto.OverdraftLimit

        };
        return Result.Ok(account);




    }
    /// <summary>
    /// Retrieves a collection of accounts asynchronously.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains an enumerable collection of <see cref="Account"/> objects, or null if no accounts are found.</returns>
    /// <exception cref="HttpRequestException">Thrown when the request to retrieve accounts fails.</exception>
    /// <exception cref="Exception">Thrown if the response content cannot be deserialized into an enumerable collection of accounts.</exception>
    public async Task<Result<IReadOnlyList<Account>>> GetAccountsAsync(CancellationToken ct)
    {
        var request = SetUpRequest(AccountsUrl);
        var response = await httpClient.SendAsync(request);

        if (response is {StatusCode: HttpStatusCode.NotFound, IsSuccessStatusCode: false})
        {
            Result.Fail("Account not found");
        }
        await EnsureSuccessOrNotFound(response, ct);

        var dtoList = await response.Content.ReadFromJsonAsync<IReadOnlyList<AccountDto>>();

        if (dtoList is null)
            return null;

        IReadOnlyList<Account> list = dtoList.Select(dto => new Account
        {
            Id = AccountId.From(dto.Id),
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Balance = dto.Balance,
            OverdraftLimit = dto.OverdraftLimit
        }).ToList();

        return Result.Ok(list);


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
    private async Task EnsureSuccessOrNotFound(HttpResponseMessage response, CancellationToken ct)
    {
        if (response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.NotFound)
        {
            return;
        }

        var body = await response.Content.ReadAsStringAsync(ct);
            throw new HttpRequestException(
                $"External Accounts Client failed. Status: {(int)response.StatusCode} {response.ReasonPhrase}. Body: {body}", null, response.StatusCode);

    }
    /// <summary>
    /// Sets up an HTTP GET request with the specified folder structure appended to the base address.
    /// </summary>
    /// <param name="folderStructure">The folder structure to append to the base address for the request URI.</param>
    /// <returns>A <see cref="HttpRequestMessage"/> object configured with the appropriate URI and headers.</returns>
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
        var key = options?.Value?.AccountsApiKey;

        if (string.IsNullOrWhiteSpace(key))
        {
            throw new InvalidOperationException("Outgoing key not found.");
        }

        return key;

    }
}