﻿using SonicRetro.SA3D.WPF.ViewModel;
using System.Windows;

namespace SonicRetro.SA3D.WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
			DataContext = new MainViewModel(this);
			InitializeComponent();
		}
	}
}
