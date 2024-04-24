
using Point = System.Windows.Point;
using System.Collections.Generic;
using System;
using Vector = System.Windows.Vector;


namespace Radar
{

    /// <summary>Statyczna klasa zawierająca stałe przeliczniki różnych wielkości używanych w programie.</summary>
    public static class Param
    {
        public const int RadarCircleTimeMs = 2500;
        public const int NumberOfShips = 12;
        public const double PixelsToMeters = 100.0;
        public const double MetersToPixels = 1.0 / PixelsToMeters;
        public const double KnotsToMs = 1852.0 / 3600.0;
        public const double MsToKnots = 3600.0 / 1852.0;
        public const double DegPerMinToDegPerSec = 1.0 / 60.0;
        public const double DegPerSecToDegPerMin = 60.0;
        public const double KnPerMinToMsPerSec = 463.0 / 54000.0;
        public const double MsPerSecToKnPerMin = 54000.0 / 463.0;
        public const double DegToRad = 2 * Math.PI / 360.0;         // 0.0174532925;
        public const double PixelsToNm = 100.0 * 1.0 / 1852.0;
        public const double NmToPixels = 1.0 / PixelsToNm;
        public const double NmToMeters = NmToPixels * PixelsToMeters;
        public const double MetersToNm = 1.0 / NmToMeters;
        public const double BearingAccuracy = 1.0;     //  Degrees
        public const double RangeAccuracy = 0.01;       //  Fraction of Range

        private const double MapHeight = 1010.0;

        /// <summary>Przelicza pozycję podaną w metrach na pozycję na ekranie.</summary>
        /// <param name="x">Współrzędna x [metry]</param>
        /// <param name="y">Współrzędna y [metry]</param>
        /// <returns>Pozycję we współrzędnych ekranu [piksele]</returns>
        /// 
        public static (double, double) PositionToScreen(double x, double y)
        {
            x *= MetersToPixels; 
            y *= MetersToPixels;
            y = -y + MapHeight;

            return (x, y);
        }

        /// <summary>Przelicza współrzędną składową <paramref name="x"/> podaną w metrach na składową położenia x we współrzędnych ekranu.</summary>
        /// <param name="x">Wspólrzędna składowa x [metry]</param>
        /// <returns>Składowa położenia x we współrzędnych ekranu.</returns>
        /// 
        public static double XPositionToScreen(double x)
        {
            x *= MetersToPixels;    
            return x;
        }

        /// <summary>Przelicza współrzędną składową <paramref name="y"/> podaną w metrach na składową położenia y we współrzędnych ekranu.</summary>
        /// <param name="y">Wspólrzędna składowa y [metry]</param>
        /// <returns>Składowa położenia y we współrzędnych ekranu.</returns>
        /// 
        public static double YPositionToScreen(double y)
        {
            y *= MetersToPixels;
            y = -y + MapHeight;

            return y;
        }

        /// <summary>Przelicza pozycję we współrzędnych ekranu na pozycję rzeczywistą podaną w metrach.</summary>
        /// <param name="x">Współrzędna składowa ekranu x [piksele]</param>
        /// <param name="y">Współrzędna składowa ekranu y [piksele]</param>
        /// <returns>Pozycję we współrzędnych rzeczywistych [metry]</returns>
        /// 
        public static (double, double) ScreenToPosition(double x, double y)
        {
            y -= MapHeight;
            y = -y;
            y *= PixelsToMeters;
            x *= PixelsToMeters;

            return (x, y);
        }
    }


    /// <summary>Interfejs określający filtry cyfrowe.</summary>
    public interface IFilter
    {
        public void InitializeDefault();
        public void ReadDataFromFile(string path);
        public void SaveOutputToFile(string path);
        /// <summary>Przetwarza wszystkie dane.</summary>
        public void ProcessAll();
        /// <summary>Wykonuje jeden krok algorytmu.</summary>
        public void OneStepProcess();
    }


    /// <summary>Rozszerzenie klasy <see cref="System.Random"/> o funkcję pozwalająca na wybranie przedziału wartości zwracanej liczby losowej.</summary>
    public class Random : System.Random
    {
        /// <summary>Returns a random floating-point number that is greater than or equal to minValue, and less than maxValue.</summary>
        /// <param name="minValue">Minimal output value.</param>
        /// <param name="maxValue">Maximal output value.</param>
        /// <returns>A random floating-point number that is greater than or equal to minValue, and less than maxValue.</returns>
        /// 
        public double NextDouble(double minValue, double maxValue)
        {
            return minValue + (maxValue - minValue) * base.NextDouble();
        }
    }


    /// <summary>Reprezentuje położenie obiektu we współrzędnych kartezjańskich z uwzględnieniem chwili czasu <see cref="Time"/> dla której lokalizacja była pobrana.</summary>
    public struct Position
    {
        /// <summary>Współrzędna x [metry]</summary>
        public double X;
        /// <summary>Współrzędna y [metry]</summary>
        public double Y;
        /// <summary>Czas pobrania lokalizacji</summary>
        public DateTime Time;

        public Heading HDG = new();
        public double Speed = 0.0;

        public Position()
        {
        }

        /// <summary>Zamienia lokalizajcę we współrzędnych kartezjańskich <see cref="Position"/> na lokalizację we wspólrzędnych sferycznych <see cref="SphericalPosition"/>.</summary>
        /// <param name="centerX">Początek układu sferycznego zadany we współrzędnych kartezjańskich układu wyjściowego</param>
        /// <param name="centerY">Początek układu sferycznego zadany we współrzędnych kartezjańskich układu wyjściowego</param>
        /// <returns>Lokalizację zadaną we współrzędnych sferycznych</returns>
        /// 
        public SphericalPosition ToSpherical(double centerX, double centerY)
        {
            Vector v2 = new(0, 1);
            Vector v1 = new(X - centerX, Y - centerY);
            Heading angle = Vector.AngleBetween(v1, v2);
            double range = Math.Sqrt(Math.Pow(X - centerX, 2) + Math.Pow(Y - centerY, 2));

            return new SphericalPosition() { Angle = angle, Range = range, Time = Time};
        }

        /// <summary>Generuje reprezentację tekstową pozycji <see cref="Position"/></summary>
        /// <returns>Reprezentację tekstową pozycji <see cref="Position"/></returns>
        /// 
        public override string ToString()
        {
            string position = $"X: {X}, Y: {Y} ";
            return position;
        }
    }


    public class FilterOutputData
    {
        public Heading ShortHeading = double.NaN;
        public Heading LongHeading = double.NaN;
        public double ShortSpeed = double.NaN;
        public double LongSpeed = double.NaN;
        public double TCPA = double.NaN;
        public double CPA = double.NaN;
        public double ShortTCPA = double.NaN;
        public double ShortCPA = double.NaN;
    }


    /// <summary>Reprezentuje położenie obiektu we współrzędnych kartezjańskich z uwzględnieniem chwili czasu <see cref="Time"/> dla której lokalizacja była pobrana.</summary>
    public class ShipsPosition
    {
        /// <summary>Współrzędna x [metry]</summary>
        public double X;
        /// <summary>Współrzędna y [metry]</summary>
        public double Y;
        /// <summary>Czas pobrania lokalizacji</summary>
        public DateTime Time;

        public Heading HDG = new();
        public double Speed = 0.0;
    }


    /// <summary>Reprezentuje położenie obiektu we współrzędnych kartezjańskich z uwzględnieniem chwili czasu <see cref="Time"/> dla której lokalizacja była pobrana.</summary>
    public struct SphericalPosition
    {
        /// <summary>Współrzędna zawierająca kąt [<see cref="Heading"/>]</summary>
        public Heading Angle;
        /// <summary>Współrzędna zawierająca odległość [metry]</summary>
        public double Range;
        /// <summary>Czas pobrania lokalizacji</summary>
        public DateTime Time;

        /// <summary>Zamienia lokalizajcę we współrzędnych sferycznych <see cref="SphericalPosition"/> na lokalizację we wspólrzędnych kartezjańskich <see cref="Position"/>.</summary>
        /// <param name="centerX">Przesunięcie początku układu współrzędnych kartezjańskich względem początku układu współrzędnych sferycznych.</param>
        /// <param name="centerY">Przesunięcie początku układu współrzędnych kartezjańskich względem początku układu współrzędnych sferycznych.</param>
        /// <returns>Lokalizację zadaną we współrzędnych kartezjańskich</returns>
        /// 
        public Position ToCartesian(double centerX, double centerY)
        {
            double x = centerX + Range * Math.Sin(Angle.InRadians);
            double y = centerY + Range * Math.Cos(Angle.InRadians);

            return new Position() { X = x, Y = y, Time = Time };
        }

    }


    /// <summary>Opisuje skalę wyświetlania obrazu radarowego oraz implementuje jej ułatwioną obsługę</summary>
    public class RadarScale
    {

        //  WŁAŚCIWOŚCI


        public const double Nm0p375 = 0.375 * 1852;
        public const double Nm0p75 = 0.75 * 1852;
        public const double Nm1p5 = 1.5 * 1852;
        public const double Nm3 = 3.0 * 1852;
        public const double Nm6 = 6.0 * 1852;
        public const double Nm12 = 12.0 * 1852;
        public const double Nm24 = 24.0 * 1852;

        /// <summary>Prywatna aktualna skala radaru.</summary>
        private double _ActualScale = Nm0p75;
        /// <summary>Aktualnie ustawiona skala radaru.</summary>
        public double ActualScale 
        {
            get => _ActualScale;
            set
            {
                _ActualScale = value;
                _ActualIndex = ScaleGrades.IndexOf(value);
            } 
        }

        /// <summary>Aktualny indeks określający stopień przybliżenia obrazu radarowego</summary>
        private int _ActualIndex = 0;
        /// <summary>Zawiera ustawione w porządku rosnącym współczynniki określające skalę obrazu radarowego</summary>
        /// <remarks>Przykładowo <c>ScaleGrades[0]</c> oznacza największe przybliżenie, natomiast <c>ScaleGrades[6]</c> oznacza najmniejsze przybliżenie.</remarks>
        private List<double> ScaleGrades = new() { Nm0p375, Nm0p75, Nm1p5, Nm3, Nm6, Nm12, Nm24 };


        //  KONSTRUKTORY


        public RadarScale()
        {

        }


        //  RZUTOWANIE TYPÓW


        /// <summary>Rzutowanie <see cref="RadarScale"/> na typ wbudowany <see cref="double"/></summary>
        /// 
        public static implicit operator double(RadarScale radarScale) { return radarScale.ActualScale;}


        /// <summary>Generowanie tekstowej reprezentacji wartości aktualnie ustawionej skali radaru</summary>
        /// 
        public static explicit operator string(RadarScale radarScale) 
        { 
            switch (radarScale.ActualScale)
            {
                case Nm0p375:   return "0.375 Nm";
                case Nm0p75:    return "0.75 Nm";
                case Nm1p5:     return "1.5 Nm";
                case Nm3:       return "3.0 Nm";
                case Nm6:       return "6.0 Nm";
                case Nm12:      return "12.0 Nm";
                case Nm24:      return "24.0 Nm";
                default:        return "Unknown";
            }
        }


        //  PRZEŁADOWANE OPERATORY


        /// <summary>Zwiększa zasięg obrazu radarowego o jeden krok</summary>
        /// 
        public static RadarScale operator ++(RadarScale radarScale) 
        {
            if (radarScale._ActualIndex < radarScale.ScaleGrades.Count - 1)
            {
                radarScale._ActualIndex++;
                radarScale.ActualScale = radarScale.ScaleGrades[radarScale._ActualIndex];
            }

            return radarScale;
        }


        /// <summary>Zmniejsza zasięg obrazu radarowego o jeden krok</summary>
        /// 
        public static RadarScale operator --(RadarScale radarScale)
        {
            if (radarScale._ActualIndex > 0)
            {
                radarScale._ActualIndex--;
                radarScale.ActualScale = radarScale.ScaleGrades[radarScale._ActualIndex];
            }

            return radarScale;
        }
    }
}
