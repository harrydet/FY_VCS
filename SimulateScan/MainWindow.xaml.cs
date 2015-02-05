using Phidgets;
using Phidgets.Events;
using System.Data;
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
using System.Net.Http;

namespace SimulateScan
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private RFID rfid;
        private ErrorEventBox errorBox;
        private TagEventArgs tag;
        private RFID reader;
        public MainWindow()
        {
            InitializeComponent();
            Initialization();
        }
        public int Initialization()
        {
            errorBox = new ErrorEventBox();
            return 0;
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
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
            reader = (RFID)sender;
            try
            {
                Dispatcher.Invoke(new Action(() =>
                {
                    Title_Text.Text += "\nReader: Connected";
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
                    Title_Text.Text = "Bring tag close to sensor to simulate scan";
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
            tag = e;
            sendPostRequest(tag);
        }

        void rfid_TagLost(object sender, TagEventArgs e)
        {
            try
            {
                Dispatcher.Invoke(new Action(() =>
                {
                    Request_Text.Text = "Tag Lost";
                }));
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.ToString());
            }
        }

        private async void sendPostRequest(TagEventArgs tag)
        {
            using (var client = new HttpClient())
            {
                var values = new List<KeyValuePair<string, string>>();
                values.Add(new KeyValuePair<string, string>("tag_code", tag.Tag));
                values.Add(new KeyValuePair<string, string>("reader_serial", reader.SerialNumber.ToString()));
                var content = new FormUrlEncodedContent(values);

                var response = await client.PostAsync("http://178.62.34.201/phpTagResponse/respondWithPush.php", content);

                var responseString = await response.Content.ReadAsStringAsync();
                try
                {
                    Dispatcher.Invoke(new Action(() =>
                    {
                        Request_Text.Text = "Request sent with code: " + tag.Tag.ToString();
                    }));
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show(ex.ToString());
                }
                System.Windows.MessageBox.Show(responseString);
            }
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
            catch
            {
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
