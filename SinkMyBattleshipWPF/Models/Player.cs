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

        public Player(string name, string address, int port, List<Boat> boats)
        {
            Name = name;
            Address = address;
            Port = port;
            Boats = boats;
        }
    }
}
