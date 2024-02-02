var builder = WebApplication.CreateBuilder(args);

var app = Initializer.Initialize(builder); 

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
app.UseHttpsRedirection();

app.MapGet("/", () => Results.Content(File.ReadAllText("C:\\Users\\Admin\\source\\repos\\ThreeMorons\\ThreeMorons\\wwwroot\\index.html"), contentType: "text/html"));


Initializer.MapGroupEndpoints(app);

app.MapGet("/periods", async (ThreeMoronsContext db) => await db.Periods.ToListAsync());

Initializer.MapSkippedClassEndpoints(app);

Initializer.MapStudentEndpoints(app);

Initializer.MapDelayEndpoints(app);

Initializer.MapUserEndpoints(app, builder);



app.UseAuthentication();
app.UseAuthorization();

app.Run();
