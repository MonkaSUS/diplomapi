using Serilog;
using Microsoft.EntityFrameworkCore;
using ThreeMorons.Model;
using FluentValidation;
using ThreeMorons.Validators;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using ThreeMorons.UserInputTypes;
using ThreeMorons.SecurityThings;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);


builder.Logging.ClearProviders();

builder.Services.AddDbContext<ThreeMoronsContext>(o => o.UseSqlServer());

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddDatabaseDeveloperPageExceptionFilter();
}
var logger = new LoggerConfiguration().WriteTo.Console().CreateLogger();

builder.Logging.AddSerilog(logger);

builder.Services.AddScoped<IValidator<RegistrationInput>, RegistrationValidator>();
builder.Services.AddScoped<IValidator<AuthorizationInput>, AuthorizationValidator>();
builder.Services.AddScoped<IValidator<GroupInput>, GroupValidator>();
builder.Services.AddSingleton<PasswordMegaHasher>();
builder.Services.AddAuthentication(o =>
    {
        o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        o.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    }).AddJwtBearer(o =>
        {
            o.TokenValidationParameters = new TokenValidationParameters
            {
                ValidIssuer = builder.Configuration["Jwt:Issuer"],
                ValidAudience = builder.Configuration["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true
            };
        });
builder.Services.AddAuthorization();

var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.MapGet("/", () => "Этот материал создан лицом, которое признано иностранным агентом на терриотрии РФ");

app.MapPost("/register", [AllowAnonymous] async (IValidator<RegistrationInput> validator, RegistrationInput inp, ThreeMoronsContext db, PasswordMegaHasher pmh) =>
    {
        var valres = await validator.ValidateAsync(inp);
        if (!valres.IsValid)
        {
            return Results.ValidationProblem(valres.ToDictionary());
        }
        if (await db.Users.AnyAsync(x=> x.Login == inp.login))
        {
            return Results.Conflict("Пользователь с таким логином уже существует");
        }
        var HashingResult = pmh.HashPass(inp.password);
        try
        {
            User UserToRegister = new()
            {
                Id = Guid.NewGuid(),
                Login = inp.login,
                Password = HashingResult.hashpass,
                Salt = HashingResult.salt,
                Name = inp.name,
                Surname = inp.surname,
                Patronymic = inp.patronymic,
                UserClassId = inp.UserClassId
            };
            await db.Users.AddAsync(UserToRegister);
            await db.SaveChangesAsync();
            return Results.Created("",UserToRegister);
        }
        catch (Exception exc)
        {
            return TypedResults.BadRequest(exc.Message);
        }
    });
app.MapPost("/authorizeTest", [AllowAnonymous] async (IValidator<AuthorizationInput> Validator, AuthorizationInput inp, ThreeMoronsContext db, PasswordMegaHasher pmh) =>
    {
        var valres = await Validator.ValidateAsync(inp);
        if (!valres.IsValid)
        {
            return Results.ValidationProblem(valres.ToDictionary());
        }
        User authUser = await db.Users.FirstOrDefaultAsync(user => user.Login == inp.login);
        if (authUser is not null)
        {
            byte[] userSalt = authUser.Salt;
            var hashedPassword = pmh.HashPass(authUser.Password, userSalt);
            if (authUser.Password != hashedPassword)
            {
                return Results.Created(hashedPassword , authUser.Password + " " + authUser.Salt);
            }
            var stringToken = JwtIssuer.IssueJwtForUser(builder.Configuration, authUser);
            return Results.Ok(stringToken);

        }
        else
        {
            return Results.Problem("пользователь в целом конча");
        }
     
    });









var groupsGroup = app.MapGroup("/group").RequireAuthorization();

groupsGroup.MapGet("/all", async(ThreeMoronsContext db)=> await db.Groups.ToListAsync());
groupsGroup.MapGet("/", async ([FromQuery(Name = "groupName")] string Name, ThreeMoronsContext db) => await db.Groups.FirstOrDefaultAsync(x=> x.GroupName==Name));
groupsGroup.MapPost("/", async (ThreeMoronsContext db, GroupInput created, IValidator<GroupInput> validator) =>
    {
        var valres = await validator.ValidateAsync(created);
        if (!valres.IsValid)
        {
            return Results.ValidationProblem(valres.ToDictionary());
        }
        try
        {
            Group toCreate = new() { Building = created.Building, GroupCurator = created.groupCurator, GroupName = created.GroupName };
            await db.Groups.AddAsync(toCreate);
            await db.SaveChangesAsync();
            return Results.Created("group",toCreate);
        }
        catch (Exception excep)
        {
            return Results.Problem(excep.ToString());
        }
    }).RequireAuthorization(r=> r.RequireClaim("userClass", "2"));
//groupsGroup.MapPut("/", async (Group toUpdate, IValidator<GroupInput> validator, ThreeMoronsContext db) =>
//    {
//        var valres = await validator.ValidateAsync(toUpdate);
//        if (!valres.IsValid)
//        {
//            return Results.ValidationProblem(valres.ToDictionary());
//        }
//        try
//        {
//            var entity = await db.Groups.FindAsync(toUpdate.GroupName);
//            entity = toUpdate;
//            await db.SaveChangesAsync();
//            return Results.Ok("very good ok nice");
//        }
//        catch (Exception exc)
//        {
//            return Results.Problem(exc.ToString());
//        }
//    }).RequireAuthorization(r => r.RequireClaim("userClass", "2"));
groupsGroup.MapDelete("", async (ThreeMoronsContext db, [FromBody]Group toDelete) =>
    {
        try
        {
            db.Groups.Remove(toDelete);
            await db.SaveChangesAsync();
            return Results.Ok("deleted cool");
        }
        catch (Exception exc)
        {
            return Results.Problem(exc.ToString());
        }
    }).RequireAuthorization(r=> r.RequireClaim("userClass", "2"));

app.MapGet("/periods", async (ThreeMoronsContext db) => await db.Periods.ToListAsync());









var SkippedClassGroup = app.MapGroup("/skippedClass").RequireAuthorization(); //возможно добавлю валидацию

SkippedClassGroup.MapGet("", async (ThreeMoronsContext db)=> await db.SkippedClasses.ToListAsync());

SkippedClassGroup.MapGet("", async ([FromQuery(Name = "id")] Guid id, ThreeMoronsContext db) => await db.SkippedClasses.FirstOrDefaultAsync(x => x.Id == id));

SkippedClassGroup.MapPost("", async (SkippedClassInput input, ThreeMoronsContext db) =>
    {
        try
        {
            SkippedClass SkipToAdd = new() { Id = input.Id, ClassName = input.className, DateOfSkip = input.DateOfSkip, StudNumber = input.StudNumber };
            await db.AddAsync(SkipToAdd);
            await db.SaveChangesAsync();
            return Results.Created("/skippedClass", SkipToAdd);
        }
        catch (Exception exc)
        {
            return Results.Problem(exc.ToString());
        }
    });
SkippedClassGroup.MapDelete("", async([FromQuery] Guid id, ThreeMoronsContext db)=>
    {
        try
        {
            var toDelete = await db.SkippedClasses.FindAsync(id);
            db.SkippedClasses.Remove(toDelete);
            await db.SaveChangesAsync();
            return Results.Ok();
        }
        catch (Exception exc)
        {
            return Results.Problem(exc.ToString());
        }
    }).RequireAuthorization(o=> o.RequireClaim("userClassId", "2"));









var StudentGroup = app.MapGroup("/student").RequireAuthorization();

StudentGroup.MapGet("", async(ThreeMoronsContext db) => await db.Students.ToListAsync());
StudentGroup.MapGet("", async([FromQuery(Name="id")]string studId, ThreeMoronsContext db)=> await db.Students.FindAsync(studId));

StudentGroup.MapPost("", async(StudentInput inp, ThreeMoronsContext db)=>
    {
        try
        {
            Student StudentToCreate = new()
            {
                StudNumber = inp.StudNumber,
                GroupName = inp.GroupName,
                Name = inp.Name,
                Surname = inp.Surname,
                Patronymic = inp.Patronymic,
                PhoneNumber = inp.PhoneNumber,
            };
            await db.Students.AddAsync(StudentToCreate);
            await db.SaveChangesAsync();
            return Results.Created("/student", StudentToCreate);
        }
        catch (Exception exc)
        {
            return Results.Problem(exc.ToString());
        }
    });
StudentGroup.MapPut("", async(StudentInput inp, ThreeMoronsContext db)=>
    {
        try //12 часов ночи, я знаю, что можно сделать элегантнее. оставлю так до первого рефакторинга
        {
            var StudentToUpdate = await db.Students.FindAsync(inp.StudNumber);
            StudentToUpdate.Name = inp.Name;
            StudentToUpdate.Surname = inp.Surname;
            StudentToUpdate.PhoneNumber = inp.PhoneNumber;
            db.Students.Update(StudentToUpdate);
            await db.SaveChangesAsync();
            return Results.Ok();
        }
        catch (Exception exc)
        {
            return Results.Problem(exc.ToString());
        }
    });
//ОТЧИСЛЯЕМ ПИДОРАСА
StudentGroup.MapDelete("", async([FromQuery(Name ="studNumber")] string StudNumber, ThreeMoronsContext db)=> 
    {
        try
        {
            //ТЕБЕ НЕ СБЕЖАТЬ
            var StudentToDelete = await db.Students.FindAsync(StudNumber);
            //ПОЛУУЧАЙ СУКА
            db.Students.Remove(StudentToDelete);

            await db.SaveChangesAsync();
            return Results.Ok(); //ВСЁ ПРОСТО ЗАЕБИСЬ ОК
        }
        catch (Exception exc)
        {
            return Results.Problem(exc.ToString()); //ахуеть мы даже отчислить человека не можем нормально
        }
        
    });









var StudentDelayGroup = app.MapGroup("/studentDelay");
app.UseAuthentication();
app.UseAuthorization();

app.Run();
