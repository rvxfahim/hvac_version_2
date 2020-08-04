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
using Windows.UI.Xaml.Media.Animation;
using Microsoft.Toolkit.Uwp.UI.Animations.Behaviors;
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
        SolidColorBrush greyBrush = new SolidColorBrush(Colors.DarkSlateGray);

        //private bool updatingValues;


        private CancellationTokenSource ReadCancellationTokenSource;
        private SerialDevice serialPort = null;
        DataWriter dataWriteObject = null;
        DataReader dataReaderObject = null;
        public string strFromPort;
        private string final_string;
        
        private int compressorAkey=0;
        private int compressorBkey=0;
        private int pump_entry = 0;
        private int compAentry=0;
        private int compBentry=0;
        private int fan1entry = 0;
        private int fan2entry = 0;
        private double Blur_amount = 0;
        /*
        public MainPage()
        {
            this.InitializeComponent();
            stop_button.IsEnabled = false;
            set_temperature.IsEnabled = false;
            set_switching_time.IsEnabled = false;
            auto_mode.IsEnabled = false;
            manual_mode.IsEnabled = false;
            compressorA_button.IsEnabled = false;
            compressorB_button.IsEnabled = false;
            fanA_button.IsEnabled = false;
            fanB_button.IsEnabled = false;
            init.IsEnabled = false;
            init_timing.IsEnabled = false;
            pump_button.IsEnabled = false;
            load_animation.Begin();
            
        }*/

        private async void stop_button_Click(object sender, RoutedEventArgs e)
        {
            //do something
            start_animation.Stop();
            if (serialPort == null) return;
            await sendToPort("end");
            set_temperature.IsEnabled = false;
            set_switching_time.IsEnabled = false;
            manual_mode.IsEnabled = true;
            auto_mode.IsEnabled = true;
            stop_button.IsEnabled = false;
            compressorA_button.IsEnabled = false;
            compressorB_button.IsEnabled = false;
            fanA_button.IsEnabled = false;
            fanB_button.IsEnabled = false;
            init.IsEnabled = false;
            init_timing.IsEnabled = false;
            pump_button.IsEnabled = false;
            start_button.IsEnabled = true;
            pump_entry = 0;
            compAentry = 0;
            compBentry = 0;
            fan1entry = 0;
            fan2entry = 0;
            stop_animation.Begin();
            start_button.Width = 162;
            GC.Collect();
        }

        
        
        private async void set_temperature_changed(object sender, EventArgs e)
        {
            //System.Diagnostics.Debug.WriteLine("entered the async void set_temp.............");
            
            
        }

        private async void set_switching_time_value_changed(object sender, EventArgs e)
        {
            //System.Diagnostics.Debug.WriteLine("entered the async void switching time.............");
            
            

        }

        private async void manual_mode_Click(object sender, RoutedEventArgs e)
        {
            if (serialPort == null) return;
            await sendToPort("manual");
            auto_mode.IsEnabled = false;
            manual_mode.IsEnabled = false;
            stop_button.IsEnabled = true;
            set_temperature.IsEnabled = true;
            set_switching_time.IsEnabled = true;
            pump_button.IsEnabled = true;
            fanA_button.IsEnabled = true;
            fanB_button.IsEnabled = true;
            compressorA_button.IsEnabled = true;
            compressorB_button.IsEnabled = true;
            init.IsEnabled = true;
            init_timing.IsEnabled = true;
            
        }

        private async void auto_mode_Click(object sender, RoutedEventArgs e)
        {
            set_switching_time.IsEnabled = true;
            set_temperature.IsEnabled = true;
            init.IsEnabled = true;
            stop_button.IsEnabled = true;
            init_timing.IsEnabled = true;
            if (serialPort == null) return;
            await sendToPort("auto");

        }

        private async void start_button_Click(object sender, RoutedEventArgs e)
        {
            stop_animation.Stop();
            await sendToPort("ready");
            auto_mode.IsEnabled = true;
            manual_mode.IsEnabled = true;
            start_button.IsEnabled = false;
            start_animation.Begin();
            System.Diagnostics.Debug.WriteLine("sending ready command");
            
            
        }

        private async void compressorA_button_Click(object sender, RoutedEventArgs e)
        {
            if (compAentry == 0)
            {
                if (serialPort == null) return;
                await sendToPort("compressorAON");   //sending key
                compAentry = 1;
            }
            else
            {
                if (serialPort == null) return;
                await sendToPort("compressorAoff");  //sending key
                compAentry = 0;
            }
        }

        private async void compressorB_button_Click(object sender, RoutedEventArgs e)
        {
            if (compBentry == 0)
            {
                if (serialPort == null) return;
                await sendToPort("compressorBON");   //sending key
                compBentry = 1;
            }
            else
            {
                if (serialPort == null) return;
                await sendToPort("compressorBoff");  //sending key
                compBentry = 0;
            }
        }

        private async void fanA_button_Click(object sender, RoutedEventArgs e)
        {
            if (fan1entry == 0)
            {
                if (serialPort == null) return;
                await sendToPort("fan1on"); 
                fan1entry = 1;
            }
            else
            {
                if (serialPort == null) return;
                await sendToPort("fan1off");  
                fan1entry = 0;
            }
        }

        private async void fanB_button_Click(object sender, RoutedEventArgs e)
        {
            if (fan2entry == 0)
            {
                if (serialPort == null) return;
                await sendToPort("fan2on");
                fan2entry = 1;
            }
            else
            {
                if (serialPort == null) return;
                await sendToPort("fan2off");
                fan2entry = 0;
            }
        }

        private async void pump_button_Click(object sender, RoutedEventArgs e)
        {
            if (pump_entry == 0)
            {
                if (serialPort == null) return;
                await sendToPort("pumpon");
                pump_entry = 1;
            }
            else
            {
                if (serialPort == null) return;
                await sendToPort("pumpoff");
                pump_entry = 0;
            }

        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            string qFilter = SerialDevice.GetDeviceSelector("COM3");
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
                serialPort.ReadTimeout = TimeSpan.FromMilliseconds(50);
                serialPort.BaudRate = 115200;
                serialPort.Parity = SerialParity.None;
                serialPort.StopBits = SerialStopBitCount.One;
                serialPort.DataBits = 8;
                serialPort.Handshake = SerialHandshake.None;
                //Console.WriteLine("Serial port configured successfully");
                //txtStatus.Text = "Serial port configured successfully";
                

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
                //txtStatus2.Text = final_string;
                if (final_string.Contains(Environment.NewLine))
                {
                    string logicstring = final_string;
                    final_string = "";

                    //System.Diagnostics.Debug.WriteLine("writing logic string " + logicstring);
                    
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
                        Storyboard_Pump_Led.RepeatBehavior = RepeatBehavior.Forever;
                        Storyboard_Pump_Led.Begin();

                    }
                    if (logicstring.StartsWith("pumpoff"))
                    {   
                        pump_status_light.Fill = redBrush;
                        Storyboard_Pump_Led.Stop();
                    }
                    if (logicstring.StartsWith("pumpfail"))
                    {
                        if (!FailPopup.IsOpen) 
                        { 
                            FailPopup.IsOpen = true; 
                        }
                    }
                    if (logicstring.StartsWith("clear the popup"))
                    {
                        if (FailPopup.IsOpen) 
                        {
                            FailPopup.IsOpen = false;
                        }
                    }
                    if (logicstring.StartsWith("switch_interval"))
                    {
                        
                            
                            logicstring = Regex.Match(logicstring, @"\d+").Value;
                            int previous_interval_setting = int.Parse(logicstring);
                            set_switching_time.Value = previous_interval_setting;


                    }
                    if (logicstring.StartsWith("timer"))
                    {


                        logicstring = Regex.Match(logicstring, @"\d+").Value;
                        int remaining_time = int.Parse(logicstring);
                        remaining_time = remaining_time / 1000;
                        timer_box.Text = remaining_time.ToString();


                    }
                    if (logicstring.StartsWith("Free RAM = "))
                    {


                        logicstring = Regex.Match(logicstring, @"\d+").Value;
                        int remaining_memory = int.Parse(logicstring);
                        free_memory.Text = remaining_memory.ToString();


                    }
                    if (logicstring.StartsWith("fan1on"))
                    {
                        fan1_status_light.Fill = greenBrush;
                        storyboard_fan1_led.RepeatBehavior = RepeatBehavior.Forever;
                        storyboard_fan1_led.Begin();
                        
                    }
                    if (logicstring.StartsWith("fan1off"))
                    {
                        fan1_status_light.Fill = redBrush;
                        storyboard_fan1_led.Stop();
                    }
                    if (logicstring.StartsWith("fan2on"))
                    {
                        fan2_status_light.Fill = greenBrush;
                        storyboard_fan_2.RepeatBehavior = RepeatBehavior.Forever;
                        storyboard_fan_2.Begin();
                    }
                    if (logicstring.StartsWith("fan2off"))
                    {
                        fan2_status_light.Fill = redBrush;
                        storyboard_fan_2.Stop();
                    }
                    if (logicstring.StartsWith("compressorAON"))   //key
                    {
                        compressorA_status_light.Fill = greyBrush;
                        compressorAkey = 1;
                    }
                    if (logicstring.StartsWith("compressorAoff"))  //key
                    {
                        compressorA_status_light.Fill = redBrush;
                        compressorAkey = 0;
                    }
                    if (logicstring.StartsWith("compressorBON"))  //key
                    {
                        compressorB_status_light.Fill = greyBrush;
                        compressorBkey = 1;
                    }
                    if (logicstring.StartsWith("compressorBoff")) //key
                    {
                        compressorB_status_light.Fill = redBrush;
                        compressorBkey = 0;
                    }
                    if (logicstring.StartsWith("compressorA on")) // actually compressor on
                    {
                        compressorA_status_light.Fill = greenBrush;
                        storyboard_compA_Led.RepeatBehavior = RepeatBehavior.Forever;
                        storyboard_compA_Led.Begin();
                    }
                    if (logicstring.StartsWith("compressorB on")) // actually compressor on
                    {
                        compressorB_status_light.Fill = greenBrush;
                        storyboard_compB_led.RepeatBehavior = RepeatBehavior.Forever;
                        storyboard_compB_led.Begin();
                    }

                    if (logicstring.StartsWith("compressor off")) // actually compressor off
                    {
                        if (compressorBkey == 1)
                        {
                            compressorB_status_light.Fill = greyBrush;
                            storyboard_compB_led.Stop();
                        }
                        if (compressorAkey == 1)
                        {
                            compressorA_status_light.Fill = greyBrush;
                            storyboard_compA_Led.Stop();
                        }
                        if (compressorBkey == 0)
                        {
                            compressorB_status_light.Fill = redBrush;
                            storyboard_compB_led.Stop();
                        }
                        if (compressorAkey == 0)
                        {
                            compressorA_status_light.Fill = redBrush;
                            storyboard_compA_Led.Stop();
                        }
                    }
                    if (logicstring.StartsWith("compressorA off")) // actually compressor off
                    {
                        compressorA_status_light.Fill = redBrush;
                        storyboard_compA_Led.Stop();
                    }
                    if (logicstring.StartsWith("compressorB off")) // actually compressor off
                    {
                        compressorB_status_light.Fill = redBrush;
                        storyboard_compB_led.Stop();
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

        private async void init_Click(object sender, RoutedEventArgs e)
        {
            double sendtempvalue = (double)set_temperature.Value;
            if (serialPort == null) return;
            await sendToPort("settemp," + sendtempvalue);
            //System.Diagnostics.Debug.WriteLine("EEPROM " + sendtempvalue);

            

        }

        private async void init_timing_Click(object sender, RoutedEventArgs e)
        {
            double switching_time_minute = (double)set_switching_time.Value;
            if (serialPort == null) return;
            await sendToPort("switch_interval," + switching_time_minute);
            //System.Diagnostics.Debug.WriteLine("EEPROM " + switching_time_minute);
        }

        

        /*
        private void Page_Unloaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            
        }*/
    }
}
