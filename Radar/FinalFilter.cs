using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Numerics;
using MathNet.Numerics;
using MathNet.Numerics.Providers.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using Matrix = MathNet.Numerics.LinearAlgebra.Matrix<double>;
using ClassLibrary;
using System.Globalization;
using System.Threading;
using System.IO;
using Vector = System.Windows.Vector;
using System.Runtime.CompilerServices;




namespace Radar
{

    //  Bardzo podobny do EKFFilter w wersji pierwotnej.
    //  Dodano wyznaczanie obserwowanej prędkości na podstawie zwykłego równania Pitagorasa przy wykorzystaniu
    //  pomiarów zaszumionych.
    public class FinalFilter : IFilter
    {
        ShipsPosition MyShipsPosition = new();
        List<Position> RealPositions = [];
        List<Position> NoisedPositions = [];
        List<SphericalPosition> NoisedSphericalPositions = [];
        List<Position> FilterOutputPositions = [];
        public List<Position> filterHDGOutput = [];
        List<Position> MeanPositions = [];
        List<Position> filterSpeedOutput = [];
        /// <summary>Zawiera kurs wyznaczony za pomocą aproksymacji liniowej na podstawie pomiarów z ostatniej minuty.</summary>
        List<Heading> shortHeadings = [];
        /// <summary>Różnica pomiędzy aproksymowanym i rzeczywistym kursem w stopniach.</summary>
        List<double> shortHeadingsDifference = [];
        /// <summary>Zawiera kurs wyznaczony za pomocą aproksymacji liniowej na podstawie pomiarów z ostatnich trzech minut.</summary>
        public List<Heading> longHeadings = [];
        /// <summary>Różnica pomiędzy aproksymowanym i rzeczywistym kursem w stopniach.</summary>
        List<double> longHeadingsDifference = [];
        /// <summary>Lista zawiera rzeczywisty TCPA [minuty]</summary>
        List<double> RealTCPA = [];
        /// <summary>Lista zawiera rzeczywisty CPA [mile morskie]</summary>
        List<double> RealCPA = [];
        /// <summary>Lista zawiera wyliczony TCPA [minuty]</summary>
        List<double> PredictedTCPA = [];
        /// <summary>Lista zawiera wyliczony CPA [mile morskie]</summary>
        List<double> PredictedCPA = [];
        /// <summary>Lista zawiera wyliczony TCPA z ostatniej minuty[minuty]</summary>
        List<double> PredictedShortTCPA = [];
        /// <summary>Lista zawiera wyliczony CPA z ostatniej minuty [mile morskie]</summary>
        List<double> PredictedShortCPA = [];
        /// <summary>Lista zawiera błąd bezwzględny wyznaczenia CPA [mile morskie]</summary>
        List<double> ErrorCPA = [];
        /// <summary>Lista zawiera błąd bezwzględny wyznaczenia TCPA [minuty]</summary>
        List<double> ErrorTCPA = [];
        /// <summary>Średnia ShortHeadings z ostatniej minuty</summary>
        List<double> MeanLongHeading = [];
        List<double> positionError = [];
        List<double> meanSpeedList = [];
        List<double> ShortSpeedList = [];
        List<double> SpeedDifferences = [];
        const int NumberOfSamples = 6;

        EKFFilterHDG? OneStepFilterHDG = null;
        EKFFilterHDG? OneStepFilterSpeed = null;

        FilterOutputData FilterOutputToARPA = new();

        LinearApproximation LinearApproximationInstance = new();

        public double T = Param.RadarCircleTimeMs / 1000;

        //Position MyShipdPosition = new() { X = 18520, Y = 18520 };

        List<Position> MyOwnShipsPositions = [];



        public FinalFilter(List<Position> realPositions, List<Position> noisePositions, List<SphericalPosition> noiseSphericalPosition, List<Position> filterOutput)
        {
            RealPositions = realPositions;
            NoisedPositions = noisePositions;
            NoisedSphericalPositions = noiseSphericalPosition;
            FilterOutputPositions = filterOutput;

        }


        public FinalFilter(List<Position> noisePositions, List<Position> filterOutput, ShipsPosition actualShipsPosition, FilterOutputData filterOutputData)
        {
            NoisedPositions = noisePositions;
            FilterOutputPositions = filterOutput;
            MyShipsPosition = actualShipsPosition;
            FilterOutputToARPA = filterOutputData;
        }


        public FinalFilter(string path)
        {
            ReadDataFromFile(path);
        }


        public void OneStepProcess()
        {
            OneStepFilterHDG ??= new(NoisedPositions, filterHDGOutput);
            OneStepFilterSpeed ??= new(NoisedPositions, filterSpeedOutput);

            OneStepFilterHDG.OneStepProcess();
            OneStepFilterSpeed.OneStepProcess();

            //  HEADING
            {
                if (filterHDGOutput.Count < 68 && filterHDGOutput.Count > 18)
                {
                    Heading hdg1, hdg2;

                    (_, _, hdg1) = LinearApproximationInstance.Calculate(filterHDGOutput, filterHDGOutput.Count - 19, filterHDGOutput.Count - 1);
                    shortHeadings.Add(hdg1);
                    FilterOutputToARPA.ShortHeading = hdg1;

                    (_, _, hdg2) = LinearApproximationInstance.Calculate(filterHDGOutput, 0, filterHDGOutput.Count - 1);
                    longHeadings.Add(hdg2);
                    FilterOutputToARPA.LongHeading = hdg2;
                }
                else if (filterHDGOutput.Count >= 68)
                {
                    Heading hdg1, hdg2;

                    (_, _, hdg1) = LinearApproximationInstance.Calculate(filterHDGOutput, filterHDGOutput.Count - 19, filterHDGOutput.Count - 1);
                    shortHeadings.Add(hdg1);
                    FilterOutputToARPA.ShortHeading = hdg1;

                    (_, _, hdg2) = LinearApproximationInstance.Calculate(filterHDGOutput, filterHDGOutput.Count - 68, filterHDGOutput.Count - 1);
                    longHeadings.Add(hdg2);
                    FilterOutputToARPA.LongHeading = hdg2;
                }
            }

            //  SPEED
            {
                double meanSpeed;
                double a, b, sum;

                if (filterSpeedOutput.Count > 12 && filterSpeedOutput.Count < 61)
                {
                    a = filterSpeedOutput[^13].X - filterSpeedOutput[^1].X;
                    b = filterSpeedOutput[^13].Y - filterSpeedOutput[^1].Y;
                    sum = Math.Pow(a, 2) + Math.Pow(b, 2);
                    meanSpeed = Math.Sqrt(sum) / 12 / (Param.RadarCircleTimeMs / 1000);

                    ShortSpeedList.Add(meanSpeed * 0.8);
                    FilterOutputToARPA.ShortSpeed = meanSpeed * 0.8;

                    a = filterSpeedOutput[0].X - filterSpeedOutput[^1].X;
                    b = filterSpeedOutput[0].Y - filterSpeedOutput[^1].Y;
                    sum = Math.Pow(a, 2) + Math.Pow(b, 2);
                    meanSpeed = Math.Sqrt(sum) / (filterSpeedOutput.Count - 1) / (Param.RadarCircleTimeMs / 1000);

                    meanSpeedList.Add(meanSpeed * 0.8);
                    FilterOutputToARPA.LongSpeed = meanSpeed * 0.8;
                }
                else if (filterSpeedOutput.Count > 60)
                {
                    a = filterSpeedOutput[^13].X - filterSpeedOutput[^1].X;
                    b = filterSpeedOutput[^13].Y - filterSpeedOutput[^1].Y;
                    sum = Math.Pow(a, 2) + Math.Pow(b, 2);
                    meanSpeed = Math.Sqrt(sum) / 12 / (Param.RadarCircleTimeMs / 1000);

                    ShortSpeedList.Add(meanSpeed * 0.8);
                    FilterOutputToARPA.ShortSpeed = meanSpeed * 0.8;

                    a = filterSpeedOutput[^61].X - filterSpeedOutput[^1].X;
                    b = filterSpeedOutput[^61].Y - filterSpeedOutput[^1].Y;
                    sum = Math.Pow(a, 2) + Math.Pow(b, 2);
                    meanSpeed = Math.Sqrt(sum) / 60 / (Param.RadarCircleTimeMs / 1000);

                    meanSpeedList.Add(meanSpeed * 0.8);
                    FilterOutputToARPA.LongSpeed = meanSpeed * 0.8;
                }
            }

            //  CPA and TCPA over last 3 minutes
            if (meanSpeedList.Count > 0 && longHeadings.Count > 0 && filterHDGOutput.Count > 0)
            {
                double v1x, v2x, v1y, v2y;
                double x10, y10, x20, y20;
                double a, b, c;
                double delta;
                double t, d;

                double x1, x2, y1, y2;

                v1x = meanSpeedList[^1] * Math.Sin(longHeadings[^1].InRadians);
                v1y = meanSpeedList[^1] * Math.Cos(longHeadings[^1].InRadians);

                v2x = MyShipsPosition.Speed * Math.Sin(MyShipsPosition.HDG.InRadians);
                v2y = MyShipsPosition.Speed * Math.Cos(MyShipsPosition.HDG.InRadians);

                x10 = filterHDGOutput[^1].X;
                y10 = filterHDGOutput[^1].Y;

                x20 = MyShipsPosition.X;
                y20 = MyShipsPosition.Y;

                a = Pow(v1x) + Pow(v1y) - 2 * v1x * v2x - 2 * v1y * v2y + Pow(v2x) + Pow(v2y);

                b = 2 * (x10 * v1x + y10 * v1y - x10 * v2x - y10 * v2y - x20 * v1x - y20 * v1y + x20 * v2x + y20 * v2y);

                c = Pow(x10) + Pow(y10) - 2 * x10 * x20 - 2 * y10 * y20 + Pow(x20) + Pow(y20);

                delta = Pow(b) - 4 * a * c;

                t = -(b / (2 * a));

                d = -(delta / (4 * a));

                if (t < 0) t = 0;

                x1 = x10 + t * v1x;
                x2 = x20 + t * v2x;
                y1 = y10 + t * v1y;
                y2 = y20 + t * v2y;

                PredictedTCPA.Add(t / 60);
                PredictedCPA.Add(Math.Sqrt(Pow(x1 - x2) + Pow(y1 - y2)) * Param.MetersToNm);
                FilterOutputToARPA.TCPA = (t / 60);
                FilterOutputToARPA.CPA = Math.Sqrt(Pow(x1 - x2) + Pow(y1 - y2)) * Param.MetersToNm;
            }

            //  CPA and TCPA over last 1 minute
            if (ShortSpeedList.Count > 0 && shortHeadings.Count > 0 && filterHDGOutput.Count > 0)
            {
                double v1x, v2x, v1y, v2y;
                double x10, y10, x20, y20;
                double a, b, c;
                double delta;
                double t, d;

                double x1, x2, y1, y2;

                v1x = ShortSpeedList[^1] * Math.Sin(shortHeadings[^1].InRadians);
                v1y = ShortSpeedList[^1] * Math.Cos(shortHeadings[^1].InRadians);

                v2x = MyShipsPosition.Speed * Math.Sin(MyShipsPosition.HDG.InRadians);
                v2y = MyShipsPosition.Speed * Math.Cos(MyShipsPosition.HDG.InRadians);

                x10 = filterHDGOutput[^1].X;
                y10 = filterHDGOutput[^1].Y;

                x20 = MyShipsPosition.X;
                y20 = MyShipsPosition.Y;

                a = Pow(v1x) + Pow(v1y) - 2 * v1x * v2x - 2 * v1y * v2y + Pow(v2x) + Pow(v2y);

                b = 2 * (x10 * v1x + y10 * v1y - x10 * v2x - y10 * v2y - x20 * v1x - y20 * v1y + x20 * v2x + y20 * v2y);

                c = Pow(x10) + Pow(y10) - 2 * x10 * x20 - 2 * y10 * y20 + Pow(x20) + Pow(y20);

                delta = Pow(b) - 4 * a * c;

                t = -(b / (2 * a));

                d = -(delta / (4 * a));

                if (t < 0) t = 0;

                x1 = x10 + t * v1x;
                x2 = x20 + t * v2x;
                y1 = y10 + t * v1y;
                y2 = y20 + t * v2y;

                PredictedShortTCPA.Add(t / 60);
                PredictedShortCPA.Add(Math.Sqrt(Pow(x1 - x2) + Pow(y1 - y2)) * Param.MetersToNm);
                FilterOutputToARPA.ShortTCPA = t / 60;
                FilterOutputToARPA.ShortCPA = Math.Sqrt(Pow(x1 - x2) + Pow(y1 - y2)) * Param.MetersToNm;

            }
        }

        public void ReadDataFromFile(string path)
        {
            string[] records = File.ReadAllLines(path);
            string[] line;
            Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("en-US");

            foreach (string record in records)
            {
                line = record.Split('\t');

                RealPositions.Add(new Position { X = double.Parse(line[0]), Y = double.Parse(line[1]) , HDG = double.Parse(line[2]), Speed = double.Parse(line[3])});
                NoisedPositions.Add(new Position { X = double.Parse(line[4]), Y = double.Parse(line[5]) });
                MyOwnShipsPositions.Add(new Position { X = double.Parse(line[6]), Y = double.Parse(line[7]), HDG = double.Parse(line[8]), Speed = double.Parse(line[9])});
            }

        }

        public void SaveOutputToFile(string path)
        {
            string[] lines = new string[RealPositions.Count];

            int j, k, l, m, n, o;

            j = RealPositions.Count - filterSpeedOutput.Count;
            k = RealPositions.Count - shortHeadings.Count;
            l = RealPositions.Count - longHeadings.Count;
            m = RealPositions.Count - meanSpeedList.Count;
            n = RealPositions.Count - PredictedCPA.Count;
            o = RealPositions.Count - MeanPositions.Count;

            for (int i = 0; i < RealPositions.Count; i++)
            {
                lines[i] = $"{i}\t" +
                    $"{RealPositions[i].X}\t" +
                    $"{RealPositions[i].Y}\t" +
                    $"{RealPositions[i].HDG.ToString()}\t" +
                    $"{RealPositions[i].Speed}\t" +
                    $"{NoisedPositions[i].X}\t" +
                    $"{NoisedPositions[i].Y}\t";

                if (i < j)
                    lines[i] += "x\t" +
                        "x\t" +
                        "x\t" ;
                else
                    lines[i] += $"{filterSpeedOutput[i - j].X}\t" +
                        $"{filterSpeedOutput[i - j].Y}\t" +
                        $"{positionError[i - j]}\t";

                if (i < k)
                    lines[i] += "x\tx\t";
                else
                    lines[i] += $"{shortHeadings[i - k].ToString()}\t{shortHeadingsDifference[i - k]}\t";

                if (i < l)
                    lines[i] += "x\tx\t";
                else
                    lines[i] += $"{longHeadings[i - l].ToString()}\t{longHeadingsDifference[i - l]}\t";

                if (i < m)
                    lines[i] += "x\tx\t";
                else
                    lines[i] += $"{meanSpeedList[i - m]}\t{SpeedDifferences[i - m]}\t";

                lines[i] += $"{RealCPA[i]}\t{RealTCPA[i]}\t";

                if (i < n)
                    lines[i] += $"x\tx\tx\tx\t";
                else
                    lines[i] += $"{PredictedCPA[i - n]}\t{PredictedTCPA[i - n]}\t{ErrorCPA[i - n]}\t{ErrorTCPA[i - n]}\t";

                if (i < o)
                    lines[i] += $"x\tx\t";
                else
                    lines[i] += $"{MeanPositions[i - o].X}\t{MeanPositions[i - o].Y}\t";


            }

            File.WriteAllLines(path, lines);
        }

        public void Initialize()
        {
            //  To be implemented.
        }
        

        public void ProcessAll()
        {
            
            EKFFilterHDG filterHDG = new(RealPositions, NoisedPositions, filterHDGOutput, MeanPositions);
            filterHDG.ProcessAll();

            EKFFilterSpeed filterSpeed = new(RealPositions, NoisedPositions, filterSpeedOutput);
            filterSpeed.ProcessAll();

            //  Wyznaczanie kursu poprzez aproksymację liniową 
            CalculateHeading();

            //  Oblicznie odległości położenia rzeczywistego od obliczonego
            CalculatePositionError();

            //  Obliczanie błędu bezwzględnego kursu statku
            CalculateHeadingsDifferences();

            //  Obliczanie średniej prędkości
            CalculateMeanSpeed();

            //  Obliczanie błędu względnego prędkości
            CalculateSpeedError();

            // Obliczanie CPA, TCPA
            CalculateRealCPAandTCPA();

            // Obliczanie przewidywanego CPA, TCPA
            CalculatePredictedCPAandTCPA();

            // Obliczanie przewidywanego CPA, TCPA
            CalculatePredictedShortCPAandTCPA();

            // Obliczanie błędu bezwzględnego CPA oraz TCPA
            CalculateErrorCPAandTCPA();

        }

        private void CalculateErrorCPAandTCPA()
        {
            //  CPA
            int difference = RealCPA.Count - PredictedCPA.Count;
            double sum;

            for (int i = difference; i < RealCPA.Count; i++)
            {
                sum = Math.Abs(RealCPA[i] - PredictedCPA[i - difference]);
                ErrorCPA.Add(sum);
            }

            difference = RealTCPA.Count - PredictedTCPA.Count;

            for (int i = difference; i < RealTCPA.Count; i++)
            {
                sum = Math.Abs(RealTCPA[i] - PredictedTCPA[i - difference]);
                ErrorTCPA.Add(sum);
            }
        }

        private void CalculatePredictedShortCPAandTCPA()
        {
            double v1x, v2x, v1y, v2y;
            double x10, y10, x20, y20;
            double a, b, c;
            double delta;
            double t, d;

            double x1, x2, y1, y2;

            for (int i = 0; i < ShortSpeedList.Count; i++)
            {
                v1x = ShortSpeedList[i] * Math.Sin(shortHeadings[i].InRadians);
                v1y = ShortSpeedList[i] * Math.Cos(shortHeadings[i].InRadians);

                v2x = MyOwnShipsPositions[i + 24].Speed * Math.Sin(MyOwnShipsPositions[i + 24].HDG.InRadians);
                v2y = MyOwnShipsPositions[i + 24].Speed * Math.Cos(MyOwnShipsPositions[i + 24].HDG.InRadians);

                x10 = filterHDGOutput[i + 18].X;
                y10 = filterHDGOutput[i + 18].Y;

                x20 = MyOwnShipsPositions[i + 24].X;
                y20 = MyOwnShipsPositions[i + 24].Y;

                a = Pow(v1x) + Pow(v1y) - 2 * v1x * v2x - 2 * v1y * v2y + Pow(v2x) + Pow(v2y);

                b = 2 * (x10 * v1x + y10 * v1y - x10 * v2x - y10 * v2y - x20 * v1x - y20 * v1y + x20 * v2x + y20 * v2y);

                c = Pow(x10) + Pow(y10) - 2 * x10 * x20 - 2 * y10 * y20 + Pow(x20) + Pow(y20);

                delta = Pow(b) - 4 * a * c;

                t = -(b / (2 * a));

                d = -(delta / (4 * a));

                if (t < 0) t = 0;

                x1 = x10 + t * v1x;
                x2 = x20 + t * v2x;
                y1 = y10 + t * v1y;
                y2 = y20 + t * v2y;

                //Console.WriteLine($"{x1}\t{x2}\t{y1}\t{y2}\t{Math.Sqrt(Pow(x1-x2) + Pow(y1-y2))}");

                PredictedShortTCPA.Add(t / 60);
                PredictedShortCPA.Add(Math.Sqrt(Pow(x1 - x2) + Pow(y1 - y2)) * Param.MetersToNm);
            }
        }

        private void CalculatePredictedCPAandTCPA()
        {
            double v1x, v2x, v1y, v2y;
            double x10, y10, x20, y20;
            double a, b, c;
            double delta;
            double t, d;

            double x1, x2, y1, y2;

            for (int i = 0; i < meanSpeedList.Count; i++)
            {
                v1x = meanSpeedList[i] * Math.Sin(longHeadings[i].InRadians);
                v1y = meanSpeedList[i] * Math.Cos(longHeadings[i].InRadians);

                v2x = MyOwnShipsPositions[i + 24].Speed * Math.Sin(MyOwnShipsPositions[i + 24].HDG.InRadians);
                v2y = MyOwnShipsPositions[i + 24].Speed * Math.Cos(MyOwnShipsPositions[i + 24].HDG.InRadians);

                x10 = filterHDGOutput[i + 18].X;
                y10 = filterHDGOutput[i + 18].Y;

                x20 = MyOwnShipsPositions[i + 24].X;
                y20 = MyOwnShipsPositions[i + 24].Y;

                a = Pow(v1x) + Pow(v1y) - 2 * v1x * v2x - 2 * v1y * v2y + Pow(v2x) + Pow(v2y);

                b = 2 * (x10 * v1x + y10 * v1y - x10 * v2x - y10 * v2y - x20 * v1x - y20 * v1y + x20 * v2x + y20 * v2y);

                c = Pow(x10) + Pow(y10) - 2 * x10 * x20 - 2 * y10 * y20 + Pow(x20) + Pow(y20);

                delta = Pow(b) - 4 * a * c;

                t = -(b / (2 * a));

                d = -(delta / (4 * a));

                if (t < 0)  t = 0;

                x1 = x10 + t * v1x;
                x2 = x20 + t * v2x;
                y1 = y10 + t * v1y;
                y2 = y20 + t * v2y;

                //Console.WriteLine($"{x1}\t{x2}\t{y1}\t{y2}\t{Math.Sqrt(Pow(x1-x2) + Pow(y1-y2))}");

                PredictedTCPA.Add(t / 60);
                PredictedCPA.Add(Math.Sqrt(Pow(x1 - x2) + Pow(y1 - y2)) * Param.MetersToNm);
            }
        }

        private void CalculateRealCPAandTCPA()
        {
            double v1x, v2x, v1y, v2y;
            double x10, y10, x20, y20;
            double a, b, c;
            double delta;
            double t, d;

            double x1, x2, y1, y2;

            for (int i = 0; i < RealPositions.Count; i++)
            {
                v1x = RealPositions[i].Speed * Math.Sin(RealPositions[i].HDG.InRadians);
                v1y = RealPositions[i].Speed * Math.Cos(RealPositions[i].HDG.InRadians);

                v2x = MyOwnShipsPositions[i].Speed * Math.Sin(MyOwnShipsPositions[i].HDG.InRadians);
                v2y = MyOwnShipsPositions[i].Speed * Math.Cos(MyOwnShipsPositions[i].HDG.InRadians);

                x10 = RealPositions[i].X;
                y10 = RealPositions[i].Y;

                x20 = MyOwnShipsPositions[i].X;
                y20 = MyOwnShipsPositions[i].Y;

                a = Pow(v1x) + Pow(v1y) - 2 * v1x * v2x - 2 * v1y * v2y + Pow(v2x) + Pow(v2y);

                b = 2 * ( x10 * v1x + y10 * v1y - x10 * v2x - y10 * v2y - x20 * v1x - y20 * v1y + x20 * v2x + y20 * v2y );

                c = Pow(x10) + Pow(y10) - 2 * x10 * x20 - 2 * y10 * y20 + Pow(x20) + Pow(y20);

                delta = Pow(b) - 4 * a * c;

                t = - (b / (2 * a));

                d = -(delta / (4 * a));

                

                //Console.WriteLine($"{x1}\t{x2}\t{y1}\t{y2}\t{Math.Sqrt(Pow(x1 - x2) + Pow(y1 - y2))}");
                if (t < 0)   t = 0;

                x1 = x10 + t * v1x;
                x2 = x20 + t * v2x;
                y1 = y10 + t * v1y;
                y2 = y20 + t * v2y;

                RealTCPA.Add(t / 60);
                RealCPA.Add(Math.Sqrt(Pow(x1 - x2) + Pow(y1 - y2)) * Param.MetersToNm);
            }
        }

        private void CalculateSpeedError()
        {
            int difference = RealPositions.Count - meanSpeedList.Count;
            double error;

            for (int i =  0; i < meanSpeedList.Count; i++)
            {
                error = (meanSpeedList[i] - RealPositions[i + difference].Speed) / RealPositions[i + difference].Speed;
                SpeedDifferences.Add(error * 100);
            }
        }

        private void CalculateMeanSpeed()
        {
            double meanSpeed;
            double a, b, sum;
            int coeff1 = 12;
            int coeff2 = 60;

            //  Wyznaczanie prędkości
            for (int i = coeff1; i < filterSpeedOutput.Count; i++)
            {
                a = filterSpeedOutput[i - coeff1].X - filterSpeedOutput[i].X;
                b = filterSpeedOutput[i - coeff1].Y - filterSpeedOutput[i].Y;
                sum = Math.Pow(a, 2) + Math.Pow(b, 2);
                meanSpeed = Math.Sqrt(sum) / coeff1 / (Param.RadarCircleTimeMs / 1000);

                ShortSpeedList.Add(meanSpeed * 0.8);
            }

            //  Wyznaczanie prędkości
            for (int i = coeff1; i < coeff2; i++)
            {
                a = filterSpeedOutput[0].X - filterSpeedOutput[i].X;
                b = filterSpeedOutput[0].Y - filterSpeedOutput[i].Y;
                sum = Math.Pow(a, 2) + Math.Pow(b, 2);
                meanSpeed = Math.Sqrt(sum) / i / (Param.RadarCircleTimeMs / 1000);

                meanSpeedList.Add(meanSpeed * 0.8);
            }

            //  Wyznaczanie prędkości
            for (int i = coeff2; i < filterSpeedOutput.Count; i++)
            {
                a = filterSpeedOutput[i - coeff2].X - filterSpeedOutput[i].X;
                b = filterSpeedOutput[i - coeff2].Y - filterSpeedOutput[i].Y;
                sum = Math.Pow(a, 2) + Math.Pow(b, 2);
                meanSpeed = Math.Sqrt(sum) / coeff2 / (Param.RadarCircleTimeMs / 1000);

                meanSpeedList.Add(meanSpeed * 0.8);
            }
        }

        private void CalculateHeadingsDifferences()
        {
            //  Short Heading
            int shortDifference = RealPositions.Count - shortHeadings.Count;
            int longDifference = RealPositions.Count - longHeadings.Count;
            double trueHeading, falseHeading;

            for (int i = 0; i < shortHeadings.Count; i++)
            {
                trueHeading = RealPositions[i + shortDifference].HDG.InRadians;
                falseHeading = shortHeadings[i].InRadians;
                shortHeadingsDifference.Add((falseHeading - trueHeading) * 360 / (2 * Math.PI));
            }

            for (int i = 0; i < longHeadings.Count; i++)
            {
                trueHeading = RealPositions[i + longDifference].HDG.InRadians;
                falseHeading = longHeadings[i].InRadians;
                longHeadingsDifference.Add((falseHeading - trueHeading) * 360 / (2 * Math.PI));
            }
        }

        private void CalculateHeading()
        {
            LinearApproximation linearApproximation = new();
            Heading hdg1, hdg2;

            //  Wyznaczanie kursu
            for (int i = 18; i < filterHDGOutput.Count; i++)
            {
                (_, _, hdg1) = linearApproximation.Calculate(filterHDGOutput, i - 18, i);
                //Console.WriteLine(Math.Abs(hdg1.Value - 308).ToString());
                shortHeadings.Add(hdg1);

                //  i >=67
                if (i <= 66)
                {
                    (_, _, hdg2) = linearApproximation.Calculate(filterHDGOutput, 0, i);

                    //Console.WriteLine(Math.Abs(hdg1.Value - 308).ToString() + "\t" + Math.Abs(hdg2.Value - 308).ToString());
                    longHeadings.Add(hdg2);

                }
                else
                {
                    (_, _, hdg2) = linearApproximation.Calculate(filterHDGOutput, i - 66, i);

                    //Console.WriteLine(Math.Abs(hdg1.Value - 308).ToString() + "\t" + Math.Abs(hdg2.Value - 308).ToString());
                    longHeadings.Add(hdg2);
                }

            }


        }

        private void CalculatePositionError()
        {
            double error;
            int difference = RealPositions.Count - filterHDGOutput.Count;

            for (int i = difference; i < RealPositions.Count; i++)
            {
                error = 0;
                error += Math.Pow(RealPositions[i].X - filterHDGOutput[i - difference].X, 2);
                error += Math.Pow(RealPositions[i].Y - filterHDGOutput[i - difference].Y, 2);

                positionError.Add(Math.Sqrt(error));
            }
        }

        private static double Pow(double input)
        {
            return Math.Pow(input, 2);
        }

        public void InitializeDefault()
        {
            throw new NotImplementedException();
        }


    }

    
}