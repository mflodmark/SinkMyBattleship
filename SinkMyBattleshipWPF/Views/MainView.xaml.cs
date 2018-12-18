using SinkMyBattleshipWPF.Models;
using SinkMyBattleshipWPF.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Automation.Provider;

namespace SinkMyBattleshipWPF.Views
{
    /// <summary>
    /// Interaction logic for MainView.xaml
    /// </summary>
    public partial class MainView : Window
    {
        //private readonly Player _player;

        public MainView()
        {
            InitializeComponent();

            var vm = this.DataContext as MainViewModel;

            //DrawTarget();
            //DrawOcean();
        }

        private void DrawTarget()
        {
            // i = row, j = column
            for (int i = 1; i <= 10; i++)
            {
                for (int j = 1; j <= 10; j++)
                {
                    var newBtn = new Button();

                    newBtn.Name = "t_" + new Position("A1", 1).GetCoordinateFrom(i, j);
                    newBtn.Content = new Position("A1", 1).GetCoordinateFrom(i, j);
                    newBtn.Background = Brushes.LightBlue;
                    newBtn.Click += SendAction_Click;

                    Grid.SetRow(newBtn, i);
                    Grid.SetColumn(newBtn, j);

                    GridLayout.Children.Add(newBtn);

                }

            }
        }

        private void DrawOcean()
        {
            // i = row, j = column
            for (int i = 1; i <= 10; i++)
            {
                for (int j = 12; j <= 21; j++)
                {
                    var newBtn = new Button();
                    var minus = 11;
                    newBtn.Name = "o_" + new Position("A1", 1).GetCoordinateFrom(i, j-minus);
                    newBtn.Content = new Position("A1", 1).GetCoordinateFrom(i, j - minus);
                    newBtn.Opacity = 0.8;

                    Grid.SetRow(newBtn, i);
                    Grid.SetColumn(newBtn, j);

                    GridLayout.Children.Add(newBtn);
                }

            }
        }

        private void SendAction_Click(object sender, RoutedEventArgs e)
        {
            var obj = sender as Button;
            //obj.Background = Brushes.White;
            Action.Text = $"FIRE {obj.Content}";
            var peer = new ButtonAutomationPeer(SendAction);
            var invokeProv = peer.GetPattern(PatternInterface.Invoke) as IInvokeProvider;
            invokeProv.Invoke();
        }


    }
}
