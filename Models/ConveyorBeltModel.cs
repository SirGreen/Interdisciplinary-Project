using System.Collections.Generic;

namespace DADN.Models
{
    public class ConveyorBeltModel
    {
        public double Force { get; set; } // Lực vòng trên băng tải F(N)
        public double Velocity { get; set; } // Vận tốc băng tải v(m/s)
        public required string MachineType { get; set; } // Loại trục máy công tác

        // Nếu là Trục Tang Quay
        public double? DrumDiameter { get; set; } // Đường kính tang D(mm)

        // Nếu là Đĩa Xích Tải
        public int? ChainTeeth { get; set; } // Số răng đĩa xích tải
        public double? ChainPitch { get; set; } // Bước xích của xích tải t(mm)

        // Thời gian phục vụ
        public int ServiceYears { get; set; }
        public int WorkingDaysPerYear { get; set; }
        public int WorkingHoursPerDay { get; set; }

        // Chế độ tải
        public List<LoadCondition> LoadConditions { get; set; } = new();

        // Hệ số quá tải (Optional)
        public double? StartTorque { get; set; } // Mômen mở máy
        public double? LoadTorque { get; set; } // Mômen tải
    }

    public class LoadCondition
    {
        public double Load { get; set; } // Tải Tn
        public double Duration { get; set; } // Thời gian tn
    }
}
