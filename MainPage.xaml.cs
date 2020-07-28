using Syncfusion.UI.Xaml.Controls.Input;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
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



        public MainPage()
        {
            this.InitializeComponent();
        }

        private async void stop_button_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            //do something
            //SolidColorBrush mySolidColorBrush = new SolidColorBrush();
            //mySolidColorBrush.Color = Color.FromArgb(255, 0, 255, 0);
            pump_status_light.Fill = greenBrush;
        }

        private async void compressorA_button_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {

        }

        private async void set_temperature_changed(object sender, System.EventArgs e)
        {
            pump_status_light.Fill = redBrush;
        }

        private void set_switching_time_value_changed(object sender, System.EventArgs e)
        {
            pump_status_light.Fill = greenBrush;
        }
    }
}
