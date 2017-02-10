using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using WhatToPlay.Model;
using WhatToPlay.ViewModel;

namespace WhatToPlay
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow(Steam steam)
        {
            InitializeComponent();
            ((SteamViewModel)this.SteamView.DataContext).Initialize(steam);
        }
        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            if (MessageBox.Show("Are you sure you want to Exit?", "Exit", MessageBoxButton.YesNo, MessageBoxImage.Question) ==  MessageBoxResult.Yes)
                e.Cancel = false;
            else
                e.Cancel = true;
        }
        protected override void OnClosed(EventArgs e)
        {
            (DataContext as WhatToPlay.ViewModel.SteamViewModel).Shutdown();
            App.Current.Shutdown();

        }
    }
}
