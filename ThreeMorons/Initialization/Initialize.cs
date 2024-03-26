
using System.Text.Json;

namespace ThreeMorons.Initialization
{
    public static partial class Initializer
    {
        private static JsonSerializerOptions _opt = new JsonSerializerOptions()
        {
            IncludeFields = true,
            AllowTrailingCommas = false,
            UnmappedMemberHandling = JsonUnmappedMemberHandling.Skip,
            ReadCommentHandling = JsonCommentHandling.Skip,
            IgnoreReadOnlyFields = false,
            IgnoreReadOnlyProperties = false,
            DictionaryKeyPolicy = JsonNamingPolicy.CamelCase
        };
        public static WebApplication Initialize(WebApplicationBuilder builder)
        {
            builder.Logging.ClearProviders();

            builder.Services.AddDbContext<ThreeMoronsContext>(o => o.UseSqlServer());

            if (builder.Environment.IsDevelopment())
            {
                builder.Services.AddDatabaseDeveloperPageExceptionFilter();
            }
            var logger = new LoggerConfiguration().WriteTo.Console().CreateLogger();

            builder.Logging.AddSerilog(logger);
            //builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            builder.Services.AddScoped<IValidator<RegistrationInput>, RegistrationValidator>();
            builder.Services.AddScoped<IValidator<AuthorizationInput>, AuthorizationValidator>();
            builder.Services.AddScoped<IValidator<GroupInput>, GroupValidator>();
            builder.Services.AddScoped<IValidator<StudentDelayInput>, StudentDelayValidator>();
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
            return builder.Build();
        }
    }
}
