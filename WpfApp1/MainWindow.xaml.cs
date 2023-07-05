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

        readonly static int progonSpeedAz = 25; //Скорость прогона по азимуту
        readonly static int progonSpeedInc = 25; //Скорость прогона по наклону
        readonly static int creetAngleAzP = 90; //Допустимый угол поворота по азимуту (положительный)
        readonly static int creetAngleAzN = -90; //Допустимый угол поворота по азимуту (отрицательный)
        readonly static int creetAngleIncP = 10; //Допустимый угол поворота по азимуту (положительный)
        readonly static int creetAngleIncN = -30; //Допустимый угол поворота по азимуту (отрицательный)

        readonly static int commandDelay = 2000; //Задержка выдачи команд в модуль !!! ПРИ РАБОТЕ НЕ БОЛЕЕ 500 !!!

        readonly sendGenerator protocol = new sendGenerator();

        readonly string ver = "0.0.1";
        readonly antennaState state = new antennaState();
        SerialPort readPortC, writePortC;
        readonly PropetyWindow propsWind = new PropetyWindow();
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
                    readPortC.WriteTimeout = 200;
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
                    writePortC.WriteTimeout = 200;
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
            protocol.setPortW(writePortC, readPortC);
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
            byte[] data = new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            Dispatcher.Invoke((Action)(() =>
            {
                //azTextBox.Text = state.getAzAngleText();
                int dataBytes = 0;
                for (int i =0; i<16; i++)
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
                    dataBytes = i;
                    data[i] = bData;
                    print(bData.ToString() + " ");
                }
                sendGenerator.InpData inpData = new sendGenerator.InpData();
                inpData = protocol.readData(data);
                if (inpData.correct)
                {
                    long time = DateTime.Now.Ticks;
                    if (state.getLastAzTime() != 0) println("az speed: " + ((int)((state.getAzAngle() - inpData.angle_a) * 1000 / (time - state.getLastAzTime()))).ToString());
                    state.setLastAzTime(time);
                    state.setAzAngle("", inpData.angle_a);
                    azTextBox.Text = state.getAzAngleText();

                    if (state.getLastIncTime() != 0) println("inc speed: " + ((int)((state.getIncAngle() - inpData.angle_n) * 1000 / (time - state.getLastIncTime()))).ToString());
                    state.setLastIncTime(time);
                    state.setIncAngle("", inpData.angle_n);
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
                    sendGenerator.Pa30_data sendData = new sendGenerator.Pa30_data(az, inc, 11, 10);
                    protocol.pa30_pack(sendData);
                    //Packet_command_registers_constructor(1, 5, az);
                }
                else
                {
                    println(state.getWorkMode().name);
                    switch (state.getWorkMode().name)
                    {
                        case "zero":
                            int az = (-state.getAzAngle()) / 158;
                            int inc = (-state.getIncAngle()) / 158;
                            if (az > 1024) az = 1024;
                            if (inc > 1024) inc = 1024;
                            if (az < -1024) az = -1024;
                            if (inc < -1024) inc = -1024;
                            state.setUstAz(az);
                            state.setUstInc(inc);
                            if (check_start.Content.ToString() == "Стоп") check_start.Content = "Проверка";
                            if (!progon.IsEnabled) progon.IsEnabled = true;
                            break;

                        case "progon":
                            if (check_start.Content.ToString() == "Стоп") check_start.Content = "Проверка";
                            if (!progon.IsEnabled) progon.IsEnabled = true;
                            println(state.getWorkMode().name);
                            int prRealSpeedInc = progonSpeedInc;
                            int prRealSpeedAz = progonSpeedAz;
                            if (state.GetCheckedParameters() != null)
                            {
                                if (!state.GetCheckedParameters().inc) prRealSpeedInc = 0;
                                if (!state.GetCheckedParameters().az) prRealSpeedAz = 0;
                            }
                            if (state.getNeedAzAngle()<0) 
                            {
                                if (state.getAzAngle() < (creetAngleAzN + 5)*3600)
                                {
                                    state.setNeedAzAngle( creetAngleAzP * 3600);
                                    state.setUstAz(0);
                                }
                                else state.setUstAz(-prRealSpeedAz);
                            }
                            else
                            {
                                if (state.getAzAngle() > (creetAngleAzP - 5) * 3600)
                                {
                                    state.setNeedAzAngle(creetAngleAzN * 3600);
                                    state.setUstAz(0);
                                }
                                else state.setUstAz(prRealSpeedAz);
                            }
                            if (state.getNeedIncAngle() < 0)
                            {
                                if (state.getIncAngle() < (creetAngleIncN + 5) * 3600)
                                {
                                    state.setNeedIncAngle(creetAngleIncP * 3600);
                                    state.setUstInc(0);
                                }
                                else state.setUstInc(-prRealSpeedInc);
                            }
                            else
                            {
                                if (state.getIncAngle() > (creetAngleIncP - 5) * 3600)
                                {
                                    state.setNeedIncAngle(creetAngleIncN * 3600);
                                    state.setUstInc(0);
                                }
                                else state.setUstInc(prRealSpeedInc);
                            }
                            break;
                        case "naprTrogan":
                            {
                                if ((Math.Abs(state.getAzAngle()) > 100) || (Math.Abs(state.getIncAngle()) > 100 ))
                                {
                                    state.SetAntennaAtPosition(0, 0);
                                }
                                else
                                {
                                    aTimer.Stop();
                                    naprTroganFunc();
                                }
                                break;
                            }
                    }
                    print(state.getUstAz().ToString() + "   ");
                    println(state.getUstInc().ToString());
                    sendGenerator.Pa30_data sendData = new sendGenerator.Pa30_data(
                        state.getUstAz(), 
                        state.getUstInc(), 
                        11,
                        (ushort)(commandDelay > 500 ? 50000 : (commandDelay*100)));
                    protocol.pa30_pack(sendData);
                    protocol.askToRead();
                }
            }));
        }

        private void naprTroganFunc()
        {
            if (state.getWorkMode().azimuth)
            {
                while ((state.getAzAngle() > ((creetAngleAzN + 3) * 3600)) || (Math.Abs(state.getIncAngle()) > 100))
                {
                    state.SetAntennaAtPosition((creetAngleIncN + 3)*3600, 0);
                    sendGenerator.Pa30_data sendData = new sendGenerator.Pa30_data(
                        state.getUstAz(),
                        state.getUstInc(),
                        11,
                        200);
                    protocol.pa30_pack(sendData);
                    Thread.Sleep(100);
                    protocol.askToRead();
                }
                int minSpeed = 0, lastAngle = state.getAzAngle();
                while (state.getAzAngle() < ((creetAngleAzP - 3) * 3600))
                {
                    if (Math.Abs(lastAngle - state.getAzAngle()) < 100) minSpeed += 10;
                    state.setUstAz(minSpeed);
                    int ust = Math.Abs(state.getIncAngle()) > 1024 ? 1024 : Math.Abs(state.getIncAngle());
                    if (state.getIncAngle() < 0) ust *= (-1);
                    state.setUstInc(ust);
                    lastAngle = state.getAzAngle();
                    ustAz.Text = state.getUstAz().ToString();
                    ustInc.Text = state.getUstInc().ToString();
                    sendGenerator.Pa30_data sendData = new sendGenerator.Pa30_data(
                        state.getUstAz(),
                        state.getUstInc(),
                        11,
                        200);
                    protocol.pa30_pack(sendData);
                    Thread.Sleep(100);
                    protocol.askToRead();
                }
                lastAngle = state.getAzAngle();
                while (state.getAzAngle() > ((creetAngleAzN + 3) * 3600))
                {
                    if (Math.Abs(lastAngle - state.getAzAngle()) < 100) minSpeed += 10;
                    state.setUstAz(-minSpeed);
                    int ust = Math.Abs(state.getIncAngle()) > 1024 ? 1024 : Math.Abs(state.getIncAngle());
                    if (state.getIncAngle() < 0) ust *= (-1);
                    state.setUstInc(ust);
                    lastAngle = state.getAzAngle();
                    ustAz.Text = state.getUstAz().ToString();
                    ustInc.Text = state.getUstInc().ToString();
                    sendGenerator.Pa30_data sendData = new sendGenerator.Pa30_data(
                        state.getUstAz(),
                        state.getUstInc(),
                        11,
                        200);
                    protocol.pa30_pack(sendData);
                    Thread.Sleep(100);
                    protocol.askToRead();
                }
                println("Напряжение трогания по оси азимута: " + minSpeed.ToString());
                state.GoToNextParameters();
            }

            if (!state.getWorkMode().azimuth)
            {
                while ((state.getIncAngle() > ((creetAngleIncN + 1) * 36000)) || (Math.Abs(state.getAzAngle()) > 100))
                {
                    state.SetAntennaAtPosition(0, creetAngleAzN + 1);
                    sendGenerator.Pa30_data sendData = new sendGenerator.Pa30_data(
                        state.getUstAz(),
                        state.getUstInc(),
                        11,
                        200);
                    protocol.pa30_pack(sendData);
                    Thread.Sleep(100);
                    protocol.askToRead();
                }
                int minSpeed = 0, lastAngle = state.getIncAngle();
                while (state.getIncAngle() < ((creetAngleIncP - 1) * 3600))
                {
                    if (Math.Abs(lastAngle - state.getIncAngle()) < 100) minSpeed += 10;
                    state.setUstInc(minSpeed);
                    int ust = Math.Abs(state.getAzAngle()) > 1024 ? 1024 : Math.Abs(state.getAzAngle());
                    if (state.getAzAngle() < 0) ust *= (-1);
                    state.setUstAz(ust);
                    lastAngle = state.getIncAngle();
                    ustAz.Text = state.getUstAz().ToString();
                    ustInc.Text = state.getUstInc().ToString();
                    sendGenerator.Pa30_data sendData = new sendGenerator.Pa30_data(
                        state.getUstAz(),
                        state.getUstInc(),
                        11,
                        200);
                    protocol.pa30_pack(sendData);
                    Thread.Sleep(100);
                    protocol.askToRead();
                }
                lastAngle = state.getIncAngle();
                while (state.getIncAngle() > (creetAngleIncN + 1 * 3600))
                {
                    if (Math.Abs(lastAngle - state.getIncAngle()) < 100) minSpeed += 10;
                    state.setUstInc(-minSpeed);
                    int ust = Math.Abs(state.getAzAngle()) > 1024 ? 1024 : Math.Abs(state.getAzAngle());
                    if (state.getAzAngle() < 0) ust *= (-1);
                    state.setUstAz(ust);
                    lastAngle = state.getIncAngle();
                    ustAz.Text = state.getUstAz().ToString();
                    ustInc.Text = state.getUstInc().ToString();
                    sendGenerator.Pa30_data sendData = new sendGenerator.Pa30_data(
                        state.getUstAz(),
                        state.getUstInc(),
                        11,
                        200);
                    protocol.pa30_pack(sendData);
                    Thread.Sleep(100);
                    protocol.askToRead();
                }
                println("Напряжение трогания по оси наклона: " + minSpeed.ToString());
                state.GoToNextParameters();
                aTimer.Start();
            }
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

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            textField.Text = "";
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

        private void start_click(object sender, RoutedEventArgs e)
        {
            if (state.getWorkMode().name == "zero")
            {
                state.setWorkMode(state.GetCheckedParameters().FirstStart());
                check_start.Content = "Стоп";
                progon.IsEnabled = false;
            }
            else
            {
                state.setWorkMode(state.GetCheckedParameters().ZeroMode());
                check_start.Content = "Проверка";
                progon.IsEnabled = true;
            }
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
