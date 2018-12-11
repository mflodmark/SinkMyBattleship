using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SinkMyBattleshipWPF.Models
{
    public class Boat
    {
        public string Name { get; set; }

        public List<string> Coordinates { get; set; } = new List<string>();

        public bool Alive { get; set; }

        public Boat(string name, List<string> coordinates)
        {
            Alive = true;
            Name = name;
            Coordinates = coordinates;
        }
    }


}
