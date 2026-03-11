namespace PayRexApplication.DTOs
{
    public class AttendanceStatsDto
    {
        public int Present { get; set; }
        public int Absent { get; set; }
        public int Late { get; set; }
        public int OnLeave { get; set; }
        public int Holidays { get; set; }
        public int TotalEmployees { get; set; }
    }
}
