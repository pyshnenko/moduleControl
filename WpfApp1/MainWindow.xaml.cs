﻿using System;
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
using System.Reflection;
using System.IO;
using static System.Net.Mime.MediaTypeNames;
using System.Text.Json;

namespace WpfApp1
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    ///   
    public partial class MainWindow : Window
    {
        //int progonSpeedAz = 25; //Скорость прогона по азимуту
        //int progonSpeedInc = 25; //Скорость прогона по наклону
        //int creetAngleAzP = 90; //Допустимый угол поворота по азимуту (положительный)
        //int creetAngleAzN = -90; //Допустимый угол поворота по азимуту (отрицательный)
        //int creetAngleIncP = 10; //Допустимый угол поворота по азимуту (положительный)
        //int creetAngleIncN = -30; //Допустимый угол поворота по азимуту (отрицательный)

        //int speedK = 158; //Коэффициент скорости

        //int commandDelay = 200; //Задержка выдачи команд в модуль
                                               //!!! ПРИ РАБОТЕ НЕ БОЛЕЕ 500 !!!
                                               //рекомендую 200. меньше 100 - тупит если подключен дебаг
                                               //без дебага выжал 18

        //bool debug = true;

        ForSave antennaParameters = new ForSave()
        {
            progonSpeedAz = 25,
            progonSpeedInc = 25,
            creetAngleAzP = 90,
            creetAngleAzN = -90,
            creetAngleIncP = 10,
            creetAngleIncN = -30,
            creetUstAz = 50,
            creetUstInc = 50,
            speedK = 158,
            commandDelay = 2000,
            debug = true,
            correct = false,
            readonlyP = true,
            addUstAz = 0,
            addUstInc = 0
        };
        readonly sendGenerator protocol = new sendGenerator();

        readonly string ver = "0.0.2";
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
            protocol.setCreetPars(antennaParameters);
            state.setWorkMode(new CheckedParameters.NowWork("zero", true));
            bool fileExist = File.Exists("settings.cfg");
            if (fileExist) 
            { 
                println("Файл найден", antennaParameters.debug);
                using (StreamReader reader = new StreamReader("settings.cfg"))
                {
                    string text = reader.ReadToEnd();
                    antennaParameters.unparseAndSave(text);
                    if (!antennaParameters.correct) fileExist = false;
                }
            }
            if (!fileExist)
            { 
                println("Файл не найден или некорректен");
                string data = JsonSerializer.Serialize(antennaParameters);
                File.WriteAllText("settings.cfg", data);
            }
            System.Timers.Timer updPortTimer = new System.Timers.Timer();
            updPortTimer.Elapsed += updPort;
            updPortTimer.AutoReset = false;
            updPortTimer.Interval = 100;
            updPortTimer.Enabled = true;
            frame.Visibility = Visibility.Visible;
        }

        private void updPort(Object source, System.Timers.ElapsedEventArgs e)
        {
            Dispatcher.Invoke((Action)(() =>
            {
                Update_ports_func();
                frame.Visibility = Visibility.Hidden;
            }));
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
                    readPortC.ReadTimeout = 1;
                    readPortC.WriteTimeout = 1;
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
                    writePortC.ReadTimeout = 1;
                    writePortC.WriteTimeout = 1;
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
            aTimer.Interval = antennaParameters.commandDelay;
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
                        print("end", antennaParameters.debug);
                        break;
                    }
                    dataBytes = i;
                    data[i] = bData;
                    print(bData.ToString() + " ", antennaParameters.debug);
                }
                sendGenerator.InpData inpData = new sendGenerator.InpData();
                inpData = protocol.readData(data);
                if (inpData.correct)
                {
                    /*long time = DateTime.Now.Ticks;
                    if ((state.getLastAzTime() != 0) && antennaParameters.debug) println("az speed: " + ((int)((state.getAzAngle() - inpData.angle_a) * 1000 / (time - state.getLastAzTime()))).ToString());
                    state.setLastAzTime(time);*/
                    state.setAzAngle("", inpData.angle_a);
                    azTextBox.Text = state.getAzAngleText();

                    /*if ((state.getLastIncTime() != 0) && antennaParameters.debug) println("inc speed: " + ((int)((state.getIncAngle() - inpData.angle_n) * 1000 / (time - state.getLastIncTime()))).ToString());
                    state.setLastIncTime(time);*/
                    state.setIncAngle("", inpData.angle_n);
                    incTextBox.Text = state.getIncAngleText();

                }
                println("", antennaParameters.debug);
            }));
        }
        private void OnTimedEvent(Object source, System.Timers.ElapsedEventArgs e)
        {
            Dispatcher.Invoke((Action)(() =>
            {
                if (aTimer.Interval != antennaParameters.commandDelay) aTimer.Interval = antennaParameters.commandDelay;
                if (state.GetCheckedParameters() != null) check_start.IsEnabled = true;
                if (state.getManualState())
                {
                    int az = (state.getManualAzAngle() - state.getAzAngle()) / antennaParameters.speedK;
                    int inc = (state.getManualIncAngle() - state.getIncAngle()) / antennaParameters.speedK;
                    if (az > antennaParameters.creetUstAz) az = antennaParameters.creetUstAz;
                    if (inc > antennaParameters.creetUstInc) inc = antennaParameters.creetUstInc;
                    if (az < -antennaParameters.creetUstAz) az = -antennaParameters.creetUstAz;
                    if (inc < -antennaParameters.creetUstInc) inc = -antennaParameters.creetUstInc;
                    print(az.ToString() + "   ");
                    println(inc.ToString());
                    sendGenerator.Pa30_data sendData = new sendGenerator.Pa30_data(az, inc, 11, 10);
                     if (!antennaParameters.readonlyP) protocol.pa30_pack(sendData);
                }
                else
                {
                    println(state.getWorkMode().name, antennaParameters.debug);
                    switch (state.getWorkMode().name)
                    {
                        case "zero":
                            int az = (-state.getAzAngle()) / antennaParameters.speedK;
                            int inc = (-state.getIncAngle()) / antennaParameters.speedK;
                            if (az > antennaParameters.creetUstAz) az = antennaParameters.creetUstAz;
                            if (inc > antennaParameters.creetUstInc) inc = antennaParameters.creetUstInc;
                            if (az < -antennaParameters.creetUstAz) az = -antennaParameters.creetUstAz;
                            if (inc < -antennaParameters.creetUstInc) inc = -antennaParameters.creetUstInc;
                            state.setUstAz((state.azSpeed == 0 && az != 0) ? az + antennaParameters.addUstAz : az);
                            state.setUstInc(state.incSpeed == 0 && az != 0 ? inc + antennaParameters.addUstInc : inc);
                            if (check_start.Content.ToString() == "Стоп") check_start.Content = "Проверка";
                            if (!progon.IsEnabled) progon.IsEnabled = true;
                            break;

                        case "progon":
                            if (check_start.Content.ToString() == "Стоп") check_start.Content = "Проверка";
                            else
                            {
                                if (state.getWorkMode().name == "progon") progon.Content = "Стоп";
                            }
                            if (!progon.IsEnabled) progon.IsEnabled = true;
                            println(state.getWorkMode().name, antennaParameters.debug);
                            int prRealSpeedInc = antennaParameters.progonSpeedInc;
                            int prRealSpeedAz = antennaParameters.progonSpeedAz;
                            if (state.GetCheckedParameters() != null)
                            {
                                if (!state.GetCheckedParameters().inc) prRealSpeedInc = 0;
                                if (!state.GetCheckedParameters().az) prRealSpeedAz = 0;
                            }
                            if (state.getNeedAzAngle()<0) 
                            {
                                if (state.getAzAngle() < (antennaParameters.creetAngleAzN + 5) * 3600)
                                {
                                    state.setNeedAzAngle(antennaParameters.creetAngleAzP * 3600);
                                    state.setUstAz(0);
                                }
                                else
                                {
                                    int ust = Math.Abs(state.getUstAz());
                                    if (state.azSpeed < prRealSpeedAz) { ust++; }
                                    else if (state.azSpeed > prRealSpeedAz) { ust = Math.Abs(ust-1); }
                                    state.setUstAz(-ust);
                                }
                            }
                            else
                            {
                                if (state.getAzAngle() > (antennaParameters.creetAngleAzP - 5) * 3600)
                                {
                                    state.setNeedAzAngle(antennaParameters.creetAngleAzN * 3600);
                                    state.setUstAz(0);
                                }
                                else
                                {
                                    int ust = Math.Abs(state.getUstAz());
                                    if (state.azSpeed < prRealSpeedAz) { ust++; }
                                    else if (state.azSpeed > prRealSpeedAz) { ust = Math.Abs(ust-1); }
                                    state.setUstAz(ust);
                                }
                            }
                            if (state.getNeedIncAngle() < 0)
                            {
                                if (state.getIncAngle() < (antennaParameters.creetAngleIncN + 5) * 3600)
                                {
                                    state.setNeedIncAngle(antennaParameters.creetAngleIncP * 3600);
                                    state.setUstInc(0);
                                }
                                else
                                {
                                    int ust = Math.Abs(state.getUstInc());
                                    if (state.incSpeed < prRealSpeedInc) { ust++; }
                                    else if (state.incSpeed > prRealSpeedInc) { ust = Math.Abs(ust - 1); }
                                    state.setUstInc(-ust);
                                }
                            }
                            else
                            {
                                if (state.getIncAngle() > (antennaParameters.creetAngleIncP - 5) * 3600)
                                {
                                    state.setNeedIncAngle(antennaParameters.creetAngleIncN * 3600);
                                    state.setUstInc(0);
                                }
                                else
                                {
                                    int ust = Math.Abs(state.getUstInc());
                                    if (state.incSpeed < prRealSpeedInc) { ust++; }
                                    else if (state.incSpeed > prRealSpeedInc) { ust = Math.Abs(ust-1); }
                                    state.setUstInc(ust);
                                }
                            }
                            ustAz.Text = state.getUstAz().ToString();
                            ustInc.Text = state.getUstInc().ToString();
                            break;
                        case "naprTrogan":
                        {
                            if (state.startVoltageObj == null)
                            {
                                ustAz.IsReadOnly = true;
                                ustInc.IsReadOnly = true;
                                ustAzBut.IsEnabled = false;
                                ustIncBut.IsEnabled = false;
                                state.setStartVoltage(new classes.StartVoltage(
                                    state,
                                    antennaParameters.creetAngleAzP,
                                    antennaParameters.creetAngleAzN,
                                    antennaParameters.creetAngleIncP,
                                    antennaParameters.creetAngleIncN,
                                    antennaParameters.speedK,
                                    antennaParameters.creetUstAz,
                                    antennaParameters.creetUstInc));
                                state.startVoltageObj.StartCheck();
                            }
                            else if (!state.startVoltageObj.ready)
                            {
                                state.startVoltageObj.StartCheck();
                                ustAz.Text = state.startVoltageObj.minSpeedAz.ToString();
                                ustInc.Text = state.startVoltageObj.minSpeedInc.ToString();
                            }
                            else
                            {
                                ustAz.Text = state.startVoltageObj.minSpeedAz.ToString();
                                ustInc.Text = state.startVoltageObj.minSpeedInc.ToString();
                                if (state.startVoltageObj.Work())
                                {
                                    println("Минимальная уставка по азимуту: " + state.startVoltageObj.minSpeedAz.ToString());
                                    println("Минимальная уставка по наклону: " + state.startVoltageObj.minSpeedInc.ToString());
                                    //state.GoToNextParameters();
                                    ustAz.IsReadOnly = false;
                                    ustInc.IsReadOnly = false;
                                    ustAzBut.IsEnabled = true;
                                    ustIncBut.IsEnabled = true;
                                    ustAz.Text = "";
                                    ustInc.Text = "";
                                    state.setStartVoltage(null);
                                }
                            }
                            break;
                            }
                        case "naprTroganSect":
                            {
                                if (state.startVoltageObjSector == null)
                                {
                                    ustAz.IsReadOnly = true;
                                    ustInc.IsReadOnly = true;
                                    ustAzBut.IsEnabled = false;
                                    ustIncBut.IsEnabled = false;
                                    state.setStartVoltageSector(new classes.StartVoltageSector(
                                        state,
                                        antennaParameters.creetAngleAzP,
                                        antennaParameters.creetAngleAzN,
                                        antennaParameters.creetAngleIncP,
                                        antennaParameters.creetAngleIncN,
                                        antennaParameters.speedK,
                                        antennaParameters.creetUstAz,
                                        antennaParameters.creetUstInc));
                                    state.startVoltageObjSector.StartCheck();
                                }
                                else if (!state.startVoltageObjSector.ready)
                                {
                                    state.startVoltageObjSector.StartCheck();
                                    ustAz.Text = state.startVoltageObjSector.nowSpeed.ToString();
                                    ustInc.Text = state.startVoltageObjSector.minSpeedInc.ToString();
                                }
                                else
                                {
                                    ustAz.Text = state.startVoltageObjSector.nowSpeed.ToString();
                                    ustInc.Text = state.startVoltageObjSector.minSpeedInc.ToString();
                                    if (state.startVoltageObjSector.Work())
                                    {
                                        println("Минимальная уставка по азимуту в секторе -90 - -45 при движении по часовой: " + state.startVoltageObjSector.minSpeedAzNP.ToString());
                                        println("Минимальная уставка по азимуту в секторе -45 - 45 при движении по часовой: " + state.startVoltageObjSector.minSpeedAzZP.ToString());
                                        println("Минимальная уставка по азимуту в секторе 45 - 90 при движении по часовой: " + state.startVoltageObjSector.minSpeedAzPP.ToString());
                                        println("Минимальная уставка по азимуту в секторе 90 - 45 при движении против часовой: " + state.startVoltageObjSector.minSpeedAzPN.ToString());
                                        println("Минимальная уставка по азимуту в секторе 45 - -45 при движении против часовой: " + state.startVoltageObjSector.minSpeedAzZN.ToString());
                                        println("Минимальная уставка по азимуту в секторе -45 - -90 при движении против часовой: " + state.startVoltageObjSector.minSpeedAzNN.ToString());
                                        println("Минимальная уставка по наклону: " + state.startVoltageObjSector.minSpeedInc.ToString());
                                        state.GoToNextParameters();
                                        ustAz.IsReadOnly = false;
                                        ustInc.IsReadOnly = false;
                                        ustAzBut.IsEnabled = true;
                                        ustIncBut.IsEnabled = true;
                                        ustAz.Text = "";
                                        ustInc.Text = "";
                                        state.setStartVoltageSector(null);
                                    }
                                }
                                break;
                            }
                    }
                    print(state.getUstAz().ToString() + "   " + state.azSpeed.ToString() + ";  ", antennaParameters.debug);
                    println(state.getUstInc().ToString() + "   " + state.incSpeed.ToString(), antennaParameters.debug);
                    sendGenerator.Pa30_data sendData = new sendGenerator.Pa30_data(
                        state.getUstAz(),
                        state.getUstInc(),
                        00,
                        (ushort)(antennaParameters.commandDelay > 500 ? 50000 : (antennaParameters.commandDelay * 200)));//0x0800);//
                    if (!antennaParameters.readonlyP) protocol.pa30_pack(sendData);
                    //Thread.Sleep(50);
                    //if (antennaParameters.readonlyP) protocol.askToRead();
                }
            }));
        }
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            CheckedParameters.NowWork work = new CheckedParameters.NowWork("other", true);
            state.setWorkMode(work);
            int data = Int32.Parse(ustAz.Text);
            if (data > antennaParameters.creetUstAz) data = antennaParameters.creetUstAz;
            else if (data < -antennaParameters.creetUstAz) data = -antennaParameters.creetUstAz;
            state.setUstAz(data);
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            CheckedParameters.NowWork work = new CheckedParameters.NowWork("other", true);
            state.setWorkMode(work);
            int data = Int32.Parse(ustInc.Text);
            if (data > antennaParameters.creetUstInc) data = antennaParameters.creetUstInc;
            else if (data < -antennaParameters.creetUstInc) data = -antennaParameters.creetUstInc;
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

        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            progon.Content = state.getWorkMode().name == "progon" ? "Прогон" : "Стоп" ;
            ustAz.IsReadOnly = state.getWorkMode().name != "progon";
            ustInc.IsReadOnly = state.getWorkMode().name != "progon";
            CheckedParameters.NowWork work = new CheckedParameters.NowWork(state.getWorkMode().name == "progon" ? "zero" : "progon", true);
            state.setWorkMode(work);
            if (state.getWorkMode().name == "progon")
            {
                state.setNeedAzAngle(antennaParameters.creetAngleAzN * 3600);
                state.setNeedIncAngle(antennaParameters.creetAngleIncN * 3600);
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
            if ((state.getWorkMode().name == "zero") || (state.getWorkMode().name == "other"))
            {
                state.setWorkMode(state.GetCheckedParameters().FirstStart());
                check_start.Content = "Стоп";
                progon.IsEnabled = false;
            }
            else
            {
                ustAz.IsReadOnly = false;
                ustInc.IsReadOnly = false;
                ustAzBut.IsEnabled = true;
                ustIncBut.IsEnabled = true;
                ustAz.Text = "";
                ustInc.Text = "";
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
                for (int i =0; i< ports.Length; i++)
                {
                    try
                    {
                        SerialPort port = new SerialPort(ports[i]);
                        port.Open();
                        port.Close();
                    }
                    catch
                    {
                        ports = ports.Where((e, j) => j != i).ToArray();
                        i--;
                    }
                }
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
                readPortC = null;
                writePortC = null;
                aTimer.Stop();
                upd_port_button.Content = "Обновить";
            }
        }

        private void BaseSettings_Click(object sender, RoutedEventArgs e)
        {
            BaseSettingsWindow BaseSettings = new BaseSettingsWindow(antennaParameters);
            BaseSettings.Show();
        }

        private void println(string text, bool debug = true)
        {
            if (debug) textFieldUpd(text + "\n");
        }
        private void print(string text, bool debug = true)
        {
            if (debug) textFieldUpd(text);
        }

        private void Update_ports(object sender, RoutedEventArgs e)
        {
            Update_ports_func();
        }

        private void textFieldUpd(string text)
        {
            if (textField.Text.Length < 1000)
                textField.Text += text;
            else
            {
                string textp = textField.Text + text;
                textField.Text = textp.Substring(textp.Length - 1000);
            }
            textField.ScrollToEnd();
        }
    }
}
