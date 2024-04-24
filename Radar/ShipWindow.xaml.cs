using System;
using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Input;


namespace Radar
{

    /// <summary>Okienko dialogowe pozwalające na zmianę parametrów już istniejącego obiektu <see cref="Ship"/>.</summary>
    public partial class ShipWindow : Window
    {
        /// <summary>Adres statku <see cref="Ship"/> do którego dane okienko <see cref="ShipWindow"/> jest przypisane.</summary>
        Ship ParentShip;

        Heading DesiredHeading;
        double _DesiredSpeed;
        double DesiredSpeed
        {
            get { return _DesiredSpeed; }
            set
            {
                //  Nie pozwalamy na ustawienie ujemnej wartości prędkości.
                if (value < 0.0) _DesiredSpeed = 0.0;
                else _DesiredSpeed = value;
            }
        }
        double _DesiredRateOfTurn;
        double DesiredRateOfTurn
        {
            get { return _DesiredRateOfTurn; }
            set
            {
                //  Nie pozwalamy na ustawienie ujemnej prędkości obrotowej.
                if (value < 0.0) _DesiredRateOfTurn = 0.0;
                else _DesiredRateOfTurn = value;
            }
        }
        double _DesiredRateOfSpeed;
        double DesiredRateOfSpeed
        {
            get { return _DesiredRateOfSpeed; }
            set
            {
                //  Nie pozwalamy na ustawienie ujemnego przyspieszenia.
                if (value < 0.0) _DesiredRateOfSpeed = 0.0;
                else _DesiredRateOfSpeed = value;
            }
        }
        

        /// <summary>Przypisuje okienko dialogowe <see cref="ShipWindow"/> do konkretnego statku <paramref name="parent"/>. Inicjalizuje okienko.</summary>
        /// <param name="parent">Statek rodzic, do którego okienko jest przypisane.</param>
        /// 
        public ShipWindow(Ship parent)
        {
            InitializeComponent();

            ParentShip = parent;

            //  Początkowe ustawienia danych w okienku dialogowym ustawiamy na
            //  odpowiadające im dane statku.
            DesiredHeading = ParentShip.DesiredHeading;
            DesiredSpeed = ParentShip.DesiredSpeed;
            DesiredRateOfTurn = ParentShip.DesiredRateOfTurn;
            DesiredRateOfSpeed = ParentShip.DesiredRateOfSpeed;

            //  Powoduje wypisywanie kropki zamiast przecinka podczas konwersji liczb
            //  niecałkowitych na tekst.
            Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("en-US");

            //  Wypisanie posiadanych danych do GUI.
            HeadingInputBox.Text = (string)DesiredHeading;
            SpeedInputBox.Text = DesiredSpeed.ToString("##0.0");
            RoTInputBox.Text = DesiredRateOfTurn.ToString("##0.0");
            RoSInputBox.Text = DesiredRateOfSpeed.ToString("##0.0");
        }


        /// <summary>Wywołanie tej funkcji powoduje przekazanie danych z okienka dialogowego do odpowiadającego mu statku.</summary>
        /// 
        private void ApplyChanges()
        {
            //  Pobieramy dane z okienka dialogowego.
            try { DesiredSpeed = double.Parse(SpeedInputBox.Text); }
            catch { Console.WriteLine("Invalid data input"); }
            SpeedInputBox.Text = DesiredSpeed.ToString("##0.0");

            try { DesiredRateOfSpeed = double.Parse(RoSInputBox.Text); }
            catch { Console.WriteLine("Invalid data input"); }
            RoSInputBox.Text = DesiredRateOfSpeed.ToString("##0.0");

            try { DesiredRateOfTurn = double.Parse(RoTInputBox.Text); }
            catch { Console.WriteLine("Invalid data input"); }
            RoTInputBox.Text = DesiredRateOfTurn.ToString("##0.0");

            try { DesiredHeading = double.Parse(HeadingInputBox.Text); }
            catch { Console.WriteLine("Invalid data input"); }
            HeadingInputBox.Text = DesiredHeading.ToString("000.0");

            //  Dane z okienka dialogowego są przekazywane do statku.
            ParentShip.DesiredHeading = DesiredHeading;
            ParentShip.DesiredSpeed = DesiredSpeed;
            ParentShip.DesiredRateOfTurn = DesiredRateOfTurn;
            ParentShip.DesiredRateOfSpeed = DesiredRateOfSpeed;
        }


        /// <summary>Wywoływane gdy myszka wyjdzie poza okienko dialogowe.</summary>
        private void window_MouseLeave(object sender, MouseEventArgs e)
        {
            Hide();
            ApplyChanges();
        }


        /// <summary>Wywoływane gdy okienko dialogowe zostanie zdezaktywowane.</summary>
        private void window_Deactivated(object sender, EventArgs e)
        {
            Hide();
            ApplyChanges();
        }

    
    //  OBSŁUGA PRZYCISKÓW


        private void HeadingMinus1Button_Click(object sender, RoutedEventArgs e)
        {
            DesiredHeading -= 1.0;
            HeadingInputBox.Text = DesiredHeading.ToString("000.0");
        }

        private void HeadingPlus1Button_Click(object sender, RoutedEventArgs e)
        {
            DesiredHeading += 1.0;
            HeadingInputBox.Text = DesiredHeading.ToString("000.0");
        }

        private void HeadingMinus10Button_Click(object sender, RoutedEventArgs e)
        {
            DesiredHeading -= 10.0;
            HeadingInputBox.Text = DesiredHeading.ToString("000.0");
        }

        private void HeadingPlus10Button_Click(object sender, RoutedEventArgs e)
        {
            DesiredHeading += 10.0;
            HeadingInputBox.Text = DesiredHeading.ToString("000.0");
        }

        private void RoTMinus1Button_Click(object sender, RoutedEventArgs e)
        {
            DesiredRateOfTurn -= 1.0;
            RoTInputBox.Text = DesiredRateOfTurn.ToString("##0.0");
        }

        private void RoTPlus1Button_Click(object sender, RoutedEventArgs e)
        {
            DesiredRateOfTurn += 1.0;
            RoTInputBox.Text = DesiredRateOfTurn.ToString("##0.0");
        }

        private void SpeedMinus1Button_Click(object sender, RoutedEventArgs e)
        {
            DesiredSpeed -= 1.0;
            SpeedInputBox.Text = DesiredSpeed.ToString("##0.0");
        }

        private void SpeedPlus1Button_Click(object sender, RoutedEventArgs e)
        {
            DesiredSpeed += 1.0;
            SpeedInputBox.Text = DesiredSpeed.ToString("##0.0");
        }

        private void RoSMinus1Button_Click(object sender, RoutedEventArgs e)
        {
            DesiredRateOfSpeed -= 1.0;
            RoSInputBox.Text = DesiredRateOfSpeed.ToString("##0.0");
        }

        private void RoSPlus1Button_Click(object sender, RoutedEventArgs e)
        {
            DesiredRateOfSpeed += 1.0;
            RoSInputBox.Text = DesiredRateOfSpeed.ToString("##0.0");
        }

        private void HeadingInputBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                try { DesiredHeading = double.Parse(HeadingInputBox.Text); }
                catch { Console.WriteLine("Invalid data input"); }
                
                HeadingInputBox.Text = DesiredHeading.ToString("000.0");
            }
        }

        private void RoTInputBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                try { DesiredRateOfTurn = double.Parse(RoTInputBox.Text); }
                catch { Console.WriteLine("Invalid data input"); }

                RoTInputBox.Text = DesiredRateOfTurn.ToString("##0.0");
            }
        }

        private void SpeedInputBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                try { DesiredSpeed = double.Parse(SpeedInputBox.Text); }
                catch { Console.WriteLine("Invalid data input"); }

                SpeedInputBox.Text = DesiredSpeed.ToString("##0.0");
            }
        }

        private void RoSInputBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                try { DesiredRateOfSpeed = double.Parse(RoSInputBox.Text); }
                catch { Console.WriteLine("Invalid data input"); }

                RoSInputBox.Text = DesiredRateOfSpeed.ToString("##0.0");
            }
        }
    }
}



