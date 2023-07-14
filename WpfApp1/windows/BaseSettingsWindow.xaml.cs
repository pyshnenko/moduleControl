using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.IO;
using static System.Net.Mime.MediaTypeNames;

namespace WpfApp1
{
    /// <summary>
    /// Логика взаимодействия для BaseSettingsWindow.xaml
    /// </summary>
    public partial class BaseSettingsWindow : Window
    {

        private ForSave pars;
        public BaseSettingsWindow(ForSave parameters)
        {
            InitializeComponent();
            pars = parameters;
            progonSpeedAz.Text = parameters.progonSpeedAz.ToString();
            progonSpeedInc.Text = parameters.progonSpeedInc.ToString();
            creetAngleAzP.Text = parameters.creetAngleAzP.ToString();
            creetAngleAzN.Text = parameters.creetAngleAzN.ToString();
            creetAngleIncP.Text = parameters.creetAngleIncP.ToString();
            creetAngleIncN.Text = parameters.creetAngleIncN.ToString();
            speedK.Text = parameters.speedK.ToString();
            commandDelay.Text = parameters.commandDelay.ToString();
            debug.IsChecked = parameters.debug;
            readonlyP.IsChecked = parameters.readonlyP;
            creetUstAz.Text = parameters.creetUstAz.ToString();
            creetUstInc.Text = parameters.creetUstInc.ToString();
            startSpeedAz.Text = parameters.addUstAz.ToString();
            startSpeedInc.Text = parameters.addUstInc.ToString();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            pars.progonSpeedAz = int.Parse(progonSpeedAz.Text);
            pars.progonSpeedInc = int.Parse(progonSpeedInc.Text);
            pars.creetAngleAzP = int.Parse(creetAngleAzP.Text);
            pars.creetAngleAzN = int.Parse(creetAngleAzN.Text);
            pars.creetAngleIncP = int.Parse(creetAngleIncP.Text);
            pars.creetAngleIncN = int.Parse(creetAngleIncN.Text);
            pars.speedK = int.Parse(speedK.Text);
            pars.commandDelay = int.Parse(commandDelay.Text);
            pars.creetUstAz = int.Parse(creetUstAz.Text);
            pars.creetUstInc = int.Parse(creetUstInc.Text);
            pars.debug = (bool)debug.IsChecked;
            pars.readonlyP = (bool)readonlyP.IsChecked;
            pars.addUstAz = int.Parse(startSpeedAz.Text);
            pars.addUstInc = int.Parse(startSpeedInc.Text);
            string data = JsonSerializer.Serialize(pars);
            File.WriteAllText("settings.cfg", data);
            Close();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
