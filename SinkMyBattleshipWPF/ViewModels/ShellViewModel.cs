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

            //Boats.Add(new Boat("Carrier", new Dictionary<string, bool>() {{ "A1", false }, {"A2", false}, {"A3", false}, {"A4", false}, {"A5", false}}));
            //Boats.Add(new Boat("Battleship", new Dictionary<string, bool>() {{ "B1", false }, {"B2", false}, {"B3", false}, {"B4", false},}));
            //Boats.Add(new Boat("Destroyer", new Dictionary<string, bool>() {{ "C1", false }, {"C2", false}, {"C3", false}}));
            //Boats.Add(new Boat("Submarine", new Dictionary<string, bool>() {{ "D1", false }, {"D2", false}, {"D3", false}}));
            Boats.Add(new Boat("Patrol Boat", new Dictionary<string, bool>() {{ "E1", false }, {"E2", false}}));

            Boat1 = Boats[0];
            Boat2= Boats[1];
            Boat3= Boats[2];
            Boat4= Boats[3];
            Boat5= Boats[4];

        }

        public string Name { get; set; }
        public string Address { get; set; }
        public int Port { get; set; }

        public Player Player { get; set; }

        public List<Boat> Boats { get; set; } = new List<Boat>();

        public Boat Boat1 { get; set; }
        public Boat Boat2 { get; set; }
        public Boat Boat3 { get; set; }
        public Boat Boat4 { get; set; }
        public Boat Boat5 { get; set; }

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
