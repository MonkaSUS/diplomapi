namespace ThreeMorons.Initialization
{
    public partial class Initializer
    {
        public void MapStatistics(WebApplication app)
        {
            var StatsGroup = app.MapGroup("/stats");
            
        }
    }
}
