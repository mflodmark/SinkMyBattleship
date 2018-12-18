using SinkMyBattleshipWPF.Extensions;
using SinkMyBattleshipWPF.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace SinkMyBattleshipWPF.Models
{
    public class Player
    {
        public string Name { get; set; }

        public string Address { get; set; }

        public int Port { get; set; }

        public List<Boat> Boats { get; set; } = new List<Boat>();

        public int Turn { get; set; }

        public List<string> FiredAtOpponent { get; set; } = new List<string>();

        public Board OceanBoard { get; set; } = new Board();

        public Board TargetBoard { get; set; } = new Board();

        public Player(string name, string address, int port, List<Boat> boats)
        {
            Name = name;
            Address = address;
            Port = port;
            Boats = boats;
        }

        public string FireAt(string input)
        {
            var arr = input.Split(' ');
            var input1 = arr[1].ToUpper();
            FiredAtOpponent.Add(input1);
            return input1;
        }

        public bool CheckFiredAt(string input)
        {
            var arr = input.Split(' ');
            var input1 = arr[1].ToUpper();

            if (FiredAtOpponent.Contains(input1))
            {
                return true;
            }
            return false;
        }

        public bool GetFiredAt(string input)
        {
            var arr = input.Split(' ');
            var input1 = arr[1].ToUpper();

            OceanBoard.Coor[input1] = 1;
            var coord = new Dictionary<string, int>();
            coord = OceanBoard.Coor;
            OceanBoard.Coor = coord;

            foreach (var boat in Boats)
            {
                foreach (var coor in boat.Coordinates)
                {
                    if (coor.Key == input1)
                    {
                        boat.Coordinates[coor.Key] = true;
                        return true;
                    }
                }
            }

            return false;
        }

        public string GetFiredAtMessage(string input)
        {
            var arr = input.Split(' ');
            var input1 = arr[1].ToUpper();
            var boatName = "";
            var boatEnum = 0;
            var sunk = true;

            if (!GetFiredAt(input))
            {
                return AnswerCodes.Miss.GetDescription();
            }

            foreach (var boat in Boats)
            {
                foreach (var coor in boat.Coordinates)
                {
                    if (coor.Key == input1)
                    {
                        boatName = boat.Name;
                    }
                }
            }

            foreach (var boat in Boats)
            {
                foreach (var coor in boat.Coordinates)
                {
                    if (boat.Name == boatName && coor.Value == false)
                    {
                        sunk = false;
                    }
                }

                if (boat.Name == boatName && sunk)
                {
                    boat.Alive = false;
                }
            }

            if (Boats.All(x => x.Alive == false))
            {
                // WON
                return AnswerCodes.YouWin.GetDescription();
            }


            // SUNK
            if (sunk)
            {
                switch (boatName)
                {
                    case "Carrier":
                        boatEnum = 251;
                        break;
                    case "Battleship":
                        boatEnum = 252;
                        break;
                    case "Destroyer":
                        boatEnum = 253;
                        break;
                    case "Submarine":
                        boatEnum = 254;
                        break;
                    case "Patrol Boat":
                        boatEnum = 255;
                        break;
                }
            }
            else
            {
                // HIT
                switch (boatName)
                {
                    case "Carrier":
                        boatEnum = 241;
                        break;
                    case "Battleship":
                        boatEnum = 242;
                        break;
                    case "Destroyer":
                        boatEnum = 243;
                        break;
                    case "Submarine":
                        boatEnum = 244;
                        break;
                    case "Patrol Boat":
                        boatEnum = 245;
                        break;
                }
            }

            var myEnum = (AnswerCodes)boatEnum;
            return myEnum.GetDescription();
        }


    }

    public class Board : INotifyPropertyChanged
    {
        public Board()
        {
            Coor = new Dictionary<string, int>();

            // i = row, j = column
            for (int i = 1; i <= 10; i++)
            {
                for (int j = 1; j <= 10; j++)
                {
                    Coor.Add(new Position("A1", 1).GetCoordinateFrom(i, j), 0);
                }

            }
        }

        // 0 == not fired at, 1= hit, 2 = miss
        private Dictionary<string, int> _coor;

        public Dictionary<string, int> Coor
        {
            get { return _coor; }
            set {
                _coor = new Dictionary<string, int>();
                _coor = value;
                OnPropertyChanged(nameof(Coor));
                //_coor.PropertyChanged += (sender, args) => OnPropertyChanged(nameof(Coor));

            }
        }


        //private SolidColorBrush _A1;

        //public SolidColorBrush A1
        //{
        //    get { return _A1; }
        //    set
        //    {
        //        switch (Coor["A1"])
        //        {
        //            case 0:
        //                _A1 = Brushes.LightBlue;
        //                break;
        //            case 1:
        //                _A1 = Brushes.Red;
        //                break;
        //            case 2:
        //                _A1 = Brushes.White;
        //                break;
        //            default:
        //                break;
        //        }

        //        OnPropertyChanged(nameof(A1));
        //    }
        //}


        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    //public static class Game
    //{
    //    public static Player Player { get; set; }

    //}
}
