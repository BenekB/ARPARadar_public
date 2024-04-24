using NMEA;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Point = System.Windows.Point;

namespace Radar
{

    //  Klasa reprezentująca komputer pokładowy radaru.
    //  Będzie otrzymywać zaszumione dane dotyczące statków oraz
    //  generować na tej podstawie dane w formacie NMEA.
    public class ARPADevice
    {

        //  WŁAŚCIWOŚCI


        /// <summary>Rzeczywiste pozycje poszczególnych statków.</summary>
        public Dictionary<int , List<Position>> ShipsPositions = [];

        /// <summary>Zaszumione pozycje poszczególnych statków.</summary>
        Dictionary<int, List<Position>> ShipsPositionsWithNoise = [];

        /// <summary>Pozycje obserwowanych statków po dokonanej filtracji</summary>
        Dictionary<int, List<Position>> PositionsAfterFiltering = [];

        Dictionary<int, FinalFilter> FiltersList = [];
        public Dictionary<int, FilterOutputData> FiltersOutputList = [];

        /// <summary>Interfejs komunikacyjny z komputerem pokładowym ARPA. Przez niego przekazywane są dane o aktualnym położeniu własnego statku do komputera ARPA.</summary>
        ShipsPosition MyShipsActualPosition = new();

        /// <summary>Po zmianach zawiera informaję o pozycji, kursie oraz prędkości.</summary>
        private Position MyShipPosition;
        public Heading MyShipActualHeading;
        /// <summary>W metrach na sekundę.</summary>
        public double MyShipActualSpeed;
        public Position MyShipActualPosition;

        public bool ConsoleOutputIsEnabled = true;
        public bool FileOutputIsEnabled = true;
        private int MyShipID;
        
        FinalFilter? filter;

        /// <summary>Interfejs poprzez który dane wyznaczone przez filtr mogą być odczytywane przez komputer ARPA</summary>
        public FilterOutputData ShipsOutputData = new();

        string path = "C:\\Users\\bened\\Desktop\\Data\\";
        List<string> savingPaths = [];
        public static string SavingPath = "";
        private readonly string PrivateSavingPath;

        public ARPADevice(int ID)
        {
            MyShipID = ID;
            Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("en-US");
            PrivateSavingPath = SavingPath + "\\Ship_" + ID + ".txt";

            //if (MyShipID == 0)
            //{
            //    savingPaths.Add(string.Empty);
            //}

        }


        public void SendOSDMessage()
        {
            OSD msg = new()
            {
                Heading = (float)MyShipsActualPosition.HDG,
                HeadingStatus = 'A',
                VesselCourse = (float)MyShipsActualPosition.HDG,
                CourseReference = 'P',
                VesselSpeed = (float)MyShipsActualPosition.Speed,
                SpeedReference = 'P',
                SpeedUnits = 'N'
            };

            try
            {
                File.AppendAllText(PrivateSavingPath, msg.GenerateMessage() + "\n");
            }
            catch { }
            
        }
        
        /// <summary>Funkcja dodaje szum do pozycji zawartej w <paramref name="position"/>.</summary>
        /// <param name="position">Parametr zawierający pozycję statku.</param>
        /// <returns>Pozycję zaszumioną.</returns>
        private Position AddNoise(Position position)
        {
            Random random = new();
            SphericalPosition spherical = position.ToSpherical(MyShipPosition.X, MyShipPosition.Y);
            spherical.Angle += random.NextDouble(-1, 1) * Param.BearingAccuracy;
            spherical.Range += random.NextDouble(-1, 1) * Param.RangeAccuracy * spherical.Range;

            return spherical.ToCartesian(MyShipPosition.X, MyShipPosition.Y);
        }


        public void InputData(int ID, Position position)
        {
            
            // Console.WriteLine(PrivateSavingPath);
            Position noisePos = AddNoise(position);

            //  Jeżeli nadchodzące dane pochodzą z innego statku
            if (MyShipID != ID)
            {
                //  Jeżeli w bazie danych nie ma statku z danym ID, to tworzymy nowy rekord w bazie danych
                if (!ShipsPositions.ContainsKey(ID))
                {
                    ShipsPositions.Add(ID, []);
                    ShipsPositionsWithNoise.Add(ID, []);
                    PositionsAfterFiltering.Add(ID, []);
                    FiltersOutputList.Add(ID, new FilterOutputData());
                    FiltersList.Add(ID, new FinalFilter(ShipsPositionsWithNoise[ID], PositionsAfterFiltering[ID], MyShipsActualPosition, FiltersOutputList[ID]));

                    TLB msgTLB = new();

                    foreach(int key in ShipsPositions.Keys)
                    {
                        msgTLB.Targets.Add((Number: key % 1023, Label: $"ID_{key}"));
                    }

                    try
                    {
                        File.AppendAllText(PrivateSavingPath, msgTLB.GenerateMessage() + "\n");
                    }
                    catch { }

                }

                ShipsPositions[ID].Add(position);
                ShipsPositionsWithNoise[ID].Add(AddNoise(position));

                FiltersList[ID].OneStepProcess();


                if (ConsoleOutputIsEnabled)
                {
                    //  Wypisuje wiadomości NMEA w konsoli
                }

                if (FileOutputIsEnabled)
                {
                    //  TUTAJ POWINNY BYĆ GENEROWANE WIADOMOSCI NMEA

                    //  TLL
                    TLL msgTLL = new();
                    msgTLL.TargetNumber = ID % 99;
                    if (FiltersList[ID].filterHDGOutput.Count > 0)
                    {
                        msgTLL.LAT = Latitude.FromCartesianCoordinates(FiltersList[ID].filterHDGOutput[^1].Y);
                        msgTLL.NorthSouth = 'N';
                        msgTLL.LON = Longitude.FromCartesianCoordinates(FiltersList[ID].filterHDGOutput[^1].X);
                        msgTLL.EastWest = 'E';
                    }
                    msgTLL.Utc = new UTC((short)noisePos.Time.Hour, (short)noisePos.Time.Minute, noisePos.Time.Second);
                    msgTLL.Status = 'T';

                    try
                    {
                        File.AppendAllText(PrivateSavingPath, msgTLL.GenerateMessage() + "\n");
                    }
                    catch { }


                    //  TTM
                    TTM msgTTM = new();
                    msgTTM.TargetNumber = (short)(ID % 99);
                    if (FiltersList[ID].filterHDGOutput.Count > 0)
                    {
                        SphericalPosition sPos = (FiltersList[ID].filterHDGOutput[^1]).ToSpherical(MyShipsActualPosition.X, MyShipsActualPosition.Y);
                        msgTTM.TargetDistance = (float)(sPos.Range * Param.MetersToNm);
                        msgTTM.BearingFromOwnShip = (float)sPos.Angle;
                        msgTTM.BearingClass = 'T';
                    }
                    if (!double.IsNaN(FiltersOutputList[ID].LongSpeed)) msgTTM.TargetSpeed = (float)(FiltersOutputList[ID].LongSpeed * Param.MsToKnots);
                    if (!double.IsNaN((double)FiltersOutputList[ID].LongHeading))
                    {
                        msgTTM.TargetCourse = (float)FiltersOutputList[ID].LongHeading;
                        msgTTM.CourseClass = 'T';
                    }
                    if (!double.IsNaN(FiltersOutputList[ID].CPA)) msgTTM.DistanceCPA = (float)FiltersOutputList[ID].CPA;
                    if (!double.IsNaN(FiltersOutputList[ID].TCPA)) msgTTM.TimeTCPA = (float)FiltersOutputList[ID].TCPA;
                    msgTTM.Status = 'T';
                    msgTTM.Utc = new UTC((short)noisePos.Time.Hour, (short)noisePos.Time.Minute, noisePos.Time.Second);
                    msgTTM.Type = 'A';

                    try
                    {
                        File.AppendAllText(PrivateSavingPath, msgTTM.GenerateMessage() + "\n");
                    }
                    catch { }




                    //string data = $"{position.X}\t" +
                    //    $"{position.Y}\t" +
                    //    $"{position.HDG.ToString()}\t" +
                    //    $"{position.Speed * Param.KnotsToMs}\t" +
                    //    $"{noisePos.X}\t" +
                    //    $"{noisePos.Y}\t" +
                    //    $"{MyShipActualPosition.X}\t" +
                    //    $"{MyShipActualPosition.Y}\t" +
                    //    $"{MyShipActualHeading.ToString()}\t" +
                    //    $"{MyShipActualSpeed}\n";
                    
                    //File.AppendAllText(PrivateSavingPath, data);
                }
            }
            // Jeżeli informacja dotyczy mojego własnego statku.
            else 
            { 
                MyShipPosition = position;
                MyShipsActualPosition.X = position.X;
                MyShipsActualPosition.Y = position.Y;
                MyShipsActualPosition.HDG = position.HDG;
                MyShipsActualPosition.Speed = position.Speed;
                MyShipsActualPosition.Time = position.Time;

                //  Generate TTD Message
                TTD msg = new();

                foreach (int key in ShipsPositions.Keys)
                {
                    EncapsulatedTrackedTargetData data = new();
                    data.TargetNumber = key % 1023;
                    if (FiltersList[key].filterHDGOutput.Count > 0)
                    {
                        SphericalPosition sPos = (FiltersList[key].filterHDGOutput[^1]).ToSpherical(MyShipsActualPosition.X, MyShipsActualPosition.Y);
                        data.Distance = (sPos.Range * Param.MetersToNm);
                        data.TrueBearing = (float)sPos.Angle;
                    }
                    if (!double.IsNaN(FiltersOutputList[key].LongSpeed)) data.Speed = FiltersOutputList[key].LongSpeed * Param.MsToKnots;
                    if (!double.IsNaN((double)FiltersOutputList[key].LongHeading))
                    {
                        data.Course = (double)FiltersOutputList[key].LongHeading;
                    }
                    if (!double.IsNaN(FiltersOutputList[key].CPA))
                    {
                        data.TargetStatus = 4;

                    }

                    msg.Add(data);
                }

                try
                {
                    File.AppendAllText(PrivateSavingPath, msg.GenerateMessage() + "\n");
                }
                catch { }

            }

        }

        public void DeleteShip(int ID)
        {
            if (ShipsPositions.ContainsKey(ID))
            {
                ShipsPositions.Remove(ID);
                ShipsPositionsWithNoise.Remove(ID);
                FiltersOutputList.Remove(ID);
                FiltersList.Remove(ID);
                PositionsAfterFiltering.Remove(ID);
            }
        }


        //public void ResetShip(int ID)
        //{
        //    if (ShipsPositions.ContainsKey(ID))
        //    {
        //        ShipsPositions[ID].Clear();
        //        ShipsPositionsWithNoise[ID].Clear();
        //        FiltersList[ID]
        //    }
        //}


        private void SendMessage(INMEAMessage message)
        {
            Console.WriteLine(message.GenerateMessage());
        }

    }
}
