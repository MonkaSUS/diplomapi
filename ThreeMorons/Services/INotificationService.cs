using FirebaseAdmin.Messaging;

namespace ThreeMorons.Services
{
    public interface INotificationService
    {
        public Task<string> SendAsync(Message message);
    }
}
