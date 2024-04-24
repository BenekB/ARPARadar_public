using System;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows;


namespace Radar
{

    /// <summary>Reprezentuje obiekt będący jednostką pływającą.</summary>
    public class Ship
    {

        //  WŁAŚCIWOŚCI


        /// <summary>Graficzna reprezentacja obiektu <see cref="Ship"/> w GUI.</summary>
        public Polygon Icon;
        /// <summary>Unikalny numer identyfikacyjny obiektu <see cref="Ship"/>.</summary>
        public int ID;
        /// <summary>Przesunięcie ikony graficznej <see cref="Icon"/> w GUI.</summary>
        public TranslateTransform Translation;
        /// <summary>Obrót ikony graficznej <see cref="Icon"/> w GUI.</summary>
        public RotateTransform Rotation;
        /// <summary>Transformacja wynikowa ikony graficznej <see cref="Icon"/> będąca złożeniem <see cref="Translation"/> oraz <see cref="Rotation"/>.</summary>
        public TransformGroup Transformation;
        /// <summary>Informacja, czy lewy przycisk myszy jest w danej chwili wciśnięty.</summary>
        public bool IsMouseLeftButtonDown;
        /// <summary>Okienko dialogowe pozwalające na zmianę parametrów naszego obiektu <see cref="Ship"/>.</summary>
        public ShipWindow ShipDialogBox;
        /// <summary>Czas na pokładzie obiektu <see cref="Ship"/></summary>
        public DateTime Time = new();
        /// <summary>Współrzędne położenia obiektu <see cref="Ship"/> podane w milach morskich.</summary>
        public Point Position;
        /// <summary>Długość statku <see cref="Ship"/></summary>
        public double Length;
        /// <summary>Szerokość statku <see cref="Ship"/></summary>
        public double Width;
        /// <summary>Komputer pokładowy ARPA.</summary>
        public ARPADevice ARPA;

        //  Docelowe wartości kursu, prędkości, zmiany prędkości kątowej oraz liniowej.
        public Heading DesiredHeading;
        public double DesiredSpeed;
        public double DesiredRateOfTurn;
        public double DesiredRateOfSpeed;

        //  Aktualne wartości kursu, prędkości, zmiany prędkości kątowej oraz liniowej.
        public Heading ActualHeading;
        public double ActualSpeed;
        public double ActualRateOfTurn;
        public double ActualRateOfSpeed;


        //  KONSTRUKTORY


        /// <summary>Ustawia długość, szerokość, numer identyfikacyjny oraz kurs tworzonego obiektu <see cref="Ship"/></summary>
        /// <param name="length">Długość statku [metry]</param>
        /// <param name="width">Szerokość statku [metry]</param>
        /// <param name="id">Unikalny numer identyfikacyjny statku</param>
        /// <param name="heading">Początkowy kurs statku [stopnie]</param>
        /// 
        public Ship(double length, double width, int id, double heading = 0)
        {
            ID = id;
            IsMouseLeftButtonDown = false;
            Time = DateTime.Now;
            Icon = new Polygon()
            {
                Fill = new SolidColorBrush(Colors.Black),
                Points = CreateListOfPoints(length, width)
            };
            Translation = new(0, 0);
            Rotation = new(heading);
            Transformation = new();
            Transformation.Children.Add(Rotation);
            Transformation.Children.Add(Translation);
            Icon.RenderTransform = Transformation;
            ShipDialogBox = new(this);
            ARPA = new(ID);
            
            DesiredHeading = Rotation.Angle;
            DesiredSpeed = 0.0;
            DesiredRateOfTurn = 0.0;
            DesiredRateOfSpeed = 0.0;

            ActualHeading = Rotation.Angle;
            ActualSpeed = 0.0;
            ActualRateOfTurn = 0.0;
            ActualRateOfSpeed = 0.0;
        }


        //  METODY


        /// <summary>Otwiera okienko dialogowe pozwalające odczytać i zmienić parametry statku. Okienko otwiera się w lokalizacji określonej przez zadany punkt <paramref name="point"/>.</summary>
        /// <param name="point">Punkt określający położenie otwieranego okienka dialogowego.</param>
        /// 
        public void OpenDialogBox(Point point)
        {
            //  Tutaj trzeba wprowadzić algorytm, który
            //  ustali odpowiednie położenie na ekranie
            //  okienka dialogowego statku.

            ShipDialogBox.Top = point.Y + 10;
            ShipDialogBox.Left = point.X + 10;
            ShipDialogBox.Show();
        }


        /// <summary>Ustawia współrzędne statku na mapie. Współrzędne podane w milach morskich.</summary>
        /// <param name="x">Współrzędna x [mila morska]</param>
        /// <param name="y">Współrzędna y [mila morska]</param>
        /// 
        public void SetPosition(double x, double y)
        {
            //  Jeżeli pozycja nie wykracza poza mapę.
            if (x >= 0 && y >= 0)
            {
                Position.X = x;
                Position.Y = y;

                //  Do obliczania ruchu statku zapisujemy moment zmiany pozycji.
                Time = DateTime.Now;
            }
        }


        /// <summary>Ustawia aktualny kurs statku.</summary>
        /// <param name="heading">Kurs statku [stopnie]</param>
        /// 
        public void SetHeading(double heading)
        {
            //  Jeżeli wartość zadanego kursu mieści się w zbiorze możliwych wartości.
            if (0 <= heading && heading <= 360)
            {
                Rotation.Angle = heading;
                ActualHeading = heading;
            }
        }


        /// <summary>Generuje kształt ikony <see cref="Icon"/> statku na podstawie jego szerokości <paramref name="width"/> i długości <paramref name="length"/>.</summary>
        /// <param name="length">Długość statku [metry]</param>
        /// <param name="width">Szerokość statku [metry]</param>
        /// <returns>Kolekcję punktów <see cref="PointCollection"/> określających kształt <see cref="Icon"/> statku w GUI.</returns>
        /// 
        private PointCollection CreateListOfPoints(double length, double width)
        {
            //  Minimalna wielkość statku.
            if (length < 50) length = 50;
            if (width < 0) width = 10;

            //  Skalowanie wielkości.
            length /= 2;
            width /= 2;
            Length = length;
            Width = width;

            //  Generowanie kolejnych punktów kształtu zgodnie z zastosowanym algorytmem.
            Point a = new(-width / 2, length / 2);
            Point b = new(width / 2, length / 2);
            Point c = new(width / 2, -length / 4);
            Point d = new(0, -length / 2);
            Point e = new(-width / 2, -length / 4);

            return new PointCollection(new[] { a, b, c, d, e });
        }
    }
}