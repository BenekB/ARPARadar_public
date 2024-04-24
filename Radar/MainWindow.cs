using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Documents;
using ClassLibrary;
using Microsoft.Win32;
using ScottPlot.Plottable;
using iEKFFilter = Radar.EKFFilter2;




namespace Radar
{
    //  Klasa reprezentująca główne okno programu.
    public partial class MainWindow : Window
    {

        //  Główna strona programu.
        public MainPage? MainPage;

        public MainWindow()
        {

            InitializeComponent();

            

            //  Inicjalizacja głównej strony programu oraz jej wyświetlenie w
            //  głównym oknie.
            MainPage = new(this);
            Content = MainPage;
        }
    }
}
