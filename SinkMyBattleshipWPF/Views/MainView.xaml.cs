using SinkMyBattleshipWPF.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SinkMyBattleshipWPF.Views
{
    /// <summary>
    /// Interaction logic for MainView.xaml
    /// </summary>
    public partial class MainView : Window
    {
        public MainView()
        {
            InitializeComponent();

            // i = row, j = column
            for (int i = 1; i <= 10; i++)
            {
                for (int j = 1; j <= 10; j++)
                {
                    var newBtn = new Button();

                    newBtn.Name = new Position("A1",1).GetCoordinateFrom(i,j);
                    newBtn.Content = newBtn.Name;
                    newBtn.Background = Brushes.AliceBlue;
                    newBtn.Click += SendAction_Click;

                    Grid.SetRow(newBtn, i);
                    Grid.SetColumn(newBtn, j);

                    GridLayout.Children.Add(newBtn);

                }

            }
        }

        private void SendAction_Click(object sender, RoutedEventArgs e)
        {
            // check whick button is the sender 
            var obj = sender as Button;
            Action.Text = $"FIRE {obj.Name}";
        }

        
    }
}
