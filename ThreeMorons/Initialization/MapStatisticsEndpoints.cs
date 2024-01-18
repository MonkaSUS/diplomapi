namespace ThreeMorons.Initialization
{
    public partial class Initializer
    {
        public static void MapStatistics(WebApplication app)
        {
            var StatsGroup = app.MapGroup("/stats");


            StatsGroup.MapGet("/poorAttendanceStudents", async (ThreeMoronsContext db, [FromQuery(Name="count")]int count)=>
                {
                    
                });
        }
    }
}
