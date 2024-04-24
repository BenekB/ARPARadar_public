using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using MathNet.Numerics;
using ScottPlot.MarkerShapes;

namespace Radar
{

    /// <summary>Implementuje algorytm aproksymacji liniowej dla zadanej listy punktów.</summary>
    public class LinearApproximation
    {

        /// <summary>Aproksymuje punkty zawarte w liście <paramref name="positions"/> linią prostą.</summary>
        /// <param name="positions">Lista <see cref="List{T}"/> zawierająca punkty <see cref="Position"/> w przestrzeni.</param>
        /// <returns>Współczynnik kierunkowy prostej A, współczynnik prostej B oraz kąt nachylenia prostej jako kurs <see cref="Heading"/>.</returns>
        /// 
        public (double, double, Heading) Calculate(List<Position> positions, int startIndex = 0, int endIndex = 0)
        {

            if (endIndex == 0)
                endIndex = positions.Count;

            //  Liczba pozycji brana pod uwagę podczas aproksymacji.
            int n = endIndex - startIndex;
            
            //  Tablica współrzędnych x oraz tablica współrzędnych y.
            double[] xData = new double[n];
            double[] yData = new double[n];
            double[] xDataT = new double[n];
            double[] yDataT = new double[n];

            //  Przeskalowanie współrzędnych tak, aby pierszy punkt znajdował się w początku układu współrzędnych.
            for (int i = 0; i < n; i++)
            {
                xData[i] = positions[i + startIndex].X - positions[0].X;
                yData[i] = positions[i + startIndex].Y - positions[0].Y;
            }

            double angle = 0;

            if (n > 6)
            {

                double xStart = xData[0];
                double yStart = yData[0];

                double xEnd = xData[^1];
                double yEnd = yData[^1];

                angle = (double)Heading.HeadingBetweenPositions(new Position { X = xEnd, Y = yEnd }, new Position { X = xStart, Y = yStart});
                angle -= 90;
        
                for (int i = 0; i < n; i++)
                {
                    xDataT[i] = Math.Cos(angle * Param.DegToRad) * xData[i] - Math.Sin(angle * Param.DegToRad) * yData[i];
                    yDataT[i] = Math.Sin(angle * Param.DegToRad) * xData[i] + Math.Cos(angle * Param.DegToRad) * yData[i];
                }

            }

            //  Współczynniki prostej.
            double A, B;

            //  Przeprowadzenie aproksymacji linią prostą.
            (B, A) = Fit.Line(xDataT, yDataT);

            //  Obliczenie kursu na podstawie współczynnika kierunkowego prostej.
            Heading hdg = Heading.FromRadians2(Math.Abs(Math.Atan(A)) - (angle * Param.DegToRad));

            return (A, B, hdg);
        }
    }
}
