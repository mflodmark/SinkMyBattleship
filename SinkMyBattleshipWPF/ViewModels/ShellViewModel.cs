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

            Boats.Add(new Boat("Carrier", new List<string>() {"A1","A2","A3","A4","A5" }));
            Boats.Add(new Boat("Battleship", new List<string>() {"B1","B2","B3","B4"}));
            Boats.Add(new Boat("Destroyer", new List<string>() {"C1","C2","C3" }));
            Boats.Add(new Boat("Submarine", new List<string>() {"D1","D2","D3"}));
            Boats.Add(new Boat("Patrol Boat", new List<string>() {"E1","E2"}));

            CarrierRow = 1;
            CarrierColumn = 1;
            CarrierHorizontal = true;
        }

        public string Name { get; set; }
        public string Address { get; set; }
        public int Port { get; set; }

        public Player Player { get; set; }

        public List<Boat> Boats { get; set; } = new List<Boat>();

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
            manager.ShowWindow(new MainViewModel(new Player(Name, Address, Port, Boats)), null);
            Application.Current.Windows[0].Close();

        }

        private int GetRow()
        {
            return 1;
        }
    }
}
