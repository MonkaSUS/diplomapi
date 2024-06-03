using EasyCaching.Core;
using FirebaseAdmin.Messaging;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using ThreeMorons.DTOs;
using ThreeMorons.Services;

namespace ThreeMorons.Initialization
{
    public partial class Initializer
    {
        public static void MapSpecialEndpoints(WebApplication app)
        {
            app.MapPost("/announcement", async (ThreeMoronsContext db, AnnouncementDTO annc, IValidator<AnnouncementDTO> val, INotificationService notifs, ILoggerFactory fac) =>
            {
                var logger = fac.CreateLogger("announcement");
                logger.LogInformation("Попытка создать и отправить оповещение");
                var valres = val.Validate(annc);
                if (!valres.IsValid)
                {
                    return Results.ValidationProblem((IDictionary<string, string[]>)valres.Errors);
                }
                Announcement newAnnc = new Announcement()
                {
                    Author = annc.author,
                    Body = annc.body,
                    CreatedOn = DateTime.Now,
                    ShortDescription = annc.shortDescription,
                    TargetAudience = annc.targetAudience,
                    Title = annc.title,
                    Id = Guid.NewGuid()
                };
                //если у оповещалки нет краткого описания, то делаем дефолтное, иначе ставим его
                string NotificationBody = "";
                if (string.IsNullOrEmpty(newAnnc.ShortDescription))
                {
                    NotificationBody = newAnnc.Body;
                }
                else
                {
                    NotificationBody = newAnnc.ShortDescription;
                }
                //в фаербейзе смешно работают темы уведомлений
                //если попытаться отправить сообщение на несуществующую тему, то фб его создаст
                //но этот процесс может занять несколько дней
                //планирую побить темы уведомлений по группам.
                string notifTopic = "";
                if (string.IsNullOrEmpty(newAnnc.TargetAudience))
                {
                    notifTopic = "announcements";
                }
                else
                {
                    notifTopic = newAnnc.TargetAudience;
                }
                Message fcmMessage = new Message()
                {
                    Notification = new Notification()
                    {
                        Title = newAnnc.Title,
                        Body = NotificationBody
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
                    Topic = notifTopic
                };
                logger.LogInformation($"Создал оповещение {newAnnc.Id}. Попытка сохранить в бд");
                try
                {
                    await db.Announcements.AddAsync(newAnnc);
                    await db.SaveChangesAsync();
                    logger.LogInformation($"Успешно сохранил увдомление {newAnnc.Id} в базу");
                }
                catch (Exception exc)
                {
                    logger.LogError(exc, "произошёл хуяк при сохранении оповещения");
                }
                string result = await notifs.SendAsync(fcmMessage);
                logger.LogInformation($"Отправил уведомление {result} об оповещении {newAnnc.Id} пользователям");
                return Results.Ok(newAnnc.Id);
            }).RequireAuthorization(r => r.RequireClaim("userClass", ["2", "3"]));
            app.MapGet("/announcement", async (IWebHostEnvironment env, ThreeMoronsContext db, IEasyCachingProvider prov) =>
            {
                if (await prov.ExistsAsync("allAnnouncements"))
                {
                    var cachedres = await prov.GetAsync<List<Announcement>>("allAnnouncements");
                    return Results.Json(cachedres, _opt);
                }
                var allAnnouncements = await db.Announcements.ToListAsync();
                await prov.SetAsync<List<Announcement>>(nameof(allAnnouncements), allAnnouncements, TimeSpan.FromMinutes(10));

                return Results.Json(allAnnouncements, options: _opt, contentType: "application/json", statusCode: 200);
            }).RequireAuthorization(r => r.RequireClaim("userClass", ["2", "3"]));
            //ДАЛЬШЕ ДУМАТЬ О РАЗДЕЛЕНИИ ПО ГРУППАМ
            //РАЗДЕЛЕНИЕ ПО ГРУППАМ БУДЕМ ДЕЛАТЬ НА КЛИЕНТЕ. 
            //ПРОТЕСТИТЬ
            app.MapPost("/createDbUser", async (ThreeMoronsContext db, [FromBody] DbServiceUser user, ILoggerFactory fac) =>
            {
                var logger = fac.CreateLogger("DbServiceUser");
                try
                {
                    user.id = Guid.NewGuid();
                    db.DbServiceUsers.Add(user);
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
                    var fullUser = await db.DbServiceUsers.FirstOrDefaultAsync(x => x.user_login == user.user_login && x.user_password == user.user_password && x.db_type == user.db_type);
                    fullUser.db_name = user.db_name;
                    var client = clientFac.CreateClient();
                    ConnectionStringDTO csd = new()
                    {
                        data = new()
                        {
                            { "account_login", fullUser.user_login },
                            { "account_password", fullUser.user_password },
                            { "user_telegram_id", fullUser.telegram_id },
                            { "database_name", fullUser.db_name }
                        },
                        dbms_name = fullUser.db_type
                    };
                    var res = await client.PostAsJsonAsync<ConnectionStringDTO>($"{DbServiceHostAdress}/database/get-connection-string", csd);
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
            app.MapGet("/createBackup", async (ThreeMoronsContext db, IWebHostEnvironment env, ILoggerFactory fac) =>
            {
                var logger = fac.CreateLogger("backup");
                try
                {
                    // Получение имени базы данных
                    var databaseName = db.Database.GetDbConnection().Database;

                    // Формирование имени файла бэкапа
                    var backupFileName = $"backup_{databaseName}_{DateTime.Now:yyyyMMdd_HHmmss}.bak";
                    var backupFilePath = Path.Combine(Environment.CurrentDirectory, backupFileName);

                    // Создание SQL-запроса для создания бэкапа
                    var backupQuery = $"BACKUP DATABASE [{databaseName}] TO DISK = '{backupFilePath}' WITH FORMAT";

                    // Выполнение SQL-запроса с использованием Raw SQL
                    await db.Database.ExecuteSqlRawAsync(backupQuery);

                    // Возвращение успешного ответа
                    logger.LogInformation($"Создан бекап базы данных {DateTime.Now}");
                    return Results.Ok($"Бэкап базы данных {databaseName} успешно создан: {backupFilePath}");
                }
                catch (Exception ex)
                {
                    // Обработка исключения
                    logger.LogError(ex, "пиздык");
                    return Results.Problem(statusCode: 500, detail: ex.Message);
                }
            });
        }
    }
}
