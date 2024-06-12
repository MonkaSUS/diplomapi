using ThreeMorons.Services;
var builder = WebApplication.CreateBuilder(args);
builder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(o =>
{
    o.SerializerOptions.IncludeFields = true;
});

//TODO ДОБАВИТЬ EasyCache

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
c.ResolveConflictingActions(a => a.First()));

builder.Services.AddHttpClient();


builder.Services.AddEasyCaching(o =>
{
    o.UseInMemory(config =>
    {
        config.DBConfig = new()
        {
            SizeLimit = 2000,
            EnableReadDeepClone = true,
            EnableWriteDeepClone = false,
            ExpirationScanFrequency = 60
        };
        config.MaxRdSecond = 9;
        config.LockMs = 5000;
        config.SleepMs = 321;
    }, "MemoryCache");
});
builder.Services.AddScoped<INotificationService, FcmNotificationService>();

var app = Initializer.Initialize(builder);
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("./v1/swagger.json", "My API V1")); //originally "./swagger/v1/swagger.json");
}
app.UseDeveloperExceptionPage();
Initializer.MapSkippedClassEndpoints(app);
Initializer.MapStudentEndpoints(app);
Initializer.MapDelayEndpoints(app);
Initializer.MapUserEndpoints(app, builder);
Initializer.MapGroupEndpoints(app);
Initializer.MapSpecialEndpoints(app);


if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.MapGet("/", (IHostEnvironment env) => Results.Content(File.ReadAllText(env.ContentRootPath + "/wwwroot/index.html"), "text/html"));

app.MapPost("/refresh", async (ThreeMoronsContext db, RefreshInput inp) =>
{
    var existingSession =
        await db.Sessions.FirstAsync(x => x.JwtToken == inp.JwtToken && x.RefreshToken == inp.RefreshToken)!;
    if (existingSession is null)
    {
        return Results.Text("Авторизуйтесь заново", statusCode: 403);
    }
    if (existingSession.SessionStart.AddDays(2) <= DateTime.Now)
    {
        existingSession.IsValid = false;
        db.SaveChanges();
        return Results.Text("Ваша сессия устарела. Авторизуйтесь заново", statusCode: 403);
    }
    var handler = new JwtSecurityTokenHandler();
    JwtSecurityToken? jwt = handler.ReadToken(inp.JwtToken) as JwtSecurityToken;
    if (jwt is null)
    {
        return Results.Problem("С токеном какая-то жижа", statusCode: 401);
    }
    var jti = jwt.Id;
    string userClass = jwt.Claims.First(x => x.Type == "userClass").Value;
    var newPair = JwtIssuer.IssueJwtForUser(builder.Configuration, jti, userClass);
    var newSession = new Session()
    {
        id = Guid.NewGuid(),
        IsValid = true,
        JwtToken = newPair.jwt,
        RefreshToken = newPair.refresh,
        SessionStart = DateTime.Now,
        //экскременты ой эксперименты
        SessionEnd = DateTime.Now.AddDays(2) //Сессия закрывается через 2 дня неактивности
    };
    db.Sessions.Add(newSession);
    db.SaveChanges();
    return Results.Json(newPair, statusCode: 200, contentType: "application/json");
});

app.MapGet("/checkAuth", () => "checking").RequireAuthorization();

app.UseAuthentication();
app.UseAuthorization();
app.UseStaticFiles();
app.Run();
app.Logger.LogInformation($"Application started at {DateTime.Now}");