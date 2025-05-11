namespace DADN.Models
{
    public class LoadData
    {
        public int Id { get; set; }
        public double Torch { get; set; }
        public double time { get; set; }
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

    public class PartialInputModel
    {
        public string field { get; set; }
        public double value { get; set; }

        public Dictionary<string, double> existingData { get; set; }
    }

    // Model nhận dữ liệu từ request
    public class CalGearRequestModel
    {
        public string transType { get; set; }
        public double force { get; set; }
        public double speed { get; set; }
        public double diameter { get; set; }
        public double serviceTime { get; set; }
        public double loadN { get; set; }
        public double[] Torchlist { get; set; }
        public double[] tlist { get; set; }
    }

    public class MomenKetQua
    {
        public double N1 { get; set; }
        public double U1 { get; set; }  // nếu có tính
        public double T1 { get; set; }

        public double N2 { get; set; }
        public double U2 { get; set; }  // nếu có tính
        public double T2 { get; set; }

        public string MoTa { get; set; }  // nếu vẫn muốn giữ chuỗi mô tả
    }

    public class TechnicalData
    {
        public double OverloadFactor { get; set; }
        public double OverallEfficiency { get; set; }
        public double RequiredMotorEfficiency { get; set; }
        public double RequiredMotorSpeed { get; set; }
        public double NsbSpeed { get; set; }
        public double Un { get; set; }
        public string MomenSoVongQuay { get; set; }
        public MotorCatalog Motor { get; set; }

        public class GearInputModel
        {
            public double Lh { get; set; }
            public double n1 { get; set; }
            public double u1 { get; set; }
            public double T1 { get; set; }
            public double n2 { get; set; }
            public double u2 { get; set; }
            public double T2 { get; set; }
        }

    }
