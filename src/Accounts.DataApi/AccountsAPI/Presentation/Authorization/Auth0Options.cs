using System.ComponentModel.DataAnnotations;

namespace AccountsAPI.Presentation.Authorization;

public class Auth0Options
{
    public const string SectionName = "Auth0";

    [Required(ErrorMessage = "Auth0 Domain is required to start the application")]
    public string Domain
    {
        get;
        init;
    } = string.Empty;

    [Required(ErrorMessage = "Auth0 Audience is required to start the application")]
    public string Audience
    {
        get;
        init;
    } = string.Empty;
}