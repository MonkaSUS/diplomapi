using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text.Json;
using ThreeMorons.HealthCheck;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.OpenApi; 
using ThreeMorons.Services;
var builder = WebApplication.CreateBuilder(args);
//���� ���������, ������ ��� ��������� ����������� ����� �Ш� ��� ���� ��� ���� ������
builder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(o =>
{
    o.SerializerOptions.IncludeFields = true;
});

//TODO �������� EasyCache

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c=>
c.ResolveConflictingActions(a=> a.First()));

builder.Services.AddHttpClient();
builder.Services.AddOutputCache(o =>
{
    o.DefaultExpirationTimeSpan = TimeSpan.FromDays(1);
    o.SizeLimit = 3076;
    o.MaximumBodySize = 300;
    o.AddBasePolicy(p => p.Expire(TimeSpan.FromHours(12)));
    o.AddPolicy("Quick", p => p.Expire(TimeSpan.FromMinutes(5)));
    o.AddPolicy("Medium", p => p.Expire(TimeSpan.FromHours(6)));

});
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

app.UseResponseCaching();



//TODO ���� ����
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}





app.MapGet("/", (IHostEnvironment env) => Results.Content(File.ReadAllText(env.ContentRootPath + "/wwwroot/index.html"), "text/html"));


app.MapGet("/periods", async (ThreeMoronsContext db) => await db.Periods.ToListAsync());

app.MapPost("/refresh", async (ThreeMoronsContext db, RefreshInput inp) =>
{
    var existingSession =
        await db.Sessions.FirstAsync(x => x.JwtToken == inp.JwtToken && x.RefreshToken == inp.RefreshToken)!;
    if (existingSession is null)
    {
        return Results.Text("������������� ������", statusCode: 403);
    }
    if (existingSession.SessionStart.AddDays(2) <= DateTime.Now)
    {
        existingSession.IsValid = false;
        db.SaveChanges();
        return Results.Text("���� ������ ��������. ������������� ������", statusCode: 403);
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
        //����������� �� ������������
        SessionEnd = DateTime.Now.AddDays(2) //������ ����������� ����� 2 ��� ������������
    };
    db.Sessions.Add(newSession);
    db.SaveChanges();
    return Results.Json(newPair, statusCode: 200, contentType: "application/json");
});

//app.MapGet("/harakiri", (ThreeMoronsContext db, [FromQuery(Name ="sqlQ")] string query) =>
//{
//    var queryf = FormattableStringFactory.Create(query);
//    var res = db.Database.ExecuteSql(queryf);
//    return Results.Ok("�������!");
//});


app.MapGet("testnotif", async (IWebHostEnvironment env, INotificationService notifs, ILoggerFactory fac) =>
{
    var logger = fac.CreateLogger("testnotif");
    Message msg = new Message()
    {
        Notification = new Notification
        {
            Title = "����������",
            Body = "���� ����������"
        },
        Android = new AndroidConfig
        {
            Notification = new AndroidNotification
            {
                ChannelId = "public_announcements"
            },
            Priority = Priority.Normal,
            RestrictedPackageName = "com.kgpk.collegeapp"
        },
        Data = new Dictionary<string, string>()
        {
            { "amogus", "amogus" },
            { "sussy", "wussy" }
        },
        //������ ������. ����� ������������ ����� ��������
        Topic = "announcements"
    };
    string result = await notifs.SendAsync(msg);
    logger.LogInformation($"������ ��������� � �������� ����������� ������������� {result}");
    return Results.Ok(result);
});

app.UseAuthentication();
app.UseAuthorization();
app.UseStaticFiles();

//����� ���� ������ ����



app.Run();
app.Logger.LogInformation($"Application started at {DateTime.Now}");