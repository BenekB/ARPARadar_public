using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.IO;
using System.Windows.Documents;

namespace Radar
{
    class EKFFilterHDG
    {
        List<Position> RealPositions = [];
        List<Position> NoisedPositions = [];
        List<Position> FilterOutputPositions = [];
        List<Position> MeanPositions = [];
        List<Position> AveragePositionsList = [];
        LinearApproximation OneStepLinearApproximation = new();
        const int Samples = 6;
        private int N = 0;

        public double T = Param.RadarCircleTimeMs / 1000;

        public Matrix F = new();
        public Matrix G = new();
        public Matrix H = new();
        public Matrix P00 = new();
        public Matrix Q = new();
        public Matrix R = new();
        public Matrix s00 = new();
        public Matrix B = new();

        private Matrix I = new();
        private Matrix s10 = new();
        private Matrix P10 = new();
        private Matrix z10 = new();
        private Matrix z1 = new(4, 1);
        private Matrix e1 = new();
        private Matrix S1 = new();
        private Matrix K1 = new();
        private Matrix s11 = new();
        private Matrix P11 = new();

        Position MyShipPosition = new Position { X = 18520, Y = 18520 };



        public EKFFilterHDG(List<Position> realPositions, List<Position> noisePositions, List<Position> filterOutput, List<Position> meanPositions)
        {
            RealPositions = realPositions;
            NoisedPositions = noisePositions;
            FilterOutputPositions = filterOutput;
            MeanPositions = meanPositions;

            InitializeDefault();

        }

        public EKFFilterHDG(List<Position> noisePositions, List<Position> filterOutput)
        {
            NoisedPositions = noisePositions;
            FilterOutputPositions = filterOutput;

            InitializeDefault();
        }


        public EKFFilterHDG(string path)
        {
            ReadDataFromFile(path);

            InitializeDefault();
        }


        public void OneStepProcess()
        {
            // Używamy tylko NoisedPositions oraz zapisujemy dane w FilterOutputPositions

            //  Jeżeli mamy mniej niż 6 próbek, to nic się nie dziej - za mało danych
            if (NoisedPositions.Count < 6) return;
            N = NoisedPositions.Count; // +6
            
            //  Położenie wypadkowe obliczone jako średnia z ostatnich 6 pomiarów
            Position averagePosition = new() { X = 0, Y = 0 };
            Heading heading;

            for (int i = 1; i <= 6; i++)
            {
                averagePosition.X += NoisedPositions[^i].X;
                averagePosition.Y += NoisedPositions[^i].Y;
            }

            averagePosition.X /= Samples;
            averagePosition.Y /= Samples;
            AveragePositionsList.Add(averagePosition);

            if (N == 6)
            {
                s00[0] = averagePosition.X;
                s00[1] = averagePosition.Y;
                s00[2] = 0;
                s00[3] = 10;
                FilterOutputPositions.Add(averagePosition);
            }
            else
            {
                B[0] = s00[3] * T * Math.Sin(s00[2]);
                B[1] = s00[3] * T * Math.Cos(s00[2]);

                s10 = F * s00 + B;
                P10 = F * P00 * F.T + Q;
                z1[0] = averagePosition.X;
                z1[1] = averagePosition.Y;
                z1[2] = s10[2];
                z1[3] = 10;

                if (N == 16)
                {
                    Q[0, 0] = 0.001;
                    Q[1, 1] = 0.001;
                    Q[2, 2] = 0.0001;
                    Q[3, 3] = 0.0001;
                }

                if (N > 16)
                {
                    (_, _, heading) = OneStepLinearApproximation.Calculate(AveragePositionsList, AveragePositionsList.Count - 11, AveragePositionsList.Count - 1);
                    z1[2] = heading.InRadians;
                }
                
                if (N > 9)
                {
                    z1[3] = Math.Sqrt(Math.Pow(AveragePositionsList[^1].X - AveragePositionsList[^4].X, 2) + Math.Pow(AveragePositionsList[^1].Y - AveragePositionsList[^4].Y, 2)) / (3 * T);
                }

                e1 = z1 - (H * s10);

                if (Heading.ShouldTurnRight(Heading.FromRadians(s10[2]), Heading.FromRadians(z1[2])))
                {
                    e1[2] = ((Heading)Math.Min(Heading.FromRadians(z1[2]) - Heading.FromRadians(s10[2]), Heading.FromRadians(s10[2]) - Heading.FromRadians(z1[2]))).InRadians;
                }
                else
                {
                    e1[2] = -((Heading)Math.Min(Heading.FromRadians(z1[2]) - Heading.FromRadians(s10[2]), Heading.FromRadians(s10[2]) - Heading.FromRadians(z1[2]))).InRadians;
                }

                S1 = H * P10 * H.T + R;
                K1 = P10 * H.T * (S1 ^ -1);
                s11 = s10 + K1 * e1;
                P11 = (I - K1 * H) * P10;
                s11[2] = Heading.FromRadians(s11[2]).InRadians;

                FilterOutputPositions.Add(new Position { X = s11[0], Y = s11[1] });

                s00 = s11.DeepCopy();
                P00 = P11.DeepCopy();
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

                RealPositions.Add(new Position { X = double.Parse(line[0]), Y = double.Parse(line[1]) });
                NoisedPositions.Add(new Position { X = double.Parse(line[2]), Y = double.Parse(line[3]) });

            }

        }

        public void SaveOutputToFile(string path)
        {
            string[] lines = new string[FilterOutputPositions.Count];

            for (int i = 0; i < FilterOutputPositions.Count; i++)
            {
                lines[i] = $"{RealPositions[i].X}\t{RealPositions[i].Y}\t{NoisedPositions[i].X}\t{NoisedPositions[i].Y}\t{FilterOutputPositions[i].X}\t{FilterOutputPositions[i].Y}";
            }

            File.WriteAllLines(path, lines);
        }

        public void InitializeDefault()
        {
            F = new Matrix(new double[,] { { 1, 0, 0, 0 },
                                           { 0, 1, 0, 0 },
                                           { 0, 0, 1, 0 },
                                           { 0, 0, 0, 1 } });

            G = new Matrix(new double[,] { { 0, 0 },
                                           { 0, 0 },
                                           { 1, 0 },
                                           { 0, 1 } });

            H = new Matrix(new double[,] {  { 1, 0, 0, 0 },
                                            { 0, 1, 0, 0 },
                                            { 0, 0, 1, 0 },
                                            { 0, 0, 0, 1 } });

            P00 = new Matrix(new double[,] { {    1,  0,  0,  0   },
                                           {    0,  1,  0,  0   },
                                           {    0,  0,  1,  0   },
                                           {    0,  0,  0,  1   }   });

            Q = new Matrix(new double[,] { {    .1,  0,  0,  0   },
                                           {    0,  .1,  0,  0   },
                                           {    0,  0,  .1,  0   },
                                           {    0,  0,  0,  0.1   }   });

            R = new Matrix(new double[,] {  { .051, 0, 0, 0 },
                                            { 0, .051, 0, 0 },
                                            { 0, 0, 0.11, 0 },
                                            { 0, 0, 0, 11.51} });

            s00 = new Matrix(new double[,] {{ 0  },
                                            { 0  },
                                            { 0.0  },
                                            { 10.0  } });

            I = Matrix.Identity(P00.Rows);

            B = new Matrix(new double[,] {  { 0  },
                                            { 0  },
                                            { 0.0  },
                                            { 0.0  } });
        }

        public void Initialize()
        {
            //  To be implemented.
        }

        public void ProcessAll()
        {
            List<Position> list = new();
            Position sumPos = new() { X = 0, Y = 0 };
            Heading heading = new Heading();
            
            LinearApproximation linearApproximation = new LinearApproximation();
            int samples = 6;

            for (int i = samples; i < this.NoisedPositions.Count; i++)
            {
                sumPos.X = 0;
                sumPos.Y = 0;

                for (int j = i - samples; j < i; j++)
                {
                    sumPos.X += NoisedPositions[j].X;
                    sumPos.Y += NoisedPositions[j].Y;
                }

                sumPos.X /= samples;
                sumPos.Y /= samples;

                list.Add(sumPos);
                MeanPositions.Add(sumPos);

            }


            s00[0] = list[0].X;
            s00[1] = list[0].Y;
            s00[2] = 0;
            s00[3] = 10;
            FilterOutputPositions.Add(list[0]);
            

            for (int i = 1; i < list.Count; i++)
            {
              
                B[0] = s00[3] * T * Math.Sin(s00[2]);
                B[1] = s00[3] * T * Math.Cos(s00[2]);

                s10 = F * s00 + B;
                P10 = F * P00 * F.T + Q;
                z1[0] = list[i].X;
                z1[1] = list[i].Y;
                z1[2] = s10[2];
                z1[3] = 10;

                if (i == 10)
                {
                    Q[0, 0] = 0.001;
                    Q[1, 1] = 0.001;
                    Q[2, 2] = 0.0001;
                    Q[3, 3] = 0.0001;
                }
                if (i > 10)
                {
                    // z1[2] = Heading.HeadingBetweenPositions(list[i], list[i - 9]).InRadians;

                    (_, _, heading) = linearApproximation.Calculate(list, i - 10, i);
                    z1[2] = heading.InRadians;

                }
                if (i > 3)
                {
                    z1[3] = Math.Sqrt(Math.Pow(list[i].X - list[i - 3].X, 2) + Math.Pow(list[i].Y - list[i - 3].Y, 2)) / (3 * T);
                }


                e1 = z1 - (H * s10);

                if (Heading.ShouldTurnRight(Heading.FromRadians(s10[2]), Heading.FromRadians(z1[2])))
                {
                    e1[2] = ((Heading)Math.Min(Heading.FromRadians(z1[2]) - Heading.FromRadians(s10[2]), Heading.FromRadians(s10[2]) - Heading.FromRadians(z1[2]))).InRadians;
                }
                else
                {
                    e1[2] = -((Heading)Math.Min(Heading.FromRadians(z1[2]) - Heading.FromRadians(s10[2]), Heading.FromRadians(s10[2]) - Heading.FromRadians(z1[2]))).InRadians;
                }

                S1 = H * P10 * H.T + R;
                K1 = P10 * H.T * (S1 ^ -1);
                s11 = s10 + K1 * e1;
                P11 = (I - K1 * H) * P10;
                s11[2] = Heading.FromRadians(s11[2]).InRadians;

                FilterOutputPositions.Add(new Position { X = s11[0], Y = s11[1] });

                s00 = s11.DeepCopy();
                P00 = P11.DeepCopy();
            }

        }
    }
}
