using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Radar
{
    /// <summary>Klasa służąca do wykrywania zmian kierunku oraz prędkości badanego obiektu na podstawie jego pozycji w kolejnych chwilach czasu.</summary>
    
    // Jak dotąd nie używana.
    public class MotionChangeDetector
    {
        /// <summary>
        /// Lista Pozycji.
        /// </summary>
        public List<Position> PositionsList;
        int NumberOfSamples;
        public const int DefaultNumberOfSamples = 12;

        /// <summary>
        /// Konstruktor pobiera listę pozycji <paramref name="positions"/> oraz liczbę badanych próbek.
        /// </summary>
        /// <param name="positions">Opis pozycji</param>
        /// <param name="numberOfSamples">Opis liczby próbek.</param>
        public MotionChangeDetector(List<Position> positions, int numberOfSamples = DefaultNumberOfSamples)
        {
            PositionsList = positions;
            NumberOfSamples = numberOfSamples;
        }

        public double Process(int index)
        {
            if (index < NumberOfSamples - 1) return double.MaxValue;
            //double v;   // Velocity
            double r;   // Pearson Coefficient
            double xMean = 0;
            double yMean = 0;


            for (int i = index; i > index - NumberOfSamples; i--)
            {
                xMean += PositionsList[i].X;
                yMean += PositionsList[i].Y;
            }

            xMean /= NumberOfSamples;
            yMean /= NumberOfSamples;

            double a1 = 0;
            double a2 = 0;
            double a3 = 0;

            for (int i = index; i > index - NumberOfSamples; i--)
            {
                Position p = PositionsList[i];
                a1 += (p.X - xMean) * (p.Y - yMean);
                a2 += Math.Pow(p.X - xMean, 2);
                a3 += Math.Pow(p.Y - yMean, 2);
            }

            a2 = Math.Sqrt(a2);
            a3 = Math.Sqrt(a3);

            r = a1 / (a2 * a3);

            // Console.WriteLine(Math.Abs(r));

            return r;
        }





    }
}
