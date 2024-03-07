using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace ThreeMorons.HealthCheck
{
    public class SampleHealthCheck : IHealthCheck
    {
        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            bool IsHealthy = true;
            ThreeMoronsContext db = new();
            string message = String.Empty;
            if (!(db.Database.CanConnect() && db.Database.EnsureCreated())) 
            {
                message += "База мертва!";
            }
            if (message != string.Empty)
            {
                return Task.FromResult(HealthCheckResult.Unhealthy(message));
            }
            return Task.FromResult(HealthCheckResult.Healthy());
        }
    }
}
