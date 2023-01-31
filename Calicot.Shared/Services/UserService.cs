using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Calicot.Shared.Helpers;
using Calicot.Shared.Models;

namespace Calicot.Shared.Services
{
    public interface IUserService
    {
        AuthenticateResponse Authenticate(AuthenticateRequestUser model);
        IEnumerable<User> GetAll();
        User? GetById(string id);
        User? GetByEmail(string email);
    }

    public class UserService : IUserService
    {
        // users hardcoded for simplicity, store in a db with hashed passwords in production applications
        private List<User> _users = new List<User>
        {
            new User { Id = new Guid().ToString(), FirstName = "Henrick", LastName = "Poliquin", UserName = "hpoliquin", Password = "test", Email = "hpoliquin@gmail.com" },
            new User { Id = new Guid().ToString(), FirstName = "Tidjani", LastName = "Belmansour", UserName = "tidjani.belmansour", Password = "sfgser9fg76earg", Email = "tidjani.belmansour@gmail.com" }
        };

        private readonly AppSettings _appSettings;

        public UserService(IOptions<AppSettings> appSettings)
        {
            _appSettings = appSettings.Value;
        }

        public AuthenticateResponse Authenticate(AuthenticateRequestUser model)
        {
            var user = _users.SingleOrDefault(x => x.UserName == model.UserName && x.Password == model.Password);

            // return null if user not found
            if (user == null) return default!;

            // authentication successful so generate jwt token
            var token = generateJwtToken(user);

            return new AuthenticateResponse(user, token);
        }

        public IEnumerable<User> GetAll()
        {
            return _users;
        }

        public User? GetById(string id)
        {
            return _users.FirstOrDefault(x => x.Id == id);
        }

        public User? GetByEmail(string email) {
            return _users.FirstOrDefault(x => {
                    var emailvalue = x.Email??"";
                    return emailvalue.Equals(email, StringComparison.InvariantCultureIgnoreCase);
                });
        }

        // helper methods

        private string generateJwtToken(User user)
        {
            // generate token that is valid for 7 days
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { new Claim("id", user.Id.ToString()) }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}