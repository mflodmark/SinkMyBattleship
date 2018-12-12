using SinkMyBattleshipWPF.Extensions;
using SinkMyBattleshipWPF.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
}
