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
using System.Windows.Shapes;

namespace WpfApp1
{
    /// <summary>
    /// Логика взаимодействия для PropetyWindow.xaml
    /// </summary>
    public partial class PropetyWindow : Window
    {
        public antennaState state;
        public PropetyWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Hide();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            CheckedParameters pars = new CheckedParameters(
                    naprTrogan.IsChecked == true,
                    naprTroganSect.IsChecked == true,
                    kPered.IsChecked == true,
                    maxSpeed.IsChecked == true,
                    minLength.IsChecked == true,
                    progon.IsChecked == true,
                    az.IsChecked == true,
                    inc.IsChecked == true
                );
            state.SetCheckedParameters( pars );
            //state.setWorkMode(pars.FirstStart());
            Hide();
        }
    }
}
