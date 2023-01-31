namespace Calicot.Shared.Models;

public class AuthenticateResponse
{
    public string Id { get; set; } = default!;
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    // public string Username { get; set; }
    public string Token { get; set; } = default!;


    public AuthenticateResponse(User user, string token)
    {
        Id = user.Id;
        FirstName = user.FirstName;
        LastName = user.LastName;
        // Username = user.Username;
        Token = token;
    }
}
