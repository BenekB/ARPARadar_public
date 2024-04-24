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

namespace Radar
{
    public class EKFFilter : IFilter
    {
        readonly List<Position> RealPositions = [];
        readonly List<Position> NoisedPositions = [];
        readonly List<Position> NoisedSphericalPositions = [];
        readonly List<Position> FilterOutputPositions = [];

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
        private readonly Matrix z1 = new(4, 1);
        private Matrix e1 = new();
        private Matrix S1 = new();
        private Matrix K1 = new();
        private Matrix s11 = new();
        private Matrix P11 = new();
        


        public EKFFilter(List<Position> realPositions, List<Position> noisePositions, List<Position> noiseSphericalPosition, List<Position> filterOutput)
        {
            RealPositions = realPositions;
            NoisedPositions = noisePositions;
            NoisedSphericalPositions = noiseSphericalPosition;
            FilterOutputPositions = filterOutput;

            InitializeDefault();

        }


        public EKFFilter(string path)
        {
            ReadDataFromFile(path);

            InitializeDefault();
        }


        public void OneStepProcess()
        {
            if (NoisedPositions.Count == 0) return;
            if (NoisedPositions.Count == 1)
            {
                s00[0] = NoisedPositions[0].X;
                s00[1] = NoisedPositions[0].Y;
                FilterOutputPositions.Add(NoisedPositions[0]);
                return;
            }

            s10 = F * s00;
            P10 = F * P00 * F.T + G * Q * G.T;
            z10 = H * s10;
            z1[0] = NoisedPositions[^1].X; z1[1] = NoisedPositions[^1].Y;
            e1 = z1 - z10;
            S1 = H * P10 * H.T + R;
            K1 = P10 * H.T * (S1 ^ -1);
            s11 = s10 + K1 * e1;
            P11 = (I - K1 * H) * P10;

            FilterOutputPositions.Add(new Position { X = s11[0], Y = s11[1] });

            Console.WriteLine(FilterOutputPositions[^1]);

            P00 = P11.DeepCopy();
            s00 = s11.DeepCopy();
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
                NoisedSphericalPositions.Add(new Position { X = double.Parse(line[4]), Y = double.Parse(line[5]) });
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

            P00 = new Matrix(new double[,] { {    25,  0,  0,  0   },
                                           {    0,  25,  0,  0   },
                                           {    0,  0,  25,  0   },
                                           {    0,  0,  0,  25   }   });

            Q = new Matrix(new double[,] { {    0.004,  0,  0,  0   },
                                           {    0,  0.004,  0,  0   },
                                           {    0,  0,  0.004,  0   },
                                           {    0,  0,  0,  0.004   }   });

            R = new Matrix(new double[,] {  { .1, 0, 0, 0 },
                                            { 0, .1, 0, 0 },
                                            { 0, 0, .1, 0 },
                                            { 0, 0, 0, .1 } });

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
            List<Position> list1 = new();
            Position sumPos = new() { X = 0, Y = 0 };
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

                list1.Add(sumPos);

            }



            LinearApproximation linApp = new();
            List<Position> list = new();
            s00[0] = list1[0].X;
            s00[1] = list1[0].Y;
            s00[2] = 0;
            s00[3] = 10.0;
            FilterOutputPositions.Add(list1[0]);
            list.Add(list1[0]);

            for (int i = 1; i < list1.Count; i++)
            {
                /*if (i > 4)
                {
                    s00[2] = Heading.HeadingBetweenPositions(NoisedPositions[i], NoisedPositions[i - 5]).InRadians;
                    Console.WriteLine(s00[2]);
                }*/
                list.Add(list1[i]);

                B[0] = s00[3] * T * Math.Sin(s00[2]);
                B[1] = s00[3] * T * Math.Cos(s00[2]);

                s10 = F * s00 + B;
                P10 = F * P00 * F.T + Q;
                z1[0] = list1[i].X; 
                z1[1] = list1[i].Y;
                z1[2] = s10[2];
                if (i > 10)
                {
                    z1[2] = Heading.HeadingBetweenPositions(list1[i], list1[i - 10]).InRadians;
                    Console.WriteLine($"{Heading.FromRadians(z1[2]).Value}\t{Heading.HeadingBetweenPositions(list1[i], list1[i - 5]).Value}");
                    list.RemoveAt(0);
                    linApp.Calculate(list);

                }
                z1[3] = 10.0;
                //z1[3] = Math.Sqrt(Math.Pow(NoisedPositions[i].X - NoisedPositions[i-1].X,2) + Math.Pow(, 2)  );
                
                e1 = z1 - (H * s10);
                if (Heading.ShouldTurnRight(Heading.FromRadians(s10[2]), Heading.FromRadians(z1[2])))
                {
                    e1[2] = ((Heading)Math.Min(Heading.FromRadians(z1[2]) - Heading.FromRadians(s10[2]), Heading.FromRadians(s10[2]) - Heading.FromRadians(z1[2]))).InRadians;
                }
                else
                {
                    e1[2] = -((Heading)Math.Min(Heading.FromRadians(z1[2]) - Heading.FromRadians(s10[2]), Heading.FromRadians(s10[2]) - Heading.FromRadians(z1[2]))).InRadians;
                }
                // e1[2] = ((Heading)Math.Min(Heading.FromRadians(z1[2]) - Heading.FromRadians(s10[2]), Heading.FromRadians(s10[2]) - Heading.FromRadians(z1[2]))).InRadians;
                S1 = H * P10 * H.T + R;
                K1 = P10 * H.T * (S1 ^ -1);
                s11 = s10 + K1 * e1;
                P11 = (I - K1 * H) * P10;
                s11[2] = (Heading.FromRadians(s11[2])).InRadians;
                // Console.WriteLine($"{s11[2]}\t{z1[2]}\t{e1[2]}");
                //Console.WriteLine($"{s11[2]}\t{s11[3]}");

                FilterOutputPositions.Add(new Position { X = s11[0], Y = s11[1] });

                Console.WriteLine(s11[3]);
                s00 = s11.DeepCopy();
                P00 = P11.DeepCopy();
            }

        }

        
    }
}
