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
#pragma warning disable CS8604 // Возможно, аргумент-ссылка, допускающий значение NULL.
            var userId = context.Principal.FindFirstValue("jti");
#pragma warning restore CS8604 // Возможно, аргумент-ссылка, допускающий значение NULL.
            if (userId is null)
            {
                context.Fail("id was null");
                return;
            }
            var thisUser = _db.Users.AsNoTracking().First(x => x.Id == Guid.Parse(userId));
            if (thisUser.IsDeleted)
            {
                context.Fail("Пользователя не активен");
                return;
            }
            var thisTokenString = jwtToken.UnsafeToString();
            var session = _db.Sessions.AsNoTracking().FirstOrDefault(s => s.JwtToken == thisTokenString);
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
