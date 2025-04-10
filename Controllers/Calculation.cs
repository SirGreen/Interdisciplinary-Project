public class GearboxDesign
{
    private double LucF; // Lực vòng trên băng tải (N)
    private double VantocV; // Vận tốc băng tải (m/s)
    private double DuongkinhD; // Đường kính tang (mm)
    private double ThoigianL; // Thời gian phục vụ (năm)
    private double loadN; // Hệ số quá tải
    private double[] Torchlist; // Hệ số quá tải
    private double[] tlist; // Hệ số quá tải

    public GearboxDesign(double force, double velocity, double diameter, double serviceLife, double load, double[] Tlst, double[] tlst)
    {
        LucF = force;
        VantocV = velocity;
        DuongkinhD = diameter;
        ThoigianL = serviceLife;
        loadN = load;
        Torchlist = Tlst;
        tlist = tlst;
    }

    // B3
    private double TinhHieuSuatChungN(double Nx = 0, double Nbr = 0, double Nol = 0, double Nkn = 0)
    {
        // Tính công suất (đang return cứng luôn)
        return 0.85676;
    }

    private double TinhHeSoQuaTai()
    {
        // Tránh chia cho 0
        return loadN;
    }


    // B4
    private double TinhCongSuatCanThietPct(double N)
    {
        double Plv = LucF * VantocV / 1000;
        double sum1 = 0; double sum2 = 0;
        int length = Math.Min(tlist.Length, Torchlist.Length);
        for (int i = 0; i < length; i++)
        {
            sum1 += tlist[i] * Torchlist[i] * Torchlist[i];
            sum2 += tlist[i];
        }

        double Ptd = Plv * (sum1 / sum2);
        double Pct = Ptd / N;
        return Pct; //(kW)
    }

    // B5
    // nếu cho chọn nhiều loại trục thì thêm 1 arg để phân biệt
    private double TinhTocDoQuayCanThietNlv()
    {
        double Nlv = 60000 * VantocV / (Math.PI * DuongkinhD);
        return Nlv;
    }

    // B6
    private double TinhTocDoSoBoNsb(double Nlv)
    {
        double Ux = 2.56; double Uh = 18;
        double Usb = Ux * Uh;
        // Tính tổng tỉ số truyền
        return Nlv * Usb;
    }

    // B7
    // function chọn động cơ, hiện đang in với return max sai số tại chưa có cấu trúc return
    private double ChonDongCo(double Nsb, double Pct)
    {
        // Giả lập việc chọn động cơ từ catalog
        Console.WriteLine($"Selected Motor: CongSuat >= {Pct} kW, VanToc khoang [{Nsb * 0.96}, {Nsb * 1.04} ] vg/gi");

        return Nsb * 1.04;
    }

    // B8
    private double TinhTiSoTruyenUn(double Ndc, double Nlv)
    {
        double Ut = Ndc / Nlv;
        // cần array 2.4
        double Un = 1;
        double Uh = Ut / Un;
        // cần array 3.1
        double u1 = 1; double u2 = 1;
        Un = Ut / (u1 * u2);

        return Un;
    }

    // B9
    // return array hay gì đó để lấy hết
    private string TinhCSMomenSoVongQuay(double Pct, double Ndc)
    {
        // cái này thông số bên hiệu suất, mà lười tra bảng quá nên lấy của file excel
        double Nol = 0.993, Nx = 0.91, Nbr = 0.97, Nk = 1;
        double Plv = LucF * VantocV / 1000;
        // lấy từ B8 mà ko bt tính
        double Uk = 1, U1 = 1, U2 = 1, Ux = 1;

        // công suất
        double P3 = Plv / (Nol * Nx);
        double P2 = P3 / (Nol * Nbr);
        double P1 = P2 / (Nol * Nbr);
        double Pdc2 = P1 / (Nol * Nk);
        // vòng quay
        double N1 = Ndc / Uk;
        double N2 = N1 / U1;
        double N3 = N2 / U2;
        double Nct = N3 / Ux;
        // momen
        double Tct = 9.55 * 1000000 * Pct / Nct;
        double T3 = 9.55 * 1000000 * P3 / N3;
        double T2 = 9.55 * 1000000 * P2 / N2;
        double T1 = 9.55 * 1000000 * P1 / N1;
        double Tdc = 9.55 * 1000000 * Pdc2 / Ndc;

        // Xây dựng chuỗi kết quả
        string result = $"Shaft 1: Power = {P1} kW, Speed = {N1} rpm, Torque = {T3} N.mm\n" +
                        $"Shaft 2: Power = {P2} kW, Speed = {N2} rpm, Torque = {T2} N.mm\n" +
                        $"Shaft 3: Power = {P3} kW, Speed = {N3} rpm, Torque = {T1} N.mm\n" +
                        $"Final Shaft: Power = {Pdc2} kW, Speed = {Nct} rpm, Torque = {Tct} N.mm";

        return result;
    }

    public Dictionary<string, object> Calculate()
    {
        // B3: Tính hiệu suất chung của hệ thống
        double hschungN = TinhHieuSuatChungN();

        // B4: Xác định công suất cần thiết cho động cơ
        double Pct = TinhCongSuatCanThietPct(hschungN);

        // B5: Xác định tốc độ quay cần thiết của động cơ
        double Nlv = TinhTocDoQuayCanThietNlv();

        // B6: Tính tỉ số truyền rồi tính tốc độ quay sơ bộ
        double Nsb = TinhTocDoSoBoNsb(Nlv);

        // B7: Chọn động cơ phù hợp từ catalog (giả lập)
        double Ndc = ChonDongCo(Nsb, Pct);

        // B8: Xác định tỷ số truyền của hệ dẫn động
        double Un = TinhTiSoTruyenUn(Ndc, Nlv);

        // B9: Tính công suất, momen và số vòng quay trên các trục
        string momenSoVongQuay = TinhCSMomenSoVongQuay(Pct, Ndc);

        // Tính hệ số quá tải
        double overloadFactor = TinhHeSoQuaTai();

        return new Dictionary<string, object>
        {
            { "overloadFactor", overloadFactor },
            { "overallEfficiency", hschungN },
            { "requiredMotorEfficiency", Pct },
            { "requiredMotorSpeed", Nlv },
            { "NsbSpeed", Nsb },
            { "Un", Un },
            { "MomenSoVongQuay", momenSoVongQuay }
        };
    }
    



    // Chương 4: 2 bánh răng trong hộp giảm tốc
    // B16:
    private Dictionary<string, object> chonVatLieuBoTruyen() {
        return new Dictionary<string, object>
        {
            {
                "Bánh nhỏ", new Dictionary<string, object>
                {
                    { "Vật liệu", "Thép 40X - Tôi cải thiện" },
                    { "Ob", 950 },
                    { "Och", 700 },
                    { "HB", "260 : 280" },
                    { "Kích thước S", "<= 60" }
                }
            },
            {
                "Bánh lớn", new Dictionary<string, object>
                {
                    { "Vật liệu", "Thép 40X - Tôi cải thiện" },
                    { "Ob", 850 },
                    { "Och", 550 },
                    { "HB", "230 : 260" },
                    { "Kích thước", "<= 100" }
                }
            }
        };
    }

    // B17
    private Dictionary<string,double> DauVaoUngSuat(double HB) {
        double Sh = 1.1;
        double Sf = 1.75;
        double HB1 = 265; double HB2 = 250;

        double Ohlim1 = 2*HB1 + 70;
        double Oflim1 = 1.8*HB1;
        double Ohlim2 = 2*HB2 + 70;
        double Oflim2 = 1.8*HB2;
        double Nho1 = 30*Math.Pow(HB1,2.4);
        double Nho2 = 30*Math.Pow(HB2,2.4);
        double Nfo = 4*1000000;

        return new Dictionary<string,double>
        {
            { "Ohlim1", Ohlim1 },
            { "Ohlim2", Ohlim2 },
            { "Sh", Sh },
            { "Oflim1", Oflim1 },
            { "Oflim2", Oflim2 },
            { "Sf", Sf },
            { "HB1", HB1 },
            { "HB2", HB2 },
            { "Nho1", Nho1 },
            { "Nho2", Nho2 },
            { "Nfo", Nfo }
        };
    }

    // B18
    private double TinhUngSuatChoPhep(double Lh,double n1, Dictionary<string,double> dauvao) {
        double u1 = 5.66;
        double sum1 = 0; double sumti = 0;
        int length = Math.Min(tlist.Length, Torchlist.Length);
        for (int i = 0; i < length; i++) {
            sumti += tlist[i];
        }
        for (int i = 0; i < length; i++) {
            sum1 += Math.Pow(Torchlist[i],3)*tlist[i]/sumti;
        }
        double Nhe2 = 60*1*(n1/u1)*Lh*sum1;
        double Nhe1 = Nhe2*u1;
        double Khl1 = 1;double Khl2 = 1;
        if(Nhe1 <= dauvao["Nho1"]) {
            double mH = 6;
            Khl1 = Math.Pow(dauvao["Nho1"]/Nhe1,1/mH);
        }
        if(Nhe2 <= dauvao["Nho2"]) {
            double mH = 6;
            Khl2 = Math.Pow(dauvao["Nho2"]/Nhe2,1/mH);
        }

        double allowOh1 = dauvao["Ohlim1"]*Khl1/dauvao["Sh"];
        double allowOh2 = dauvao["Ohlim2"]*Khl2/dauvao["Sh"];
        return (allowOh1 + allowOh2)/2;
    }

    // B19
    // thieu nfo1,2 va nfe2?
    private double TinhUngXuatUonChoPhep(double Lh, double n1, Dictionary<string,double> dauvao) {
        double u1 = 5.66;
        double sum1 = 0; double sumti = 0;
        int length = Math.Min(tlist.Length, Torchlist.Length);
        for (int i = 0; i < length; i++) {
            sumti += tlist[i];
        }
        for (int i = 0; i < length; i++) {
            sum1 += Math.Pow(Torchlist[i],6)*tlist[i]/sumti;
        }
        double Nfe2 = 60*1*(n1/u1)*Lh*sum1;
        double Nfe1 = Nfe2*u1;
        double Kfl1 = 1;double Kfl2 = 1;
        if(Nfe1 <= dauvao["Nfo1"]) {
            double mF = 6;
            Kfl1 = Math.Pow(dauvao["Nho1"]/Nfe1,1/mF);
        }
        if(Nfe2 <= dauvao["Nfo2"]) {
            double mF = 6;
            Kfl2 = Math.Pow(dauvao["Nfo2"]/Nfe2,1/mF);
        }

        double allowOf1 = dauvao["Oflim1"]*Kfl1/dauvao["Sf"];
        double allowOf2 = dauvao["Oflim2"]*Kfl2/dauvao["Sf"];
        return (allowOf1 + allowOf2)/2;
    }

    public void CalcBoTruyen() {
        // B16
        chonVatLieuBoTruyen();
        // B17
        var inp = DauVaoUngSuat(260);
        // B18
        double allowOh = TinhUngSuatChoPhep(43200,1450,inp);
        // B19
        TinhUngXuatUonChoPhep(43200,1450,inp);
    }

    public Dictionary<string, object> CalcTruyen()
    {
        // B16: Chọn vật liệu
        var vatlieu = chonVatLieuBoTruyen();

        // B17: Tính đầu vào ứng suất
        var input = DauVaoUngSuat(260);

        // B18: Tính ứng suất tiếp xúc cho phép
        double oh = TinhUngSuatChoPhep(43200, 1450, input);

        // B19: Tính ứng suất uốn cho phép
        double of = TinhUngXuatUonChoPhep(43200, 1450, input);

        return new Dictionary<string, object>
        {
            { "VatLieuBoTruyen", vatlieu },
            { "DauVaoUngSuat", input },
            { "UngSuatTiepXucChoPhep", oh },
            { "UngSuatUonChoPhep", of }
        };
    }


}

// đoạn này là structure của mấy bộ truyền
public interface ITransmissionCalculation
{
    double Calculate();
     Dictionary<string, object> CalChain(); // Thêm dòng này nếu muốn dùng CalChain
}

public class BeltTransmission : ITransmissionCalculation
{
    public double Calculate()
    {
        return 0.95;
    }
    public Dictionary<string, object> CalChain()
    {
        return new Dictionary<string, object> {
            { "Message", "BeltTransmission chưa hỗ trợ tính chi tiết CalChain" }
        };
    }
}

public class GearTransmission : ITransmissionCalculation
{
    public double Calculate()
    {
        return 0.98;
    }
    public Dictionary<string, object> CalChain()
    {
        return new Dictionary<string, object> {
            { "Message", "GearTransmission chưa hỗ trợ tính chi tiết CalChain" }
        };
    }
}

public class ChainTransmission : ITransmissionCalculation
{
    private double P3;
    private double U3;
    private double Uct;
    private double N3;
    private double T3;
    private double Z1;
    private double Z2;

    public ChainTransmission(double Power3, double UTruc3, double Speed3, double UCongTac, double Torque3)
    {
        P3 = Power3;
        U3 = UTruc3;
        Uct = UCongTac;
        N3 = Speed3;
        T3 = Torque3;
    }

    // B10.1
    private void TinhSoRangDiaXich(double limit = 29)
    {
        double Ux = 2.578;
        Z1 = Math.Floor(limit - 2 * Ux);
        Z2 = Math.Floor(Z1 * Ux);
    }
    // B10.2
    private double TinhBuocXichP(double limit = 29)
    {
        double K = 1.95;
        double Kz = 25 / Z1;

        // cần 1 array các giá trị bảng 5.5 để lọc tìm số gần nhất thay vì =50
        double N01 = 50;
        double Kn = N01 / N3;
        double Pt = P3 * K * Kz * Kn;
        // cần array object bảng 5.5 ở trên để lọc tìm thông số
        double Psquare = 10.5;
        double p = 38.1;
        double Dc = 11.12;
        double B = 35.46;
        // cần array bảng 5.8 để xác định n1 thỏa mãn >N3
        return p;
    }

    // B11
    private double TinhKhoangCachTruc(double p, bool Above, double a, int controlType, int Shift, int envi, int LubeType)
    {
        double K0 = Above ? 1.25 : 1;
        double Ka;
        if (a > 30 * p && a < 50 * p)
        {
            Ka = 1;
        }
        else if (a <= 25 * p)
        {
            Ka = 1.25;
        }
        else if (a >= 80 * p)
        {
            Ka = 0.8;
        }
        double Kdc = controlType == 1 ? 1 : controlType == 2 ? 1.1 : 1.25;
        // Kd
        double Kc = Shift == 1 ? 1 : Shift == 2 ? 1.25 : 1.45;
        double Kbt;
        if (envi == 0 && LubeType == 1)
        {
            Kbt = 0.8;
        }
        else if (envi == 0 && LubeType == 2)
        {
            Kbt = 1;
        }
        else if (envi == 1 && LubeType == 2)
        {
            Kbt = 1.3;
        }
        else if (envi == 1 && LubeType == 3)
        {
            Kbt = 1.8;
        }
        else if (envi == 2 && LubeType == 3)
        {
            Kbt = 3;
        }
        else if (envi == 2 && LubeType == 4)
        {
            Kbt = 6;
        }

        double x = Math.Floor(2 * a / p + (Z1 + Z2) / 2 + (Z2 - Z1) * (Z2 - Z1) * p / (4 * a * Math.PI * Math.PI));
        if (x % 2 == 1) x -= 1;

        double aNew = 0.25 * p * (x - 0.5 * (Z2 + Z1) + Math.Sqrt(Math.Pow(x - 0.5 * (Z2 + Z1), 2) - 2 * Math.Pow((Z2 - Z1) / Math.PI, 2)));
        double aStan = aNew - 0.002 * aNew;
        double i = Z1 * N3 / (15 * x);

        return aStan;
    }

    // B12
    private bool KiemNghiemXich(double p, int LoadType, double aStan)
    {
        // cần array bảng 5.2 để dò từ p
        double Q = 127;
        double q = 5.5;
        double Kd = LoadType == 1 ? 1.2 : LoadType == 2 ? 1.7 : 2.0;

        double v = Z1 * p * N3 / 60000;
        double Ft = 1000 * P3 / v;
        double Fv = q * v * v;
        double F0 = 9.81 * 6 * q * aStan;
        double s = Q / (Kd * Ft + F0 + Fv);

        // array bảng 5.10 để tìm sLimit
        double sLimit = 8.5;
        return s > sLimit;
    }

    // B13
    private void TinhDuongKinhDiaXich(double p)
    {
        double d1 = p / Math.Sin(Math.PI / Z1);
        double d2 = p / Math.Sin(Math.PI / Z2);
        double da1 = p * (0.5 + 1 / Math.Tan(Math.PI / Z1));
        double da2 = p * (0.5 + 1 / Math.Tan(Math.PI / Z2));
        // cần array bảng 5.2 để dò từ p
        double dl = 22.23;
        double r = 0.5025 * dl + 0.05;
        double df1 = d1 - 2 * r;
        double df2 = d2 - 2 * r;
    }

    // B14
    private double KiemNghiemDoBen(double p, int LoadType)
    {
        // hình như cần tra bảng tr87 dựa trên z1
        double Kr = 0.44;

        double v = Z1 * p * N3 / 60000;
        double Ft = 1000 * P3 / v;
        double Kd = LoadType == 1 ? 1.2 : LoadType == 2 ? 1.2 : 1.8;
        double Fvd = 13 * Math.Pow(10, -7) * N3 * p * p * p;
        double E = 1.6 * 100000;
        // tra bảng 5.12
        double A = 395;
        double Kde = 1;
        double Oh1 = 0.47 * Math.Sqrt(Kr * (Ft * Kd + Fvd) * E / (A * Kde));
        // tra bảng 5.11 và tìm số/vật liệu lớn hơn Oh1
        double Oh = 550;

        return Oh;
    }

    // B15
    private double TinhLucTrenTruc(double p, bool Below)
    {
        double Kx = Below ? 1.15 : 1.05;
        double v = Z1 * p * N3 / 60000;
        double Ft = 1000 * P3 / v;
        double Frk = Kx * Ft;

        return Frk;
    }

    public double Calculate()
    {
        // B10: Xác định thông số xích
        TinhSoRangDiaXich();
        double p = TinhBuocXichP();

        // B11: Khoảng cách trục và số mắt xích
        double aStan = TinhKhoangCachTruc(p, false, 40 * p, 1, 1, 0, 1);

        // B12: Tính kiểm nghiệm xích về độ bền (true nếu an toàn)
        bool safe = KiemNghiemXich(p, 1, aStan);

        // B13: Tính đường kính đĩa xích
        TinhDuongKinhDiaXich(p);

        // B14: Kiểm nghiệm độ bền tiếp xúc và chọn vật liệu cho bộ truyền xích
        double materialOh = KiemNghiemDoBen(p, 1);

        // B15: Tính lực tác dụng lên trục
        double Frk = TinhLucTrenTruc(p, true);

        return Frk;
    }

    public Dictionary<string, object> CalChain()
    {
        var result = new Dictionary<string, object>();

        // B10: Xác định thông số xích
        TinhSoRangDiaXich();
        double p = TinhBuocXichP();
        result["BuocXich_p"] = p;

        // B11: Khoảng cách trục và số mắt xích
        double aStan = TinhKhoangCachTruc(p, false, 40 * p, 1, 1, 0, 1);
        result["KhoangCachTruc_aStan"] = aStan;

        // B12: Kiểm nghiệm xích về độ bền
        bool safe = KiemNghiemXich(p, 1, aStan);
        result["XichAnToan"] = safe;

        // B13: Tính đường kính đĩa xích
        TinhDuongKinhDiaXich(p); // Không có giá trị trả về, giả sử cập nhật bên trong
        result["DuongKinhDiaXich_TinhToan"] = "Đã tính";

        // B14: Kiểm nghiệm độ bền tiếp xúc & vật liệu
        double materialOh = KiemNghiemDoBen(p, 1);
        result["DoBenTiepXuc_Oh"] = materialOh;

        // B15: Lực tác dụng lên trục
        double Frk = TinhLucTrenTruc(p, true);
        result["LucTacDungTrenTruc_Frk"] = Frk;

        return result;
    }

}

public class TransmissionFactory
{
    public static ITransmissionCalculation CreateTransmission(string type, double Power3, double UTruc3, double Speed3, double UCongTac, double Torque3)
    {
        switch (type.ToLower())
        {
            case "belt":
                return new BeltTransmission();
            case "chain":
                return new ChainTransmission(Power3, UTruc3, Speed3, UCongTac, Torque3);
            case "gear":
                return new GearTransmission();
            default:
                throw new ArgumentException("Invalid transmission type");
        }
    }
}

// code chạy mẫu
// class Program
// {
//     static void Main(string[] args)
//     {
//         // Tạo đối tượng thiết kế hộp giảm tốc
//         GearboxDesign gearboxDesign = new GearboxDesign(6000, 1.5, 650, 10, 1, []);

//         // Tạo bộ truyền xích bằng Factory Pattern
//         ITransmissionCalculation transmission = TransmissionFactory.CreateTransmission("chain",0, 0, 0, 0, 0);

//         // Thực hiện tính toán
//         gearboxDesign.Calculate();
//     }
// }