using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Numerics;
using MathNet.Numerics;
using MathNet.Numerics.Providers.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using Matrix = MathNet.Numerics.LinearAlgebra.Matrix<double>;
using ClassLibrary;
using System.IO;
using Vector = System.Windows.Vector;

namespace Radar
{
    public class LinearApproximationFilter
    {

        List<Position> RealPositions = new();
        List<Position> NoisedPositions = new();
        List<SphericalPosition> NoisedSphericalPositions = new();
        List<Position> FilterOutputPositions = new();

        public double T = Param.RadarCircleTimeMs / 1000;

        Position MyShipPosition = new Position { X = 18520, Y = 18520 };



        public LinearApproximationFilter(List<Position> realPositions, List<Position> noisePositions, List<SphericalPosition> noiseSphericalPosition, List<Position> filterOutput)
        {
            RealPositions = realPositions;
            NoisedPositions = noisePositions;
            NoisedSphericalPositions = noiseSphericalPosition;
            FilterOutputPositions = filterOutput;

        }


        public LinearApproximationFilter(string path)
        {
            ReadDataFromFile(path);
        }


        public void OneStepProcess()
        {
            if (NoisedPositions.Count == 0) return;
            if (NoisedPositions.Count == 1)
            {
                return;
            }
            else
            {

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
                NoisedSphericalPositions.Add(new SphericalPosition { Angle = double.Parse(line[4]), Range = double.Parse(line[5]) });
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


        public void Initialize()
        {
            //  To be implemented.
        }

        public void ProcessAll()
        {
            List<Position> meanPositions = new();
            Position sumPos = new() { X = 0, Y = 0 };
            int coeff = 1;

            for (int i = coeff; i < this.NoisedPositions.Count; i++)
            {
                sumPos.X = 0;
                sumPos.Y = 0;

                for (int j = i - coeff; j < i; j++)
                {
                    sumPos.X += NoisedPositions[j].X;
                    sumPos.Y += NoisedPositions[j].Y;
                }

                sumPos.X /= coeff;
                sumPos.Y /= coeff;

                meanPositions.Add(sumPos);
            }

            // Obliczanie średniej prędkości

            for (int i = 0; i < meanPositions.Count - 24; i++)
            {
                //Console.WriteLine(Math.Sqrt(Math.Pow(meanPositions[i].X - meanPositions[i+24].X,2) + Math.Pow(meanPositions[i].Y - meanPositions[i+24].Y,2)) / Param.RadarCircleTimeMs * 1000 / 24);
            }

            LinearApproximation linearApproximation = new();
            double a, b;
            Heading hdg1 = new();
            Heading hdg2 = new();

            for (int i = 16; i < meanPositions.Count; i++)
            {
                (a, b, hdg1) = linearApproximation.Calculate(meanPositions, i - 16, i);

                if (i > 64)
                {
                    (_, _, hdg2) = linearApproximation.Calculate(meanPositions, i - 64, i);
                    (_, _, hdg1) = linearApproximation.Calculate(meanPositions, i - 16, i);

                    Console.WriteLine(hdg1.ToString() + "\t" + hdg2.ToString());
                }
                else
                {
                    (_, _, hdg1) = linearApproximation.Calculate(meanPositions, i - 16, i);
                    Console.WriteLine(hdg1.ToString());
                }
                
                

            }

            



            

        }

    }
}
