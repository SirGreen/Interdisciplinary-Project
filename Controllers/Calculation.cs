using DADN.Models;

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
        Console.WriteLine($"Selected Motor: CongSuat >= {Pct} kW, VanToc khoang [{Nsb * 0.96}, {Nsb * 1.04} ] vg/ph");

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
    private MomenKetQua TinhCSMomenSoVongQuay(double Pct, double Ndc)
    {
        double Nol = 0.993, Nx = 0.91, Nbr = 0.97, Nk = 1;
        double Plv = LucF * VantocV / 1000;

        double U1 = 3.5, U2 = 2.5, Ux = 2.56;
        double Uk = U1 * U2 * Ux;

        double P3 = Plv / (Nol * Nx);
        double P2 = P3 / (Nol * Nbr);
        double P1 = P2 / (Nol * Nbr);
        double Pdc2 = P1 / (Nol * Nk);

        double N1 = Ndc / Uk;
        double N2 = N1 / U1;
        double N3 = N2 / U2;
        double Nct = N3 / Ux;

        double Tct = 9.55 * 1000 * Pct / Nct;
        double T3 = 9.55 * 1000 * P3 / N3;
        double T2 = 9.55 * 1000 * P2 / N2;
        double T1 = 9.55 * 1000 * P1 / N1;

        return new MomenKetQua
        {
            N1 = N1, U1 = U1, T1 = T1,
            N2 = N2, U2 = U2, T2 = T2,
            MoTa = $"Shaft 1: Power = {P1} kW, Speed = {N1} rpm, Torque = {T1} Nm\n" +
                $"Shaft 2: Power = {P2} kW, Speed = {N2} rpm, Torque = {T2} Nm\n" +
                $"Shaft 3: Power = {P3} kW, Speed = {N3} rpm, Torque = {T3} Nm"
        };
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
        MomenKetQua momenSoVongQuay = TinhCSMomenSoVongQuay(Pct, Ndc);

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
    private Dictionary<string,double> DauVaoUngSuat(double HB1, double HB2) {
        double Sh = 1.1;
        double Sf = 1.75;

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
    private double TinhUngXuatUonChoPhep(double Lh,double n1, double u1, Dictionary<string,double> dauvao) {
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
        if(Nfe1 <= dauvao["Nfo"]) {
            double mF = 6;
            Kfl1 = Math.Pow(dauvao["Nho1"]/Nfe1,1/mF);
        }
        if(Nfe2 <= dauvao["Nfo"]) {
            double mF = 6;
            Kfl2 = Math.Pow(dauvao["Nfo2"]/Nfe2,1/mF);
        }

        double allowOf1 = dauvao["Oflim1"]*Kfl1/dauvao["Sf"];
        double allowOf2 = dauvao["Oflim2"]*Kfl2/dauvao["Sf"];
        return (allowOf1 + allowOf2)/2;
        // hình như ko cần tính TB? còn oh max và of max tính nhờ Och bước 16
    }
    // B20
    // helper func
    private double Deg2Rad(double Deg) {
        return Deg*Math.PI/180;
    }
    private double Rad2Deg(double Rad) {
        return Rad*180/Math.PI;
    }

    private Dictionary<string, double> TinhBanhRangCapNhanh(double n1, double u1, double T1,double allowOh,double allowOf,int sodo) {
        // 20.1
        double Wba = 0.315; 
        double Ka = 43;
        double Wbd = 0.53*Wba*(u1+1);

        double[] comp = [0.2,0.4,0.6,0.8,1,1.2,1.4,1.6];
        double[][] matching = [[],[],[1.02,1.05,1.07,1.12,1.15,1.2,1.24,1.28],[],[1.01,1.02,1.03,1.05,1.07,1.1,1.13,1.28]];
        int closest = Array.IndexOf(comp,comp.MinBy(x => Math.Abs((double) x - Wbd)));
        double Khb = matching[sodo-1][closest];

        double aW1 = Ka*(u1+1)*Math.Pow(T1*Khb/(allowOh*allowOh*u1*Wba),1/3);
        double[] day1 = [40,50,63,80,100,125,160,200,250,315,400];
        aW1 = day1.Where(x => x > aW1).Min();
        // 20.2
        double[] possibleM1 = [1.25,1.5,2,2.5,3,4,5,6,8,10,12];
        double m1 = possibleM1.Where(x => x > 0.01*aW1 && x < 0.02*aW1).Min();
        
        double B = 10;
        double Z1 = Math.Floor(2*aW1*Math.Cos(Deg2Rad(B))/(m1*(u1+1)));
        double Z2 = Math.Floor(u1*Z1);
        double Um1 = Z2/Z1;
        B = Rad2Deg(Math.Acos(m1*(Z1+Z2)/(2*aW1)));
        
        // 20.3
        double ZM = 274;
        double at = Rad2Deg(Math.Atan(Math.Tan(Deg2Rad(20)) / Math.Cos(Deg2Rad(B))));
        double Bb = Rad2Deg(Math.Cos(Deg2Rad(at))*Math.Tan(Deg2Rad(B)));
        double ZH = Math.Sqrt(2*Math.Cos(Deg2Rad(Bb))/Math.Sin(Deg2Rad(2*at)));
        double bW1 = Wba*aW1;
        double eB = bW1*Math.Sin(Deg2Rad(B))/(m1*Math.PI);
        double eA = (1.88 - 3.2*(1/Z1 + 1/Z2))*Math.Cos(Deg2Rad(B));
        double Ze = Math.Sqrt(1/eA);
        double dW1 = 2*aW1/(Um1+1);
        double v = Math.PI*dW1*n1/60000;

        double[] vlim = [4,10,15,30];
        int[] capChinhXac = [9,8,7,6];
        int vlimidx = Array.IndexOf(vlim,vlim.Where(x => v <= x).Min());
        int level = capChinhXac[vlimidx];
        double[] VelocityRanges = { 2.5, 5, 10, 15, 20, 25 };
        // 3D array [velocityIndex, levelIndex, KHα/KFα]
        int[] AccuracyLevels = { 6, 7, 8, 9 };
        double[,,] CoefficientTable = new double[,,]
        {
            // ≤ 2.5 m/s
            { {1.01, 1.05}, {1.03, 1.12}, {1.05, 1.22}, {1.13, 1.37} },
            // 5 m/s
            { {1.02, 1.07}, {1.05, 1.16}, {1.09, 1.27}, {1.16, 1.40} },
            // 10 m/s
            { {1.03, 1.10}, {1.07, 1.22}, {1.13, 1.37}, {double.NaN, double.NaN} },
            // 15 m/s
            { {1.04, 1.13}, {1.09, 1.25}, {1.17, 1.45}, {double.NaN, double.NaN} },
            // 20 m/s
            { {1.05, 1.17}, {1.12, 1.35}, {double.NaN, double.NaN}, {double.NaN, double.NaN} },
            // 25 m/s
            { {1.06, 1.20}, {double.NaN, double.NaN}, {double.NaN, double.NaN}, {double.NaN, double.NaN} }
        };
        // Find velocity index
        int velocityIndex = -1;
        for (int i = 0; i < VelocityRanges.Length; i++)
        {
            if (v <= VelocityRanges[i])
            {
                velocityIndex = i;
                break;
            }
        }
        // Handle velocities above maximum
        if (velocityIndex == -1)
        {
            velocityIndex = VelocityRanges.Length - 1;
        }
        // Find accuracy level index
        int levelIndex = Array.IndexOf(AccuracyLevels, level);
        // Get coefficients
        double KHa = CoefficientTable[velocityIndex, levelIndex, 0];
        double KFa = CoefficientTable[velocityIndex, levelIndex, 1];
        
        double OmegaH = 0.002;
        double g0 = 73;
        if(m1>3.55 && m1<10) { // chưa có kiểm tra level ở đây thì phải, hi vọng ko sao :v
            g0 = 82;
        } else if(m1>10) {
            g0 = 100;
        }
        double vH = OmegaH*g0*v*Math.Sqrt(aW1/Um1);
        double Khv = 1 + vH*bW1*dW1/(2*T1*Khb*KHa);
        double KH = Khb*KHa*Khv;
        double thisOH = ZM*ZH*Ze*Math.Sqrt(2*T1*KH*(Um1+1)/(bW1*Um1*dW1*dW1));
        bool thoaManDoBentx = thisOH < allowOh;

        // 20.4
        double Ye = 1/eA;
        double Yb = 1-B/140;
        double Zv1 = Z1/Math.Pow(Math.Cos(Deg2Rad(B)),3);
        double Zv2 = Z2/Math.Pow(Math.Cos(Deg2Rad(B)),3);
        double[] Zvlim = [17,20,22,25,30,40,50,60,80,100,150];
        double[] YFval = [4.26,4.08,4,3.9,3.8,3.7,3.65,3.62,3.61,3.6,3.6];
        int Yf1idx = Array.IndexOf(Zvlim,Zvlim.MinBy(x => Math.Abs((double) x - Zv1)));
        double YF1 = YFval[Yf1idx];
        int Yf2idx = Array.IndexOf(Zvlim,Zvlim.MinBy(x => Math.Abs((double) x - Zv2)));
        double YF2 = YFval[Yf2idx];
        double[][] KFbval = [[],[],[1.05,1.11,1.17,1.24,1.32,1.41,1.5,1.6],[],[1.02,1.05,1.08,1.12,1.16,1.22,1.28,1.37]];
        double KFb = KFbval[sodo-1][closest];
        double omegaF = 0.006;
        double vF = omegaF*g0*v*Math.Sqrt(aW1*Um1);
        double KFv = 1 + vF*bW1*dW1/(2*T1*KFb*KFa);
        double KF = KFb*KFa*KFv;
        double thisOF1 = 2*T1*KF*Ye*Yb*YF1/(bW1*dW1*m1);
        double thisOF2 = thisOF1*YF2/YF1;

        // còn kiểm tra hai cái Of kia với điều kiện OF', chắc vậy
        bool thoaManBenUon = thisOF1 < allowOf && thisOF2 < allowOf;
        // 20.5
        // 20.5 là so sánh điều kiện với câu trước mà đang không rõ đk nào lắm :v
        // double Kqt = 2.2;
        // double OHmax = thisOH*Math.Sqrt(Kqt);
        bool thoaManQuaTai = thisOF1 < allowOf && thisOF2 < allowOf;

        // 20.6
        double duongKinhVongChia1 = m1*Z1/Math.Cos(Deg2Rad(B));
        double duongKinhVongChia2 = m1*Z2/Math.Cos(Deg2Rad(B));

        return new Dictionary<string,double>
        {
            { "aw", aW1},               // Khoảng cách trục (mm)
            { "m", m1 },                   // Modul pháp (mm)
            { "bw", bW1 },                // Chiều rộng vành răng (mm)
            { "um", Um1 },               // Tỷ số truyền
            { "gocNghieng", B },            // Góc nghiêng răng (độ)
            
            // Số răng bánh răng
            { "z1", Z1 },                 // Bánh răng 1
            { "z2", Z2 },                // Bánh răng 2
            
            // Hệ số dịch chỉnh
            { "x1", 0 },                  // Bánh răng 1
            { "x2", 0 },                  // Bánh răng 2
            
            // Đường kính vòng chia (mm)
            { "d1", duongKinhVongChia1 },              // Bánh răng 1
            { "d2", duongKinhVongChia2 },             // Bánh răng 2
            
            // // Đường kính đỉnh răng (mm)
            { "da1", duongKinhVongChia1 + 2*m1 },             // Bánh răng 1
            { "da2", duongKinhVongChia2 + 2*m1 },            // Bánh răng 2
            
            // // Đường kính đáy răng (mm)
            { "df1", duongKinhVongChia1 - 2.5*m1 },             // Bánh răng 1
            { "df2", duongKinhVongChia2 - 2.5*m1 },            // Bánh răng 2
            
            // // Đường kính vòng lăn (mm)
            { "dw1", dW1 },             // Bánh răng 1
            { "dw2", dW1*Um1 }             // Bánh răng 2
        }; 
    }
    // B21
    private Dictionary<string,double> TinhBoTruyenCapCham(double Lh,double u2,double n2, double T2) {
        // B21.1
        chonVatLieuBoTruyen();
        // B21.2
        var inp = DauVaoUngSuat(280,260);
        double allowOh = TinhUngSuatChoPhep(Lh,n2,inp);
        double allowOf = TinhUngXuatUonChoPhep(Lh,n2,u2,inp);
        // B21 con lai
        return TinhBanhRangCapNhanh(n2,u2,T2,allowOh,allowOf,5);
    }
    // B22
    private bool kiemTraBoiTron(Dictionary<string, double> res1, Dictionary<string, double> res2) {
        double h2 = (res1["da2"]-res1["df2"])/2;
        double H = res1["da2"]/2 - 10 - 10;
        double secondCond = res2["da2"];
        return h2<10 && H>secondCond;
    }
                                    // n1,u1,... lấy từ tập kết quả ở bước 9
    public void CalcBoTruyen(double Lh, double n1,double u1,double T1, double n2,double u2,double T2) {
        // B16
        chonVatLieuBoTruyen();
        // B17
        var inp = DauVaoUngSuat(265,250);
        // B18
        double allowOh = TinhUngSuatChoPhep(Lh,n1,inp);
        // B19
        double allowOf = TinhUngXuatUonChoPhep(Lh,n1,u1,inp);
        // B20
        var res1 = TinhBanhRangCapNhanh(n1,u1,T1,allowOh,allowOf,3);
        // B21
        var res2 = TinhBoTruyenCapCham(Lh,u2,n2,T2);
        // B22
        bool duBoiTron = kiemTraBoiTron(res1,res2);
    }

    // Gọi tính toán
                                    // n1,u1,... lấy từ tập kết quả ở bước 9
    // public void CalcBoTruyen(double Lh, double n1,double u1,double T1, double n2,double u2,double T2) {
    //     // B16
    //     chonVatLieuBoTruyen();
    //     // B17
    //     var inp = DauVaoUngSuat(265,250);
    //     // B18
    //     double allowOh = TinhUngSuatChoPhep(Lh,n1,inp);
    //     // B19
    //     double allowOf = TinhUngXuatUonChoPhep(Lh,n1,u1,inp);
    //     // B20
    //     var res1 = TinhBanhRangCapNhanh(n1,u1,T1,allowOh,3);
    //     // B21
    //     var res2 = TinhBoTruyenCapCham(Lh,u2,n2,T2);
    //     // B22
    //     // do chưa rõ dữ liệu trên 2 bước kia nên chưa dùng đc nha
    //     // bool duBoiTron = kiemTraBoiTron(res1,res2);
    // }

    // public Dictionary<string, object> CalcTruyen()
    // {
    //     // B16: Chọn vật liệu
    //     var vatlieu = chonVatLieuBoTruyen();

    //     // B17: Tính đầu vào ứng suất
    //     var input = DauVaoUngSuat(265, 250);

    //     // B18: Tính ứng suất tiếp xúc cho phép
    //     double oh = TinhUngSuatChoPhep(43200, 1450, input);

    //     // B19: Tính ứng suất uốn cho phép
    //     double of = TinhUngXuatUonChoPhep(43200, 1450, 5.3, input);

    //     // B20: Tính bánh răng cấp nhanh
    //     var res1 = TinhBanhRangCapNhanh(1450, 5.3, 285, oh, 3);

    //     // B21: Tính bánh răng cấp chậm
    //     var res2 = TinhBoTruyenCapCham(43200, 3.4, 273.58, 4390.5);

    //     // Trả kết quả
    //     return new Dictionary<string, object>
    //     {
    //         { "VatLieuBoTruyen", vatlieu },
    //         { "DauVaoUngSuat", input },
    //         { "UngSuatTiepXucChoPhep", oh },
    //         { "UngSuatUonChoPhep", of },
    //         { "CapNhanh", res1 },
    //         { "CapCham", res2 }
    //     };
    // }

    public Dictionary<string, object> CalcBoTruyen(Dictionary<string, double> dict, double Lh)
    {
        var result = new Dictionary<string, object>();

        // B16
        var vatlieu = chonVatLieuBoTruyen();

        // B17
        var inp = DauVaoUngSuat(265, 250);

        // B18
        double allowOh = TinhUngSuatChoPhep(Lh, dict["n1"], inp);

        // B19
        double allowOf = TinhUngXuatUonChoPhep(Lh, dict["n1"], dict["u1"], inp);

        // B20
        var res1 = TinhBanhRangCapNhanh(dict["n1"], dict["u1"], dict["T1"], allowOh, 3);

        // B21
        var res2 = TinhBoTruyenCapCham(Lh, dict["u2"], dict["n2"], dict["T2"]);

        // Add kết quả vào result dictionary
        result["VatLieuBoTruyen"] = vatlieu;
        result["DauVaoUngSuat"] = inp;
        result["UngSuatTiepXucChoPhep"] = allowOh;
        result["UngSuatUonChoPhep"] = allowOf;
        result["TinhBangRangCapNhanh"] = res1;
        result["TinhBangRangCapCham"] = res2;

        return result;
    }





}

// đoạn này là structure của mấy bộ truyền
public interface ITransmissionCalculation
{
    double Calculate();
    Dictionary<string, object> CalChain();
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
    // Thông số đầu vào
    private double P3;
    private double U3;
    private double Uct;
    private double N3;
    private double T3;

    // Các thông số cần hiển thị
    public int z1, z2, soMatXich;
    public double pitch, ongLot, duongKinhChot, shaftDistance;
    public double diaDan, diaBiDan, shaftForce;
    public double diskDiameterCalc, contactStrength;
    public bool chainSafe;
    public string vatLieuDia1 = "Thép C45", vatLieuDia2 = "Thép C45";

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
        z1 = (int)Math.Floor(limit - 2 * Ux);
        z2 = (int)Math.Floor(z1 * Ux);
    }

    // B10.2
    private double TinhBuocXichP()
    {
        double N01 = 50;
        double Kn = N01 / N3;
        double K = 1.95;
        double Kz = 25.0 / z1;
        double Pt = P3 * K * Kz * Kn;
        pitch = 38.1;
        ongLot = 35.46;
        duongKinhChot = 11.12;
        return pitch;
    }
    

    // B11
    private double TinhKhoangCachTruc(double p)
    {
        double a = 40 * p;
        double x = Math.Floor(2 * a / p + (z1 + z2) / 2 + (z2 - z1) * (z2 - z1) * p / (4 * a * Math.PI * Math.PI));
        if (x % 2 == 1) x -= 1;
        soMatXich = (int)x;

        double aNew = 0.25 * p * (x - 0.5 * (z2 + z1) + Math.Sqrt(Math.Pow(x - 0.5 * (z2 + z1), 2) - 2 * Math.Pow((z2 - z1) / Math.PI, 2)));
        shaftDistance = aNew;
        return aNew;
    }

    // B12
    private bool KiemNghiemXich(double p)
    {
        double Q = 127;
        double q = 5.5;
        double Kd = 1.2;
        double v = z1 * p * N3 / 60000;
        double Ft = 1000 * P3 / v;
        double Fv = q * v * v;
        double F0 = 9.81 * 6 * q * shaftDistance;
        double s = Q / (Kd * Ft + F0 + Fv);
        double sLimit = 8.5;

        chainSafe = s > sLimit;
        return chainSafe;
    }

    // B13
    private void TinhDuongKinhDiaXich(double p)
    {
        diaDan = p / Math.Sin(Math.PI / z1);
        diaBiDan = p / Math.Sin(Math.PI / z2);
        diskDiameterCalc = (diaDan + diaBiDan) / 2;
    }

    // B14
    private double KiemNghiemDoBen(double p)
    {
        double v = z1 * p * N3 / 60000;
        double Ft = 1000 * P3 / v;
        double Kd = 1.2;
        double Fvd = 13e-7 * N3 * p * p * p;
        double E = 1.6e5;
        double A = 395;
        double Kde = 1;
        double Kr = 0.44;

        contactStrength = 0.47 * Math.Sqrt(Kr * (Ft * Kd + Fvd) * E / (A * Kde));
        return contactStrength;
    }

    // B15
    private double TinhLucTrenTruc(double p)
    {
        double v = z1 * p * N3 / 60000;
        double Ft = 1000 * P3 / v;
        double Kx = 1.15;
        shaftForce = Kx * Ft;
        return shaftForce;
    }

    // Tổng hợp tính toán và trả kết quả
    public Dictionary<string, object> CalChain()
    {
        var result = new Dictionary<string, object>();

        TinhSoRangDiaXich();
        result["soRangDan"] = z1;
        result["soRangBiDan"] = z2;

        double p = TinhBuocXichP();
        result["pitch"] = p;
        result["ongLot"] = ongLot;
        result["duongKinhChot"] = duongKinhChot;

        double aStan = TinhKhoangCachTruc(p);
        result["soMatXich"] = soMatXich;
        result["shaftDistance"] = shaftDistance;

        bool safe = KiemNghiemXich(p);
        result["chainSafe"] = chainSafe;

        TinhDuongKinhDiaXich(p);
        result["diaDan"] = diaDan;
        result["diaBiDan"] = diaBiDan;
        result["diskDiameterCalc"] = diskDiameterCalc;

        double contact = KiemNghiemDoBen(p);
        result["contactStrength"] = contactStrength;

        double Frk = TinhLucTrenTruc(p);
        result["shaftForce"] = shaftForce;

        result["vatLieuDia1"] = vatLieuDia1;
        result["vatLieuDia2"] = vatLieuDia2;

        return result;
    }

    public double Calculate()
    {
        // B10: Xác định thông số xích
        TinhSoRangDiaXich();
        double p = TinhBuocXichP();

        // B11: Khoảng cách trục và số mắt xích
        double aStan = TinhKhoangCachTruc(p);

        // B12: Tính kiểm nghiệm xích về độ bền (true nếu an toàn)
        bool safe = KiemNghiemXich(p);

        // B13: Tính đường kính đĩa xích
        TinhDuongKinhDiaXich(p);

        // B14: Kiểm nghiệm độ bền tiếp xúc và chọn vật liệu cho bộ truyền xích
        double materialOh = KiemNghiemDoBen(p);

        // B15: Tính lực tác dụng lên trục
        double Frk = TinhLucTrenTruc(p);

        return Frk;
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