using Syncfusion.UI.Xaml.Controls.Input;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.Devices.SerialCommunication;
using Windows.Storage.Streams;
using System.Threading;
using Windows.Devices.Enumeration;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Windows.UI.Xaml;
// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace hvac_version_2
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        SolidColorBrush greenBrush = new SolidColorBrush(Colors.Green);
        SolidColorBrush redBrush = new SolidColorBrush(Colors.Red);
        //private bool updatingValues;


        private CancellationTokenSource ReadCancellationTokenSource;
        private SerialDevice serialPort = null;
        DataWriter dataWriteObject = null;
        DataReader dataReaderObject = null;
        public string strFromPort;
        private string final_string;

        public MainPage()
        {
            this.InitializeComponent();

                        
        }

        private async void stop_button_Click(object sender, RoutedEventArgs e)
        {
            //do something
            if (serialPort == null) return;
            await sendToPort("end");
        }

        private async void compressorA_button_Click(object sender, RoutedEventArgs e)
        {
            if (serialPort == null) return;
            await sendToPort("compressorAON");
        }

        private void set_temperature_changed(object sender, EventArgs e)
        {
            pump_status_light.Fill = redBrush;
        }

        private async void set_switching_time_value_changed(object sender, EventArgs e)
        {   
            double switching_time_minute = (double)(set_switching_time.Value);
            if (serialPort == null) return;
            await sendToPort("switch_interval,"+ switching_time_minute);

        }

        private async void manual_mode_Click(object sender, RoutedEventArgs e)
        {
            if (serialPort == null) return;
            await sendToPort("manual");

        }

        private void auto_mode_Click(object sender, RoutedEventArgs e)
        {

        }

        private void start_button_Click(object sender, RoutedEventArgs e)
        {

        }

        private async void compressorB_button_Click(object sender, RoutedEventArgs e)
        {
            if (serialPort == null) return;
            await sendToPort("compressorBON");
        }

        private async void fanA_button_Click(object sender, RoutedEventArgs e)
        {
            if (serialPort == null) return;
            await sendToPort("fan1on");
        }

        private async void fanB_button_Click(object sender, RoutedEventArgs e)
        {
            if (serialPort == null) return;
            await sendToPort("fan2on");
        }

        private async void pump_button_Click(object sender, RoutedEventArgs e)
        {
            if (serialPort == null) return;
            await sendToPort("pumpon");

        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            string qFilter = SerialDevice.GetDeviceSelector("COM6");
            DeviceInformationCollection devices = await DeviceInformation.FindAllAsync(qFilter);

            if (devices.Any())
            {
                string deviceId = devices.First().Id;

                 await OpenPort(deviceId);
            }

            ReadCancellationTokenSource = new CancellationTokenSource();

            while (true)
            {
                
                //System.Diagnostics.Debug.WriteLine("program came before await listen");
                await Listen();
            }
        }


        
        private async Task OpenPort(string deviceId)
        {
            serialPort = await SerialDevice.FromIdAsync(deviceId);

            if (serialPort != null)
            {
                serialPort.WriteTimeout = TimeSpan.FromMilliseconds(1000);
                serialPort.ReadTimeout = TimeSpan.FromMilliseconds(1000);
                serialPort.BaudRate = 115200;
                serialPort.Parity = SerialParity.None;
                serialPort.StopBits = SerialStopBitCount.One;
                serialPort.DataBits = 8;
                serialPort.Handshake = SerialHandshake.None;
                //Console.WriteLine("Serial port configured successfully");
                txtStatus.Text = "Serial port configured successfully";


            }
        }

        
        private async Task Listen()
        {
            try
            {
                if (serialPort != null)
                {
                    dataReaderObject = new DataReader(serialPort.InputStream);
                    await ReadAsync(ReadCancellationTokenSource.Token);
                    
                }
            }
            catch (Exception ex)
            {
                txtStatus.Text = ex.Message;
            }
            finally
            {
                if (dataReaderObject != null)    // Cleanup once complete
                {
                    dataReaderObject.DetachStream();
                    dataReaderObject = null;
                    
                }
            }
        }
        
        private async Task ReadAsync(CancellationToken cancellationToken)
        {
            Task<UInt32> loadAsyncTask;

            uint ReadBufferLength = 1;  // only when this buffer would be full next code would be executed

            dataReaderObject.InputStreamOptions = InputStreamOptions.Partial;

            loadAsyncTask = dataReaderObject.LoadAsync(ReadBufferLength).AsTask(cancellationToken);   // Create a task object
            //string debug = dataReaderObject.ReadString(bytesRead2);
            //System.Diagnostics.Debug.WriteLine("writing string debug"+ debug);

            UInt32 bytesRead = await loadAsyncTask;    // Launch the task and wait until buffer would be full

            if (bytesRead > 0)
            {
                strFromPort = dataReaderObject.ReadString(bytesRead);
                final_string = final_string + strFromPort;
                if(final_string.Contains(Environment.NewLine))
                {
                    string logicstring = final_string;
                    final_string = "";

                    System.Diagnostics.Debug.WriteLine("writing logic string " + logicstring);
                    if (logicstring.StartsWith("A"))
                    {   
                        if(logicstring.Contains("-"))
                        {
                            //it is a negative number
                            logicstring = Regex.Match(logicstring, @"\d+").Value;
                            int gauge1_pass = int.Parse(logicstring) * -1;
                            tempgauge1.Value = gauge1_pass;
                        }
                        else
                        {
                            logicstring = Regex.Match(logicstring, @"\d+").Value;
                            int gauge1_pass = int.Parse(logicstring);
                            tempgauge1.Value = gauge1_pass;
                        }
                    }



                    if (logicstring.StartsWith("B"))
                    {
                        if (logicstring.Contains("-"))
                        {
                            //it is a negative number
                            logicstring = Regex.Match(logicstring, @"\d+").Value;
                            int gauge2_pass = int.Parse(logicstring) * -1;
                            tempgauge1.Value = gauge2_pass;
                        }
                        else
                        {
                            logicstring = Regex.Match(logicstring, @"\d+").Value;
                            int gauge2_pass = int.Parse(logicstring);
                            tempgauge2.Value = gauge2_pass;
                        }
                    }
                    if (logicstring.StartsWith("setting"))
                    {
                        if (logicstring.Contains("-"))
                        {
                            //it is a negative number
                            logicstring = Regex.Match(logicstring, @"\d+").Value;
                            int previous_temp_setting = int.Parse(logicstring) * -1;
                            set_temperature.Value = previous_temp_setting;
                        }
                        else
                        {
                            logicstring = Regex.Match(logicstring, @"\d+").Value;
                            int previous_temp_setting = int.Parse(logicstring);
                            set_temperature.Value = previous_temp_setting;
                        }
                    }

                    if (logicstring.StartsWith("pumpon"))
                    {
                        pump_status_light.Fill = greenBrush;
                    }

                    if (logicstring.StartsWith("switch_interval"))
                    {
                        
                            
                            logicstring = Regex.Match(logicstring, @"\d+").Value;
                            int previous_interval_setting = int.Parse(logicstring);
                            set_switching_time.Value = previous_interval_setting;


                    }

                }
            }
        }

        private async Task WriteAsync(string text2write)
        {
            Task<UInt32> storeAsyncTask;

            if (text2write.Length != 0)
            {
                dataWriteObject.WriteString(text2write);

                storeAsyncTask = dataWriteObject.StoreAsync().AsTask();  // Create a task object

                UInt32 bytesWritten = await storeAsyncTask;   // Launch the task and wait
                if (bytesWritten > 0)
                {
                    //txtStatus.Text = bytesWritten + " bytes written at " + DateTime.Now.ToString(System.Globalization.CultureInfo.CurrentUICulture.DateTimeFormat.LongTimePattern);
                }
            }
            else { }
        }


        private async Task sendToPort(string sometext)
        {
            try
            {
                if (serialPort != null)
                {
                    dataWriteObject = new DataWriter(serialPort.OutputStream);

                    await WriteAsync(sometext);
                }
                else { }
            }
            catch (Exception ex)
            {
                txtStatus.Text = ex.Message;
            }
            finally
            {
                if (dataWriteObject != null)   // Cleanup once complete
                {
                    dataWriteObject.DetachStream();
                    dataWriteObject = null;
                }
            }
        }


        private void CancelReadTask()
        {
            if (ReadCancellationTokenSource != null)
            {
                if (!ReadCancellationTokenSource.IsCancellationRequested)
                {
                    ReadCancellationTokenSource.Cancel();
                }
            }
        }
        
        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            CancelReadTask();
            if (serialPort != null)
            {
                serialPort.Dispose();
            }
            serialPort = null;
        }

        private void init_Click(object sender, RoutedEventArgs e)
        {
            
        }

        /*
        private void Page_Unloaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            
        }*/
    }
}
