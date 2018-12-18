using System.Collections.Generic;
using System.ComponentModel;

namespace SinkMyBattleshipWPF.Models
{
    public class Board : INotifyPropertyChanged
    {
        public Board()
        {
            Coor = new Dictionary<string, int>();

            InitBoard();
        }

        public void InitBoard()
        {
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

            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }


}
