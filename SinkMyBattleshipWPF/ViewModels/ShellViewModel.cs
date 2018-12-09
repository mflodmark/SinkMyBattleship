using Caliburn.Micro;
using SinkMyBattleshipWPF.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SinkMyBattleshipWPF.ViewModels
{
    public class ShellViewModel : INotifyPropertyChanged
    {


        public ShellViewModel()
        {

        }

        public string Name { get; set; }
        public string Address { get; set; }
        public int Port { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void PlayGame()
        {
            var manager = new WindowManager();
            manager.ShowWindow(new MainViewModel(Address, Port), null);
            Application.Current.Windows[0].Close();
            
        }
    }
}
