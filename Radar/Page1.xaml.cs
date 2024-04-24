using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Radar
{
    /// <summary>
    /// Logika interakcji dla klasy Page1.xaml
    /// </summary>
    public partial class Page1 : Page
    {
        RotateTransform rotateTransform1;
        ScaleTransform scaler;
        private bool isMouseLeftButtonDown = false;
        Point oldMousePosition;

        public Page1()
        {
            InitializeComponent();
            rotateTransform1 = new();

            Poli.MouseEnter += new MouseEventHandler(ExecuteEvent);
            Elipsa.MouseEnter += new MouseEventHandler(ExecuteEvent);
            

            scaler = new ScaleTransform(1.0, 1.0);
            scaler.CenterX = 1400;// MousePosition.X;
            scaler.CenterY = 1400;//MousePosition.Y;
            Main.LayoutTransform = scaler;
            scaler.ScaleX = 1.5;
            scaler.ScaleY = 1.5;
            


        }

        private void ExecuteEvent(object sender, MouseEventArgs e)
        {
            if (e.Source == Poli)
            {
                Console.WriteLine("Mouse Enter Poli");
            }
            else if (e.Source == Elipsa)
            {
                Console.WriteLine("Mouse Enter Elipsa");
            }

            e.Handled = true;
        }



        private void Mouse_In(object sender, MouseEventArgs e)
        {
            this.Text.Text = "IN";
            //this.Triangle.Fill.Opacity = 1.0;
            

        }

        private void Mouse_Out(object sender, MouseEventArgs e)
        {
            this.Text.Text = "OUT";
            //this.Triangle.Fill.Opacity = 0.0;
        }

        private void Mouse_Wheel(object sender, MouseWheelEventArgs e)
        {
            Point MousePositionFromPanel = Mouse.GetPosition(Main);
            Point MousePositionFromScroll = Mouse.GetPosition(ScrollVIewer);
            double first = MousePositionFromPanel.X * scaler.ScaleX;
            double second = ScrollVIewer.ActualWidth + ScrollVIewer.ContentHorizontalOffset - (ScrollVIewer.ActualWidth - MousePositionFromScroll.X);

            double firstY = MousePositionFromPanel.Y * scaler.ScaleY;
            double secondY = ScrollVIewer.ActualHeight + ScrollVIewer.ContentVerticalOffset - (ScrollVIewer.ActualHeight - MousePositionFromScroll.Y);


            //Trace.WriteLine(MousePositionFromPanel.X.ToString() + " " + MousePositionFromPanel.Y.ToString() + " " + ScrollVIewer.VerticalOffset.ToString() + " " + ScrollVIewer.ActualHeight);
            Trace.WriteLine(first.ToString() + " " + second.ToString());


            if (e.Delta > 0) { //this.Triangle.Fill.Opacity += 0.01;
                double scale = 1.2;
                scaler.ScaleX *= scale;
                scaler.ScaleY *= scale;
                //ScrollVIewer.LineLeft();
                double first_new = MousePositionFromPanel.X * scaler.ScaleX;
                double difference = first_new - first;
                ScrollVIewer.ScrollToHorizontalOffset(ScrollVIewer.HorizontalOffset + difference);

                double firstY_new = MousePositionFromPanel.Y * scaler.ScaleY;
                double differenceY = firstY_new - firstY;
                ScrollVIewer.ScrollToVerticalOffset(ScrollVIewer.VerticalOffset + differenceY);
                




            }
            else if (e.Delta < 0 && scaler.ScaleX >= 1.1 && scaler.ScaleY >= 1.1) { //this.Triangle.Fill.Opacity -= 0.01;
                double scale = 0.8333333333333;
                scaler.ScaleX *= scale;
                scaler.ScaleY *= scale;

                double first_new = MousePositionFromPanel.X * scaler.ScaleX;
                double difference = first_new - first;
                ScrollVIewer.ScrollToHorizontalOffset(ScrollVIewer.HorizontalOffset + difference);
                //ScrollVIewer.LineRight();
                //ScrollVIewer.ScrollToVerticalOffset(ScrollVIewer.VerticalOffset + 10);
                double firstY_new = MousePositionFromPanel.Y * scaler.ScaleY;
                double differenceY = firstY_new - firstY;
                ScrollVIewer.ScrollToVerticalOffset(ScrollVIewer.VerticalOffset + differenceY);
            }


        }

       private void HandleMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (sender is ScrollViewer && !e.Handled)
            {
                e.Handled = true;
                Mouse_Wheel(sender, e);  
                //var eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta);
                //eventArg.RoutedEvent = MouseWheelEvent;
                //eventArg.Source = sender;
                //var parent = ((Control)sender).Parent as UIElement;
                //parent.RaiseEvent(eventArg);
            }
        }

        private void Mouse_Wheel_Preview(object sender, MouseWheelEventArgs e)
        {

        }

        private void ScrollVIewer_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //scaler.ScaleX += 0.05;
            //scaler.ScaleY += 0.05;
        }

        private void DockPanel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            
        }

        private void Strona_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            scaler.ScaleX += 0.05;
            scaler.ScaleY += 0.05;

        }

        private void Panel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //scaler.ScaleX += 0.05;
            //scaler.ScaleY += 0.05;
        }

        private void Strona_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.Source == Text)
            {

            }
            else if (e.Source == ScrollVIewer || e.Source == Panel || e.Source == Main || e.Source == Ships || e.Source == Scroll2)
            {
                // Trace.WriteLine(Scroll2.(Mouse.GetPosition(Scroll2)));
                isMouseLeftButtonDown = true;
                Text.Text = "true";
                oldMousePosition = Mouse.GetPosition(ScrollVIewer);
                ScrollVIewer_MouseLeftButtonDown(sender, e);    
            }
            else    e.Handled = true; 
        }

        private void DockPanel_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void ScrollVIewer_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void Panel_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
             
        }

        private void Text_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            
        }

        private void Text_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void Strona_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (isMouseLeftButtonDown)
            {
                isMouseLeftButtonDown = false;
                Text.Text = "false";
                // ScrollVIewer_MouseLeftButtonDown(sender, e);
            }
            else if (e.Source == Text)
            {

            }
            else { e.Handled = true; }
        }

        private void ScrollVIewer_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            double difference = 0.0;
            //tb.Text = newMousePosition.X + " | " + newMousePosition.Y;

            if (isMouseLeftButtonDown)
            {
                Point newMousePosition = Mouse.GetPosition(ScrollVIewer);

                difference = newMousePosition.Y - oldMousePosition.Y;
                ScrollVIewer.ScrollToVerticalOffset(ScrollVIewer.VerticalOffset - difference);

                /*                if (newMousePosition.Y > oldMousePosition.Y)
                                    ScrollVIewer.ScrollToVerticalOffset(ScrollVIewer.VerticalOffset - 1);*/

                difference = newMousePosition.X - oldMousePosition.X;
                ScrollVIewer.ScrollToHorizontalOffset(ScrollVIewer.HorizontalOffset - difference);

                /*if (newMousePosition.X > oldMousePosition.X)
                    ScrollVIewer.ScrollToHorizontalOffset(ScrollVIewer.HorizontalOffset - 1);*/

                oldMousePosition = newMousePosition;
            }
            e.Handled = true;
        }

        private void ScrollVIewer_MouseLeave(object sender, MouseEventArgs e)
        {
            if (isMouseLeftButtonDown)
            {
                isMouseLeftButtonDown = false;
                Text.Text = "false";
                // ScrollVIewer_MouseLeftButtonDown(sender, e);
            }
        }
    }
}
