﻿using System.ComponentModel;
using System.Windows;

namespace SonicRetro.SA3D.WPF.ViewModel.Base
{
    public class BaseViewModel : INotifyPropertyChanged
    {
        private static readonly DependencyObject _dummyDependencyObject = new DependencyObject();

        protected static bool IsDesignMode => !DesignerProperties.GetIsInDesignMode(_dummyDependencyObject);

        public event PropertyChangedEventHandler PropertyChanged = (sender, e) => { };

        protected void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
