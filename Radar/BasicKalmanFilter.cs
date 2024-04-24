using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Radar
{

    public class BasicKalmanFilter : IFilter
    {
        List<Position> RealPositions = new();
        List<Position> NoisedPositions = new();
        List<Position> FilterOutputPositions = new();
        public Matrix F = new();
        public Matrix G = new();
        public Matrix H = new();
        public Matrix P00 = new();
        public Matrix Q = new();
        public Matrix R = new();
        public Matrix s00 = new();
        private Matrix I = new();
        private Matrix s10 = new();
        private Matrix P10 = new();
        private Matrix z10 = new();
        private Matrix z1 = new(2, 1);
        private Matrix e1 = new();
        private Matrix S1 = new();
        private Matrix K1 = new();
        private Matrix s11 = new();
        private Matrix P11 = new();
        public double T = Param.RadarCircleTimeMs / 1000;


        public BasicKalmanFilter(List<Position> realPositions, List<Position> noisePositions, List<Position> filterOutput)
        {
            RealPositions = realPositions;
            NoisedPositions = noisePositions;
            FilterOutputPositions = filterOutput;

            InitializeDefault();
        }

        public BasicKalmanFilter(string path)
        {
            ReadDataFromFile(path);

            InitializeDefault();
        }


        public void ReadDataFromFile(string path)
        {
            string[] records = File.ReadAllLines(path);
            string[] line;
            Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("en-US");

            foreach (string record in records)
            {
                line = record.Split('\t');
                
                RealPositions.Add( new Position { X = double.Parse(line[0]), Y = double.Parse(line[1]) } );
                NoisedPositions.Add( new Position { X = double.Parse(line[2]), Y = double.Parse(line[3]) } );
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


        public void Initialize(Matrix F, Matrix G, Matrix H, Matrix P, Matrix Q, Matrix R, Matrix s00)
        {
            this.F = F;
            this.G = G;
            this.H = H;
            this.P00 = P;
            this.Q = Q;
            this.R = R;
            this.s00 = s00;
            I = Matrix.Identity(P.Rows);
        }


        public void InitializeDefault()
        {
            F = new Matrix(new double[,] { { 1, 0, T, 0 },
                                           { 0, 1, 0, T },
                                           { 0, 0, 1, 0 },
                                           { 0, 0, 0, 1 } });

            G = new Matrix(new double[,] { { 0, 0 },
                                           { 0, 0 },
                                           { 1, 0 },
                                           { 0, 1 } });

            H = new Matrix(new double[,] { { 1, 0, 0, 0 },
                                           { 0, 1, 0, 0 } });

            P00 = new Matrix(new double[,] { {    5,  0,  0,  0   },
                                           {    0,  5,  0,  0   },
                                           {    0,  0,  5,  0   },
                                           {    0,  0,  0,  5   }   });

            Q = new Matrix(new double[,] { { 0.0001   , 0     },
                                           { 0      , 0.0001  } });

            R = new Matrix(new double[,] { { 22.0    , 0     },
                                           { 0      , 22.0   } });

            s00 = new Matrix(new double[,] {{ 0  },
                                            { 0  },
                                            { 0  },
                                            { 0  } });

            I = Matrix.Identity(P00.Rows);
        }


        public void ProcessAll()
        {
            s00[0] = NoisedPositions[0].X;
            s00[1] = NoisedPositions[0].Y;
            FilterOutputPositions.Add(NoisedPositions[0]);


            for (int i = 1; i < NoisedPositions.Count; i++)
            {
                s10 = F * s00;
                P10 = F * P00 * F.T + G * Q * G.T;
                z10 = H * s10;
                z1[0] = NoisedPositions[i].X; z1[1] = NoisedPositions[i].Y;
                e1 = z1 - z10;
                S1 = H * P10 * H.T + R;
                K1 = P10 * H.T * (S1 ^ -1);
                s11 = s10 + K1 * e1;
                P11 = (I - K1 * H) * P10;

                FilterOutputPositions.Add(new Position { X = s11[0], Y = s11[1] });

                // Console.WriteLine(FilterOutputPositions[i]);

                P00 = P11.DeepCopy();
                s00 = s11.DeepCopy();
            }
            
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
    }
}
