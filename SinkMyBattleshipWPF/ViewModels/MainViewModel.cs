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
                    if (LastAction == "QUIT") break;

                    while (true)
                    {
                        var line = reader.ReadLine();

                        writer.WriteLine($"START");
                        if (line.ToString().Contains("fire")) break;
                    }

                    // Skicka text
                    while (true)
                    {
                        //var text = Console.ReadLine(); // LÄS FRÅN TEXTRUTA
                        writer.WriteLine(LastAction);
                        if (LastAction.Contains("fire")) break;
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
                    var clientPlayer = new Player(null, null, 0, null);
                    var continuePlay = true;

                    while (client.Connected && continuePlay)
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
                                clientPlayer.Name = command.Split(' ')[1];
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
                        if (!start && handshake)
                        {
                            command = reader.ReadLine();

                            if (command.ToUpper() == "START")
                            {
                                Logger.AddToLog(command);
                                start = true;

                                // logic for who starts the game
                                var random = new Random();
                                var number = random.Next(0, 2) + 1;
                                player.Turn = number;
                                clientPlayer.Turn = number == 1 ? 2 : 1;

                                if (player.Turn == 1)
                                {
                                    writer.WriteLine($"222 Host Starts");
                                    Logger.AddToLog($"222 Host Starts");
                                }
                                else
                                {
                                    writer.WriteLine($"221 Client Starts");
                                    Logger.AddToLog($"221 Client Starts");
                                }
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

                        // Loop host vs client
                        for (int i = 1; i < 3; i++)
                        {
                            // wait for correct action from client
                            while (start && clientPlayer.Turn == i && continuePlay)
                            {
                                command = reader.ReadLine();

                                if (command.ToUpper() == "QUIT")
                                {
                                    continuePlay = false;
                                    break; // TODO: do some logging
                                }

                                if (!CommandSyntaxCheck(command))
                                {
                                    writer.WriteLine("501 Syntax Error");
                                }
                                else if (!CommandSequenceCheck(command, handshake, start))
                                {
                                    writer.WriteLine("500 Sequence Error");
                                }
                                else if (!string.IsNullOrEmpty(command))
                                {
                                    if (command.ToLower().StartsWith("fire "))
                                    {
                                        // Game logic
                                        Logger.AddToLog($"Klient: {command}");
                                        writer.WriteLine($"Waiting for opponents action..");
                                        LastAction = "";
                                        break;
                                    }

                                }

                            }

                            // Wait for correct action from server
                            while (start && player.Turn == i && continuePlay)
                            {
                                if (LastAction.ToUpper() == "QUIT")
                                {
                                    continuePlay = false;
                                    break; // TODO: do some logging
                                }


                                if (!CommandSyntaxCheck(LastAction))
                                {
                                    Logger.AddToLog("501 Syntax Error");
                                }
                                else if (!CommandSequenceCheck(LastAction, handshake, start))
                                {
                                    Logger.AddToLog("500 Sequence Error");
                                }
                                else if (!string.IsNullOrEmpty(LastAction))
                                {
                                    if (LastAction.ToLower().StartsWith("fire "))
                                    {
                                        writer.WriteLine(LastAction);
                                        Logger.AddToLog(LastAction);
                                        Logger.AddToLog("Waiting for opponents action..");
                                        LastAction = "";
                                        break;
                                    }

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

        public bool CommandSyntaxCheck(string input)
        {
            var input1 = input.Split(' ')[0].ToUpper();
            var input2 = input.Split(' ')[1].ToUpper();
            var commands = new List<string>() { "HELO", "START", "FIRE", "HELP" };

            if (!commands.Contains(input1))
                return false;

            if (input1 == "FIRE")
            {
                return FireSyntaxCheck(input2);
            }

            return true;
        }

        private bool CommandSequenceCheck(string input, bool handshake, bool start)
        {
            if (input.ToLower().Split(' ')[0] == "fire" && handshake && start)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool FireSyntaxCheck(string input)
        {
            if (input.Length < 1 || input.Length > 3)
                return false;
            var character = input[0];
            if (character < 'A' || character > 'J')
            {
                return false;
            }
            int num;
            int num2;
            var isNum = int.TryParse(input[1].ToString(), out num);
            if (!isNum)
            {
                return false;
            }
            if (input.Length == 3)
            {
                isNum = int.TryParse(input[2].ToString(), out num2);
                if (!isNum)
                    return false;
                var textNum = num.ToString() + num2.ToString();
                num = int.Parse(textNum);
            }
            if (num < 1 || num > 10)
            {
                return false;
            }
            return true;
        }

    }
}
