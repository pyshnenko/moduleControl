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
using System.Windows.Markup;

namespace WpfApp1
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    ///   
    public partial class MainWindow : Window
    {
        bool debugComWriteMode = true;

        ushort[] ccitt_crc16_table = new ushort[256]{ 0x0000, 0x1021, 0x2042, 0x3063, 0x4084, 0x50a5, 0x60c6, 0x70e7, 0x8108, 0x9129, 0xa14a, 0xb16b, 0xc18c, 0xd1ad, 0xe1ce, 0xf1ef, 0x1231, 0x0210, 0x3273, 0x2252, 0x52b5, 0x4294, 0x72f7, 0x62d6, 0x9339, 0x8318, 0xb37b, 0xa35a, 0xd3bd, 0xc39c, 0xf3ff, 0xe3de, 0x2462, 0x3443, 0x0420, 0x1401, 0x64e6, 0x74c7, 0x44a4, 0x5485, 0xa56a, 0xb54b, 0x8528, 0x9509, 0xe5ee, 0xf5cf, 0xc5ac, 0xd58d, 0x3653, 0x2672, 0x1611, 0x0630, 0x76d7, 0x66f6, 0x5695, 0x46b4, 0xb75b, 0xa77a, 0x9719, 0x8738, 0xf7df, 0xe7fe, 0xd79d, 0xc7bc, 0x48c4, 0x58e5, 0x6886, 0x78a7, 0x0840, 0x1861, 0x2802, 0x3823, 0xc9cc, 0xd9ed, 0xe98e, 0xf9af, 0x8948, 0x9969, 0xa90a, 0xb92b, 0x5af5, 0x4ad4, 0x7ab7, 0x6a96, 0x1a71, 0x0a50, 0x3a33, 0x2a12, 0xdbfd, 0xcbdc, 0xfbbf, 0xeb9e, 0x9b79, 0x8b58, 0xbb3b, 0xab1a, 0x6ca6, 0x7c87, 0x4ce4, 0x5cc5, 0x2c22, 0x3c03, 0x0c60, 0x1c41, 0xedae, 0xfd8f, 0xcdec, 0xddcd, 0xad2a, 0xbd0b, 0x8d68, 0x9d49, 0x7e97, 0x6eb6, 0x5ed5, 0x4ef4, 0x3e13, 0x2e32, 0x1e51, 0x0e70, 0xff9f, 0xefbe, 0xdfdd, 0xcffc, 0xbf1b, 0xaf3a, 0x9f59, 0x8f78, 0x9188, 0x81a9, 0xb1ca, 0xa1eb, 0xd10c, 0xc12d, 0xf14e, 0xe16f, 0x1080, 0x00a1, 0x30c2, 0x20e3, 0x5004, 0x4025, 0x7046, 0x6067, 0x83b9, 0x9398, 0xa3fb, 0xb3da, 0xc33d, 0xd31c, 0xe37f, 0xf35e, 0x02b1, 0x1290, 0x22f3, 0x32d2, 0x4235, 0x5214, 0x6277, 0x7256, 0xb5ea, 0xa5cb, 0x95a8, 0x8589, 0xf56e, 0xe54f, 0xd52c, 0xc50d, 0x34e2, 0x24c3, 0x14a0, 0x0481, 0x7466, 0x6447, 0x5424, 0x4405, 0xa7db, 0xb7fa, 0x8799, 0x97b8, 0xe75f, 0xf77e, 0xc71d, 0xd73c, 0x26d3, 0x36f2, 0x0691, 0x16b0, 0x6657, 0x7676, 0x4615, 0x5634, 0xd94c, 0xc96d, 0xf90e, 0xe92f, 0x99c8, 0x89e9, 0xb98a, 0xa9ab, 0x5844, 0x4865, 0x7806, 0x6827, 0x18c0, 0x08e1, 0x3882, 0x28a3, 0xcb7d, 0xdb5c, 0xeb3f, 0xfb1e, 0x8bf9, 0x9bd8, 0xabbb, 0xbb9a, 0x4a75, 0x5a54, 0x6a37, 0x7a16, 0x0af1, 0x1ad0, 0x2ab3, 0x3a92, 0xfd2e, 0xed0f, 0xdd6c, 0xcd4d, 0xbdaa, 0xad8b, 0x9de8, 0x8dc9, 0x7c26, 0x6c07, 0x5c64, 0x4c45, 0x3ca2, 0x2c83, 0x1ce0, 0x0cc1, 0xef1f, 0xff3e, 0xcf5d, 0xdf7c, 0xaf9b, 0xbfba, 0x8fd9, 0x9ff8, 0x6e17, 0x7e36, 0x4e55, 0x5e74, 0x2e93, 0x3eb2, 0x0ed1, 0x1ef0 };
        private static ushort[] crc16tab = new ushort[256] {
         0x0000,0x1021,0x2042,0x3063,0x4084,0x50a5,0x60c6,0x70e7,
         0x8108,0x9129,0xa14a,0xb16b,0xc18c,0xd1ad,0xe1ce,0xf1ef,
         0x1231,0x0210,0x3273,0x2252,0x52b5,0x4294,0x72f7,0x62d6,
         0x9339,0x8318,0xb37b,0xa35a,0xd3bd,0xc39c,0xf3ff,0xe3de,
         0x2462,0x3443,0x0420,0x1401,0x64e6,0x74c7,0x44a4,0x5485,
         0xa56a,0xb54b,0x8528,0x9509,0xe5ee,0xf5cf,0xc5ac,0xd58d,
         0x3653,0x2672,0x1611,0x0630,0x76d7,0x66f6,0x5695,0x46b4,
         0xb75b,0xa77a,0x9719,0x8738,0xf7df,0xe7fe,0xd79d,0xc7bc,
         0x48c4,0x58e5,0x6886,0x78a7,0x0840,0x1861,0x2802,0x3823,
         0xc9cc,0xd9ed,0xe98e,0xf9af,0x8948,0x9969,0xa90a,0xb92b,
         0x5af5,0x4ad4,0x7ab7,0x6a96,0x1a71,0x0a50,0x3a33,0x2a12,
         0xdbfd,0xcbdc,0xfbbf,0xeb9e,0x9b79,0x8b58,0xbb3b,0xab1a,
         0x6ca6,0x7c87,0x4ce4,0x5cc5,0x2c22,0x3c03,0x0c60,0x1c41,
         0xedae,0xfd8f,0xcdec,0xddcd,0xad2a,0xbd0b,0x8d68,0x9d49,
         0x7e97,0x6eb6,0x5ed5,0x4ef4,0x3e13,0x2e32,0x1e51,0x0e70,
         0xff9f,0xefbe,0xdfdd,0xcffc,0xbf1b,0xaf3a,0x9f59,0x8f78,
         0x9188,0x81a9,0xb1ca,0xa1eb,0xd10c,0xc12d,0xf14e,0xe16f,
         0x1080,0x00a1,0x30c2,0x20e3,0x5004,0x4025,0x7046,0x6067,
         0x83b9,0x9398,0xa3fb,0xb3da,0xc33d,0xd31c,0xe37f,0xf35e,
         0x02b1,0x1290,0x22f3,0x32d2,0x4235,0x5214,0x6277,0x7256,
         0xb5ea,0xa5cb,0x95a8,0x8589,0xf56e,0xe54f,0xd52c,0xc50d,
         0x34e2,0x24c3,0x14a0,0x0481,0x7466,0x6447,0x5424,0x4405,
         0xa7db,0xb7fa,0x8799,0x97b8,0xe75f,0xf77e,0xc71d,0xd73c,
         0x26d3,0x36f2,0x0691,0x16b0,0x6657,0x7676,0x4615,0x5634,
         0xd94c,0xc96d,0xf90e,0xe92f,0x99c8,0x89e9,0xb98a,0xa9ab,
         0x5844,0x4865,0x7806,0x6827,0x18c0,0x08e1,0x3882,0x28a3,
         0xcb7d,0xdb5c,0xeb3f,0xfb1e,0x8bf9,0x9bd8,0xabbb,0xbb9a,
         0x4a75,0x5a54,0x6a37,0x7a16,0x0af1,0x1ad0,0x2ab3,0x3a92,
         0xfd2e,0xed0f,0xdd6c,0xcd4d,0xbdaa,0xad8b,0x9de8,0x8dc9,
         0x7c26,0x6c07,0x5c64,0x4c45,0x3ca2,0x2c83,0x1ce0,0x0cc1,
         0xef1f,0xff3e,0xcf5d,0xdf7c,0xaf9b,0xbfba,0x8fd9,0x9ff8,
         0x6e17,0x7e36,0x4e55,0x5e74,0x2e93,0x3eb2,0x0ed1,0x1ef0
      };
        readonly static byte SYNC = (byte)0xAA; //Байт синхронизации (1010
        readonly static byte Packet_size = (byte)14; //Размер пакета
        readonly static byte Mask = (byte)0x32; 
        readonly static byte ID = (byte)0x01;
        static ushort CrcCalc = (byte)0;

        static int Register_mode_operation = 65;
        static int Register_Universal_input = 34;
        static int Register_sensored_position = 32;
        static int Register_sensored_velocity = 31;

        readonly static int progonSpeedAz = 25; //Скорость прогона по азимуту
        readonly static int progonSpeedInc = 25; //Скорость прогона по наклону
        readonly static int creetAngleAzP = 90; //Допустимый угол поворота по азимуту (положительный)
        readonly static int creetAngleAzN = -90; //Допустимый угол поворота по азимуту (отрицательный)
        readonly static int creetAngleIncP = 10; //Допустимый угол поворота по азимуту (положительный)
        readonly static int creetAngleIncN = -30; //Допустимый угол поворота по азимуту (отрицательный)

        readonly static int commandDelay = 2000; //Задержка выдачи команд в модуль

        readonly string ver = "0.0.1";
        antennaState state = new antennaState();
        SerialPort readPortC, writePortC;
        PropetyWindow propsWind = new PropetyWindow();
        private static System.Timers.Timer aTimer;
        public MainWindow()
        {
            InitializeComponent();
            azMeas.SelectedIndex = 0;
            incMeas.SelectedIndex = 0;
            state.connectionSet(false);
            Update_ports_func();
            azTextBox.Text = state.getAzAngleText();
            incTextBox.Text = state.getIncAngleText();
            groupBox.IsEnabled = state.connsectionGet();
            tabControl.IsEnabled = state.connsectionGet();
            modeBox.IsEnabled = state.connsectionGet();
            angleBox.IsEnabled = state.getManualState();
            manualSwitch.IsEnabled = state.connsectionGet();
            state.setTextBox(textField);
            manualAzAngle.Text = "0";
            manualIncAngle.Text = "0";
            state.setWorkMode(new CheckedParameters.NowWork("zero", true));
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            println("Hello world");
            state.sendMessage("Hello");
        }
        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            if (writePortC != null) writePortC.Close();
            if (readPortC != null) readPortC.Close();
            Close();
        }
        private void on_close(object sender, EventArgs e) { propsWind.Close(); }
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
            CheckedParameters.NowWork work = new CheckedParameters.NowWork("zero", true);
            state.setWorkMode(work);
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
            readPortC = new SerialPort();
            try
            {
                if (readPortC != null) readPortC.Close();
            }
            catch
            {
                println("Нет портов");
            }
            try
            {
                if (writePort.SelectedItem == readPort.SelectedItem) readPortC = writePortC;
                else if (readPort.SelectedIndex != -1)
                {
                    readPortC.PortName = readPort.SelectedItem.ToString();
                    readPortC.BaudRate = 3000000;
                    readPortC.DataBits = 8;
                    readPortC.Parity = System.IO.Ports.Parity.None;
                    readPortC.StopBits = System.IO.Ports.StopBits.One;
                    readPortC.ReadTimeout = 200;
                    readPortC.WriteTimeout = 1000;
                    readPortC.DataReceived += new SerialDataReceivedEventHandler(port_DataReceived);
                    readPortC.Open();
                    state.setReadPort(readPortC);
                }
                if (writePortC != null && readPortC != null)
                {
                    start();
                }
            }
            catch (Exception err)
            {
                println("ERROR: невозможно открыть порт:" + err.ToString());
                return;
            }
        }

        private void writePort_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            writePortC = new SerialPort();
            try
            {
                writePortC?.Close();
            }
            catch
            {
                println("Нет портов");
            }
            try
            {
                if (writePort.SelectedItem == readPort.SelectedItem) writePortC = readPortC;
                else if (writePort.SelectedIndex != -1)
                {
                    writePortC.PortName = writePort.SelectedItem.ToString();
                    writePortC.BaudRate = 3000000;
                    writePortC.DataBits = 8;
                    writePortC.Parity = System.IO.Ports.Parity.None;
                    writePortC.StopBits = System.IO.Ports.StopBits.One;
                    writePortC.ReadTimeout = 200;
                    writePortC.WriteTimeout = 1000;
                    writePortC.DataReceived += new SerialDataReceivedEventHandler(port_DataReceived);
                    writePortC.Open();
                }
                if (writePortC != null && readPortC != null)
                {
                    start();
                }
            }
            catch (Exception err)
            {
                println("ERROR: невозможно открыть порт:" + err.ToString());
                return;
            }
        }

        private void start()
        {
            aTimer?.Stop();
            aTimer = new System.Timers.Timer();
            aTimer.Interval = commandDelay;
            aTimer.Elapsed += OnTimedEvent;
            aTimer.AutoReset = true;
            aTimer.Enabled = true;
            state.connectionSet(true);
            groupBox.IsEnabled = state.connsectionGet();
            tabControl.IsEnabled = state.connsectionGet();
            modeBox.IsEnabled = state.connsectionGet();
            angleBox.IsEnabled = state.getManualState();
            manualSwitch.IsEnabled = state.connsectionGet();
            upd_port_button.Content = "Отключить";
        }
        private void port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            //int data = Int32.Parse(readPortC.ReadExisting());
            //state.setAzAngle("", data);
            byte[] data = new byte[28];
            Dispatcher.Invoke((Action)(() =>
            {
                //azTextBox.Text = state.getAzAngleText();
                for (int i =0; i<28; i++)
                {
                    byte bData=0;
                    try
                    {
                        bData = (byte)readPortC.ReadByte();
                    }catch
                    {
                        print("end");
                        break;
                    }
                    data[i] = bData;
                    print(bData.ToString() + " ");
                }
                if ((int)data[9] == Register_sensored_position)
                {
                    int p1 = 0;
                    p1 = data[11] << 8;
                    p1 = (p1 + data[12]) << 8;
                    p1 = (p1 + data[13]) << 8;
                    p1 = (p1 + data[14]);
                    if (data[10] != 0) p1 *= (-1);
                    long time = DateTime.Now.Ticks;
                    if (state.getLastAzTime()!=0) println("az speed: " + ((int)((state.getAzAngle() - p1)*1000/(time - state.getLastAzTime()))).ToString());
                    state.setLastAzTime(time);
                    state.setAzAngle("", p1);
                    azTextBox.Text = state.getAzAngleText();
                }
                if (((int)data[23] == Register_sensored_position))
                {
                    print("im here inc");
                    int p1 = 0;
                    p1 = data[25] << 8;
                    p1 = (p1 + data[26]) << 8;
                    p1 = (p1 + data[27]);
                    if (data[24] != 0) p1 *= (-1);
                    long time = DateTime.Now.Ticks;
                    if (state.getLastIncTime() != 0) println("inc speed: " + ((int)((state.getIncAngle() - p1)*1000/ (time - state.getLastIncTime()))).ToString());
                    state.setLastIncTime(time);
                    state.setIncAngle("", p1);
                    incTextBox.Text = state.getIncAngleText();
                }
                println("");
            }));
        }
        private void OnTimedEvent(Object source, System.Timers.ElapsedEventArgs e)
        {
            Dispatcher.Invoke((Action)(() =>
            {
                if (state.getManualState())
                {
                    int az = (state.getManualAzAngle() - state.getAzAngle()) /158;
                    int inc = (state.getManualIncAngle() - state.getIncAngle()) /158;
                    if (az > 1024) az = 1024;
                    if (inc > 1024) inc = 1024;
                    if (az < -1024) az = -1024;
                    if (inc < -1024) inc = -1024;
                    print(az.ToString() + "   ");
                    println(inc.ToString());
                    Packet_command_registers_constructor(1, 5, az);
                }
                else
                {
                    println(state.getWorkMode().name);
                    switch (state.getWorkMode().name)
                    {
                        case "zero":
                            println(state.getWorkMode().name);
                            int az = (-state.getAzAngle()) / 158;
                            int inc = (-state.getIncAngle()) / 158;
                            if (az > 1024) az = 1024;
                            if (inc > 1024) inc = 1024;
                            if (az < -1024) az = -1024;
                            if (inc < -1024) inc = -1024;
                            state.setUstAz(az);
                            state.setUstInc(inc);
                            break;

                        case "progon":
                            println(state.getWorkMode().name);
                            if (state.getNeedAzAngle()<0) 
                            {
                                if (state.getAzAngle() < (creetAngleAzN + 5)*3600)
                                {
                                    state.setNeedAzAngle( creetAngleAzP * 3600);
                                    state.setUstAz(0);
                                }
                                else state.setUstAz(-progonSpeedAz);
                            }
                            else
                            {
                                if (state.getAzAngle() > (creetAngleAzP - 5) * 3600)
                                {
                                    state.setNeedAzAngle(creetAngleAzN * 3600);
                                    state.setUstAz(0);
                                }
                                else state.setUstAz(progonSpeedAz);
                            }
                            if (state.getNeedIncAngle() < 0)
                            {
                                if (state.getIncAngle() < (creetAngleIncN + 5) * 3600)
                                {
                                    state.setNeedIncAngle(creetAngleIncP * 3600);
                                    state.setUstInc(0);
                                }
                                else state.setUstInc(-progonSpeedInc);
                            }
                            else
                            {
                                if (state.getIncAngle() > (creetAngleIncP - 5) * 3600)
                                {
                                    state.setNeedIncAngle(creetAngleIncN * 3600);
                                    state.setUstInc(0);
                                }
                                else state.setUstInc(progonSpeedInc);
                            }
                            break;
                    }
                    print(state.getUstAz().ToString() + "   ");
                    println(state.getUstInc().ToString());
                    Packet_command_registers_constructor(1, 5, state.getUstAz());
                    Packet_command_registers_constructor(1, 2, state.getUstInc());
                }
            }));
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            int data = Int32.Parse(ustAz.Text);
            if (data > 1024) data = 1024;
            state.setUstAz(data);
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            int data = Int32.Parse(ustInc.Text);
            if (data > 1024) data = 1024;
            state.setUstInc(data);
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            int data;
            try
            {
                data = Int32.Parse(manualAzAngle.Text) * 3600;
            }
            catch
            {
                data = 0;
                manualAzAngle.Text = "0";
            }
            state.setManualAzAngle(data);
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            int data;
            try
            {
                data = Int32.Parse(manualIncAngle.Text) * 3600;
            }
            catch
            {
                data = 0;
                manualIncAngle.Text = "0";
            }
            state.setManualIncAngle(data);
        }

        private void Packet_command_registers_constructor(byte Command, byte Register, int data)
        {
            byte[] Transmitted_data = new byte[17] ;

            Transmitted_data[0] = SYNC;
            Transmitted_data[1] = SYNC;
            Transmitted_data[2] = Packet_size;
            Transmitted_data[3] = Mask;
            Transmitted_data[4] = ID;
            Transmitted_data[5] = (byte)0;
            Transmitted_data[6] = (byte)0;
            Transmitted_data[7] = (byte)0;
            Transmitted_data[8] = Command;
            Transmitted_data[9] = Register;
            Transmitted_data[10] = (byte)0;
            Transmitted_data[11] = (byte)((data >> 12) & 0xFF);
            Transmitted_data[12] = (byte)((data >> 8) & 0xFF);
            Transmitted_data[13] = (byte)((data >> 4) & 0xFF);
            Transmitted_data[14] = (byte)(data & 0xFF);

            //CrcCalc = crc16_ccitt(Transmitted_data);
            CrcCalc = Crc16Ccitt(Transmitted_data);
            /*CrcCalc = 0;
            for (int i = 3; i < Packet_size + 1; i++)
            {
                CrcCalc = add_CRC(CrcCalc, Transmitted_data[i]);
            }*/
            byte Crc_high;
            byte Crc_low;
            Crc_low = (byte)((CrcCalc & 0xFF00) >> 8);
            Crc_high = (byte)(CrcCalc & 0x00FF);
            Transmitted_data[Packet_size + 1] = Crc_high;
            Transmitted_data[Packet_size + 2] = Crc_low;

            try { writePortC.Write(Transmitted_data, 0, 17); }
            catch { println("Порт уставки закрыт или отключен"); };
        }
        ushort add_CRC(ushort fcs, byte c)
        {
            ushort fcsOUT = (ushort)(ccitt_crc16_table[((fcs >> 8) ^ c) & 0x00ff] ^ (fcs << 8));
            return fcsOUT;
        }
        private ushort Crc16Ccitt(byte[] bytes)
        {
            const ushort poly = 4129;
            ushort[] table = new ushort[256];
            ushort initialValue = 0xffff;
            ushort temp, a;
            ushort crc = initialValue;
            for (int i = 0; i < table.Length; ++i)
            {
                temp = 0;
                a = (ushort)(i << 8);
                for (int j = 0; j < 8; ++j)
                {
                    if (((temp ^ a) & 0x8000) != 0)
                        temp = (ushort)((temp << 1) ^ poly);
                    else
                        temp <<= 1;
                    a <<= 1;
                }
                table[i] = temp;
            }
            for (int i = 0; i < bytes.Length - 2; ++i)
            {
                crc = (ushort)((crc << 8) ^ table[((crc >> 8) ^ (0xff & bytes[i]))]);
            }
            return crc;
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            textField.Text = "";
        }

        public static ushort crc16_ccitt(byte[] buf)
        {
            ushort crc = 0;
            for (int counter = 0; counter < buf.Length - 2; counter++)
            {
                crc = (ushort)(crc16tab[(crc >> 8) & 0x00FF] ^ (crc << 8) ^ buf[counter]);
            }
            return crc;
        }

        private void println(string text)
        {
            textFieldUpd(text + "\n");
        }
        private void print(string text)
        {
            textFieldUpd(text);
        }

        private void Update_ports(object sender, RoutedEventArgs e)
        {
            Update_ports_func();
        }

        private void textFieldUpd(string text)
        {
            if (textField.Text.Length <1000)
                textField.Text += text;
            else
            {
                string textp = textField.Text + text;
                textField.Text = textp.Substring(textp.Length - 1000);
            }
            textField.ScrollToEnd();
        }

        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            progon.Content = state.getWorkMode().name == "progon" ? "Прогон" : "Стоп" ;
            CheckedParameters.NowWork work = new CheckedParameters.NowWork(state.getWorkMode().name == "progon" ? "zero" : "progon", true);
            state.setWorkMode(work);
            if (state.getWorkMode().name == "progon")
            {
                state.setNeedAzAngle(creetAngleAzN * 3600);
                state.setNeedIncAngle(creetAngleIncN * 3600);
            }
        }

        private void Open_props_window(object sender, RoutedEventArgs e)
        {
            propsWind.Show();
            propsWind.Focus();
            propsWind.state = state;
        }

        private void Update_ports_func()
        {
            if (upd_port_button.Content.ToString() == "Обновить")
            {
                string[] ports = SerialPort.GetPortNames();
                string portsTotal = "";
                if (ports.Length == 0) portsTotal = "no com\n";
                else
                    for (int i = 0; i < ports.Length; i++)
                    {
                        portsTotal += ports[i].ToString() + "\n";
                    }
                readPort.ItemsSource = ports;
                writePort.ItemsSource = ports;
            }
            else
            {
                readPort.SelectedIndex = -1;
                writePort.SelectedIndex = -1;
                readPortC.Close();
                writePortC.Close();
                aTimer.Stop();
                upd_port_button.Content = "Обновить";
            }
        }
    }
}
