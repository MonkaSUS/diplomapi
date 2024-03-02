
namespace ThreeMorons.SecurityThings
{
    /// <summary>
    /// Класс, ответственный за выдачу JWT-токенов для пользователя.
    /// </summary>
    public static class JwtIssuer
    {
        /// <summary>
        /// Метод, выдающий JWT для авторизующегося пользователя. Claim пока один - по guid пользователя.
        /// </summary>
        /// <param name="config">builder.Config текущего приложения</param>
        /// <param name="authUser">Пользователь, который авторизуется</param>
        /// <returns>Строковую версию JWT токена для текущего пользователя. Identity содержит определение для jti, который устанавливается на основе guid пользователя</returns>
        public static (string jwt, string refresh) IssueJwtForUser(ConfigurationManager config, User authUser)
        {
            var issuer = config["Jwt:issuer"];
            var audience = config["Jwt:audience"];
            var key = Encoding.ASCII.GetBytes(config["Jwt:Key"]);
            var jwtTokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(JwtRegisteredClaimNames.Jti, authUser.Id.ToString()),
                    new Claim("userClass", authUser.UserClassId.ToString())
                }),
                Expires = DateTime.UtcNow.AddMinutes(3),
                Issuer = issuer,
                Audience = audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha512Signature)
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.CreateToken(jwtTokenDescriptor);
            var stringToken = tokenHandler.WriteToken(jwtToken);
            var RefreshToken = IssueRefreshToken(stringToken);
            return (stringToken, RefreshToken);
        }
        public static (string jwt, string refresh) IssueJwtForUser(ConfigurationManager config, string userId, string UserClassId)
        {
            var issuer = config["Jwt:issuer"];
            var audience = config["Jwt:audience"];
            var key = Encoding.ASCII.GetBytes(config["Jwt:Key"]);
            var jwtTokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(JwtRegisteredClaimNames.Jti, userId),
                    new Claim("userClass", UserClassId)
                }),
                Expires = DateTime.UtcNow.AddMinutes(3),
                Issuer = issuer,
                Audience = audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha512Signature)
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.CreateToken(jwtTokenDescriptor);
            var stringToken = tokenHandler.WriteToken(jwtToken);
            var RefreshToken = IssueRefreshToken(stringToken);
            return (stringToken, RefreshToken);
        }
        private static string IssueRefreshToken(string JwtToken)
        {
            ThreeMoronsContext context = new ThreeMoronsContext();
            var handler = new JwtSecurityTokenHandler();
            var decryptedToken = handler.ReadJwtToken(JwtToken);


            var idToSearch = decryptedToken.Claims.First(c => c.Type == "jti").Value; //КОГДА Я ПЫТАЛСЯ СДЕЛАТЬ ЭТО В ОДНУ СТРОКУ, ВСЁ ЛОМАЛОСЬ
            var salt = context.Users.FirstOrDefault(x => x.Id.ToString() == idToSearch).Salt;

            var refreshString = PasswordMegaHasher.HashPass(JwtToken, salt);
            return refreshString;
        }
    }
}
