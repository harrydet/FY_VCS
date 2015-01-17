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
using Phidgets.Events;
using System.Net.Http;
using System.Data; //Needed for the phidget event handling classes

namespace SensorGUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml Subtle
    /// </summary>
    /// 
    public partial class MainWindow : Window
    {
        private int isConnectedReader;
        private int isConnectedTag;
        private RFID rfid; //Declare an RFID object
        private ErrorEventBox errorBox;
        enum Reader_States
        {
            DISCONNECTED = 0,
            CONNECTING = 1,
            CONNECTED = 2
        };
        enum Tag_States
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
            isConnectedReader = (int)Reader_States.DISCONNECTED;
            return 0;
        }

        private void On_Loaded()
        {
            updateStatus();
        }

        public void updateStatus()
        {
            SolidColorBrush readerBrush = new SolidColorBrush();
            SolidColorBrush tagBrush = new SolidColorBrush();

            switch (isConnectedReader)
            {
                case (int)Tag_States.DISCONNECTED:
                    readerBrush.Color = Color.FromArgb(255, 255, 0, 0);
                    statusReader.Fill = readerBrush;
                    break;
                case (int)Tag_States.CONNECTING:
                    readerBrush.Color = Color.FromArgb(255, 255, 255, 0);
                    statusReader.Fill = readerBrush;
                    break;
                case (int)Tag_States.CONNECTED:
                    readerBrush.Color = Color.FromArgb(255, 0, 255, 0);
                    statusReader.Fill = readerBrush;
                    break;
                default:
                    break;
            }

            switch (isConnectedTag)
            {
                case (int)Tag_States.DISCONNECTED:
                    tagBrush.Color = Color.FromArgb(255, 255, 0, 0);
                    statusTag.Fill = tagBrush;
                    break;
                case (int)Tag_States.CONNECTING:
                    tagBrush.Color = Color.FromArgb(255, 255, 255, 0);
                    statusTag.Fill = tagBrush;
                    break;
                case (int)Tag_States.CONNECTED:
                    tagBrush.Color = Color.FromArgb(255, 0, 255, 0);
                    statusTag.Fill = tagBrush;
                    break;
                default:
                    break;
            }
            readerBrush = null;
            tagBrush = null;
        }

        private async void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            using (var client = new HttpClient())
            {
                var values = new List<KeyValuePair<string, string>>();
                values.Add(new KeyValuePair<string, string>("first_name", "namefromc"));
                values.Add(new KeyValuePair<string, string>("last_name", "lastnamefromc"));
                
                var content = new FormUrlEncodedContent(values);

                var response = await client.PostAsync("http://178.62.34.201/phpDatabase/db_createTagEntry.php", content);

                var responseString = await response.Content.ReadAsStringAsync();
                Request_Status_Info.Text = responseString;
                System.Windows.MessageBox.Show(responseString);
            }
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

            openCmdLine(rfid, null);

        }

        void rfid_Attach(object sender, AttachEventArgs e)
        {
            RFID attached = (RFID)sender;
            try
            {
                Dispatcher.Invoke(new Action(() =>
                {
                    Reader_Information.Text = "Name: " + attached.Name;
                    isConnectedReader = (int)Reader_States.CONNECTING;
                    updateStatus();
                }));
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.ToString());
            }
        }

        void rfid_Detach(object sender, DetachEventArgs e)
        {
            try
            {
                Dispatcher.Invoke(new Action(() =>
                {
                    RFID detached = (RFID)sender;
                    Reader_Information.Text = String.Empty;
                    isConnectedReader = (int)Reader_States.DISCONNECTED;
                    updateStatus();
                }));
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.ToString());
            }
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
            try
            {
                Dispatcher.Invoke(new Action(() =>
                {
                    Tag_Information.Text = "Tag Code: " + e.Tag;
                    Tag_Information.Text += "\nProtocol: " + e.protocol.ToString();
                    isConnectedTag = (int)Reader_States.CONNECTED;
                    updateStatus();
                    Submit_Tag_Button.IsEnabled = true;
                }));
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.ToString());
            }
        }

        void rfid_TagLost(object sender, TagEventArgs e)
        {
            try
            {
                Dispatcher.Invoke(new Action(() =>
                {
                    Tag_Information.Text = "";
                    isConnectedTag = (int)Reader_States.DISCONNECTED;
                    updateStatus();
                    Submit_Tag_Button.IsEnabled = false;
                }));
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.ToString());
            }
        }

        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            rfid.Attach -= new AttachEventHandler(rfid_Attach);
            rfid.Detach -= new DetachEventHandler(rfid_Detach);
            rfid.Tag -= new TagEventHandler(rfid_Tag);
            rfid.TagLost -= new TagEventHandler(rfid_TagLost);

            rfid.close();
        }

        #region Command line open functions
        private void openCmdLine(Phidget p, String pass)
        {
            int serial = -1;
            String logFile = null;
            int port = 5001;
            String host = null;
            bool remote = false, remoteIP = false;
            string[] args = Environment.GetCommandLineArgs();
            String appName = args[0];

            try
            { //Parse the flags
                for (int i = 1; i < args.Length; i++)
                {
                    if (args[i].StartsWith("-"))
                        switch (args[i].Remove(0, 1).ToLower())
                        {
                            case "l":
                                logFile = (args[++i]);
                                break;
                            case "n":
                                serial = int.Parse(args[++i]);
                                break;
                            case "r":
                                remote = true;
                                break;
                            case "s":
                                remote = true;
                                host = args[++i];
                                break;
                            case "p":
                                pass = args[++i];
                                break;
                            case "i":
                                remoteIP = true;
                                host = args[++i];
                                if (host.Contains(":"))
                                {
                                    port = int.Parse(host.Split(':')[1]);
                                    host = host.Split(':')[0];
                                }
                                break;
                            default:
                                goto usage;
                        }
                    else
                        goto usage;
                }
                if (logFile != null)
                    Phidget.enableLogging(Phidget.LogLevel.PHIDGET_LOG_INFO, logFile);
                if (remoteIP)
                    p.open(serial, host, port, pass);
                else if (remote)
                    p.open(serial, host, pass);
                else
                    p.open(serial);
                return; //success
            }
            catch {
                System.Windows.MessageBox.Show("Oops");
            }
        usage:
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Invalid Command line arguments." + Environment.NewLine);
            sb.AppendLine("Usage: " + appName + " [Flags...]");
            sb.AppendLine("Flags:\t-n   serialNumber\tSerial Number, omit for any serial");
            sb.AppendLine("\t-l   logFile\tEnable phidget21 logging to logFile.");
            sb.AppendLine("\t-r\t\tOpen remotely");
            sb.AppendLine("\t-s   serverID\tServer ID, omit for any server");
            sb.AppendLine("\t-i   ipAddress:port\tIp Address and Port. Port is optional, defaults to 5001");
            sb.AppendLine("\t-p   password\tPassword, omit for no password" + Environment.NewLine);
            sb.AppendLine("Examples: ");
            sb.AppendLine(appName + " -n 50098");
            sb.AppendLine(appName + " -r");
            sb.AppendLine(appName + " -s myphidgetserver");
            sb.AppendLine(appName + " -n 45670 -i 127.0.0.1:5001 -p paswrd");
            System.Windows.MessageBox.Show(sb.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);

            System.Windows.Application.Current.Shutdown();
        }
        #endregion


    }
}
