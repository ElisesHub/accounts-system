namespace PortfolioApplicationAPI.Application.Dtos.ApiResponses;

public class AccountResponse
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public decimal Balance { get; set; }
    public decimal OverdraftLimit { get; set; }

}