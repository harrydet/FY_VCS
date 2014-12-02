using System;
using System.Collections.Generic;
using System.Linq;
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
using System.Windows.Forms;

namespace SensorGUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml Subtle
    /// </summary>
    /// 
    public partial class MainWindow : Window
    {
        private int isConnected;
        enum States
        {
            DISCONNECTED = 0,
            CONNECTING = 1,
            CONNECTED = 2
        };

        


        public MainWindow()
        {
            InitializeComponent();
        }

        public int Initialization()
        {
            isConnected = (int)States.DISCONNECTED;
            return 0;
        }

        public void updateStatus()
        {
            SolidColorBrush onBrush = new SolidColorBrush();
            SolidColorBrush offBrush = new SolidColorBrush();

            if (isConnected == (int)States.DISCONNECTED)
            {
                offBrush.Color = Color.FromArgb(255, 255, 0, 0);
                StatusOFF.Fill = offBrush;
                onBrush.Color = Color.FromArgb(255, 255, 255, 255);
                StatusON.Fill = onBrush;
            }
            else if (isConnected == (int)States.CONNECTING)
            {
                offBrush.Color = Color.FromArgb(255, 255, 255, 0);
                StatusOFF.Fill = offBrush;
                onBrush.Color = Color.FromArgb(255, 255, 255, 255);
                StatusON.Fill = onBrush;
            }
            else
            {
                onBrush.Color = Color.FromArgb(255, 0, 255, 0);
                StatusON.Fill = onBrush;
                offBrush.Color = Color.FromArgb(255, 255, 255, 255);
                StatusOFF.Fill = offBrush;
            }
            onBrush = null;
            offBrush = null;
        }

        private void SimulateButton_Click(object sender, RoutedEventArgs e)
        {
            if (isConnected == (int)States.DISCONNECTED)
            {
                isConnected = (int)States.CONNECTING;
            }
            else if (isConnected == (int)States.CONNECTING)
            {
                isConnected = (int)States.CONNECTED;
            }
            else
            {
                isConnected = (int)States.DISCONNECTED;
            }
            updateStatus();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.MessageBox.Show("Please bring sensor in close proximity to the scanner", "Scanning...", MessageBoxButtons.OK);
        }

        
    }
}
