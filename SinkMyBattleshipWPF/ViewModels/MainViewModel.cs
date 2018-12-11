using SinkMyBattleshipWPF.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SinkMyBattleshipWPF.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private string _action;

        public MainViewModel(Player player)
        {
            if (string.IsNullOrEmpty(player.Address))

            {
                Task.Run(() => StartServer(player));
            }
            else
            {
                Task.Run(() => StartClient(player));
            }
        }

        public static Logger Logger { get; set; } = new Logger();

        public event PropertyChangedEventHandler PropertyChanged;

        static TcpListener listener;

        public string LastAction { get; set; }

        public string Action
        {
            get => _action;
            set
            {
                _action = value;
                OnPropertyChanged(nameof(Action));
            }
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        private void StartClient(Player player)
        {
            using (var client = new TcpClient(player.Address, player.Port))
            using (var networkStream = client.GetStream())
            using (var reader = new StreamReader(networkStream, Encoding.UTF8))
            using (var writer = new StreamWriter(networkStream, Encoding.UTF8) { AutoFlush = true })
            {
                Logger.AddToLog($"Ansluten till {client.Client.RemoteEndPoint}");
                writer.WriteLine($"HELO {player.Name}");

                while (client.Connected)
                {
                    if (Action == "QUIT") break;

                    // Skicka text
                    while (true)
                    {
                        //var text = Console.ReadLine(); // LÄS FRÅN TEXTRUTA
                        writer.WriteLine(Action);
                        if (LastAction.Contains("fire")) break;
                    }

                    while (true)
                    {
                        var line = reader.ReadLineAsync();
                        writer.WriteLine($"Svar: {line}");
                        if (line.ToString().Contains("fire")) break;
                    }

                };

            }
        }


        static void StartListen(int port)
        {
            try
            {
                listener = new TcpListener(IPAddress.Any, port);
                listener.Start();
                Logger.AddToLog($"Starts listening on port: {port}");
            }
            catch (SocketException)
            {
                Logger.AddToLog("Misslyckades att öppna socket. Troligtvis upptagen.");
                Environment.Exit(1);
            }
        }

        private async Task StartServer(Player player)
        {
            Logger.AddToLog("Välkommen till servern");

            StartListen(player.Port);

            while (true)
            {
                Logger.AddToLog("Väntar på att någon ska ansluta sig...");


                using (var client = await listener.AcceptTcpClientAsync())
                using (var networkStream = client.GetStream())
                using (var reader = new StreamReader(networkStream, Encoding.UTF8))
                using (var writer = new StreamWriter(networkStream, Encoding.UTF8) { AutoFlush = true })
                {
                    Logger.AddToLog($"Klient har anslutit sig {client.Client.RemoteEndPoint}!");
                    writer.WriteLine("210 BATTLESHIP/1.0");
                    Logger.AddToLog("210 BATTLESHIP/1.0");
                    var handshake = false;
                    var start = false;

                    while (client.Connected)
                    {
                        var command = "";

                        // handskake
                        if (!handshake)
                        {
                            command = reader.ReadLine();

                            if (command.StartsWith("helo ", StringComparison.InvariantCultureIgnoreCase))
                            {
                                handshake = true;
                                Logger.AddToLog(command);
                                writer.WriteLine($"220 {player.Name}");
                                Logger.AddToLog($"220 {player.Name}");
                            }
                            else if (command.ToUpper() == "QUIT")
                            {
                                break; // TODO: do some logging
                            }
                            else if (true)
                            {
                                // check for 501 sequence error
                            }
                            else
                            {
                                // check for 500 syntax error
                            }

                        }

                        // start game
                        if(!start && handshake)
                        {
                            command = reader.ReadLine();

                            if (command.ToUpper() == "START")
                            {
                                writer.WriteLine(command);
                                Logger.AddToLog(command);
                                start = true;

                                // logic for who starts the game
                            }
                            else if (command.ToUpper() == "QUIT")
                            {
                                break; // TODO: do some logging
                            }
                            else if (true)
                            {
                                // check for 501 sequence error
                            }
                            else
                            {
                                // check for 500 syntax error
                            }
                        }


                        // wait for correct action from client
                        while (handshake)
                        {
                            command = reader.ReadLine();

                            if (!string.IsNullOrEmpty(command))
                            {
                                if (command.Contains("fire "))
                                {
                                    Logger.AddToLog($"Klient: {command}");
                                    writer.WriteLine($"Waiting for opponents action..");
                                    LastAction = "";
                                    break;
                                }
                                else
                                {
                                    writer.WriteLine("501 Sequence error");
                                }

                            }

                            if (command.ToUpper() == "QUIT")
                            {
                                break; // TODO: do some logging
                            }
                        }

                        // Wait for correct action from server
                        while (handshake)
                        {
                            if (!string.IsNullOrEmpty(LastAction))
                            {
                                if (LastAction.Contains("fire"))
                                {
                                    writer.WriteLine(LastAction + " LastAction");
                                    Logger.AddToLog(LastAction + " LastAction");
                                    Logger.AddToLog("Waiting for opponents action..");
                                    LastAction = "";
                                    break;
                                }
                                else if (LastAction.ToUpper() == "QUIT")
                                {
                                    break;
                                }

                                else
                                {
                                    //writer.WriteLine("501 Sequence error");
                                    Logger.AddToLog("501 Sequence error");
                                    LastAction = "";
                                }

                            }



                        }

                    }

                }


            }
        }


        public void SendAction()
        {
            LastAction = Action;
        }

    }
}
