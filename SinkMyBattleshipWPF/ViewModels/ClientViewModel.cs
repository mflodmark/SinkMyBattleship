using SinkMyBattleshipWPF.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SinkMyBattleshipWPF.ViewModels
{
    public class ClientViewModel : INotifyPropertyChanged
    {
        private string _action;

        public ClientViewModel(string address, int port)
        {
            StartClient(address, port);
        }

        public Logger Logger { get; set; } = new Logger();

        public string Action {
            get => _action;
            set {
                _action = value;
                OnPropertyChanged(nameof(Action));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        private void StartClient(string address, int port)
        {
            using (var client = new TcpClient(address, port))
            using (var networkStream = client.GetStream())
            using (var reader = new StreamReader(networkStream, Encoding.UTF8))
            using (var writer = new StreamWriter(networkStream, Encoding.UTF8) { AutoFlush = true })
            {
                Logger.AddToLog($"Ansluten till {client.Client.RemoteEndPoint}");
                while (client.Connected)
                {
                    Logger.AddToLog("Ange text att skicka: (Skriv QUIT för att avsluta)");

                    if (Action == "QUIT") break;

                    // Skicka text
                    writer.WriteLine(Action);

                    if (!client.Connected) break;

                    // Läs minst en rad
                    do
                    {
                        var line = reader.ReadLine();
                        Logger.AddToLog($"Svar: {line}");

                    } while (networkStream.DataAvailable);

                };

            }
        }

    }
}
