using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;

namespace ThreeMorons.Services
{
    public class FcmNotificationService : INotificationService
    {
        private readonly string _configpath;
        public async Task<string> SendAsync(Message message)
        {
            FirebaseApp firebaseApp = null;
            try
            {
                firebaseApp = FirebaseApp.Create(new AppOptions()
                {
                    Credential = GoogleCredential.FromFile(_configpath),
                    ProjectId = "kgpknotif"
                }, "Kgpknotif"); ;
            }
            catch (Exception)
            {
                firebaseApp = FirebaseApp.GetInstance("Kgpknotif");
            }
            var fcm = FirebaseMessaging.GetMessaging(firebaseApp);
            string result = await fcm.SendAsync(message);
            return result;
        }
        public FcmNotificationService(IWebHostEnvironment env) 
        {
            _configpath = env.ContentRootPath + "\\GoogleAuth.json";
        }
    }
}
