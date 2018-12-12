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
using SinkMyBattleshipWPF.Extensions;
using SinkMyBattleshipWPF.Utils;
using System.Threading;

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


        private async Task StartClient(Player player)
        {
            using (var client = new TcpClient(player.Address, player.Port))
            using (var networkStream = client.GetStream())
            using (var reader = new StreamReader(networkStream, Encoding.UTF8))
            using (var writer = new StreamWriter(networkStream, Encoding.UTF8) { AutoFlush = true })
            {
                var command = "";
                LastAction = "";
                var playerServer = new Player(null, null, 0, null);
                var continuePlay = true;

                Logger.AddToLog($"Ansluten till {client.Client.RemoteEndPoint}");
                
                // Check battleship
                command = reader.ReadLine();
                if (!command.StartsWith("210"))
                {
                    continuePlay = false;
                }
                else
                {
                    Logger.AddToLog(command);
                }

                writer.WriteLine($"HELO {player.Name}");
                Logger.AddToLog($"HELO {player.Name}");

                // Check handshake
                command = reader.ReadLine();
                if (!command.StartsWith("220"))
                {
                    continuePlay = false;
                }
                else
                {
                    Logger.AddToLog(command);
                }

                //Check which player starts
                writer.WriteLine("START");
                Logger.AddToLog("START");

                command = reader.ReadLine();
                if(command.StartsWith("221"))
                {
                    player.Turn = 1;
                    playerServer.Turn = 2;
                }
                else
                {
                    player.Turn = 2;
                    playerServer.Turn = 1;
                }
                Logger.AddToLog(command);

                while (client.Connected && continuePlay)
                {
                    for (int i = 1; i < 3; i++)
                    {
                        // Server command - game logic
                        while (playerServer.Turn == i && continuePlay)
                        {
                            command = await reader.ReadLineAsync();
                            Logger.AddToLog($"Server: {command}");

                            if (command.ToLower().StartsWith("fire "))
                            {
                                // Game logic
                                //writer.WriteLine(AnswerCodes.YouHitMyBattleship.GetDescription());
                                LastAction = "";
                                break;
                            }
                            else if (command.ToLower().StartsWith("270"))
                            {
                                // Connection lost
                            }
                            else if (command.ToLower().StartsWith("260"))
                            {
                                // You win
                            }


                            

                        }


                        // Client command
                        while (player.Turn == i && continuePlay)
                        {
                            if (LastAction.ToUpper() == "QUIT")
                            {
                                continuePlay = false;
                                break; // TODO: do some logging
                            }

                            if (!string.IsNullOrEmpty(LastAction))
                            {
                                if (LastAction.ToLower().StartsWith("fire "))
                                {
                                    writer.WriteLine(LastAction);
                                    Logger.AddToLog(LastAction);

                                    var response = reader.ReadLine();
                                    if (response.StartsWith("5"))
                                    {
                                        Logger.AddToLog(response);
                                        LastAction = "";
                                        continue;
                                    }

                                    LastAction = "";
                                    break;
                                }

                                LastAction = "";

                            }
                            else
                            {
                                Thread.Sleep(500);
                            }
                        }

                        //Logger.AddToLog("Waiting for opponents action..");

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
                    writer.WriteLine(AnswerCodes.Battleship.GetDescription());
                    Logger.AddToLog(AnswerCodes.Battleship.GetDescription());

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
                            else if (!CommandSyntaxCheck(command))
                            {
                                Logger.AddToLog(AnswerCodes.Syntax_Error.GetDescription());
                            }
                            else if (!CommandSequenceCheck(command, handshake, true))
                            {
                                Logger.AddToLog(AnswerCodes.Sequence_Error.GetDescription());
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
                                    writer.WriteLine(AnswerCodes.HostStarts.GetDescription());
                                    Logger.AddToLog(AnswerCodes.HostStarts.GetDescription());
                                }
                                else
                                {
                                    writer.WriteLine(AnswerCodes.ClientStarts.GetDescription());
                                    Logger.AddToLog(AnswerCodes.ClientStarts.GetDescription());
                                }
                            }
                            else if (command.ToUpper() == "QUIT")
                            {
                                break; // TODO: do some logging
                            }
                            else if (!CommandSyntaxCheck(command))
                            {
                                Logger.AddToLog(AnswerCodes.Syntax_Error.GetDescription());
                            }
                            else if (!CommandSequenceCheck(command, handshake, start))
                            {
                                Logger.AddToLog(AnswerCodes.Sequence_Error.GetDescription());
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
                                    writer.WriteLine(AnswerCodes.Syntax_Error.GetDescription());
                                }
                                else if (!CommandSequenceCheck(command, handshake, start))
                                {
                                    writer.WriteLine(AnswerCodes.Sequence_Error.GetDescription());
                                }
                                else if (!string.IsNullOrEmpty(command))
                                {
                                    if (command.ToLower().StartsWith("fire "))
                                    {
                                        // Game logic
                                        Logger.AddToLog($"Klient: {command}");
                                        //writer.WriteLine($"Waiting for opponents action..");
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

                                if (!string.IsNullOrEmpty(LastAction))
                                {
                                    if (LastAction.ToLower().StartsWith("fire "))
                                    {
                                        writer.WriteLine(LastAction);
                                        Logger.AddToLog(LastAction);
                                        Logger.AddToLog("Waiting for opponents action..");
                                        LastAction = "";
                                        break;
                                    }
                                    else if (!CommandSyntaxCheck(LastAction))
                                    {
                                        Logger.AddToLog(AnswerCodes.Syntax_Error.GetDescription());
                                    }
                                    else if (!CommandSequenceCheck(LastAction, handshake, start))
                                    {
                                        Logger.AddToLog(AnswerCodes.Sequence_Error.GetDescription());
                                    }

                                    LastAction = "";

                                }
                                else
                                {
                                    Thread.Sleep(500);
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
            var arr = input.Split(' ');
            var input1 = arr[0].ToUpper();
            string input2 = "";
            if (arr.Length >= 2) input2 = arr[1].ToUpper();

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
