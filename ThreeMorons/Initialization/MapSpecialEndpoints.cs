using FirebaseAdmin.Messaging;
using System.Text.Json;
using ThreeMorons.DTOs;
using ThreeMorons.Services;

namespace ThreeMorons.Initialization
{
    public partial class Initializer
    {
        public static void MapSpecialEndpoints(WebApplication app)
        {
            app.MapPost("/announcement", async (AnnouncementDTO annc, IValidator<AnnouncementDTO> val, IWebHostEnvironment env, INotificationService notifs, ILoggerFactory fac) =>
            {
                var logger = fac.CreateLogger("announcement");
                logger.LogInformation("Попытка создать и отправить оповещение");
                var localPath = env.ContentRootPath;
                var valres = val.Validate(annc);
                if (!valres.IsValid)
                {
                    return Results.ValidationProblem((IDictionary<string, string[]>)valres.Errors);
                }
                if (!Directory.Exists(localPath + "/announcements"))
                {
                    Directory.CreateDirectory(localPath + "/announcements");
                }
                PublicAnnouncement announcement = new PublicAnnouncement()
                {
                    Id = Guid.NewGuid(),
                    Author = annc.author,
                    Body = annc.body,
                    Description = annc.description,
                    Title = annc.title,
                    Created = annc.created
                };
                var serializedAnnouncement = JsonSerializer.Serialize(announcement);
                string FileName = announcement.Created.ToShortDateString() + announcement.Title;
                using (FileStream fs = File.Create(localPath + "/announcements/" + FileName))
                {
                    byte[] jsondata = new UTF8Encoding(true).GetBytes(serializedAnnouncement);
                    fs.Write(jsondata, 0, jsondata.Length);
                }
                logger.LogInformation($"Создал и сериализовал оповещение {announcement.Id}");

                Message fcmMessage = new Message()
                {
                    Notification = new Notification()
                    {
                        Title = announcement.Title,
                        Body = announcement.Body
                    },
                    Android = new AndroidConfig()
                    {
                        Notification = new AndroidNotification()
                        {
                            ChannelId = "public_announcements"
                        },
                        Priority = Priority.High,
                        RestrictedPackageName = "com.kgpk.collegeapp"
                    },
                    Topic = "announcements"
                };
                string result = await notifs.SendAsync(fcmMessage);
                logger.LogInformation($"Отправил уведомление {result} пользователям");
                return Results.Ok(result);
            }).RequireAuthorization(r => r.RequireClaim("userClass", "2"));
            app.MapGet("/announcement", async (IWebHostEnvironment env) =>
            {
                List<PublicAnnouncement> allAnnc = new List<PublicAnnouncement>();
                var filenames = Directory.GetFiles(env.ContentRootPath + "/announcements");
                foreach (var file in filenames)
                {
                    var annc = JsonSerializer.Deserialize<PublicAnnouncement>(File.ReadAllText(file));
                    allAnnc.Add(annc);
                }
                return Results.Json(allAnnc, _opt, contentType: "application/json", statusCode: 200);
            });
            app.MapGet("/schedule", async ([FromQuery(Name = "forGroup")] string forGroup, IHttpClientFactory clientFactory) =>
            {
                var client = clientFactory.CreateClient();
                var res = await client.GetAsync($"kgpkschedule.somee.com/schedule?forGroup={forGroup}");
                if (res.IsSuccessStatusCode)
                {
                    return Results.Ok(await res.Content.ReadAsStringAsync());
                }
                else
                {
                    return Results.Problem(statusCode: (int)res.StatusCode, detail: await res.Content.ReadAsStringAsync());
                }
            });
            app.MapPost("/createDbUser", async (ThreeMoronsContext db, [FromBody] DbServiceUser user, ILoggerFactory fac) =>
            {
                var logger = fac.CreateLogger("DbServiceUser");
                try
                {
                    db.DbServiceUser.Add(user);
                    await db.SaveChangesAsync();
                    logger.LogInformation($"Создан новый пользователь бд сервиса {user.user_login}");
                    return Results.Ok();
                }
                catch (Exception exc)
                {
                    logger.LogError(exc, " отстрел произошёл");
                    throw;
                }
            });
            app.MapPost("/getDbConnectionString", async (ThreeMoronsContext db, [FromBody] DbServiceUserDTO user, IHttpClientFactory clientFac, ILoggerFactory loggerFac) =>
            {
                var logger = loggerFac.CreateLogger("GetDbConnectionString");
                try
                {
                    var fullUser = await db.DbServiceUser.FirstOrDefaultAsync(x => x.user_login == user.user_login && x.user_password == user.user_password);
                    var client = clientFac.CreateClient();
                    Dictionary<string, string> fullUserDict = new()
                    {
                        { "user_login", fullUser.user_login },
                        { "user_password", fullUser.user_password },
                        { "user_telegram_id", fullUser.telegram_id },
                        { "database_name", user.db_name },
                        {"db_type", fullUser.db_type }
                    };
                    var sendContent = new StringContent(JsonSerializer.Serialize(fullUserDict, _opt), Encoding.UTF8, "application/json");
                    logger.LogInformation($"Найден пользователь бд сервиса {fullUser}");
                    HttpResponseMessage res = new();
                    logger.LogInformation($"Попытка найти бд {fullUser.db_name}");
                    switch (fullUser.db_type)
                    {
                        case PostgreSqlName:
                            res = await client.PostAsync($"{DbServiceHostAdress}postgresql/database/get-connection-string", sendContent);
                            break;
                        case MariaDbName:
                            res = await client.PostAsync($"{DbServiceHostAdress}mariadb/database/get-connection-string", sendContent);
                            break;
                        case MsSqlName:
                            res = await client.PostAsync($"{DbServiceHostAdress}mssql/database/get-connection-string", sendContent);
                            break;
                        default:
                            return Results.Problem("название бд не было найдено");
                    }
                    if (!res.IsSuccessStatusCode)
                    {
                        logger.LogError($"{res.StatusCode} при попытке получить строку подключения. {await res.Content.ReadAsStringAsync()}");
                        return Results.Problem(statusCode: (int)res.StatusCode, detail: await res.Content.ReadAsStringAsync());
                    }
                    string connectionString = await res.Content.ReadAsStringAsync();
                    logger.LogInformation($"{connectionString} получена успешно");
                    return Results.Ok(connectionString);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "случился отстрел");
                    throw;
                }
            });
            app.MapPost("/changeDbAdress", async (string hostadress) =>
            {
                Initializer.DbServiceHostAdress = hostadress;
                return Results.Ok();
            });
        }
    }
}
