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
using Phidgets; //Needed for the RFID class and the PhidgetException class
using Phidgets.Events; //Needed for the phidget event handling classes

namespace SensorGUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml Subtle
    /// </summary>
    /// 
    public partial class MainWindow : Window
    {
        private int isConnected;
        private RFID rfid; //Declare an RFID object
        private ErrorEventBox errorBox;
        enum States
        {
            DISCONNECTED = 0,
            CONNECTING = 1,
            CONNECTED = 2
        };

        
        public MainWindow()
        {
            InitializeComponent();
            Initialization();
        }

        public int Initialization()
        {
            errorBox = new ErrorEventBox();
            isConnected = (int)States.CONNECTED;
            return 0;
        }

        private void On_Loaded()
        {
            updateStatus();
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

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            updateStatus();
            rfid = new RFID();
            rfid.Attach += new AttachEventHandler(rfid_Attach);
            rfid.Detach += new DetachEventHandler(rfid_Detach);
            rfid.Error += new ErrorEventHandler(rfid_Error);

            rfid.Tag += new TagEventHandler(rfid_Tag);
            rfid.TagLost += new TagEventHandler(rfid_TagLost);

        }

        void rfid_Attach(object sender, AttachEventArgs e)
        {
            RFID attached = (RFID)sender;
            Reader_Information.Text += attached.Attached.ToString();
            Reader_Information.Text += "\nName: " + attached.Name;
            isConnected = (int)States.CONNECTED;
        }

        void rfid_Detach(object sender, DetachEventArgs e)
        {
            RFID detached = (RFID)sender;
            Reader_Information.Text = String.Empty;
            isConnected = (int)States.DISCONNECTED;
        }

        void rfid_Error(object sender, ErrorEventArgs e)
        {
            Phidget phid = (Phidget)sender;
            switch (e.Type)
            {
                case PhidgetException.ErrorType.PHIDGET_ERR_BADPASSWORD:
                    phid.close();
                    break;
                default:
                    if (!errorBox.Visible)
                        errorBox.Show();
                    break;
            }
            errorBox.addMessage(DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString() + " : " + e.Description);
        }

        void rfid_Tag(object sender, TagEventArgs e)
        {
            
        }

        
    }
}
