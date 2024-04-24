using System;
using System.Windows.Controls;
using System.Windows.Threading;


namespace Radar
{
    //  MainPage - partial
    //  Part 1 - MainPage.xaml.cs
    //  Part 2 - below
    public partial class MainPage : Page
    {
        //  Funkcja oblicza kolejne pozycje statków
        //  - odpowiada za symulację ruchu statków.
        private void ComputeAllShipsMovement()
        {
            //  Zmienne wykorzystywane dla wszystkich statków.
            Heading desiredHeading;
            Heading startHeading;
            Heading endHeading;
            Heading stepHeadingChange;
            Heading meanHeading;
            double desiredSpeed;
            double startSpeed;
            double endSpeed;
            double meanSpeed;
            double stepSpeedChange;
            double desiredRoT;
            double startRoT;
            double stepRoT;
            double endRoT;
            double desiredRoS;
            double startRoS;
            double stepRoS;
            double endRoS;
            double positionX;
            double positionY;

            //  Moment obliczania nowej pozycji statku.
            DateTime now = DateTime.Now;
            TimeSpan span;
            double time;
            
            //  Dla każdego statku.
            foreach (Ship ship in ShipsList)
            {
                //  Przypisujemy parametry konkretnego statku do lokalnych zmiennych
                //  uwzględniając odpowiednie przeliczenia jednostek.
                //  Funkcja przelicza w jednostkach systemu SI.
                desiredHeading = ship.DesiredHeading;
                startHeading = ship.ActualHeading;
                desiredSpeed = ship.DesiredSpeed * Param.KnotsToMs;
                startSpeed = ship.ActualSpeed * Param.KnotsToMs;
                desiredRoT = ship.DesiredRateOfTurn * Param.DegPerMinToDegPerSec;
                startRoT = ship.ActualRateOfTurn * Param.DegPerMinToDegPerSec;
                desiredRoS = ship.DesiredRateOfSpeed * Param.KnPerMinToMsPerSec;
                startRoS = ship.ActualRateOfSpeed * Param.KnPerMinToMsPerSec;
                positionX = ship.Position.X;
                positionY = ship.Position.Y;

                //  Pobieramy czas jaki upłynął od ostatniej zmiany pozycji statku.
                span = now - ship.Time;
                time = span.TotalSeconds;


                //  OBLICZAMY HEADING I RATE OF TURN

                //  Jeżeli płyniemy kursem docelowym.
                if (startHeading == desiredHeading)
                {
                    meanHeading = startHeading;
                    endHeading = startHeading;
                    endRoT = 0.0;
                }
                //  Jeżeli statek powinien skręcić w lewo.
                else if (Heading.ShouldTurnLeft(startHeading, desiredHeading))
                {
                    stepRoT = desiredRoT;
                    stepHeadingChange = stepRoT * time;
                    endHeading = startHeading - stepHeadingChange;

                    //  Jeżeli statek nie osiągnął kursu docelowego.
                    if (Heading.ShouldTurnLeft(endHeading, desiredHeading))
                    {
                        meanHeading = startHeading - 0.5 * (double)stepHeadingChange;
                        endRoT = desiredRoT;
                    }
                    //  Jeżeli statek osiągnął kurs docelowy lub go przekroczył.
                    else
                    {
                        meanHeading = startHeading - 0.5 * (double)stepHeadingChange;
                        endRoT = 0.0;
                        endHeading = desiredHeading;
                    }
                }
                //  Jeżeli statek powinien skręcić w prawo.
                else
                {
                    stepRoT = desiredRoT;
                    stepHeadingChange = stepRoT * time;
                    endHeading = startHeading + stepHeadingChange;

                    //  Jeżeli statek nie osiągnął kursu docelowego.
                    if (Heading.ShouldTurnRight(endHeading, desiredHeading))
                    {
                        meanHeading = startHeading + 0.5 * (double)stepHeadingChange;
                        endRoT = desiredRoT;
                    }
                    //  Jeżeli statek osiągnął kurs docelowy lub go przekroczył.
                    else
                    {
                        meanHeading = startHeading + 0.5 * (double)stepHeadingChange;
                        endRoT = 0.0;
                        endHeading = desiredHeading;
                    }
                }


                //  OBLICZAMY SPEED I RATE OF SPEED

                //  Jeżeli statek płynie z prędkością docelową.
                if (startSpeed == desiredSpeed)
                {
                    meanSpeed = startSpeed;
                    endSpeed = startSpeed;
                    endRoS = 0.0;
                }
                //  Jeżeli statek powinien przyspieszyć.
                else if (startSpeed < desiredSpeed)
                {
                    stepRoS = desiredRoS;
                    stepSpeedChange = stepRoS * time;
                    endSpeed = startSpeed + stepSpeedChange;

                    //  Jeżeli statek nie osiągnął prędkości docelowej.
                    if (endSpeed < desiredSpeed)
                    {
                        meanSpeed = startSpeed + 0.5 * stepSpeedChange;
                        endRoS = desiredRoS;
                    }
                    //  Jeżeli statek osiągnął prędkość docelową lub ją przekroczył.
                    else
                    {
                        meanSpeed = startSpeed + 0.5 * stepSpeedChange;
                        endRoS = 0.0;
                        endSpeed = desiredSpeed;
                    }
                }
                //  Jeżeli statek powinien zwolnić.
                else
                {
                    stepRoS = desiredRoS;
                    stepSpeedChange = stepRoS * time;
                    endSpeed = startSpeed - stepSpeedChange;

                    //  Jeżeli statek nie osiągnął prędkości docelowej.
                    if (endSpeed > desiredSpeed)
                    {
                        meanSpeed = startSpeed - 0.5 * stepSpeedChange;
                        endRoS = desiredRoS;
                    }
                    //  Jeżeli statek osiągnął prędkość docelową lub ją przekroczył.
                    else
                    {
                        meanSpeed = startSpeed - 0.5 * stepSpeedChange;
                        endRoS = 0.0;
                        endSpeed = desiredSpeed;
                    }
                }


                //  OBLICZAMY NOWĄ POZYCJĘ STATKU

                positionX += meanSpeed * time * Math.Sin(meanHeading.InRadians);
                positionY += meanSpeed * time * Math.Cos(meanHeading.InRadians);


                //  PRZYPISUJEMY NOWE ZMIENNE DO STATKU 

                ship.ActualHeading = endHeading;
                ship.ActualSpeed = endSpeed * Param.MsToKnots;
                ship.ActualRateOfTurn = endRoT * Param.DegPerSecToDegPerMin;
                ship.ActualRateOfSpeed = endRoS * Param.MsPerSecToKnPerMin;
                ship.Position.X = positionX;
                ship.Position.Y = positionY;
                ship.Time = now;
                ship.ARPA.MyShipActualPosition.X = positionX;
                ship.ARPA.MyShipActualPosition.Y = positionY;
                ship.ARPA.MyShipActualHeading = endHeading;
                ship.ARPA.MyShipActualSpeed = endSpeed;

                ship.ARPA.SendOSDMessage();
            }

            //  Aktualizujemy pozycje statków na mapie.
            Dispatcher.BeginInvoke((Action)delegate () { UpdateAllShipsPosition(); });
        }
    }
}
