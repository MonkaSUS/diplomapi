using System.Net;

var builder = WebApplication.CreateBuilder(args);

var app = Initializer.Initialize(builder); 

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
app.UseHttpsRedirection();

app.MapGet("/", () => Results.Content(File.ReadAllText("C:\\Users\\Student29\\Source\\Repos\\ThreeMorons\\ThreeMorons\\wwwroot\\index.html"), contentType: "text/html"));


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
