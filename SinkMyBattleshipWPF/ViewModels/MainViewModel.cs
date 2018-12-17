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
using System.Windows.Controls;
using System.Windows.Media;
using Caliburn.Micro;
using System.Windows;

namespace SinkMyBattleshipWPF.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private string _action;

        public MainViewModel(Player player)
        {
            Game.Player = player;

            Player = player;
            Opponent = new Player(null, null, 0, new List<Boat>());
            Opponent.Boats.Add(new Boat("Carrier", new Dictionary<string, bool>() { { "A1", false }, { "A2", false }, { "A3", false }, { "A4", false }, { "A5", false } }));
            Opponent.Boats.Add(new Boat("Battleship", new Dictionary<string, bool>() { { "B1", false }, { "B2", false }, { "B3", false }, { "B4", false }, }));
            Opponent.Boats.Add(new Boat("Destroyer", new Dictionary<string, bool>() { { "C1", false }, { "C2", false }, { "C3", false } }));
            Opponent.Boats.Add(new Boat("Submarine", new Dictionary<string, bool>() { { "D1", false }, { "D2", false }, { "D3", false } }));
            Opponent.Boats.Add(new Boat("Patrol Boat", new Dictionary<string, bool>() { { "E1", false }, { "E2", false } }));

            Boat1 = Player.Boats[0];
            Boat2 = Player.Boats[1];
            Boat3 = Player.Boats[2];
            Boat4 = Player.Boats[3];
            Boat5 = Player.Boats[4];

            foreach (var item in Player.Boats)
            {
                item.Position.Column += 11;
            }

            if (string.IsNullOrEmpty(player.Address))
            {
                Task.Run(() => StartServer(player));
            }
            else
            {
                Task.Run(() => StartClient(player));
            }


        }

        public Player Opponent { get; set; }

        public Player Player { get; set; }

        public Boat Boat1 { get; set; }
        public Boat Boat2 { get; set; }
        public Boat Boat3 { get; set; }
        public Boat Boat4 { get; set; }
        public Boat Boat5 { get; set; }

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
                    Opponent.Turn = 2;
                }
                else
                {
                    player.Turn = 2;
                    Opponent.Turn = 1;
                }
                Logger.AddToLog(command);

                while (client.Connected && continuePlay)
                {
                    for (int i = 1; i < 3; i++)
                    {
                        // Server command - game logic
                        while (Opponent.Turn == i && continuePlay)
                        {
                            command = await reader.ReadLineAsync();
                            Logger.AddToLog($"Server: {command}");

                            if (command.ToLower().StartsWith("fire "))
                            {
                                // Game logic
                                Logger.AddToLog(command);
                                Logger.AddToLog(reader.ReadLine());
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
                    var continuePlay = true;
                    var errorCounterClient = 0;
                    var errorCounterServer = 0;

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
                                Opponent.Name = command.Split(' ')[1];
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
                                Opponent.Turn = number == 1 ? 2 : 1;

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
                            while (start && Opponent.Turn == i && continuePlay)
                            {
                                command = reader.ReadLine();

                                if (command.ToUpper() == "QUIT")
                                {
                                    continuePlay = false;
                                    break; // TODO: do some logging
                                }

                                if (command.StartsWith("270") || command == null)
                                {
                                    Logger.AddToLog(AnswerCodes.ConnectionClosed.GetDescription());
                                    continuePlay = false;
                                    errorCounterClient = 0;
                                    break;
                                }

                                if (command.StartsWith("260"))
                                {
                                    writer.WriteLine(command);
                                    Logger.AddToLog(command); 
                                    continuePlay = false;
                                    break;
                                }

                                if (!CommandSyntaxCheck(command))
                                {
                                    errorCounterClient += 1;
                                    writer.WriteLine(AnswerCodes.Syntax_Error.GetDescription());
                                }
                                else if (!CommandSequenceCheck(command, handshake, start))
                                {
                                    errorCounterClient += 1;
                                    writer.WriteLine(AnswerCodes.Sequence_Error.GetDescription());
                                }
                                else if (!string.IsNullOrEmpty(command))
                                {
                                    if (command.ToLower().StartsWith("fire "))
                                    {
                                        // Game logic
                                        Logger.AddToLog($"Klient: {command}");
                                        Opponent.FireAt(command);
                                        player.GetFiredAt(command);
                                        writer.WriteLine(player.GetFiredAtMessage(command));
                                        Logger.AddToLog(player.GetFiredAtMessage(command));
                                        LastAction = "";
                                        errorCounterClient = 0;
                                        break;
                                    }
                                    else
                                    {
                                        // Answer on a fired shot
                                        Opponent.GetFiredAt(command);
                                        writer.WriteLine(Opponent.GetFiredAtMessage(command));
                                        Logger.AddToLog(Opponent.GetFiredAtMessage(command));
                                        LastAction = "";
                                        errorCounterClient = 0;
                                        break;
                                    }

                                }

                                if (errorCounterClient == 3)
                                {
                                    continuePlay = false;
                                    Logger.AddToLog(AnswerCodes.ConnectionClosed.GetDescription());
                                    writer.WriteLine(AnswerCodes.ConnectionClosed.GetDescription());
                                    break;
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

                                if (LastAction.StartsWith("270"))
                                {
                                    writer.WriteLine(AnswerCodes.ConnectionClosed.GetDescription());
                                    Logger.AddToLog(AnswerCodes.ConnectionClosed.GetDescription());
                                    continuePlay = false;
                                    break;
                                }

                                if (!string.IsNullOrEmpty(LastAction))
                                {
                                    if (LastAction.ToLower().StartsWith("fire "))
                                    {

                                        if (player.CheckFiredAt(LastAction))
                                        {
                                            writer.WriteLine(LastAction);
                                            Logger.AddToLog(LastAction);

                                            player.FireAt(LastAction);

                                        }

                                        //Logger.AddToLog("Waiting for opponents action..");
                                        LastAction = "";
                                        errorCounterServer = 0;
                                        break;
                                    }
                                    else if (!CommandSyntaxCheck(LastAction))
                                    {
                                        Logger.AddToLog(AnswerCodes.Syntax_Error.GetDescription());
                                        errorCounterServer += 1;
                                    }
                                    else if (!CommandSequenceCheck(LastAction, handshake, start))
                                    {
                                        Logger.AddToLog(AnswerCodes.Sequence_Error.GetDescription());
                                        errorCounterServer += 1;
                                    }

                                    LastAction = "";
                                    if (errorCounterServer == 3)
                                    {
                                        Logger.AddToLog(AnswerCodes.ConnectionClosed.GetDescription());
                                        writer.WriteLine(AnswerCodes.ConnectionClosed.GetDescription());
                                        break;
                                    }

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

        public void RestartServer()
        {
            var manager = new WindowManager();
            manager.ShowWindow(new ShellViewModel(), null);
            Application.Current.Windows[0].Close();

        }

    }

    
}
