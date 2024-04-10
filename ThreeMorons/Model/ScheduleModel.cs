namespace ThreeMorons.Model
{

    public record Para()
    {
        public int classNumber { get; set; }
        public string[] NameOfClasses { get; set; }
        public string[] TeacherNames { get; set; }
        public int[] ClassroomNumbers { get; set; }
    }
    public record DayOfWeek()
    {
        public List<Para> spisokPar { get; set; } = new List<Para>(7);
        public DateOnly date { get; set; }
    }
    public record ScheduleOfWeek()
    {
        public List<DayOfWeek> days { get; set; } = new List<DayOfWeek>(5);
    }
}
