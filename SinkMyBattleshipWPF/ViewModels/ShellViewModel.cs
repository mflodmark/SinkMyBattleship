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
            if(string.IsNullOrEmpty(Address))
            {
                StartServer();
            } else
            {
                StartClient();
            }
        }

        private void StartServer()
        {
            var manager = new WindowManager();
            manager.ShowDialog(new ServerViewModel(Port), null);
        }

        private void StartClient()
        {
            var manager = new WindowManager();
            manager.ShowDialog(new ClientViewModel(Address, Port), null);
        }
    }
}
