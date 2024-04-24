using MathNet.Numerics.Distributions;
using NMEA;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using iEKFFilter = Radar.FinalFilter;
//using iEKFFilter = Radar.LinearApproximationFilter;

namespace Radar
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public class MyApp : Application
    {
        private bool _contentLoaded;

        /// <summary>
        /// Application Entry Point.
        /// </summary>
        [System.STAThreadAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "8.0.0.0")]
        public static void Main()
        {
            //double a = double.NaN;
            //Heading hdg = a;
            //double b = 2;

            //Console.WriteLine(a);
            //Console.WriteLine((double)hdg is 2);
            //Console.WriteLine(b is not double.NaN);
            //EncapsulatedTrackedTargetData ETTD = new();
            //ETTD.GenerateData();

            //NMEA.Message message = new NMEA.Message();
            //Console.WriteLine(message.Checksum("$GPGLL,5057.970,N,00146.110,E,142451,A"));

            



            // Przed uruchomieniem GUI
            bool isInputCorrect = false;

            while (isInputCorrect)
            {

                Console.Write("1 -> Aby otworzyæ pe³n¹ aplikacjê\n" +
                "2 -> Aby zastosowaæ filtr na gotowych danych\n" +
                "-> ");

                char input = (char)Console.Read();

                switch (input)
                {
                    case '1':
                        Console.Clear();
                        isInputCorrect = false;
                        MyApp app = new();
                        app.InitializeComponent();
                        app.Run();
                        break;
                    case '2':
                        Console.Clear();
                        isInputCorrect = false;
                        PerformFiltering();
                        break;
                    default:
                        Console.Clear();
                        Console.WriteLine("Niepoprawne dane wejœciowe. Spróbuj jeszcze raz.\n");
                        break;

                }
            }

            Console.Clear();
            MyApp mainApp = new();
            mainApp.InitializeComponent();
            mainApp.Run();
        }

        public static void PerformFiltering()
        {
            string folder = "new";

            var start = DateTime.Now;
            string[] paths = Directory.GetFiles("C:\\Users\\bened\\Desktop\\Data\\" + folder);
            string[] filesNames = new string[paths.Length];

            for (int i = 0; i < paths.Length; i++)
            {
                filesNames[i] = Path.GetFileName(paths[i]);
            }

            string path;
            string output;

            for (int i = 0; i < paths.Length; i++)
            {
                path = paths[i];
                output = "C:\\Users\\bened\\Desktop\\Data\\OUTEKF2\\" + filesNames[i];

                iEKFFilter filter = new iEKFFilter(path);

                filter.ProcessAll();
                filter.SaveOutputToFile(output);

                Console.WriteLine("\n\n");
            }

            //string path = "C:\\Users\\bened\\Desktop\\Data\\" + folder + "\\ID_1.txt";
            //string output = "C:\\Users\\bened\\Desktop\\Data\\OUTEKF2\\ID_1.txt";

            //iEKFFilter filter = new iEKFFilter(path);

            //filter.ProcessAll();
            //filter.SaveOutputToFile(output);

            
            //Console.WriteLine("\n\n");

            //path = "C:\\Users\\bened\\Desktop\\Data\\" + folder + "\\ID_2.txt";
            //output = "C:\\Users\\bened\\Desktop\\Data\\OUTEKF2\\ID_2.txt";

            //filter = new iEKFFilter(path);

            //filter.ProcessAll();
            //filter.SaveOutputToFile(output);

            //Console.WriteLine("\n\n");

            //path = "C:\\Users\\bened\\Desktop\\Data\\" + folder + "\\ID_3.txt";
            //output = "C:\\Users\\bened\\Desktop\\Data\\OUTEKF2\\ID_3.txt";

            //filter = new iEKFFilter(path);

            //filter.ProcessAll();
            //filter.SaveOutputToFile(output);

            //Console.WriteLine("\n\n");

            //path = "C:\\Users\\bened\\Desktop\\Data\\" + folder + "\\ID_4.txt";
            //output = "C:\\Users\\bened\\Desktop\\Data\\OUTEKF2\\ID_4.txt";

            //filter = new iEKFFilter(path);

            //filter.ProcessAll();
            //filter.SaveOutputToFile(output);

            //Console.WriteLine("\n\n");

            //path = "C:\\Users\\bened\\Desktop\\Data\\" + folder + "\\ID_5.txt";
            //output = "C:\\Users\\bened\\Desktop\\Data\\OUTEKF2\\ID_5.txt";

            //filter = new iEKFFilter(path);

            //filter.ProcessAll();
            //filter.SaveOutputToFile(output);

            //Console.WriteLine("\n\n");


            //path = "C:\\Users\\bened\\Desktop\\Data\\" + folder + "\\ID_6.txt";
            //output = "C:\\Users\\bened\\Desktop\\Data\\OUTEKF2\\ID_6.txt";

            //filter = new iEKFFilter(path);

            //filter.ProcessAll();
            //filter.SaveOutputToFile(output);

            Console.WriteLine("\n\n");
            var stop = DateTime.Now;
            TimeSpan diff = stop - start;
            

            //Console.WriteLine(diff.TotalMilliseconds);

        }

        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "8.0.0.0")]
        public void InitializeComponent()
        {
            if (_contentLoaded)
            {
                return;
            }
            _contentLoaded = true;

            this.StartupUri = new System.Uri("MainWindow.xaml", System.UriKind.Relative);

            System.Uri resourceLocater = new System.Uri("/Radar;component/app.xaml", System.UriKind.Relative);

            System.Windows.Application.LoadComponent(this, resourceLocater);

        }


    }
}