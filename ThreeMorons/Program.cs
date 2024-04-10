using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text.Json;
using ThreeMorons.HealthCheck;
using ThreeMorons.Services;
var builder = WebApplication.CreateBuilder(args); 
//БЫЛО ДОБАВЛЕНО, ПОТОМУ ЧТО ДЕФОЛТНЫЙ СЕРИАЛАЙЗЕР ЖИДКО СРЁТ ПОД СЕБЯ ПРИ ВИДЕ ТУПЛОВ
builder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(o =>
{
    o.SerializerOptions.IncludeFields = true;
});
var otel = builder.Services.AddOpenTelemetry();

var TotalRequestMeter = new Meter("TotalRequestMeter", "1.0.0");
var countRequests = TotalRequestMeter.CreateCounter<int>("requests.count", description: "Counts the total number of request since the last restart of the server");
var TotalActivitySource = new ActivitySource("TotalRequestMeter");

otel.ConfigureResource(r => r.AddService(serviceName: builder.Environment.ApplicationName));
otel.WithMetrics(m => m
    .AddPrometheusExporter()
    .AddAspNetCoreInstrumentation()
    .AddMeter(TotalRequestMeter.Name)
    .AddMeter("Microsoft.AspNetCore.Hosting")
    .AddMeter("Microsoft.AspNetCore.Server.Kestrel")
    .AddPrometheusExporter()) ;

otel.WithTracing(t =>
{
    t.AddAspNetCoreInstrumentation();
    t.AddHttpClientInstrumentation();
    t.AddOtlpExporter();
});

builder.Services.AddHttpClient();
builder.Services.AddOutputCache(o=>
{
    o.DefaultExpirationTimeSpan = TimeSpan.FromDays(1);
    o.SizeLimit = 3076;
    o.MaximumBodySize = 300;
    o.AddBasePolicy(p => p.Expire(TimeSpan.FromHours(12)));
    o.AddPolicy("Quick", p=> p.Expire(TimeSpan.FromMinutes(5)));
    o.AddPolicy("Medium", p => p.Expire(TimeSpan.FromHours(6)));
    
});

var app = Initializer.Initialize(builder);
app.UseCors();
app.UseResponseCaching();


app.Use(async (context, next) =>
{
    await next();
    using var activity = TotalActivitySource.StartActivity("RequestMeter");
    countRequests.Add(1);
    activity?.SetTag("request happened", context.GetEndpoint().ToString());
});
app.MapPrometheusScrapingEndpoint();
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
//TODO ХЕЛФ ЧЕКС
app.UseHttpsRedirection();





app.MapGet("/", () => Results.Content("amogus"));

Initializer.MapGroupEndpoints(app);

app.MapGet("/periods", async (ThreeMoronsContext db) => await db.Periods.ToListAsync());

app.MapPost("/refresh", async (ThreeMoronsContext db, RefreshInput inp) =>
{
    var existingSession =
        await db.Sessions.FirstAsync(x => x.JwtToken == inp.JwtToken && x.RefreshToken == inp.RefreshToken)!;
    if (existingSession is null)
    {
        return Results.Text("Авторизуйтесь заново", statusCode:403);
    }
    if (existingSession.SessionStart.AddDays(2) <= DateTime.Now)
    {
        existingSession.IsValid = false;
        db.SaveChanges();
        return Results.Text("Ваша сессия устарела. Авторизуйтесь заново", statusCode: 403);
    }
    var handler = new JwtSecurityTokenHandler();
    var jwt = handler.ReadToken(inp.JwtToken) as JwtSecurityToken;
    var jti = jwt.Id;
    var userClass = jwt.Claims.FirstOrDefault(x => x.Type == "userClass").Value;
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

//app.MapGet("/harakiri", (ThreeMoronsContext db, [FromQuery(Name ="sqlQ")] string query) =>
//{
//    var queryf = FormattableStringFactory.Create(query);
//    var res = db.Database.ExecuteSql(queryf);
//    return Results.Ok("Заебись!");
//});

Initializer.MapSkippedClassEndpoints(app);

Initializer.MapStudentEndpoints(app);

Initializer.MapDelayEndpoints(app);

Initializer.MapUserEndpoints(app, builder);



app.UseAuthentication();
app.UseAuthorization();




app.Run();
