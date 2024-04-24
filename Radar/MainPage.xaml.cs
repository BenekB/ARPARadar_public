using System;
using System.Collections.Generic;
using System.DirectoryServices.ActiveDirectory;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ScottPlot;
using Extreme;
using Extreme.DataAnalysis;
using Extreme.Statistics;
using Extreme.Mathematics;
using System.Diagnostics;
using System.Windows.Interop;
using NMEA;
using System.Transactions;
using ScottPlot.MarkerShapes;
using Point = System.Windows.Point;
using MyColor = System.Drawing.Color;
using System.Threading;
using Timer = System.Timers.Timer;
using System.Timers;
using System.Windows.Threading;
using System.Runtime.CompilerServices;
using static ScottPlot.Plottable.PopulationPlot;
using Vector = System.Windows.Vector;
using System.Collections;
using Microsoft.Win32;

namespace Radar
{

    //  MainPage - partial
    //  Part 1 - below
    //  Part 2 - ShipsMovement.cs
    /// <summary>
    /// 
    /// </summary>
    /// <param name="MinScale">Stała wskazująca minimalną skalę powiększenia mapy.</param>
    public partial class MainPage : Page
    {
        ///<remarks>To są remarks</remarks>
        const double MinScale = 1.5; ///<summary>Opis.</summary>
        readonly ScaleTransform Scaler;
        bool IsRadarSourceChanging;
        bool IsShipBeingDeleted = false;
        Ship? RadarSource;
        Point OldMousePosition;
        bool isMouseLeftButtonDown = false;
        readonly List<Ship> ShipsList = [];
        DateTime IsMouseLeftButtonDownEnabled;
        DateTime IsMouseLeftButtonUpEnabled;
        Ship? ShipBeingDragged;
        Ship? FocusedShip;
        double _MapOffsetX;
        double MapOffsetX
        {
            get { return _MapOffsetX; }
            set 
            {
                if (value >= 0) _MapOffsetX = value;
                else _MapOffsetX = 0;
            }
        }
        double _MapOffsetY;
        double MapOffsetY
        {
            get { return _MapOffsetY; }
            set
            {
                if (value >= 0) _MapOffsetY = value;
                else _MapOffsetY = 0;
            }
        }
        private readonly Timer RadarTimer;
        private readonly Timer ShipsTimer;
        RadarScale ActualRadarScale = new();
        private int _ShipID = 1;
        private int ShipID
        {
            get { return _ShipID++; }
        }
        private int FirstObjectsID = 2;
        private int SecondObjectsID = 3;





        /// <summary>
        /// hjghj
        /// </summary>
        
        public MainPage(MainWindow main)
        {
            MessageBox.Show("Po pierwsze wybierz lokalizację zapisu danych wyjściowych programu", "Ważna informacja!");

            OpenFolderDialog dialog = new() { Title = "Select location to save output data" };
            dialog.ShowDialog();
            System.IO.Directory.CreateDirectory(dialog.FolderName + "\\ARPAOutputData");
            ARPADevice.SavingPath = dialog.FolderName + "\\ARPAOutputData";

            InitializeComponent();

            IsMouseLeftButtonDownEnabled = DateTime.MinValue;
            IsMouseLeftButtonUpEnabled = DateTime.MinValue;
            ShipBeingDragged = null;
            MapOffsetX = MapScroll.HorizontalOffset;
            MapOffsetY = MapScroll.VerticalOffset;
            ShipInfoGrid.Visibility = Visibility.Collapsed;
            Scaler = new ScaleTransform(1.0, 1.0);
            Map.LayoutTransform = Scaler;
            Scaler.ScaleX = MinScale;
            Scaler.ScaleY = MinScale;
            ScaleLabel.Text = (56000 / 1.5).ToString("#0") + " m";
            IsRadarSourceChanging = false;
            ActualRadarScale.ActualScale = RadarScale.Nm0p75;
            RangeTextBlock.Text = (string)ActualRadarScale;

            {
                RadarTimer = new Timer(Param.RadarCircleTimeMs);
                RadarTimer.Elapsed += RadarTimerElapsed;
                RadarTimer.AutoReset = true;
                RadarTimer.Enabled = true;

                ShipsTimer = new Timer(1000);
                ShipsTimer.Elapsed += ShipsTimerElapsed;
                ShipsTimer.AutoReset = true;
                ShipsTimer.Enabled = true;

                CreateNewShip(1852 * 10, 1852 * 10, 0, 100, 30);
                ShipsList[^1].ActualSpeed = 5;
                ShipsList[^1].DesiredSpeed = 5;
                RadarSource = ShipsList[^1];

                CreateNewShip(1852 * 15.0, 1852 * 15.0, 237, 100, 30);
                ShipsList[^1].ActualSpeed = 16.6;
                ShipsList[^1].DesiredSpeed = 16.6;

                CreateNewShip(1852 * 15.0, 1852 * 16.0, 237, 100, 30);
                ShipsList[^1].ActualSpeed = 16.6;
                ShipsList[^1].DesiredSpeed = 16.6;

                CreateNewShip(1852 * 15.0, 1852 * 17.0, 237, 100, 30);
                ShipsList[^1].ActualSpeed = 16.6;
                ShipsList[^1].DesiredSpeed = 16.6;

                CreateNewShip(1852 * 16.0, 1852 * 15.0, 237, 100, 30);
                ShipsList[^1].ActualSpeed = 16.6;
                ShipsList[^1].DesiredSpeed = 16.6;

                CreateNewShip(1852 * 17.0, 1852 * 15.0, 237, 100, 30);
                ShipsList[^1].ActualSpeed = 16.6;
                ShipsList[^1].DesiredSpeed = 16.6;

                /*
                CreateNewShip(1852 * 9, 1852 * 9, 45, 100, 30);
                ShipsList[^1].ActualSpeed = 10;
                ShipsList[^1].DesiredSpeed = 10;

                CreateNewShip(1852 * 7, 1852 * 9, 45, 100, 30);
                ShipsList[^1].ActualSpeed = 10;
                ShipsList[^1].DesiredSpeed = 10;

                CreateNewShip(1852 * 4, 1852 * 9, 45, 100, 30);
                ShipsList[^1].ActualSpeed = 10;
                ShipsList[^1].DesiredSpeed = 10;

                CreateNewShip(1852 * 10, 1852 * 11, 45, 100, 30);
                ShipsList[^1].ActualSpeed = 10;
                ShipsList[^1].DesiredSpeed = 10;

                CreateNewShip(1852 * 10, 1852 * 13, 45, 100, 30);
                ShipsList[^1].ActualSpeed = 10;
                ShipsList[^1].DesiredSpeed = 10;

                CreateNewShip(1852 * 10, 1852 * 16, 45, 100, 30);
                ShipsList[^1].ActualSpeed = 10;
                ShipsList[^1].DesiredSpeed = 10;
                */

            }

            InitializeRadarPlot();

            
        }


        //  FUNKCJE ROBOCZE

        /// <summary>
        /// Tworzy nowy statek
        /// </summary>
        /// <param name="x">Współrzędna horyzontalna [mila morska]</param>
        /// <param name="y">Współrzędna wertykalna [mila morska]</param>
        /// <param name="heading"></param>
        /// <param name="length"></param>
        /// <param name="beam"></param>
        private void CreateNewShip(double x, double y, double heading, double length, double beam)
        {
            Ship ship = new(length, beam, ShipID, heading: heading);
            ship.SetPosition(x, y);
            ConnectMouseActivity(ship);
            Ships.Children.Add(ship.Icon);
            ShipsList.Add(ship);
            UpdateAllShipsPosition();
        }


        private void CreateNewShipButton_Clicked(object sender, EventArgs e)
        {
            try
            {
                double positionX = double.Parse(AddShipPositionXInputBox.Text);
                double positionY = double.Parse(AddShipPositionYInputBox.Text);
                double heading = double.Parse(AddShipHeadingInputBox.Text);
                double beam = double.Parse(AddShipBeamInputBox.Text);
                double length = double.Parse(AddShipLengthInputBox.Text);

                CreateNewShip(positionX, positionY, heading, length, beam);
            }
            catch  { Console.WriteLine("Invalid input data."); }         
        }


        private void RangePlus_Clicked(object sender, EventArgs e)
        {
            ActualRadarScale++;
            RangeTextBlock.Text = (string)ActualRadarScale;
            RefreshRadarWindow();
        }

        private void RangeMinus_Clicked(object sender, EventArgs e)
        {
            ActualRadarScale--;
            RangeTextBlock.Text = (string)ActualRadarScale;
            RefreshRadarWindow();
        }

        private void SetRadarWindowSourceButton_Clicked(object sender, EventArgs e)
        {
            SetRadarWindowSourceButtonText.Text = "Choose one of the ships";
            IsRadarSourceChanging = true;
        }


        private void RadarTimerElapsed(object? source, ElapsedEventArgs e)
        {
            Dispatcher.BeginInvoke((Action)delegate () { RefreshRadarWindow(true); });
        }

        
        private void ShipsTimerElapsed(object? source, ElapsedEventArgs e)
        {
            
            ComputeAllShipsMovement();

            if (FocusedShip != null)
            {
                Dispatcher.BeginInvoke(delegate () { UpdateShipInfoBox(FocusedShip); });
            }
        }


        private void RefreshRadarWindow(bool isStandardRefresh = false)
        {
            if(RadarSource != null)
            {
                Position position = new();

                plt.Plot.Clear();
                plt.Plot.SetAxisLimits(
                    xMin: -ActualRadarScale,
                    xMax: ActualRadarScale,
                    yMin: -ActualRadarScale,
                    yMax: ActualRadarScale);

                plt.Plot.AddCircle(0, 0, ActualRadarScale / 3.0, color: MyColor.Red);
                plt.Plot.AddCircle(0, 0, ActualRadarScale * 2.0 / 3.0, color: MyColor.Red);
                plt.Plot.AddCircle(0, 0, ActualRadarScale, color: MyColor.Red);

                foreach (Ship ship in ShipsList)
                {

                    var el = plt.Plot.AddEllipse(
                        x: ship.Position.X - RadarSource.Position.X,
                        y: ship.Position.Y - RadarSource.Position.Y,
                        xRadius: ship.Width * ActualRadarScale / 1000.0,
                        yRadius: ship.Length * ActualRadarScale / 1000.0,
                        color: MyColor.Yellow
                        );
                    el.Rotation = (float)ship.Rotation.Angle;
                    el.Color = MyColor.Yellow;

                    if (!(RadarSource.ID == ship.ID))
                        plt.Plot.AddText(
                            label: ship.ID.ToString(), 
                            x: ship.Position.X - RadarSource.Position.X, 
                            y: ship.Position.Y - RadarSource.Position.Y, 
                            size: 20, 
                            color: MyColor.Cyan);
                    
                    if (isStandardRefresh)
                    {
                        position.X = ship.Position.X;
                        position.Y = ship.Position.Y;
                        position.Time = ship.Time;
                        position.HDG = ship.ActualHeading;
                        position.Speed = ship.ActualSpeed;


                        foreach (Ship arpaShip in ShipsList)
                        {
                            arpaShip.ARPA.InputData(ship.ID, position);
                        }
                    }
                    
                   
                }

                plt.Refresh();

                //  Odświeżanie danych śledzonych statków
                if (RadarSource.ARPA.FiltersOutputList.ContainsKey(FirstObjectsID))
                {
                   
                    FirstObjectsInfoText.Content = $"Object - ID {FirstObjectsID}";
                    FirstObjectsShortHeadingText.Content = $"Heading 1:   {RadarSource.ARPA.FiltersOutputList[FirstObjectsID].ShortHeading.ToString("000.0")}";
                    FirstObjectsShortSpeedText.Content =   $"Speed 1:      {RadarSource.ARPA.FiltersOutputList[FirstObjectsID].ShortSpeed * Param.MsToKnots:#0.0} kn";
                    FirstObjectsShortTCPAText.Content =    $"TCPA 1:       {RadarSource.ARPA.FiltersOutputList[FirstObjectsID].ShortTCPA:##0.0} min";
                    FirstObjectsShortCPAText.Content =     $"CPA 1:         {RadarSource.ARPA.FiltersOutputList[FirstObjectsID].ShortCPA:#0.00} Nm";
                    FirstObjectsLongHeadingText.Content =  $"Heading 3:   {RadarSource.ARPA.FiltersOutputList[FirstObjectsID].LongHeading.ToString("000.0")}";
                    FirstObjectsLongSpeedText.Content =    $"Speed 3:      {RadarSource.ARPA.FiltersOutputList[FirstObjectsID].LongSpeed * Param.MsToKnots:#0.0} kn";
                    FirstObjectsLongTCPAText.Content =     $"TCPA 3:       {RadarSource.ARPA.FiltersOutputList[FirstObjectsID].TCPA:##0.0} min";
                    FirstObjectsLongCPAText.Content =      $"CPA 3:         {RadarSource.ARPA.FiltersOutputList[FirstObjectsID].CPA:#0.00} Nm";
                }

                if (RadarSource.ARPA.FiltersOutputList.ContainsKey(SecondObjectsID))
                {
                    SecondObjectsInfoText.Content = $"Object - ID {SecondObjectsID}";
                    SecondObjectsShortHeadingText.Content = $"Heading 1:   {RadarSource.ARPA.FiltersOutputList[SecondObjectsID].ShortHeading.ToString("000.0")}";
                    SecondObjectsShortSpeedText.Content = $"Speed 1:      {RadarSource.ARPA.FiltersOutputList[SecondObjectsID].ShortSpeed*Param.MsToKnots:#0.0} kn";
                    SecondObjectsShortTCPAText.Content = $"TCPA 1:       {RadarSource.ARPA.FiltersOutputList[SecondObjectsID].ShortTCPA:##0.0} min";
                    SecondObjectsShortCPAText.Content = $"CPA 1:         {RadarSource.ARPA.FiltersOutputList[SecondObjectsID].ShortCPA:#0.00} Nm";
                    SecondObjectsLongHeadingText.Content = $"Heading 3:   {RadarSource.ARPA.FiltersOutputList[SecondObjectsID].LongHeading.ToString("000.0")}";
                    SecondObjectsLongSpeedText.Content = $"Speed 3:      {RadarSource.ARPA.FiltersOutputList[SecondObjectsID].LongSpeed * Param.MsToKnots:#0.0} kn";
                    SecondObjectsLongTCPAText.Content = $"TCPA 3:       {RadarSource.ARPA.FiltersOutputList[SecondObjectsID].TCPA:##0.0} min";
                    SecondObjectsLongCPAText.Content = $"CPA 3:         {RadarSource.ARPA.FiltersOutputList[SecondObjectsID].CPA:#0.00} Nm";
                }

                OwnShipInfoText.Content = $"My Ship Data";
                OwnShipHeadingText.Content = $"Heading:   {RadarSource.ActualHeading.ToString("000.0")}";
                OwnShipSpeedText.Content = $"Speed:       {RadarSource.ActualSpeed:#0.0} kn";
                OwnShipLATText.Content = $"Position X:  {RadarSource.Position.X:##0} m";
                OwnShipLONText.Content = $"Position Y:  {RadarSource.Position.Y:##0} m";

            }


        }


        private void InitializeRadarPlot()
        {
            plt.Plot.Style(ScottPlot.Style.Black);
            plt.Plot.Frameless();
            plt.Configuration.LeftClickDragPan = false;
            plt.Configuration.Zoom = false;
            plt.Plot.SetAxisLimits(-600, 600, -600, 600);
            plt.Plot.Grid(enable: false);
            plt.Plot.AxisScaleLock(true); ;
            RefreshRadarWindow();
        }


    //  SKOMENTOWANE I GOTOWE FUNKCJE


        //  Funkcja aktualizuje położenie wszystkich statków na mapie.
        private void UpdateAllShipsPosition()
        {
            foreach (Ship ship in ShipsList)
            {
                ship.Translation.X = Param.XPositionToScreen(ship.Position.X) * Scaler.ScaleX - MapOffsetX;
                ship.Translation.Y = Param.YPositionToScreen(ship.Position.Y) * Scaler.ScaleY - MapOffsetY;
                ship.Rotation.Angle = (double)ship.ActualHeading;
            }
        }


        //  Funkcja aktualizuje dane dotyczące rozpatrywanego statku w okienku
        //  informacyjnym w lewym górnym rogu programu.
        private void UpdateShipInfoBox(Ship ship)
        {
            PositionXValueTextBlock.Text = (ship.Position.X).ToString("##0");
            PositionYValueTextBlock.Text = (ship.Position.Y).ToString("##0");
            SpeedValueTextBlock.Text = ship.ActualSpeed.ToString("##0.0");
            HeadingValueTextBlock.Text = (string)ship.ActualHeading;
        }


        //  Funkcja łączy zdarzenia instancji klasy Ship z funkcjami ich obsługi.
        private void ConnectMouseActivity(Ship ship)
        {
            ship.Icon.MouseEnter += (sender, e) => Ship_MouseEnter(ship);
            ship.Icon.MouseLeave += (sender, e) => Ship_MouseLeave(ship);
            ship.Icon.MouseLeftButtonDown += (sender, e) => Ship_MouseLeftButtonDown(ship);
            ship.Icon.MouseLeftButtonUp += (sender, e) => Ship_MouseLeftButtonUp(ship);
            ship.Icon.MouseMove += (sender, e) => Ship_MouseMove(ship);
            
            UpdateAllShipsPosition();
        }

        /// <summary>
        /// Funkcja wywoływana gdy nastąpi ruch myszy w obiekcie Map_Grid lub potomnym. Służy do obsługi przeciągania statku na inne miejsce zapomocą myszki.
        /// </summary>
        private void Map_Grid_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            //  Jeżeli jakiś statek jest obecnie przeciągany.
            if (ShipBeingDragged != null)
            {
                //  Pobieram pozycję myszki po przeciągnięciu.
                Point newMousePosition = Mouse.GetPosition(Map);

                //  Ustawiam pozycję przeciąganego statku na miejsce,
                //  w którym znajduje się kursor myszy.
                (ShipBeingDragged.Position.X, ShipBeingDragged.Position.Y) = Param.ScreenToPosition(newMousePosition.X, newMousePosition.Y);
                ShipBeingDragged.Time = DateTime.Now;

                UpdateShipInfoBox(ShipBeingDragged);

                UpdateAllShipsPosition();
            }

            //  Pobieram pozycję myszki po przeciągnięciu.
            Point new2MousePosition = Mouse.GetPosition(Map);
            double x, y;
            (x, y) = Param.ScreenToPosition(new2MousePosition.X, new2MousePosition.Y);
            MousePositionXValueTextBlock.Text = x.ToString("#0");
            MousePositionYValueTextBlock.Text = y.ToString("#0");



        }


        //  Funkcja obsługuje ruch myszki nad obiektem typu Ship
        private void Ship_MouseMove(Ship source)
        {
            //  Jeżeli lewy przycisk myszki jest wciśnięty.
            if (source.IsMouseLeftButtonDown)
            {
                //  Jeżeli wykryta została próba przeciągania statku, to tło obszaru Ships,
                //  na którym statek może być przeciągany jest ustawiane na przezroczyste
                //  (wcześcniej było null). Jednocześnie przeciągany statek jest przesuwany
                //  na pierwszy plan oraz przypisywany do zmiennej globalnej ShipBeingDragged.
                Ships.Background = new SolidColorBrush(Color.FromArgb(0,0,0,0));
                Panel.SetZIndex(source.Icon, 1);
                ShipBeingDragged = source;

                UpdateShipInfoBox(source);
            }
            
        }


        //  Funkcja obsługuje puszczenie lewego przycisku myszki nad obiektem typu Ship.
        private void Ship_MouseLeftButtonUp(Ship source)
        {
            source.IsMouseLeftButtonDown = false;
            Ships.Background = null;
            Panel.SetZIndex(source.Icon, 0);

            //  Jeżeli żaden statek nie jest przesuwany oraz od ostatniego kliknięcia
            //  minęło ponad 100 milisekund.
            if (DateTime.Now > IsMouseLeftButtonUpEnabled && ShipBeingDragged == null)
            {
                IsMouseLeftButtonUpEnabled = DateTime.Now.AddMilliseconds(100);

                if (IsRadarSourceChanging)
                {
                    RadarSource = source;
                    SetRadarWindowSourceButtonText.Text = "Change Radar Window Source";
                    IsRadarSourceChanging = false;
                    RefreshRadarWindow();
                }
                else
                {
                    Point punkt = Mouse.GetPosition(Main);
                    punkt = Main.PointToScreen(punkt);
                    source.OpenDialogBox(punkt);
                }
            }

            //  Jeżeli jakiś statek był przeciągany, to od nowa zaczynamy wyliczanie jego parametrów
            foreach (Ship ship in ShipsList)
            {
                if (ShipBeingDragged is not null)
                    ship.ARPA.DeleteShip(ShipBeingDragged.ID);
            }

            //  Puszczenie przycisku myszki powoduje koniec przeciągania statku.
            ShipBeingDragged = null;

            

        }


        //  Funkcja obsługuje wciśnięcie lewego przycisku myszki nad obiektem typu Ship.
        private void Ship_MouseLeftButtonDown(Ship source)
        {
            //  Jeżeli od ostatniego wciśnięcia minęło co najmniej 100 milisekund.
            if (DateTime.Now > IsMouseLeftButtonDownEnabled)
            {
                source.IsMouseLeftButtonDown = true;
                OldMousePosition = Mouse.GetPosition(Ships);
                IsMouseLeftButtonDownEnabled = DateTime.Now.AddMilliseconds(100);

                if (IsShipBeingDeleted)
                {
                    int id = source.ID;

                    for (int i = 0; i < ShipsList.Count; i++)
                    {
                        if (ShipsList[i].ID == id)
                        {
                            ShipsList.RemoveAt(i);
                            break;
                        }
                    }

                    foreach (Ship ship in ShipsList)
                    {
                        ship.ARPA.DeleteShip(id);
                    }

                    IsShipBeingDeleted = false;
                    DeleteShipButtonText.Text = "Delete Ship";
                    Ships.Children.Remove(source.Icon);
                    source.IsMouseLeftButtonDown = false;
                }
            }

        }


        //  Funkcja obsługuje wyjście myszki poza obszar Ships.
        private void Ships_MouseLeave(object sender, MouseEventArgs e)
        {
            //  Wyjście myszki poza obszar Ships podczas przeciągania statku powoduje
            //  reakcję tożsamą z puszczeniem lewego przycisku myszy.
            if (ShipBeingDragged != null)
            {
                Panel.SetZIndex(ShipBeingDragged.Icon, 0);
                Ships.Background = null;
                ShipBeingDragged.IsMouseLeftButtonDown = false;
                ShipBeingDragged = null;
                ShipInfoGrid.Visibility = Visibility.Collapsed;
            }
        }


        //  Funkcja obsługuje wyjście myszki z obiektu typu Ship.
        private void Ship_MouseLeave(Ship source)
        {
            //  Jeżeli myszka jest nad statkiem, to zmienia się jego kolor.
            if (!source.Icon.IsMouseOver)
            {
                source.Icon.Fill = new SolidColorBrush(Colors.Black);

                if (!source.IsMouseLeftButtonDown)
                {
                    ShipInfoGrid.Visibility = Visibility.Collapsed;
                    FocusedShip = null;
                }
                    
            }
                
        }


        //  Funkcja obsługuje wejście myszki nad obiekt typu Ship.
        private void Ship_MouseEnter(Ship source)
        {
            //  Jeżeli myszka jest nad statkiem, to zmienia się jego kolor.
            if (source.Icon.IsMouseOver)
            {
                source.Icon.Fill = new SolidColorBrush(Colors.White);

                UpdateShipInfoBox(source);
                ShipInfoGrid.Visibility = Visibility.Visible;
                FocusedShip = source;
            }         
        }


        //  Funkcja obsługuje poruszanie kółkiem myszy nad obszarem mapy.
        private void Map_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            //  Proste skalowanie mapy zostało ulepszone, aby punktem skalowania było miejsce w
            //  którym znajduje się kursor myszki. Wymaga to określonego przesunięcia mapy w
            //  połączeniu ze skalowaniem.

            Point mousePositionFromMap = Mouse.GetPosition(Map);
            double firstX = mousePositionFromMap.X * Scaler.ScaleX;
            double firstY = mousePositionFromMap.Y * Scaler.ScaleY;

            //  Jeżeli kółko poruszyło się do przodu.
            if (e.Delta > 0)
            {
                //  Ustawianie skali mapy.
                double scale = 1.2;
                Scaler.ScaleX *= scale;
                Scaler.ScaleY *= scale;
                ScaleLabel.Text = (56000 / Scaler.ScaleX).ToString("#0") + " m";
                //Console.WriteLine(Scaler.ScaleX);

                double first_new = mousePositionFromMap.X * Scaler.ScaleX;
                double difference = first_new - firstX;
                MapOffsetX = MapScroll.HorizontalOffset + difference;
                MapScroll.ScrollToHorizontalOffset(MapOffsetX);

                double firstY_new = mousePositionFromMap.Y * Scaler.ScaleY;
                double differenceY = firstY_new - firstY;
                MapOffsetY = MapScroll.VerticalOffset + differenceY;
                MapScroll.ScrollToVerticalOffset(MapOffsetY);

                UpdateAllShipsPosition();
            }
            //  Jeżeli kółko poruszyło się do tyłu oraz skala mapy nie jest zbyt mała.
            else if (e.Delta < 0 && Scaler.ScaleX >= MinScale && Scaler.ScaleY >= MinScale)
            {
                double scale = 0.8333333333333;
                Scaler.ScaleX *= scale;
                Scaler.ScaleY *= scale;
                ScaleLabel.Text = (56000 / Scaler.ScaleX).ToString("#0") + " m";
                //Console.WriteLine(Scaler.ScaleX);

                double first_new = mousePositionFromMap.X * Scaler.ScaleX;
                double difference = first_new - firstX;
                MapOffsetX = MapScroll.HorizontalOffset + difference;
                MapScroll.ScrollToHorizontalOffset(MapOffsetX);

                double firstY_new = mousePositionFromMap.Y * Scaler.ScaleY;
                double differenceY = firstY_new - firstY;
                MapOffsetY = MapScroll.VerticalOffset + differenceY;
                MapScroll.ScrollToVerticalOffset(MapOffsetY);

                UpdateAllShipsPosition();
            }

            e.Handled = true;
        }


        //  Funkcja obsługuje wciśnięcie lewego przycisku myszki nad mapą. Wykorzystywane
        //  do przeciągania mapy. 
        private void Map_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            isMouseLeftButtonDown = true;
            OldMousePosition = Mouse.GetPosition(MapScroll);
            e.Handled = true;
        }


        //  Funkcja obsługuje zdarzenie opuszczenia mapy przez myszkę.
        private void Map_MouseLeave(object sender, MouseEventArgs e)
        {
            //  Jeżeli lewy przycisk myszki był wciśnięty i opuścił obszar mapy.
            if (isMouseLeftButtonDown)
            {
                isMouseLeftButtonDown = false;
            }
        }


        //  Funkcja obsługuje puszczenie lewego przycisku myszki nad mapą.
        private void Map_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (isMouseLeftButtonDown)
            {
                isMouseLeftButtonDown = false;
            }

            e.Handled = true;
        }


        //  Funkcja obsługuje zdarzenie poruszania się myszki nad mapą.
        private void Map_MouseMove(object sender, MouseEventArgs e)
        {
            //  Jeżeli lewy przycisk myszki został wciśnięty nad mapą i myszka zaczęła się poruszać,
            //  następuje przesuwanie mapy zgodnie z kierunkiem ruchu myszki.
            if (isMouseLeftButtonDown)
            {
                double difference;
                Point newMousePosition = Mouse.GetPosition(MapScroll);

                difference = newMousePosition.X - OldMousePosition.X;
                MapOffsetX = MapScroll.HorizontalOffset - difference;
                MapScroll.ScrollToHorizontalOffset(MapOffsetX);

                difference = newMousePosition.Y - OldMousePosition.Y;
                MapOffsetY = MapScroll.VerticalOffset - difference;
                MapScroll.ScrollToVerticalOffset(MapOffsetY);

                OldMousePosition = newMousePosition;
                UpdateAllShipsPosition();
            }



            e.Handled = true;
        }


        //  Funkcja obsługi Mouse Wheel w przypadku wykrycia przez warstwę Ships
        private void Ships_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            //  Funkcja obsługuje sutuację, gdy próba skalowania mapy została wykryta przez
            //  obiekt warstwy Ships zamiast warstwy mapy. Obsługa zdarzenia jest przekazywana
            //  do funkcji wykrywającej ruch kółka myszki nad mapą.

            Map_PreviewMouseWheel(sender, e);
        }

        private void FirstObjectUpChangeButton_Click(object sender, RoutedEventArgs e)
        {
            if (RadarSource is not null)
            {
                List<int> keys = new(RadarSource.ARPA.ShipsPositions.Keys);
                keys.Sort();
                int index = keys.IndexOf(FirstObjectsID);
                index++;
                
                if (index > keys.Count - 1)
                {
                    index = 0;
                }
                
                FirstObjectsID = keys[index];
                FirstObjectsInfoText.Content = $"Object - ID {keys[index]}";
            }
        }

        private void FirstObjectDownChangeButton_Click(object sender, RoutedEventArgs e)
        {
            if (RadarSource is not null)
            {
                List<int> keys = new(RadarSource.ARPA.ShipsPositions.Keys);
                keys.Sort();
                int index = keys.IndexOf(FirstObjectsID);
                index--;

                if (index < 0)
                {
                    index = keys.Count - 1;
                }

                FirstObjectsID = keys[index];
                FirstObjectsInfoText.Content = $"Object - ID {keys[index]}";
            }
        }

        private void SecondObjectUpChangeButton_Click(object sender, RoutedEventArgs e)
        {
            if (RadarSource is not null)
            {
                List<int> keys = new(RadarSource.ARPA.ShipsPositions.Keys);
                keys.Sort();
                int index = keys.IndexOf(SecondObjectsID);
                index++;

                if (index > keys.Count - 1)
                {
                    index = 0;
                }

                SecondObjectsID = keys[index];
                SecondObjectsInfoText.Content = $"Object - ID {keys[index]}";
            }
        }

        private void SecondObjectDownChangeButton_Click(object sender, RoutedEventArgs e)
        {
            if (RadarSource is not null)
            {
                List<int> keys = new(RadarSource.ARPA.ShipsPositions.Keys);
                keys.Sort();
                int index = keys.IndexOf(SecondObjectsID);
                index--;

                if (index < 0)
                {
                    index = keys.Count - 1;
                }

                SecondObjectsID = keys[index];
                SecondObjectsInfoText.Content = $"Object - ID {keys[index]}";
            }
        }

        private void DeleteShipButton_Click(object sender, RoutedEventArgs e)
        {
            DeleteShipButtonText.Text = "Choose The Ship You Want to Delete";
            IsShipBeingDeleted = true;

        }
    }
}