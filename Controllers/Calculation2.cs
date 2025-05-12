namespace Calculation2
{
    class CalculationSecond
    {
        // Lookup table for Bang 10.2
        private readonly Dictionary<int, int> Bang10_2 = new Dictionary<int, int>()
    {
        { 20, 15 }, { 25, 17 }, { 30, 19 }, { 35, 21 }, { 40, 23 },
        { 45, 25 }, { 50, 27 }, { 55, 29 }, { 60, 31 }, { 65, 33 },
        { 70, 35 }, { 75, 37 }, { 80, 39 }, { 85, 41 }, { 90, 43 }, { 100, 47 }
    };

        // Lookup table for d to Do conversion
        private readonly List<(int MinD, int MaxD, int Do)> DToDoTable = new List<(int, int, int)>
    {
        (20, 24, 45),
        (25, 37, 63),
        (100, 124, 71),
        (125, 139, 90),
        (140, 169, 105),
        (170, 209, 130),
        (210, 259, 160),
        (260, 300, 200)
    };

        // Helper method to get Do value from d
        public int GetDoFromDiameter(int diameter)
        {
            foreach (var range in DToDoTable)
            {
                if (diameter >= range.MinD && diameter <= range.MaxD)
                {
                    return range.Do;
                }
            }
            // Return -1 if diameter is outside all specified ranges
            return -1;
        }

        // Helper method to get bo value from d
        public int GetBoFromDiameter(int diameter)
        {
            if (Bang10_2.ContainsKey(diameter))
            {
                return Bang10_2[diameter];
            }
            // For values not in the table, could implement interpolation
            // For now, returning -1 to indicate not found
            return -1;
        }



        // Các thuộc tính và phương thức của lớp Calculation2
        public Dictionary<string, object> TinhDuongKinhTruc(double T1, double T2, double T3, double bw1, double bw2)
        {
            const int t1 = 15, t2 = 20, t3 = 30;
            double d1 = Math.Pow(T1 / (0.2 * t1), 1.0 / 3.0);
            d1 = Math.Round(d1 / 5.0) * 5.0;
            double d2 = Math.Pow(T2 / (0.2 * t2), 1.0 / 3.0);
            d2 = Math.Round(d2 / 5.0) * 5.0 + 5.0;
            double d3 = Math.Pow(T3 / (0.2 * t3), 1.0 / 3.0);
            d3 = Math.Round(d3 / 5.0) * 5.0 + 5.0;

            int bo1 = GetBoFromDiameter((int)d1);
            int bo2 = GetBoFromDiameter((int)d2);
            int bo3 = GetBoFromDiameter((int)d3);

            const int k1 = 10, k2 = 7, k3 = 16, hn = 17;

            double lm13 = 1.5 * d1, lm22 = 1.5 * d2, lm23 = 1.5 * d2, lm32 = 1.5 * d3;
            double lm33 = 1.5 * d3;
            double lm12 = 1.6 * d1;

            lm13 = Math.Max(lm13, bw1);
            lm22 = Math.Max(lm22, bw1);
            lm23 = Math.Max(lm23, bw2);
            lm32 = Math.Max(lm32, bw2);

            double l22 = 0.5 * (lm22 + bo2) + k1 + k2;
            double l23 = l22 + 0.5 * (lm22 + lm23) + k1;
            double l21 = lm22 + lm23 + 3 * k1 + 2 * k2 + bo2;

            double l13 = l22;
            double l12 = 0.5 * (lm12 + bo1) + k3 + hn, lc12 = -l12;
            double l11 = l21;

            double l32 = l23;
            double lc33 = 0.5 * (lm33 + bo3) + k3 + hn;
            double l31 = l21;
            double l33 = l31 + lc33;

            Console.WriteLine($"{l11} {l12} {l13}");
            return new Dictionary<string, object>
            {
                ["dI"] = d1,
                ["dII"] = d2,
                ["dIII"] = d3,
                ["l22"] = l22,
                ["l23"] = l23,
                ["l21"] = l21,
                ["l13"] = l13,
                ["l12"] = l12,
                ["l11"] = l11,
                ["l32"] = l32,
                ["lc33"] = lc33,
                ["l31"] = l31,
                ["l33"] = l33
            };
        }

        //d1 của B20.7 đg kính vg chia; alphatw của B20.3
        public Dictionary<string, object> TinhDuongKinhTrucI(double T1, int dI, double d1, double Beta, double alphatw,
            double l11, double l12, double l13)
        {
            double Dt = GetDoFromDiameter(dI);
            double Fr = 0.2 * 2 * (T1 / Dt);

            double Ft1 = 2 * (T1 / d1);
            double Fa1 = Ft1 * Math.Tan(Beta * Math.PI / 180);
            double Fr1 = Ft1 * (Math.Tan(alphatw * Math.PI / 180) /
                                Math.Cos(Beta * Math.PI / 180));

            // Calculate reaction forces based on equilibrium equations
            double RBz = Fa1; // From equation: RBz - Fa1 = 0

            // From equations in the image
            double RBx = ((Fr * (l11 + l12)) + (Ft1 * (l11 - l13))) / l11;
            double RDx = Fr + Ft1 - RBx;

            // For vertical forces
            double RBy = (Fr1 * (l11 - l13) + Fa1 * (d1 / 2)) / l11;
            double RDy = Fr1 - RBy;

            double Qxl12 = -Fr;
            double Qxl13 = Qxl12 + RBx;
            double Qxl11 = Qxl13 - Ft1;

            double Qyl12 = 0;
            double Qyl13 = Qyl12 + RBy;
            double Qyl11 = Qyl13 - Fr1;

            double MxA = 0, MxB = 0, MxD = 0;
            double MxCl = Qyl13 * l13;
            double MxCr = MxCl - Fa1 * d1 / 2;

            double MyA = 0, MyD = 0;
            double MyB = Qxl12 * l12;
            double MyC = MyB + Qxl13 * l13;

            double MzA = T1, MzB = T1, MzC = T1, MzD = 0;

            double MtdA = Math.Sqrt(MxA * MxA + MyA * MyA + MzA * MzA * 0.75);
            double MtdB = Math.Sqrt(MxB * MxB + MyB * MyB + MzB * MzB * 0.75);
            double MtdC = Math.Sqrt(MxCr * MxCr + MyC * MyC + MzC * MzC * 0.75);
            double MtdD = Math.Sqrt(MxD * MxD + MyD * MyD + MzD * MzD * 0.75);

            double dA = Math.Pow(MtdA / (0.1 * 63), 1.0 / 3.0);
            dA = Math.Round(dA / 5.0) * 5.0;
            double dB = Math.Pow(MtdB / (0.1 * 63), 1.0 / 3.0);
            dB = Math.Round(dB / 5.0) * 5.0 + 5.0;
            double dC = Math.Pow(MtdC / (0.1 * 63), 1.0 / 3.0);
            dC = Math.Round(dC / 5.0) * 5.0 + 5.0;
            double dD = dB;

            Console.WriteLine($"dA: {dA}, dB: {dB}, dC: {dC}, dD: {dD}");

            return new Dictionary<string, object>
            {
                ["Ft1"] = Ft1,
                ["Fr1"] = Fr1,
                ["Fa1"] = Fa1,
                ["dA"] = dA,
                ["dB"] = dB,
                ["dC"] = dC,
                ["dD"] = dD
            };
        }

        //d2 của B20.7, d3 của B21.6, alphatw + beta của B21
        public Dictionary<string, object> TinhDuongKinhTrucII(double T2, double d2, double d3, double Ft1, double Fr1,
        double alphatw, double beta, double Fa1, double l21, double l22, double l23)
        {
            double Ft2 = Ft1;
            double Ft3 = 2 * (T2 / d3);
            double Fr2 = Fr1;
            double Fr3 = Ft3 * (Math.Tan(alphatw * Math.PI / 180) /
                                Math.Cos(beta * Math.PI / 180));
            double Fa2 = Fa1;
            double Fa3 = Ft3 * Math.Tan(beta * Math.PI / 180);

            double RAx = (Ft2 * (l21 - l22) + Ft3 * (l21 - l23)) / l21;
            double RAy = (Fr2 * (l21 - l22) - Fr3 * (l21 - l23) - Fa2 * (d2 / 2) - Fa3 * (d3 / 2)) / l21;
            double RAz = Fa3 - Fa2;
            double RDx = Ft2 + Ft3 - RAx;
            double RDy = Fr2 - Fr3 - RAy;

            // Print the calculated reaction forces
            Console.WriteLine($"Reaction Forces:");
            Console.WriteLine($"RAx: {RAx:F2}N (Expected: 3270.25N)");
            Console.WriteLine($"RAy: {RAy:F2}N (Expected: -824.43N)");
            Console.WriteLine($"RAz: {RAz:F2}N (Expected: 520.14N)");
            Console.WriteLine($"RDx: {RDx:F2}N (Expected: 4654.98N)");
            Console.WriteLine($"RDy: {RDy:F2}N (Expected: -544.62N)");

            // Verify equilibrium equations
            double eq1 = RAx - Ft2 - Ft3 + RDx;
            double eq2 = RAy - Fr2 + Fr3 + RDy;
            double eq3 = RAz + Fa2 - Fa3;
            double eq4 = -Fr2 * (l21 - l22) + Fr3 * (l21 - l23) + RAy * l21 + Fa2 * (d2 / 2) + Fa3 * (d3 / 2);
            double eq5 = -Ft2 * (l21 - l22) - Ft3 * (l21 - l23) + RAx * l21;

            Console.WriteLine("\nEquilibrium Verification (should be close to zero):");
            Console.WriteLine($"Eq1 (forces in x): {eq1:F6}");
            Console.WriteLine($"Eq2 (forces in y): {eq2:F6}");
            Console.WriteLine($"Eq3 (forces in z): {eq3:F6}");
            Console.WriteLine($"Eq4 (moments around x): {eq4:F6}");
            Console.WriteLine($"Eq5 (moments around y): {eq5:F6}");

            double Qxl22 = -RAx;
            double Qxl23 = Qxl22 + Ft2;
            double Qxl21 = Qxl23 + Ft3;

            Console.WriteLine("\nShear Forces:");
            Console.WriteLine($"Qxl22: {Qxl22:F2}N");
            Console.WriteLine($"Qxl23: {Qxl23:F2}N");
            Console.WriteLine($"Qxl21: {Qxl21:F2}N");

            double Qyl22 = -RAy;
            double Qyl23 = Qyl22 + Fr2;
            double Qyl21 = Qyl23 - Fr3;

            Console.WriteLine("\nVertical Forces:");
            Console.WriteLine($"Qyl22: {Qyl22:F2}N");
            Console.WriteLine($"Qyl23: {Qyl23:F2}N");
            Console.WriteLine($"Qyl21: {Qyl21:F2}N");

            // Bending moments
            double Mx_A = 0;
            double Mx_Bl = Qyl22 * l22;
            double Mx_Br = Mx_Bl - Fa2 * (d2 / 2);
            double Mx_Cl = Mx_Br + Qyl23 * (l23 - l22);
            double Mx_Cr = Mx_Cl - Fa3 * (d3 / 2);
            double Mx_D = 0;

            Console.WriteLine("\nBending Moments:");
            Console.WriteLine($"Mx_A: {Mx_A:F2}Nmm");
            Console.WriteLine($"Mx_Bl: {Mx_Bl:F2}Nmm");
            Console.WriteLine($"Mx_Br: {Mx_Br:F2}Nmm");
            Console.WriteLine($"Mx_Cl: {Mx_Cl:F2}Nmm");
            Console.WriteLine($"Mx_Cr: {Mx_Cr:F2}Nmm");
            Console.WriteLine($"Mx_D: {Mx_D:F2}Nmm");

            double My_A = 0;
            double My_B = -Qxl22 * l22;
            double My_C = My_B - Qxl23 * (l23 - l22);
            double My_D = 0;

            Console.WriteLine("\nBending Moments:");
            Console.WriteLine($"My_A: {My_A:F2}Nmm");
            Console.WriteLine($"My_B: {My_B:F2}Nmm");
            Console.WriteLine($"My_C: {My_C:F2}Nmm");
            Console.WriteLine($"My_D: {My_D:F2}Nmm");

            double Mz_A = -T2;
            double Mz_B = -T2;
            double Mz_C = -T2;
            double Mz_D = -T2;

            double a = 0.75; // Coefficient for torsional stress
            double Mtd_A = Math.Sqrt(Mx_A * Mx_A + My_A * My_A + a * Mz_A * Mz_A);
            double Mtd_B = Math.Sqrt(Mx_Bl * Mx_Bl + My_B * My_B + a * Mz_B * Mz_B);
            double Mtd_C = Math.Sqrt(Mx_Cl * Mx_Cl + My_C * My_C + a * Mz_C * Mz_C);
            double Mtd_D = Math.Sqrt(Mx_D * Mx_D + My_D * My_D + a * Mz_D * Mz_D);

            Console.WriteLine("\nTorsional Moments:");
            Console.WriteLine($"Mtd_A: {Mtd_A:F2}Nmm");
            Console.WriteLine($"Mtd_B: {Mtd_B:F2}Nmm");
            Console.WriteLine($"Mtd_C: {Mtd_C:F2}Nmm");
            Console.WriteLine($"Mtd_D: {Mtd_D:F2}Nmm");

            double dA = Math.Pow(Mtd_A / (0.1 * 50), 1.0 / 3.0);
            dA = Math.Round(dA / 5.0) * 5.0 + 5.0;
            double dB = Math.Pow(Mtd_B / (0.1 * 50), 1.0 / 3.0);
            dB = Math.Round(dB / 5.0) * 5.0 + 5.0;
            double dC = Math.Pow(Mtd_C / (0.1 * 50), 1.0 / 3.0);
            dC = Math.Round(dC / 5.0) * 5.0 + 5.0;
            double dD = dA;

            Console.WriteLine("\nCalculated Diameters:");
            Console.WriteLine($"dA: {dA:F2}mm");
            Console.WriteLine($"dB: {dB:F2}mm");
            Console.WriteLine($"dC: {dC:F2}mm");
            Console.WriteLine($"dD: {dD:F2}mm");


            return new Dictionary<string, object>
            {
                ["Ft3"] = Ft3,
                ["Fr3"] = Fr3,
                ["Fa3"] = Fa3,
                ["dA"] = dA,
                ["dB"] = dB,
                ["dC"] = dC,
                ["dD"] = dD
            };
        }

        // Frx là B15: Lực tác dụng lên trục
        public Dictionary<string, object> TinhDuongKinhTrucIII(double T3, double Frx, double d4, double Ft3,
        double Fr3, double Fa3, double l31, double l32, double l33)
        {
            double Ft4 = Ft3, Fr4 = Fr3, Fa4 = Fa3;

            double RCz = Fa4;
            double RAy = (Fr4 * (l31 - l32) - Fa4 * (d4 / 2)) / l31;
            double RCy = Fr4 - RAy;

            // Solve for RAx from the fifth equation
            double RAx = (Ft4 * (l31 - l32) + Frx * (l33 - l31)) / l31;

            // Solve for RCx from the first equation
            double RCx = -(Ft4 - RAx - Frx);

            // Print the calculated reaction forces and compare with expected values
            Console.WriteLine($"Reaction Forces:");
            Console.WriteLine($"RAx: {RAx:F2}N (Expected: 4807.58N)");
            Console.WriteLine($"RAy: {RAy:F2}N (Expected: -196.046N)");
            Console.WriteLine($"RCy: {RCy:F2}N (Expected: 2387.906N)");
            Console.WriteLine($"RCx: {RCx:F2}N (Expected: 5950.25N)");
            Console.WriteLine($"RCz: {RCz:F2}N (Expected: 1174.48N)");

            // Calculate internal forces
            double Qxl32 = RAx;
            double Qxl31 = Qxl32 - Ft4;
            double Qxl33 = Qxl31 - RCx;

            double Qyl32 = -RAy;
            double Qyl31 = Qyl32 + Fr4;
            double Qyl33 = Qyl31 - RCy;

            // Calculate bending moments
            double Mx_A = 0;
            double Mx_Bl = -Qyl32 * l32;
            double Mx_Br = Mx_Bl - Fa4 * (d4 / 2);
            double Mx_C = 0;
            double Mx_D = 0;

            double My_A = 0;
            double My_B = -Qxl32 * l32;
            double My_C = My_B - Qxl31 * (l31 - l32);
            double My_D = 0;

            // Torsional moments
            double Mz_A = 0;
            double Mz_B = T3;
            double Mz_C = T3;
            double Mz_D = T3;

            // Calculate equivalent moments
            double a = 0.75; // Coefficient for torsional stress
            double Mtd_B = Math.Sqrt(Mx_Bl * Mx_Bl + My_B * My_B + a * Mz_B * Mz_B);
            double Mtd_C = Math.Sqrt(Mx_C * Mx_C + My_C * My_C + a * Mz_C * Mz_C);
            double Mtd_A = Mtd_C;
            double Mtd_D = Math.Sqrt(Mx_D * Mx_D + My_D * My_D + a * Mz_D * Mz_D);

            Console.WriteLine("\nInternal Forces:");
            Console.WriteLine($"Qxl32: {Qxl32:F2}N, Qxl31: {Qxl31:F2}N, Qxl33: {Qxl33:F2}N");
            Console.WriteLine($"Qyl32: {Qyl32:F2}N, Qyl31: {Qyl31:F2}N, Qyl33: {Qyl33:F2}N");

            Console.WriteLine("\nBending Moments:");
            Console.WriteLine($"Mx_A: {Mx_A:F2}Nmm, Mx_Bl: {Mx_Bl:F2}Nmm, Mx_Br: {Mx_Br:F2}Nmm, Mx_C: {Mx_C:F2}Nmm");
            Console.WriteLine($"My_A: {My_A:F2}Nmm, My_B: {My_B:F2}Nmm, My_C: {My_C:F2}Nmm, My_D: {My_D:F2}Nmm");
            Console.WriteLine($"Mz_A: {Mz_A:F2}Nmm, Mz_B: {Mz_B:F2}Nmm, Mz_C: {Mz_C:F2}Nmm, Mz_D: {Mz_D:F2}Nmm");

            Console.WriteLine("\nDesign Moments:");
            Console.WriteLine($"Mtd_A: {Mtd_A:F2}Nmm, Mtd_B: {Mtd_B:F2}Nmm, Mtd_C: {Mtd_C:F2}Nmm, Mtd_D: {Mtd_D:F2}Nmm");

            // Calculate required shaft diameters
            double dA = Math.Pow(Mtd_A / (0.1 * 40), 1.0 / 3.0);
            dA = Math.Round(dA / 5.0) * 5.0;
            double dB = Math.Pow(Mtd_B / (0.1 * 40), 1.0 / 3.0);
            dB = Math.Round(dB / 5.0) * 5.0 + 10.0;
            double dC = Math.Pow(Mtd_C / (0.1 * 40), 1.0 / 3.0);
            dC = Math.Round(dC / 5.0) * 5.0;
            double dD = Math.Pow(Mtd_D / (0.1 * 40), 1.0 / 3.0);
            dD = Math.Round(dD / 5.0) * 5.0;

            Console.WriteLine("\nCalculated Diameters:");
            Console.WriteLine($"dA: {dA:F2}mm");
            Console.WriteLine($"dB: {dB:F2}mm");
            Console.WriteLine($"dC: {dC:F2}mm");
            Console.WriteLine($"dD: {dD:F2}mm");

            return new Dictionary<string, object>
            {
                ["dA"] = dA,
                ["dB"] = dB,
                ["dC"] = dC,
                ["dD"] = dD
            };
        }

        public void testTinhDuongKinhTrucIII()
        {
            double T3 = 840232.2606;
            double Frx = 6927.76;
            double d4 = 304.077;
            double Ft3 = 5785.09;
            double Fr3 = 2191.86;
            double Fa3 = 1174.89;
            double l31 = 204, l32 = 140.75, l33 = 292.75;

            TinhDuongKinhTrucIII(T3, Frx, d4, Ft3, Fr3, Fa3, l31, l32, l33);
        }

        public void testTinhDuongKinhTrucII()
        {
            double T2 = 277452.9161;
            double d2 = 271.9;
            double d3 = 95.92;
            double Ft1 = 2140.139;
            double Fr1 = 822.81;
            double alphatw = 20.37;
            double beta = 11.48;
            double Fa1 = 654.74;
            double l21 = 204, l22 = 63.25, l23 = 140.75;

            TinhDuongKinhTrucII(T2, d2, d3, Ft1, Fr1, alphatw, beta, Fa1, l21, l22, l23);
        }

        public void testTinhDuongKinhTrucI()
        {
            double T1 = 51474.29978;
            int dI = 25;
            double d1 = 48.1037;
            double Beta = 17.0107;
            double alphatw = 20.8379;
            double l11 = 204, l12 = 61.5, l13 = 63.25;
            TinhDuongKinhTrucI(T1, dI, d1, Beta, alphatw, l11, l12, l13);
        }

        public void testDuongKinhTruc()
        {
            double T1 = 51474.29978;
            double T2 = 277452.9161;
            double T3 = 840232.2606;
            double bw1 = 50.4;
            double bw2 = 63;

            TinhDuongKinhTruc(T1, T2, T3, bw1, bw2);
        }

        public Dictionary<string, List<double>> tinhFullTruc(double T1, double T2, double T3, double bw1, double bw2,
        double d1, double beta1, double alphatw1, double beta2, double alphatw2, double d2, double d3, double Frx, double d4)
        {
            var dicTruc = TinhDuongKinhTruc(T1, T2, T3, bw1, bw2);
            var dicT1 = TinhDuongKinhTrucI(T1, (int)dicTruc["dI"], d1, beta1, alphatw1, (double)dicTruc["l11"],
            (double)dicTruc["l12"], (double)dicTruc["l13"]);
            var dicT2 = TinhDuongKinhTrucII(T2, d2, d3, (double)dicT1["Ft1"], (double)dicT1["Fr1"], alphatw2,
            beta2, (double)dicT1["Fa1"], (double)dicTruc["l21"], (double)dicTruc["l22"],
            (double)dicTruc["l23"]);
            var dicT3 = TinhDuongKinhTrucIII(T3, Frx, d4, (double)dicT2["Ft3"], (double)dicT2["Fr3"],
            (double)dicT2["Fa3"], (double)dicTruc["l31"], (double)dicTruc["l32"],
            (double)dicTruc["l33"]);
            var Truc1 = new List<double> { (double)dicT1["dA"], (double)dicT1["dB"] , (double)dicT1["dC"], (double)dicT1["dD"] };
            var Truc2 = new List<double> { (double)dicT2["dA"], (double)dicT2["dB"] , (double)dicT2["dC"], (double)dicT2["dD"] };
            var Truc3 = new List<double> { (double)dicT3["dA"], (double)dicT3["dB"] , (double)dicT3["dC"], (double)dicT3["dD"] };  
            var Len1 = new List<double> {(double)dicTruc["l11"], (double)dicTruc["l12"],(double)dicTruc["l13"]};
            var Len2 = new List<double> {(double) dicTruc["l21"], (double)dicTruc["l22"], (double)dicTruc["l23"]};
            var Len3 = new List<double> {(double)dicTruc["l31"],(double)dicTruc["l32"],(double)dicTruc["l33"]};

            return new Dictionary<string, List<double>>
            {
                ["Truc1"] = Truc1,
                ["Truc2"] = Truc2,
                ["Truc3"] = Truc3,
                ["Len1"] = Len1,
                ["Len2"] = Len2,
                ["Len3"] = Len3
            };
        }

    }
}











// new Calculation2().testDuongKinhTruc();
// new Calculation2().testTinhDuongKinhTrucI();
// new Calculation2().testTinhDuongKinhTrucII();
// new Calculation2().testTinhDuongKinhTrucIII();
