using MathNet.Numerics;
using Radar;
using ScottPlot.MarkerShapes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace NMEA
{


    public interface INMEAMessage
    {
        public string GenerateMessage();
    }


    class Message
    {
        protected readonly Dictionary<char, byte> ASCII = [];
        protected readonly Dictionary<char, string> Codes = [];
        protected readonly Dictionary<string, char> ReversCodes = [];

        // Konstruktor
        public Message() { Load_ASCII(); Load_Codes(); }

        protected void Load_Codes()
        {
            // słownik znaki służy do zamiany znaku ASCII na kod
            // składający się z 6 bitów
            Codes.Add('0', "000000");
            Codes.Add('1', "000001");
            Codes.Add('2', "000010");
            Codes.Add('3', "000011");
            Codes.Add('4', "000100");
            Codes.Add('5', "000101");
            Codes.Add('6', "000110");
            Codes.Add('7', "000111");
            Codes.Add('8', "001000");
            Codes.Add('9', "001001");
            Codes.Add(':', "001010");
            Codes.Add(';', "001011");
            Codes.Add('<', "001100");
            Codes.Add('=', "001101");
            Codes.Add('>', "001110");
            Codes.Add('?', "001111");
            Codes.Add('@', "010000");
            Codes.Add('A', "010001");
            Codes.Add('B', "010010");
            Codes.Add('C', "010011");
            Codes.Add('D', "010100");
            Codes.Add('E', "010101");
            Codes.Add('F', "010110");
            Codes.Add('G', "010111");
            Codes.Add('H', "011000");
            Codes.Add('I', "011001");
            Codes.Add('J', "011010");
            Codes.Add('K', "011011");
            Codes.Add('L', "011100");
            Codes.Add('M', "011101");
            Codes.Add('N', "011110");
            Codes.Add('O', "011111");
            Codes.Add('P', "100000");
            Codes.Add('Q', "100001");
            Codes.Add('R', "100010");
            Codes.Add('S', "100011");
            Codes.Add('T', "100100");
            Codes.Add('U', "100101");
            Codes.Add('V', "100110");
            Codes.Add('W', "100111");
            Codes.Add('`', "101000");
            Codes.Add('a', "101001");
            Codes.Add('b', "101010");
            Codes.Add('c', "101011");
            Codes.Add('d', "101100");
            Codes.Add('e', "101101");
            Codes.Add('f', "101110");
            Codes.Add('g', "101111");
            Codes.Add('h', "110000");
            Codes.Add('i', "110001");
            Codes.Add('j', "110010");
            Codes.Add('k', "110011");
            Codes.Add('l', "110100");
            Codes.Add('m', "110101");
            Codes.Add('n', "110110");
            Codes.Add('o', "110111");
            Codes.Add('p', "111000");
            Codes.Add('q', "111001");
            Codes.Add('r', "111010");
            Codes.Add('s', "111011");
            Codes.Add('t', "111100");
            Codes.Add('u', "111101");
            Codes.Add('v', "111110");
            Codes.Add('w', "111111");

            string keys = "0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVW`abcdefghijklmnopqrstuvw";

            // słownik kody zamienia 6 bitowy kod binarny na znak ASCII
            // ten kod jest związany z AIS i nie ma związku z kodem ASCII
            for (int i = 0; i < keys.Length; i++)
                ReversCodes.Add(Codes[keys[i]], keys[i]);
        }
        
        protected void Load_ASCII()
        {
            ASCII.Add(' ', 32);
            ASCII.Add('!', 33);
            ASCII.Add('"', 34);
            ASCII.Add('#', 35);
            ASCII.Add('$', 36);
            ASCII.Add('%', 37);
            ASCII.Add('&', 38);
            ASCII.Add('\'', 39);
            ASCII.Add('(', 40);
            ASCII.Add(')', 41);
            ASCII.Add('*', 42);
            ASCII.Add('+', 43);
            ASCII.Add(',', 44);
            ASCII.Add('-', 45);
            ASCII.Add('.', 46);
            ASCII.Add('/', 47);
            ASCII.Add('0', 48);
            ASCII.Add('1', 49);
            ASCII.Add('2', 50);
            ASCII.Add('3', 51);
            ASCII.Add('4', 52);
            ASCII.Add('5', 53);
            ASCII.Add('6', 54);
            ASCII.Add('7', 55);
            ASCII.Add('8', 56);
            ASCII.Add('9', 57);
            ASCII.Add(':', 58);
            ASCII.Add(';', 59);
            ASCII.Add('<', 60);
            ASCII.Add('=', 61);
            ASCII.Add('>', 62);
            ASCII.Add('?', 63);
            ASCII.Add('@', 64);
            ASCII.Add('A', 65);
            ASCII.Add('B', 66);
            ASCII.Add('C', 67);
            ASCII.Add('D', 68);
            ASCII.Add('E', 69);
            ASCII.Add('F', 70);
            ASCII.Add('G', 71);
            ASCII.Add('H', 72);
            ASCII.Add('I', 73);
            ASCII.Add('J', 74);
            ASCII.Add('K', 75);
            ASCII.Add('L', 76);
            ASCII.Add('M', 77);
            ASCII.Add('N', 78);
            ASCII.Add('O', 79);
            ASCII.Add('P', 80);
            ASCII.Add('Q', 81);
            ASCII.Add('R', 82);
            ASCII.Add('S', 83);
            ASCII.Add('T', 84);
            ASCII.Add('U', 85);
            ASCII.Add('V', 86);
            ASCII.Add('W', 87);
            ASCII.Add('X', 88);
            ASCII.Add('Y', 89);
            ASCII.Add('Z', 90);
            ASCII.Add('[', 91);
            ASCII.Add('\\', 92);
            ASCII.Add(']', 93);
            ASCII.Add('^', 94);
            ASCII.Add('_', 95);
            ASCII.Add('`', 96);
            ASCII.Add('a', 97);
            ASCII.Add('b', 98);
            ASCII.Add('c', 99);
            ASCII.Add('d', 100);
            ASCII.Add('e', 101);
            ASCII.Add('f', 102);
            ASCII.Add('g', 103);
            ASCII.Add('h', 104);
            ASCII.Add('i', 105);
            ASCII.Add('j', 106);
            ASCII.Add('k', 107);
            ASCII.Add('l', 108);
            ASCII.Add('m', 109);
            ASCII.Add('n', 110);
            ASCII.Add('o', 111);
            ASCII.Add('p', 112);
            ASCII.Add('q', 113);
            ASCII.Add('r', 114);
            ASCII.Add('s', 115);
            ASCII.Add('t', 116);
            ASCII.Add('u', 117);
            ASCII.Add('v', 118);
            ASCII.Add('w', 119);
            ASCII.Add('x', 120);
            ASCII.Add('y', 121);
            ASCII.Add('z', 122);
            ASCII.Add('{', 123);
            ASCII.Add('|', 124);
            ASCII.Add('}', 125);
            ASCII.Add('~', 126);
        }

        public string Checksum(string input)
        {
            int xor = 0b_0000_0000;

            foreach (char c in input)
            {
                if (c != '!' && c != '$' && c != '*' && c != '\0')
                {
                    xor ^= ASCII[c];
                }
            }

            string output = xor.ToString("X2");

            return output;
        }

    }


    //  Klasa niezbędna do wiadomości typu TTD
    class EncapsulatedTrackedTargetData : Message
    {
        /// <summary>The protocol version Value 0 to 3. Should always be set to zero. Other values reserved for future.</summary>
        public int ProtocolVersion = 0; //
        /// <summary>The target number associated with the label with corresponding number. Number zero means no tracking target. Values 0 to 1023.</summary>
        public int TargetNumber = 0; //
        /// <summary>North up coordinate system. Values 0 to 359.9 with step 0.1 deg. Value 409.5 = Invalid or N/A data.</summary>
        public double TrueBearing = 409.5; //
        /// <summary>Speed. Values 0 to 409.4 kn with step 0.1 kn. Value 409.5 = Invalid or N/A data.</summary>
        public double Speed = 409.5; //
        /// <summary>See speed. Values 0 to 359,9 with step 0,1. Value 409,5 = Invalid or N/A data.</summary>
        public double Course = 409.5; //
        /// <summary>Reported heading from AIS. Values 0 to 359,9. Value 409,5 and 409,4 = Invalid or N/A data.</summary>
        public double HeadingAIS = 409.5; //
        /// <summary>Target Status. 0 - Non-tracking; 1 - Acquiring target; 2 - Lost target; 4 - Established tracking; 6 - Established tracking, CPA Alarm; 7 - Established tracking, acknowledged CPA Alarm.</summary>
        public int TargetStatus = 0; //
        /// <summary>Operation mode. 0 - Autonomous (normal); 1 - Test target.</summary>
        public int OperationMode = 0; //
        /// <summary>Distance. Values 0 to 163,83 with step 0,01. Value 163.84 = Invalid or N/A data.</summary>
        public double Distance = 163.84; //
        /// <summary>0 - True speed & course; 1 - Relative speed & course.</summary>
        public int SpeedMode = 0; //
        /// <summary>0 - over the ground; 1 - through the water.</summary>
        public int StabilizationMode = 0; //
        /// <summary>Correlated / associated targets are assigned a common number. Values 0 to 255. Value 0 is reserved for no correlation / association.</summary>
        public int CorrelationAssociationNumber = 0; //


        public EncapsulatedTrackedTargetData(
            int protocolVersion = 0,
            int targetNumber = 0,
            double trueBearing = 409.5,
            double speed = 409.5,
            double course = 409.5,
            double headingAIS = 409.5,
            int targetStatus = 0,
            int operationMode = 0,
            double distance = 163.84,
            int speedMode = 0,
            int stabilizationMode = 0,
            int correlationAssociationNumber = 0) : base()
            {
                ProtocolVersion = protocolVersion;
                TargetNumber = targetNumber;
                TrueBearing = trueBearing;
                Speed = speed;
                Course = course;
                HeadingAIS = headingAIS;
                TargetStatus = targetStatus;
                OperationMode = operationMode;
                Distance = distance;
                SpeedMode = speedMode;
                StabilizationMode = stabilizationMode;
                CorrelationAssociationNumber = correlationAssociationNumber;
        }

        public string GenerateData()
        {
            string output = "";

            output += IntToBinaryString(ProtocolVersion, 2);
            output += IntToBinaryString(TargetNumber, 10);
            output += IntToBinaryString((int)(TrueBearing * 10), 12);
            output += IntToBinaryString((int)(Speed * 10), 12);
            output += IntToBinaryString((int)(Course * 10), 12);
            output += IntToBinaryString((int)(HeadingAIS * 10), 12);
            output += IntToBinaryString(TargetStatus, 3);
            output += IntToBinaryString(OperationMode, 1);
            output += IntToBinaryString((int)(Distance * 100), 14);
            output += IntToBinaryString(SpeedMode, 1);
            output += IntToBinaryString(StabilizationMode, 1);
            output += "00";
            output += IntToBinaryString(CorrelationAssociationNumber, 8);

            string finalOutput = "";

            for (int i = 0; i < 15; i++)
            {
                finalOutput += ReversCodes[output.Substring(6 * i, 6)];
            }

            return finalOutput;
        }

        private string IntToBinaryString(int input, int length)
        {
            string output;
            string binary = Convert.ToString(input, 2);

            string zeros = "";
            for (int i = 0; i < length - binary.Length; i++)
                zeros += '0';

            output = binary.Insert(0, zeros);

            return output;
        }

    }


    //  Gotowa funkcjonalność, można jeszcze poprawić składnię - Tracked Target Data
    class TTD : Message, INMEAMessage
    {
        List<EncapsulatedTrackedTargetData> EncapsulatedTrackedTargetDataList = [];
        private int TotalNumberOfSentencesInMessage;
        private int SentenceNumber = 1;
        private static int _SequentialMessageIdentifier = 0;
        private static int SequentialMessageIdentifier
        {
            get 
            {   
                if (_SequentialMessageIdentifier == 10)
                    _SequentialMessageIdentifier = 0;

                return _SequentialMessageIdentifier++;
            }
        }

        public TTD() : base() { }

        public void Add(EncapsulatedTrackedTargetData data)
        {
            EncapsulatedTrackedTargetDataList.Add(data);
        }

        public string GenerateMessage()
        {
            //  Jeżeli nie ma żadnych obiektów na liście, to zwracamy pusty string
            if (EncapsulatedTrackedTargetDataList.Count == 0) return "";

            //  Identyfikator danej sekwencji wiadomości
            int sequentialMessageIdentifier;
            //  Liczba obiektów zawartych w ostatniej wiadomości (od jednego do czterech)
            int lastSentenceTargets = EncapsulatedTrackedTargetDataList.Count % 4;
            //  Liczba wszystkich sentencji danej wiadomości
            int totalNumberOfSentencesInMessage = EncapsulatedTrackedTargetDataList.Count / 4;
            if (lastSentenceTargets != 0) totalNumberOfSentencesInMessage++;
            //  Liczba pełnych sentencji danej wiadomości - liczba sentencji zawierających informacje o czterech obiektach
            int numberOfFullSentencesInMessage = EncapsulatedTrackedTargetDataList.Count / 4;
            string finalOutput = "";
            //  Aktualny indeks sentenncji w danej wiadomości
            int actualSentenceNumber = 1;

            if (EncapsulatedTrackedTargetDataList.Count <= 4)
            {
                string output = "$RATTD,01,01,,";
                
                for (int i = 0; i < lastSentenceTargets; i++)
                {
                    output += EncapsulatedTrackedTargetDataList[i].GenerateData();
                }

                output += ",0*";
                output += Checksum(output);
                output += "<CR><LF>";

                return output;
            }
            else
            {
                sequentialMessageIdentifier = SequentialMessageIdentifier;

                for (int i = 0; i < numberOfFullSentencesInMessage; i++)
                {
                    string output = "$RATTD,";
                    output += totalNumberOfSentencesInMessage.ToString("X2") + ",";
                    output += actualSentenceNumber.ToString("X2") + ",";
                    output += sequentialMessageIdentifier.ToString() + ",";

                    for (int j = 0; j < 4; j++)
                    {
                        output += EncapsulatedTrackedTargetDataList[4 * i + j].GenerateData();
                    }

                    output += ",0*";
                    output += Checksum(output);
                    output += "<CR><LF>";

                    actualSentenceNumber++;
                    finalOutput += output + "\n";
                }

                if (lastSentenceTargets != 0)
                {
                    string output = "$RATTD,";
                    output += totalNumberOfSentencesInMessage.ToString("X2") + ",";
                    output += actualSentenceNumber.ToString("X2") + ",";
                    output += sequentialMessageIdentifier.ToString() + ",";

                    for (int j = lastSentenceTargets; j > 0; j--)
                    {
                        output += EncapsulatedTrackedTargetDataList[^j].GenerateData();
                    }

                    output += ",0*";
                    output += Checksum(output);
                    output += "<CR><LF>";

                    finalOutput += output;
                }
                else
                {
                    finalOutput = finalOutput.Remove(finalOutput.Length - 1);
                }

                return finalOutput;
            }
        }
    }


    //  Gotowa funkcja - Tracked Target Message 
    class TTM : Message, INMEAMessage
    {
        public  short   TargetNumber = 0;
        public  float   TargetDistance = float.NaN;
        public  float   BearingFromOwnShip = float.NaN;
        /// <summary>'T' - true, 'R' - relative</summary>
        public char     BearingClass = '\0';
        public  float   TargetSpeed = float.NaN;
        public  float   TargetCourse = float.NaN;
        /// <summary>'T' - true, 'R' - relative</summary>
        public char     CourseClass = '\0';
        public  float   DistanceCPA = float.NaN;
        public  float   TimeTCPA = float.NaN;
        public  char    Units = '\0';
        public  string  TargetName = "";
        public  char    Status = '\0';
        public  char    ReferenceTarget = '\0';
        public  UTC?    Utc = null;
        public  char    Type = '\0';

        public TTM() : base() { }

        public TTM(short targetNumber = 0, 
            float   targetDistance = float.NaN, 
            float   bearingFromOwnShip = float.NaN, 
            char    bearingClass = '\0', 
            float   targetSpeed = float.NaN, 
            float   targetCourse = float.NaN, 
            char    courseClass = '\0', 
            float   distanceCPA = float.NaN, 
            float   timeTCPA = float.NaN, 
            char    units = '\0', 
            string  targetName = "", 
            char    status = '\0', 
            char    referenceTarget = '\0', 
            UTC?    utc = null, 
            char    type = '\0') : base()
        {
            TargetNumber = targetNumber;
            TargetDistance = targetDistance;
            BearingFromOwnShip = bearingFromOwnShip;
            BearingClass = bearingClass;
            TargetSpeed = targetSpeed;
            TargetCourse = targetCourse;
            CourseClass = courseClass;
            DistanceCPA = distanceCPA;
            TimeTCPA = timeTCPA;
            Units = units;
            TargetName = targetName;
            Status = status;
            ReferenceTarget = referenceTarget;
            Utc = utc;
            Type = type;
        }

        public string GenerateMessage()
        {
            string output = "";

            output += "$RATTM,";
            output += TargetNumber.ToString("00") + ",";
            if (!float.IsNaN(TargetDistance)) output += TargetDistance.ToString("##.###").Replace(',', '.'); output += ",";
            if (!float.IsNaN(BearingFromOwnShip)) output += BearingFromOwnShip.ToString("###.#").Replace(',', '.'); output += ",";
            if (BearingClass != '\0') output += BearingClass; output += ",";
            if (!float.IsNaN(TargetSpeed)) output += TargetSpeed.ToString("##.##").Replace(',', '.'); output += ",";
            if (!float.IsNaN(TargetCourse)) output += TargetCourse.ToString("###.#").Replace(',', '.'); output += ",";
            if (CourseClass != '\0') output += CourseClass; output += ",";
            if (!float.IsNaN(DistanceCPA)) output += DistanceCPA.ToString("##.###").Replace(',', '.'); output += ",";
            if (!float.IsNaN(TimeTCPA)) output += TimeTCPA.ToString("#.###").Replace(',', '.'); output += ",";
            if (Units != '\0') output += Units; output += ",";
            output += TargetName + ",";
            if (Status != '\0') output += Status; output += ",";
            if (ReferenceTarget != '\0') output += ReferenceTarget; output += ",";
            if (Utc is not null) output += Utc.StringUTC; output += ",";
            if (Type != '\0') output += Type;
            output += "*" + Checksum(output);
            output += "<CR><LF>";

            return output;
        }

    }


    //  Gotowa funkcja - Target Latitude and Longitude
    class TLL : Message, INMEAMessage
    {
        public int TargetNumber = 0;
        public Latitude? LAT = null;
        public char NorthSouth = '\0';
        public Longitude? LON = null;
        public char EastWest = '\0';
        public string TargetName = "";
        public UTC? Utc = null;
        /// <summary>'L' - lost, 'Q' - acquisition, 'T' - tracking</summary>
        public char Status = '\0';
        public string ReferenceTarget = "";

        public TLL() : base() { }

        public TLL(int targetNumber = 0,
            Latitude? latitude = null,
            char northSouth = '\0',
            Longitude? longitude = null,
            char eastWest = '\0',
            string targetName = "",
            UTC? utc = null,
            char status = '\0',
            string referenceTarget = "") : base()
        {
            TargetNumber = targetNumber;
            LAT = latitude;
            NorthSouth = northSouth;
            LON = longitude;
            EastWest = eastWest;
            TargetName = targetName;
            Utc = utc;
            Status = status;
            ReferenceTarget = referenceTarget;
        }

        public string GenerateMessage()
        {
            string output = "";

            output += "$RATLL,";
            output += TargetNumber.ToString("00") + ",";
            if (LAT is not null) output += LAT.StringLAT; output += ",";
            if (NorthSouth != '\0') output += NorthSouth.ToString(); output += ",";
            if (LON is not null) output += LON.StringLON; output += ",";
            if (EastWest != '\0') output += EastWest.ToString(); output += ",";
            output += TargetName + ",";
            if (Utc is not null) output += Utc.StringUTC; output += ",";
            if (Status != '\0') output += Status.ToString(); output += ",";
            output += ReferenceTarget;
            output += "*" + Checksum(output);
            output += "<CR><LF>";

            return output;
        }
    }


    //  Gotowa funkcja - Own Ship Data
    class OSD : Message, INMEAMessage
    {
        /// <summary>Kurs statku - heading</summary>
        public  float   Heading = float.NaN;
        /// <summary>Status kursu: 'A' - sprawdzone dane, 'V' - brak danych</summary>
        public  char    HeadingStatus = '\0';
        /// <summary>Kurs rzeczywisty - kurs nad dnem</summary>
        public  float   VesselCourse = float.NaN;
        /// <summary>Źródło danych kursu: 'B' - bottom tracking log, 'M' - manually entered,
        /// 'W' - water referenced, 'R' - radar tracking (ro fixed target), 'P' - positioning system ground reference</summary>
        public  char    CourseReference = '\0';
        /// <summary>Prędkość statku</summary>
        public  float   VesselSpeed = float.NaN;
        /// <summary>Źródło danych prędkości: 'B' - bottom tracking log, 'M' - manually entered,
        /// 'W' - water referenced, 'R' - radar tracking (ro fixed target), 'P' - positioning system ground reference</summary>
        public char    SpeedReference = '\0';
        /// <summary>Vessel set, degrees true, manually entered</summary>
        public  float   VesselSet = float.NaN;
        /// <summary>Vessel drift (speed) - manually entered</summary>
        public  float   VesselDrift = float.NaN;
        /// <summary>Jednostka prędkości: 'K' - km/h, 'N' - knots</summary>
        public char    SpeedUnits = '\0';


        public OSD() : base() { }

        public OSD(float heading = float.NaN, 
            char    headingStatus = '\0',
            float   vesselCourse = float.NaN,
            char    courseReference = '\0',
            float   vesselSpeed = float.NaN,
            char    speedReference = '\0',
            float   vesselSet = float.NaN,
            float   vesselDrift = float.NaN,
            char    speedUnits = '\0') : base()
        {
            Heading = heading;
            HeadingStatus = headingStatus;
            VesselCourse = vesselCourse;
            CourseReference = courseReference;
            VesselSpeed = vesselSpeed;
            SpeedReference = speedReference;
            VesselSet = vesselSet;
            VesselDrift = vesselDrift;
            SpeedUnits = speedUnits;
        }

        public string GenerateMessage()
        {
            string output = "";

            output += "$RAOSD,";
            if (!float.IsNaN(Heading)) output += Heading.ToString("000.0##").Replace(",", "."); output += ",";
            if (HeadingStatus != '\0') output += HeadingStatus.ToString(); output += ",";
            if (!float.IsNaN(VesselCourse)) output += VesselCourse.ToString("000.0##").Replace(",", "."); output += ",";
            if (CourseReference != '\0') output += CourseReference.ToString(); output += ",";
            if (!float.IsNaN(VesselSpeed)) output += VesselSpeed.ToString("#00.0##").Replace(",", "."); output += ",";
            if (SpeedReference != '\0') output += SpeedReference.ToString(); output += ",";
            if (!float.IsNaN(VesselSet)) output += VesselSet.ToString("000.0#").Replace(",", "."); output += ",";
            if (!float.IsNaN(VesselDrift)) output += VesselDrift.ToString("000.0#").Replace(",", "."); output += ",";
            if (SpeedUnits != '\0') output += SpeedUnits.ToString();
            output += "*" + Checksum(output);
            output += "<CR><LF>";

            return output;
        }
    }


    //  Gotowa funkcja
    class TLB : Message, INMEAMessage
    {
        public List<(int Number, string Label)> Targets = [];

        public TLB() : base() { }

        public TLB(List<(int, string)> targets) : base()
        {
            Targets = targets;
        }

        public string GenerateMessage()
        {
            string output = "";

            output += "$RATLB";

            foreach (var (Number, Label) in Targets)
            {
                output += "," + Number.ToString();
                output += "," + Label;
            }

            output += "*";
            output += Checksum(output);
            output += "<CR><LF>";

            return output;
        }
    }


    class UTC
    {
        public int Hours = 0;
        int Minutes = 0;
        float Seconds = 0;
        private string _StringUTC = "";

        public string StringUTC
        {
            get
            {
                _StringUTC = "";
                _StringUTC += Hours.ToString("00");
                _StringUTC += Minutes.ToString("00");
                _StringUTC += Seconds.ToString("00.00").Replace(",", ".");

                return _StringUTC;
            }
        }

        public UTC(short hours, short minutes, float seconds)
        {
            Hours = hours;
            Minutes = minutes;
            Seconds = seconds;
        }

        public void Now()
        {
            Hours = DateTime.Now.Hour;
            Minutes = DateTime.Now.Minute;
            Seconds = (float)(DateTime.Now.Millisecond / 1000);
        }

    }

    class Latitude
    {
        public short Degrees;
        public float Minutes;
        public string StringLAT
        {
            get
            {
                string Latitude = Degrees.ToString("00");
                Latitude += Minutes.ToString("00.00").Replace(',', '.');

                return Latitude;
            }
        }

        public Latitude(float minutes, short degrees)
        {
            Degrees = degrees;
            Minutes = minutes;
        }
        
        public static Latitude FromCartesianCoordinates(double y)
        {
            short degres = (short)(y / 1852);
            float minutes = (float)(y % 1852 * 60 / 1852);

            return new Latitude(minutes, degres);
        }
    }

    class Longitude
    {
        public Longitude(float minutes, short degrees)
        {
            this.Degrees = degrees;
            this.Minutes = minutes;
        }

        public short Degrees;
        public float Minutes;
        public string StringLON
        {
            get
            {
                string Latitude = Degrees.ToString("000");
                Latitude += Minutes.ToString("00.00").Replace(',', '.');

                return Latitude;
            }
        }

        public static Longitude FromCartesianCoordinates(double x)
        {
            short degres = (short)(x / 1852);
            float minutes = (float)(x % 1852 * 60 / 1852);

            return new Longitude(minutes, degres);
        }
    }

}
