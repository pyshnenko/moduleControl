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
using System.IO.Ports;
using System.Threading;
using System.Timers;

namespace WpfApp1
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    ///   
    public partial class MainWindow : Window
    {
        readonly string ver = "0.0.1";
        antennaState state = new antennaState();
        SerialPort port;
        public MainWindow()
        {
            InitializeComponent();
            azMeas.SelectedIndex = 0;
            incMeas.SelectedIndex = 0;
            state.connectionSet(true);
            string[] ports = SerialPort.GetPortNames();
            string portsTotal = "";
            if (ports.Length == 0) portsTotal = "no com\n";
            else 
                for (int i =0; i < ports.Length; i++)
                {
                    portsTotal += ports[i].ToString() + "\n";
                }
            readPort.ItemsSource = ports;
            writePort.ItemsSource = ports;
            textField.Text = portsTotal;
            azTextBox.Text = state.getAzAngleText();
            incTextBox.Text = state.getIncAngleText();
            groupBox.IsEnabled = state.connsectionGet();
            tabControl.IsEnabled = state.connsectionGet();
            modeBox.IsEnabled = state.connsectionGet();
            angleBox.IsEnabled = state.connsectionGet();
            manualSwitch.IsEnabled = state.connsectionGet();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            textField.Text += "Hello world";
            state.sendMessage("Hello");
        }
        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        private void Version_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                "Версия " + ver,
                "О программе",
                MessageBoxButton.OK
            );
        }

        private void azMeas_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string val = "Градус";
            if (azMeas.SelectedIndex == 0) val = "Градус";
            else if (azMeas.SelectedIndex == 1) val = "Код";
            else if (azMeas.SelectedIndex == 2) val = "Значение";
            state.setAzMeas(val);
            azTextBox.Text = state.getAzAngleText();
        }
        private void incMeas_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string val = "Градус";
            if (incMeas.SelectedIndex == 0) val = "Градус";
            else if (incMeas.SelectedIndex == 1) val = "Код";
            else if (incMeas.SelectedIndex == 2) val = "Значение";
            state.setIncMeas(val);
            incTextBox.Text = state.getIncAngleText();
        }

        private void manualSwitch_Click(object sender, RoutedEventArgs e)
        {
            bool manualState = state.getManualState();
            manualState = state.setManualState(!manualState);
            if (manualState)
            {
                manualSwitch.Content = "Выйти из режима отработки угла";
                angleBox.IsEnabled = true;
                groupBox.IsEnabled = false;
            }
            else
            {
                manualSwitch.Content = "Перейти в режим отработки угла";
                angleBox.IsEnabled = false;
                groupBox.IsEnabled = state.connsectionGet();
            }
        }
        private void readPort_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            port = new SerialPort();
            try
            {
                if (port != null) port.Close();
            }
            catch
            {
                textField.Text += "Нет портов\n";
            }
            try
            {
                port.PortName = "COM1";
                port.BaudRate = 9600;
                port.DataBits = 8;
                port.Parity = System.IO.Ports.Parity.None;
                port.StopBits = System.IO.Ports.StopBits.One;
                port.ReadTimeout = 1000;
                port.WriteTimeout = 1000; 
                port.DataReceived += new SerialDataReceivedEventHandler(port_DataReceived);
                port.Open(); 
                state.setReadPort(port);
            }
            catch (Exception err)
            {
                textField.Text += ("ERROR: невозможно открыть порт:" + err.ToString() +"\n");
                return;
            }
        }
        private void port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            // Show all the incoming data in the port's buffer;
            SerialPort port = state.getPort();
            textField.Text += (port.ReadExisting()) + "\n";
        }
    }
}
