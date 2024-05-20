using EasyCaching.Core;
using FirebaseAdmin.Messaging;
using Microsoft.AspNetCore.Components.Forms;
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
        }
    }
}
