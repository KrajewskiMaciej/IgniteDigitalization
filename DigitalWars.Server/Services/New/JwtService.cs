using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using backend.Data;
using DigitalWars.Server.Dtos;

namespace DigitalWars.Server.Services
{
    public class JwtService
    {
        public static List<Claim> GenerateUserToken(User user)
        {
            return new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Users_Id.ToString()),
                new Claim(ClaimTypes.Name, user.Names),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };
        }

        public static List<Claim> GenerateTeamToken(TeamTokenInfo team)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, team.Teams_Id.ToString()),
                new Claim(ClaimTypes.Name, team.Teams_Name),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(nameof(team.Teams_Token), team.Teams_Token),
                new Claim(nameof(team.Teams_Id), team.Teams_Id.ToString()),
                new Claim(nameof(team.Games_Id), team.Games_Id.ToString()),
                new Claim(nameof(team.Teams_Name), team.Teams_Name),
                new Claim(nameof(team.Teams_Color), team.Teams_Color),
                new Claim(nameof(team.Is_Online), team.Is_Online.ToString()),
                new Claim(nameof(team.Is_Independent), team.Is_Independent.ToString()),
                new Claim(nameof(team.Teams_Boards_Id), team.Teams_Boards_Id.ToString()),
                new Claim(nameof(team.Rivals_Boards_Id), team.Rivals_Boards_Id.ToString()),
                new Claim(nameof(team.Decks_Id), team.Decks_Id.ToString())
            };

            if (team.Modules_Id.HasValue)
            {
                claims.Add(new Claim(nameof(team.Modules_Id), team.Modules_Id.Value.ToString()));
            }

            return claims;
        }
    }
}