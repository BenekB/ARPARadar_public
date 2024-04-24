using System;
using Vector = System.Windows.Vector;


namespace Radar
{
    /// <summary>Struktura reprezentuje kurs statku oraz zawiera implementację potrzebnych mechanizmów jego obsługi.</summary>
    /// 
    public struct Heading
    {

        //  WŁAŚCIWOŚCI


        /// <summary>Prywatna zmienna reprezentująca wartość liczbową kursu.</summary>
        private double _Value;

        /// <summary>Publiczna zmienna reprezentująca wartość liczbową kursu.</summary>
        /// <remarks>Zwraca prywatną zmienną <see cref="_Value"/>. Pobraną wielkść przetwarza na wartość z zakresu od 0 do 360 stopni.</remarks>
        /// 
        public double Value
        {
            readonly get { return _Value; }

            //  Dane wejściowe są przeliczane, aby kurs był reprezentowany
            //  przez kąt z zakresu 0 do 360 stopni.
            set
            {
                if (value < 0)
                    _Value = value % 360 + 360;
                else
                    _Value = value % 360;
            }
        }

        /// <summary>Zwraca wartość kursu w Radianach.</summary> 
        /// 
        public readonly double InRadians
        {
            get { return _Value * 2 * Math.PI / 360.0; }
        }


        //  KONSTRUKTORY


        /// <summary>Tworzy obiekt <see cref="Heading"/> o określonej wartości kursu <paramref name="value"/></summary>
        /// <param name="value">Wartość kątowa kursu [stopnie]</param>
        /// 
        public Heading(double value)
        {
            Value = value;
        }


        //  METODY


        /// <summary>Funkcja zwraca wartość kursu w formie napisu typu <see cref="string"/> w odpowiednio zadanym formacie <paramref name="format"/>.</summary>
        /// <param name="format">Zadany format generowania tekstu.</param>
        /// 
        public readonly string ToString(string format = "##0.0##")
        {
            return Value.ToString(format);
        }


        //  METODY STATYCZNE


        /// <summary>Zamienia wartość kątową podaną w radianach na obiekt typu <see cref="Heading"/>.</summary>
        /// <param name="rad">Wartość kąta podana w Radianach.</param>
        /// <returns>Obiekt typu <see cref="Heading"/>.</returns>
        /// 
        public static Heading FromRadians(double rad)
        {
            return new Heading { Value = (rad) * (360.0 / 2 / Math.PI)};
        }


        public static Heading FromRadians2(double rad)
        {
            return new Heading { Value = (-rad + Math.PI / 2) * (360.0 / 2 / Math.PI) };
        }


        /// <summary>Zamienia obiekt typu <see cref="System.Windows.Vector"/> na obiekt typu <see cref="Heading"/>.</summary>
        /// <param name="vector">Obiekt typu <see cref="Vector"/>.</param>
        /// <returns>Obiekt typu <see cref="Heading"/>.</returns>
        /// 
        public static Heading Parse(System.Windows.Vector vector)
        {
            Vector v0 = new(0, 10);
            Heading heading = Vector.AngleBetween(vector, v0);

            return heading;
        }


        /// <summary>Wyznacza kurs prowadzący od pozycji <paramref name="previousPos"/> do pozycji <paramref name="nextPos"/>.</summary>
        /// <param name="nextPos">Pozycja końcowa w której kończymy.</param>
        /// <param name="previousPos">Pozycja początkowa od której zaczynamy.</param>
        /// <returns>Obiekt typu <see cref="Heading"/> reprezentujący wyznaczony kurs.</returns>
        /// 
        public static Heading HeadingBetweenPositions(Position nextPos, Position previousPos)
        {
            Vector v2 = new(0, 1);
            Vector v1 = new(nextPos.X - previousPos.X, nextPos.Y - previousPos.Y);
            
            Heading angle = Vector.AngleBetween(v1, v2);

            return angle;
        }


        /// <summary>Funkcja sprawdza, czy kąt między <paramref name="actualHeading"/> a <paramref name="desiredHeading"/> jest mniejszy w przypadku obrotu statku zgodnie z ruchem wskazówek zegara. Innymi słowy - czy szybciej osiągniemy kurs <paramref name="desiredHeading"/> skręcając w prawo niż w lewo.</summary>
        /// <param name="actualHeading">Kurs, którym aktualnie się poruszamy.</param>
        /// <param name="desiredHeading">Kurs, który zamierzamy osiągnąć.</param>
        /// <returns><para><c>True</c> jeżeli powinniśmy skręcić w prawo.</para>
        /// <para><c>False</c> jeżeli powinniśmy skręcić w lewo.</para></returns>
        /// 
        public static bool ShouldTurnRight(Heading actualHeading, Heading desiredHeading)
        {
            if (180.0 < actualHeading - desiredHeading && actualHeading - desiredHeading < 360.0)
                return true;
            else 
                return false;
        }


        /// <summary>Funkcja sprawdza, czy kąt między <paramref name="actualHeading"/> a <paramref name="desiredHeading"/> jest mniejszy w przypadku obrotu statku przeciwnie do ruchu wskazówek zegara. Innymi słowy - czy szybciej osiągniemy kurs <paramref name="desiredHeading"/> skręcając w lewo niż w prawo.</summary>
        /// <param name="actualHeading">Kurs, którym aktualnie się poruszamy.</param>
        /// <param name="desiredHeading">Kurs, który zamierzamy osiągnąć.</param>
        /// <returns><para><c>True</c> jeżeli powinniśmy skręcić w lewo.</para>
        /// <para><c>False</c> jeżeli powinniśmy skręcić w prawo.</para></returns>
        /// 
        public static bool ShouldTurnLeft(Heading actualHeading, Heading desiredHeading)
        {
            if (0 < actualHeading - desiredHeading && actualHeading - desiredHeading < 180.0)
                return true;
            else
                return false;
        }


        //  RZUTOWANIE


        /// <summary>Niejawne rzutowanie z typu <see cref="double"/> na <see cref="Heading"/>.</summary>
        /// 
        public static implicit operator Heading(double input)
        {
            return new Heading() { Value = input };
        }


        /// <summary>Jawne rzutowanie z typu <see cref="Heading"/> na <see cref="double"/></summary>
        ///
        public static explicit operator double(Heading input) { return input.Value; }


        /// <summary>Jawne rzutowanie z typu <see cref="Heading"/> na <see cref="string"/>.</summary>
        ///
        public static explicit operator string(Heading input)
        {
            return input.Value.ToString("000.0");
        }


        //  PRZECIĄŻANIE OPERATORÓW


        /// <summary>Przeciążenie operatora odejmowania (-).</summary>
        /// <returns>Zwraca obiekt typu <see cref="double"/>.</returns>
        /// 
        public static double operator -(Heading left, Heading right)
        {
            //  Odejmowanie od siebie dwóch obiektów Heading powoduje zwrócenie różnicy
            //  wartości kursów rzutowanej na typ double.
            Heading heading = new() { Value = left.Value - right.Value };
            return (double)heading;
        }

        public static double operator -(Heading input)
        {
            //  Odejmowanie od siebie dwóch obiektów Heading powoduje zwrócenie różnicy
            //  wartości kursów rzutowanej na typ double.
            Heading heading = new() { Value = input - 180 };
            return (double)heading;
        }


        /// <summary>Przeciążenie operatora dodawania (+).</summary>
        /// <returns>Zwraca obiekt typu <see cref="double"/>.</returns>
        /// 
        public static double operator +(Heading left, Heading right)
        {
            //  Dodawanie do siebie dwóch obiektów Heading powoduje zwrócenie sumy
            //  wartości kursów rzutowanej na typ double.
            Heading heading = new() { Value = left.Value + right.Value };
            return (double)heading;
        }


        /// <summary>Przeciążenie operatora równości (==).</summary>
        /// 
        public static bool operator ==(Heading left, Heading right)
        {
            //  Porównujemy wartości kursu.
            return left.Value == right.Value;
        }


        /// <summary>Przeciążenie operatora nierówności (!=).</summary>
        /// 
        public static bool operator !=(Heading left, Heading right)
        {
            //  Porównujemy wartości kursu.
            return left.Value != right.Value;
        }


        //  METODY NADPISANE


        public override bool Equals(object? obj) { throw new NotImplementedException(); }

        public override int GetHashCode() {  throw new NotImplementedException(); }
    }
}
