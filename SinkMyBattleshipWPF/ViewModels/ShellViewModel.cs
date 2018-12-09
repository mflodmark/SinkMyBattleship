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
        private int _carrierRow;
        private int _carrierColumn;
        private int _carrierColumnSpan;
        private int _carrierRowSpan;
        private bool _carrierHorizontal;

        public ShellViewModel()
        {
            CarrierRow = 1;
            CarrierColumn = 1;
            CarrierHorizontal = true;
        }

        public string Name { get; set; }
        public string Address { get; set; }
        public int Port { get; set; }

        public int CarrierRow
        {
            get => _carrierRow;
            set
            {
                _carrierRow = value;
                OnPropertyChanged(nameof(CarrierRow));
            }
        }

        public int CarrierColumn
        {
            get => _carrierColumn;
            set
            {
                _carrierColumn = value;
                OnPropertyChanged(nameof(CarrierColumn));
            }
        }

        public int CarrierColumnSpan
        {
            get => _carrierColumnSpan;
            set
            {
                _carrierColumnSpan = value;
                OnPropertyChanged(nameof(CarrierColumnSpan));
            }
        }

        public int CarrierRowSpan
        {
            get => _carrierRowSpan;
            set
            {
                _carrierRowSpan = value;
                OnPropertyChanged(nameof(CarrierRowSpan));
            }
        }

        public bool CarrierHorizontal
        {
            get => _carrierHorizontal;
            set
            {
                _carrierHorizontal = value;
                if(_carrierHorizontal)
                {
                    CarrierRowSpan = 1;
                    CarrierColumnSpan = 5;
                }
                else
                {
                    CarrierRowSpan = 5;
                    CarrierColumnSpan = 1;
                }
            }
        }


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
