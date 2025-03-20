// đoạn này là structure của mấy bộ truyền dẫn ngoài, tới class Design ở dưới là tính hộp giảm tốc
public interface ITransmissionCalculation
{
    double CalculateEfficiency();
    double CalculateTransmissionRatio();
}

public class BeltTransmission : ITransmissionCalculation
{
    public double CalculateEfficiency()
    {
        // Tính hiệu suất bộ truyền đai
        return 0.95; // Ví dụ
    }

    public double CalculateTransmissionRatio()
    {
        // Tính tỉ số truyền bộ truyền đai
        return 2.0; // Ví dụ
    }
}

public class ChainTransmission : ITransmissionCalculation
{
    public double CalculateEfficiency()
    {
        // Tính hiệu suất bộ truyền xích
        return 0.92; // Ví dụ
    }

    public double CalculateTransmissionRatio()
    {
        // Tính tỉ số truyền bộ truyền xích
        return 2.56; // Ví dụ
    }
}

public class GearTransmission : ITransmissionCalculation
{
    public double CalculateEfficiency()
    {
        // Tính hiệu suất bộ truyền bánh răng
        return 0.98; // Ví dụ
    }

    public double CalculateTransmissionRatio()
    {
        // Tính tỉ số truyền bộ truyền bánh răng
        return 3.0; // Ví dụ
    }
}

public class GearboxFactory
{
    public static ITransmissionCalculation CreateTransmission(string type)
    {
        switch (type.ToLower())
        {
            case "belt":
                return new BeltTransmission();
            case "chain":
                return new ChainTransmission();
            case "gear":
                return new GearTransmission();
            default:
                throw new ArgumentException("Invalid transmission type");
        }
    }
}

public class GearboxDesign
{
    private ITransmissionCalculation _transmission;
    private double _force; // Lực vòng trên băng tải (N)
    private double _velocity; // Vận tốc băng tải (m/s)
    private double _diameter; // Đường kính tang (mm)
    private double _serviceLife; // Thời gian phục vụ (năm)
    private double _loadT1; // Hệ số quá tải
    private double _loadT2; // Hệ số quá tải

    public GearboxDesign(ITransmissionCalculation transmission, double force, double velocity, double diameter, double serviceLife, double loadT1,double loadT2)
    {
        _transmission = transmission;
        _force = force;
        _velocity = velocity;
        _diameter = diameter;
        _serviceLife = serviceLife;
        _loadT1 = loadT1;
        _loadT2 = loadT2;
    }

    public void Calculate()
    {
        // B3: Tính hiệu suất chung của hệ thống
        double engine_efficiency = CalculateEfficiency();

        // B4: Xác định công suất cần thiết cho động cơ
        double power = CalculateRequiredPower(engine_efficiency);

        // B5: Xác định tốc độ quay cần thiết của động cơ
        double rotationSpeed = CalculateRequiredRotationSpeed();

        // B6: Tính tỉ số truyền
        double transmissionRatio = CalculateTransmissionRatio(rotationSpeed);

        // B7: Chọn động cơ phù hợp từ catalog (giả lập)
        double engine = SelectEngine(power, transmissionRatio);

        // B8: Xác định tỷ số truyền của hệ dẫn động
        double totalTransmissionRatio = CalculateTotalTransmissionRatio(transmissionRatio,engine);

        // B9: Tính công suất, momen và số vòng quay trên các trục
        CalculateShaftParameters(power, rotationSpeed, totalTransmissionRatio);
    }

    private double CalculateEfficiency(double n1 = 0, double n2 = 0, double n3 = 0, double n4 = 0)
    {
        // Tính công suất (đang return cứng luôn)
        return 0.85676;
    }

    private double CalculateRequiredPower(double efficiency)
    {
        // Tính công suất cần thiết
        double power_lever = _force * _velocity / 1000;
        // tui đang cho T1/T với T2/T theo cái excel, ko bt có cần input cái đó ko
        double power_approx = power_lever * Math.Sqrt((_loadT1 + _loadT2 * 0.8*0.8 )/(_loadT1 + _loadT2));
        double power = power_approx / efficiency;
        return power;
    }

// nếu cho chọn nhiều loại trục thì thêm 1 arg để phân biệt, giống cái transmission ở đầu
    private double CalculateRequiredRotationSpeed()
    {
        // Tính tốc độ quay cần thiết
        double rotationSpeed = (60000 * _velocity) / (Math.PI * _diameter);
        return rotationSpeed;
    }

// theo tui hiểu thì sau sẽ cần thêm input loại thiết kế để biết ux, uh?
    private double CalculateTransmissionRatio(double rotationSpeed)
    {
        double Ux = 2.56; double Uh = 18;
        double Usb = Ux * Uh; 
        // Tính tổng tỉ số truyền
        return rotationSpeed * Usb;
    }

    // function chọn động cơ, hiện đang in với return max sai số, sau chắc return nguyên object engine
    private double SelectEngine(double power, double transmissionRatio)
    {
        // Giả lập việc chọn động cơ từ catalog
        Console.WriteLine($"Selected Motor: Power > {power} kW, Rotation Speed in [{transmissionRatio * 0.96}, {transmissionRatio * 1.04} ] rpm");
        Console.WriteLine("T");
        return transmissionRatio * 1.04;
    }

// chắc cũng cần arg loại thiết kế
    private double CalculateTotalTransmissionRatio(double transmissionRatio,double engine_speed)
    {
        double Ut = engine_speed / transmissionRatio; 
        // Còn tính 3 cái U bằng tra bảng mà ko rõ cách làm ở đây lắm

        return Ut;
    }

    private void CalculateShaftParameters(double power, double rotationSpeed, double totalTransmissionRatio)
    {   
        // cái này thông số bên hiệu suất, mà lười tra bảng quá nên lấy của file excel :))
        double Nol = power / 0.993, Nx = 0.91, Nbr = 0.97, Nk = 1;
        // ko bt lấy đâu, mà dự là sẽ truyền qua cái totalTransmissionRatio kia
        double Uk = 1, U1 = 1, U2 = 1, Ux = 1;
        // Tính toán các thông số trên các trục
        // công suất
        double Power3 = power / (Nol * Nx);
        double Power2 = Power3 / (Nol * Nbr);
        double Power1 = Power2 / (Nol * Nbr);
        double PowerEngine = Power1 / (Nol * Nk);
        // vòng quay
        double shaft1Speed = rotationSpeed / Uk;
        double shaft2Speed = shaft1Speed / U1;
        double shaft3Speed = shaft2Speed / U2;
        double shaftFinal = shaft3Speed / Ux;
        // momen
        double TorqueCT = 9.55 * Math.Pow(10,6) * power / shaftFinal;
        double shaft3Torque = 9.55 * Math.Pow(10,6) * Power3 / shaft3Speed;
        double shaft2Torque = 9.55 * Math.Pow(10,6) * Power2 / shaft2Speed;
        double shaft1Torque = 9.55 * Math.Pow(10,6) * Power1 / shaft1Speed;
        double shaftEngine = 9.55 * Math.Pow(10,6) * PowerEngine / rotationSpeed;

        Console.WriteLine($"Shaft 1: Power = {PowerEngine} kW, Speed = {shaftFinal} rpm, Torque = {shaftEngine} N.mm");
    }
}

// code chạy mẫu
// class Program
// {
//     static void Main(string[] args)
//     {
//         // Tạo bộ truyền xích bằng Factory Pattern
//         ITransmissionCalculation transmission = GearboxFactory.CreateTransmission("chain");

//         // Tạo đối tượng thiết kế hộp giảm tốc
//         GearboxDesign gearboxDesign = new GearboxDesign(transmission, 6000, 1.5, 650, 10, 45, 30);

//         // Thực hiện tính toán
//         gearboxDesign.Calculate();
//     }
// }