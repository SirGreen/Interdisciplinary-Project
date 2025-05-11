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

        // public GearMaterial vatLieuBoTruyen { get; set; }
        // public StressInput dauVaoUngSuat { get; set; }
        public Dictionary<string, Dictionary<string, object>> vatLieuBoTruyen { get; set; }
        public Dictionary<string, object> dauVaoUngSuat { get; set; }
        public double ungSuatTiepXucChoPhep { get; set; }
        public double pitch { get; set; }
        public double shaftDistance { get; set; }
        public bool chainSafe { get; set; }
        public int contactStrength { get; set; }
        public double shaftForce { get; set; }
        public string diskDiameterCalc { get; set; }
    }

    // public class GearMaterial
    // {
    //     public Gear BanhNho { get; set; }  // "Bánh nhỏ"
    //     public Gear BanhLon { get; set; }  // "Bánh lớn"

    //     public class Gear
    //     {
    //         public string VatLieu { get; set; }  // "Vật liệu"
    //         public int Ob { get; set; }
    //         public int Och { get; set; }
    //         public string HB { get; set; }
    //         public string KichThuocS { get; set; }  // "Kích thước S"
    //         public string KichThuoc { get; set; }  // "Kích thước"
    //     }
    // }

    // public class StressInput
    // {
    //     public double Ohlim1 { get; set; }
    //     public double Ohlim2 { get; set; }
    //     public double Sh { get; set; }
    //     public double Oflim1 { get; set; }
    //     public double Oflim2 { get; set; }
    //     public double Sf { get; set; }
    //     public int HB1 { get; set; }
    //     public int HB2 { get; set; }
    //     public double Nho1 { get; set; }
    //     public double Nho2 { get; set; }
    //     public double Nfo { get; set; }
    // }

}
