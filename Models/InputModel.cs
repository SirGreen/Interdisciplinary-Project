namespace DADN.Models
{
    public class LoadData
    {
        public int Id { get; set; }
        public double T { get; set; }
        public double t { get; set; }
    }

    public class InputModel
    {
        public double Force { get; set; }
        public double Speed { get; set; }
        public required string ShaftType { get; set; }
        public double Diameter { get; set; }
        public int ServiceTime { get; set; }
        public int WorkDays { get; set; }
        public int WorkHours { get; set; }
        public double StartupMoment { get; set; }
        public double LoadMoment { get; set; }
        public double OverloadFactor { get; set; }
        public double OverallEfficiency { get; set; }
        public double RequiredMotorEfficiency { get; set; }
        public List<LoadData> LoadData { get; set; } = new List<LoadData>();
    }
}
