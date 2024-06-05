namespace ThreeMorons.Security
{
    public class CustomJwtBearerEvents : JwtBearerEvents
    {
        private readonly ThreeMoronsContext _db;
        public CustomJwtBearerEvents(ThreeMoronsContext dbContext)
        {
            _db = dbContext;
        }
        public override async Task TokenValidated(TokenValidatedContext context)
        {
            var jwtToken = context.SecurityToken;
            var userId = context.Principal.FindFirstValue("jti");
            if (_db.Users.Find(Guid.Parse(userId)).IsDeleted)
            {
                context.Fail("Пользователя не активен");
                return;
            }
            var thisTokenString = jwtToken.UnsafeToString();
            var session = await _db.Sessions.FirstOrDefaultAsync(s => s.JwtToken == thisTokenString);
            if (session == null)
            {
                context.Fail("Сессии не существует");
                return;
            }
            if (!session.IsValid)
            {
                context.Fail("Сессия пользователя недействительна");
                return;
            }
            if (session.SessionEnd <= DateTime.Now)
            {
                context.Fail("Сессия пользователя истекла");
                return;
            }
            context.Success();
        }
    }
}
