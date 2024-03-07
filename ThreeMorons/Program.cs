using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Net;
using System.Text.Json;
using ThreeMorons.HealthCheck;
var builder = WebApplication.CreateBuilder(args); 
//ашкн днаюбкемн, онрнлс врн детнкрмши яепхюкюигеп фхдйн яп╗р онд яеаъ опх бхде рсокнб
builder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(o =>
{
    o.SerializerOptions.IncludeFields = true;
});

var app = Initializer.Initialize(builder); 
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
//TODO уект вейя
app.UseHttpsRedirection();





app.MapGet("/", () => Results.Content("amogus"));

Initializer.MapGroupEndpoints(app);

app.MapGet("/periods", async (ThreeMoronsContext db) => await db.Periods.ToListAsync());
app.MapGet("/refresh", async(ThreeMoronsContext db, HttpContext c)=>
{
    c.Request.Headers.TryGetValue("Authorization", out var jwt);
    c.Request.Headers.TryGetValue("Refresh", out var rft);
    //гдеяа мсфмн мюохяюрэ опнбепйс ясыеярнбюбмхъ рюйни оюпш б ад
    //бюфмн
    var iden = c.User.Identity as ClaimsIdentity;
    var id = iden.FindFirst("jti").Value;
    var uclass = iden.FindFirst("UserClass").Value;
    var newTokens = JwtIssuer.IssueJwtForUser(builder.Configuration, id, uclass);
    return Results.Ok(newTokens);

});

Initializer.MapSkippedClassEndpoints(app);

Initializer.MapStudentEndpoints(app);

Initializer.MapDelayEndpoints(app);

Initializer.MapUserEndpoints(app, builder);



app.UseAuthentication();
app.UseAuthorization();




app.Run();
