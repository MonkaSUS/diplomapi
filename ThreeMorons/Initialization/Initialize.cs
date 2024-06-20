
using Serilog.Core;
using System.Text.Json;
using ThreeMorons.DTOs;
using ThreeMorons.Security;

namespace ThreeMorons.Initialization
{
    public static partial class Initializer
    {

        private static string DbServiceHostAdress = "http://192.168.29.2:8000";
        /// <summary>
        /// Дефолтные настройки сериализации, которые используются и на клиенте, и на сервере.
        /// </summary>
        public static JsonSerializerOptions _opt = new JsonSerializerOptions()
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
            var logger = new LoggerConfiguration()
                .WriteTo.File("apilogs.txt", rollingInterval: RollingInterval.Day,
                flushToDiskInterval: TimeSpan.FromSeconds(10), shared: true, encoding: Encoding.UTF8)
                .WriteTo.Console().CreateLogger();
            builder.Logging.AddSerilog(logger);
            builder.Services.AddDbContext<ThreeMoronsContext>(o => o.UseSqlServer(), ServiceLifetime.Scoped);

            if (builder.Environment.IsDevelopment())
            {
                builder.Services.AddDatabaseDeveloperPageExceptionFilter();
            }

            //builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            builder.Services.AddScoped<IValidator<RegistrationInput>, RegistrationValidator>();
            builder.Services.AddScoped<IValidator<AuthorizationInput>, AuthorizationValidator>();
            builder.Services.AddScoped<IValidator<GroupInput>, GroupValidator>();
            builder.Services.AddScoped<IValidator<StudentDelayInput>, StudentDelayValidator>();
            builder.Services.AddScoped<IValidator<AnnouncementDTO>, AnnouncementValidator>();
            builder.Services.AddScoped<IValidator<PasswordRefreshDTO>, PasswordRefreshValidator>();

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
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)), //ключ шифрования
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true
                };
                o.Events = new CustomJwtBearerEvents(builder.Services.BuildServiceProvider().GetRequiredService<ThreeMoronsContext>());
            });
            builder.Services.AddAuthorization();
            return builder.Build();
        }
    }
}
