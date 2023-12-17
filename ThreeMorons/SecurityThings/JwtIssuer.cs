using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ThreeMorons.Model;

namespace ThreeMorons.SecurityThings
{
    public static class JwtIssuer
    {
        /// <summary>
        /// Метод, выдающий JWT для авторизующегося пользователя. Claim пока один - по guid пользователя.
        /// </summary>
        /// <param name="config">builder.Config текущего приложения</param>
        /// <param name="authUser">Пользователь, который авторизуется</param>
        /// <returns>Строковую версию JWT токена для текущего пользователя. Identity содержит определение для jti, который устанавливается на основе guid пользователя</returns>
        public static string IssueJwtForUser(ConfigurationManager config, User authUser)
        {
            var issuer = config["Jwt:issuer"];
            var audience = config["Jwt:audience"];
            var key = Encoding.ASCII.GetBytes(config["Jwt:Key"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(JwtRegisteredClaimNames.Jti, authUser.Id.ToString()),
                }),
                Expires = DateTime.UtcNow.AddMinutes(5),
                Issuer = issuer,
                Audience = audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha512Signature)
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.CreateToken(tokenDescriptor);
            var stringToken = tokenHandler.WriteToken(jwtToken);
            return stringToken;
        }
    }
}
