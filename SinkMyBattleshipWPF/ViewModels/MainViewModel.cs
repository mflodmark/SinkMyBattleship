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
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Timers;


namespace SinkMyBattleshipWPF.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private string _action;

        public MainViewModel(Player player)
        {
            Player = player;
            Opponent = new Player(null, null, 0, new List<Boat>());

            if (player.Boats != null)
            {
                Boat1 = Player.Boats[0];
                Boat2 = Player.Boats[1];
                Boat3 = Player.Boats[2];
                Boat4 = Player.Boats[3];
                Boat5 = Player.Boats[4];

                //Boat5 = player.Boats[0];

                foreach (var item in Player.Boats)
                {
                    item.Position.Column += 11;
                }

            }

            try
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
            catch (SocketException e)
            {
                Logger.AddToLog(e.Message);
                Logger.AddToLog("Error - Restart!");
            }
            catch (IOException)
            {
                //Logger.AddToLog(e.Message);
                Logger.AddToLog(AnswerCodes.ConnectionClosed.GetDescription());
                Logger.AddToLog("Error - Restart!");
                listener?.Stop();
            }
            catch (Exception)
            {
                Logger.AddToLog(AnswerCodes.ConnectionClosed.GetDescription());
                Logger.AddToLog("Error - Restart!");
                listener?.Stop();
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

        public TcpClient Client { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        static TcpListener listener;

        public string LastAction { get; set; } = "";

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


        private static System.Timers.Timer aTimer;

        private static void SetTimer()
        {
            // Create a timer with a two second interval.
            aTimer = new System.Timers.Timer(2000);
            // Hook up the Elapsed event for the timer. 
            aTimer.AutoReset = true;
            aTimer.Enabled = true;
        }

        private void StopTimer()
        {
            aTimer.Close();
            aTimer.Dispose();
        }

        private async Task StartClient(Player player)
        {
            // Reset boats status
            foreach (var boat in player.Boats)
            {
                boat.Coordinates = boat.Coordinates.ToDictionary(x => x.Key, x => false);
            }
            player.ClearBoardUI();
            Opponent.ClearBoardUI();

            player.FiredAtOpponent = new List<string>();

            try
            {

                using (Client = new TcpClient(player.Address, player.Port))
                using (var networkStream = Client.GetStream())
                using (var reader = new StreamReader(networkStream, Encoding.UTF8))
                using (var writer = new StreamWriter(networkStream, Encoding.UTF8) { AutoFlush = true })
                {
                    var command = "";
                    LastAction = "";
                    var continuePlay = true;

                    Logger.AddToLog($"Ansluten till {Client.Client.RemoteEndPoint}");

                    command = reader.ReadLine();

                    if (command == null)
                    {
                        continuePlay = false;
                        Logger.AddToLog(AnswerCodes.ConnectionClosed.GetDescription());
                    }

                    // Check battleship
                    if (!string.IsNullOrEmpty(command))
                    {
                        if (!(command.StartsWith("210 ")))
                        {
                            Logger.AddToLog(AnswerCodes.ConnectionClosed.GetDescription());
                            continuePlay = false;
                        }
                        else
                        {
                            Logger.AddToLog(command);
                        }

                        if (string.IsNullOrEmpty(player.Name))
                        {
                            Logger.AddToLog("No name - Restart!");
                            continuePlay = false;
                        }
                        else
                        {
                            writer.WriteLine($"HELO {player.Name}");
                            Logger.AddToLog($"HELO {player.Name}");

                            // Check handshake
                            command = reader.ReadLine();

                            if (command == null)
                            {
                                continuePlay = false;
                                Logger.AddToLog(AnswerCodes.ConnectionClosed.GetDescription());
                            }

                            if (!command.StartsWith("220 "))
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

                            if (command == null)
                            {
                                continuePlay = false;
                                Logger.AddToLog(AnswerCodes.ConnectionClosed.GetDescription());
                            }

                            if (command.ToLower() == AnswerCodes.ClientStarts.GetDescription().ToLower())
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
                        }
                    }




                    while (Client.Connected && continuePlay)
                    {
                        for (int i = 1; i < 3; i++)
                        {
                            // Server command - game logic
                            while (Opponent.Turn == i && continuePlay)
                            {
                                command = reader.ReadLine();

                                if (command == null)
                                {
                                    continuePlay = false;
                                    Logger.AddToLog(AnswerCodes.ConnectionClosed.GetDescription());
                                    break;
                                }

                                if (command.ToLower().StartsWith("fire ") && CommandSyntaxCheck(command))
                                {
                                    // Game logic
                                    Logger.AddToLog(command);
                                    player.GetFiredAt(command);
                                    writer.WriteLine(player.GetFiredAtMessage(command));
                                    Logger.AddToLog(player.GetFiredAtMessage(command));
                                    LastAction = "";
                                    Logger.AddToLog("Your turn!");
                                    break;
                                }
                                else if (command.Trim().ToLower() == "quit")
                                {
                                    Logger.AddToLog("Server quit..");

                                    break;
                                }
                                else if (command.ToLower() == AnswerCodes.ConnectionClosed.GetDescription().ToLower())
                                {
                                    Logger.AddToLog(command);
                                }
                                else
                                {
                                    writer.WriteLine(AnswerCodes.Sequence_Error.GetDescription());
                                }



                            }


                            // Client command
                            while (player.Turn == i && continuePlay)
                            {

                                if (LastAction.ToUpper() == "QUIT")
                                {
                                    writer.WriteLine(LastAction);
                                    Logger.AddToLog(AnswerCodes.ConnectionClosed.GetDescription());
                                    continuePlay = false;
                                    break;
                                }

                                if (!string.IsNullOrEmpty(LastAction))
                                {
                                    if (LastAction.ToLower().StartsWith("fire "))
                                    {
                                        writer.WriteLine(LastAction);
                                        Logger.AddToLog(LastAction);
                                        player.FireAt(LastAction);
                                        Opponent.Command = LastAction;

                                        Logger.AddToLog("Waiting for opponents response");

                                        var response = reader.ReadLine();
                                        if (command == null)
                                        {
                                            continuePlay = false;
                                            Logger.AddToLog(AnswerCodes.ConnectionClosed.GetDescription());
                                            break;
                                        }

                                        if (response.StartsWith("5"))
                                        {
                                            Logger.AddToLog(response);
                                            LastAction = "";
                                            continue;
                                        }

                                        if (response.StartsWith("23") || response.StartsWith("24") || response.StartsWith("25"))
                                        {
                                            Logger.AddToLog(response);
                                            Opponent.Command += $" {response}";
                                            Opponent.GetFiredAtForUI();
                                        }

                                        if (response.ToLower() == AnswerCodes.ConnectionClosed.GetDescription().ToLower())
                                        {
                                            Logger.AddToLog(response);
                                            continuePlay = false;
                                            break;
                                        }

                                        if (response.ToLower() == AnswerCodes.YouWin.GetDescription().ToLower())
                                        {
                                            Logger.AddToLog(response);
                                            continuePlay = false;
                                            break;
                                        }

                                        Logger.AddToLog("Waiting for opponents action");
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
            catch (SocketException e)
            {
                Logger.AddToLog(e.Message);
                Logger.AddToLog("Error - Restart!");
            }
            catch (IOException)
            {
                //Logger.AddToLog(e.Message);
                Logger.AddToLog(AnswerCodes.ConnectionClosed.GetDescription());
                Logger.AddToLog("Error - Restart!");
                listener?.Stop();
            }
            catch (Exception)
            {
                Logger.AddToLog(AnswerCodes.ConnectionClosed.GetDescription());
                Logger.AddToLog("Error - Restart!");
                listener?.Stop();
            }
            finally
            {
                Client.Close();
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
                Logger.AddToLog("Misslyckades att öppna socket. Troligtvis upptagen. Restart!");
                listener?.Stop();
            }
            catch (IOException)
            {
                Logger.AddToLog("Something went wrong.. RESTART!");
                listener?.Stop();
            }
        }


        private async Task StartServer(Player player)
        {
            Logger.AddToLog("Välkommen till servern");

            StartListen(player.Port);

            while (true)
            {
                Logger.AddToLog("Waiting for someone to connect...");

                // Reset boats status
                foreach (var boat in player.Boats)
                {
                    boat.Coordinates = boat.Coordinates.ToDictionary(x => x.Key, x => false);
                }
                player.ClearBoardUI();
                Opponent.ClearBoardUI();

                player.FiredAtOpponent = new List<string>();

                try
                {


                    using (var client = await listener.AcceptTcpClientAsync())
                    using (var networkStream = client.GetStream())
                    using (var reader = new StreamReader(networkStream, Encoding.UTF8))
                    using (var writer = new StreamWriter(networkStream, Encoding.UTF8) { AutoFlush = true })
                    {
                        Logger.AddToLog($"Klient har anslutit sig {client.Client.RemoteEndPoint}!");
                        writer.WriteLine(AnswerCodes.Battleship.GetDescription());
                        Logger.AddToLog(AnswerCodes.Battleship.GetDescription());
                        SetTimer();

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

                                if (command == null)
                                {
                                    continuePlay = false;
                                    Logger.AddToLog(AnswerCodes.ConnectionClosed.GetDescription());
                                    break;
                                }
                                command = command.Trim().ToLower();

                                if (command.StartsWith("helo ", StringComparison.InvariantCultureIgnoreCase) ||
                                    command.StartsWith("hello ", StringComparison.InvariantCultureIgnoreCase))
                                {
                                    handshake = true;
                                    Logger.AddToLog(command);
                                    writer.WriteLine($"220 {player.Name}");
                                    Logger.AddToLog($"220 {player.Name}");
                                    Opponent.Name = command.Split(' ')[1];
                                }
                                else if (command == "quit")
                                {
                                    writer.WriteLine(AnswerCodes.ConnectionClosed.GetDescription());
                                    Logger.AddToLog(AnswerCodes.ConnectionClosed.GetDescription());
                                    break;
                                }
                                else if (!CommandSyntaxCheck(command))
                                {
                                    writer.WriteLine(AnswerCodes.Syntax_Error.GetDescription());
                                    errorCounterClient += 1;
                                }
                                else if (!CommandSequenceCheck(command, handshake, true))
                                {
                                    writer.WriteLine(AnswerCodes.Sequence_Error.GetDescription());
                                    errorCounterClient += 1;
                                }

                                if (errorCounterClient > 3)
                                {
                                    continuePlay = false;
                                    Logger.AddToLog(AnswerCodes.ConnectionClosed.GetDescription());
                                    writer.WriteLine(AnswerCodes.ConnectionClosed.GetDescription());
                                    break;
                                }
                            }

                            // start game
                            if (!start && handshake)
                            {
                                command = reader.ReadLine();

                                if (command == null)
                                {
                                    continuePlay = false;
                                    Logger.AddToLog(AnswerCodes.ConnectionClosed.GetDescription());
                                    break;
                                }

                                command = command.Trim().ToUpper();

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
                                else if (command == "QUIT")
                                {
                                    writer.WriteLine(AnswerCodes.ConnectionClosed.GetDescription());
                                    Logger.AddToLog(AnswerCodes.ConnectionClosed.GetDescription());
                                    break;
                                }
                                else if (!CommandSyntaxCheck(command))
                                {
                                    writer.WriteLine(AnswerCodes.Syntax_Error.GetDescription());
                                    errorCounterClient += 1;
                                }
                                else if (!CommandSequenceCheck(command, handshake, start))
                                {
                                    writer.WriteLine(AnswerCodes.Sequence_Error.GetDescription());
                                    errorCounterClient += 1;
                                }

                                if (errorCounterClient > 3)
                                {
                                    continuePlay = false;
                                    Logger.AddToLog(AnswerCodes.ConnectionClosed.GetDescription());
                                    writer.WriteLine(AnswerCodes.ConnectionClosed.GetDescription());
                                    break;
                                }
                            }

                            // Loop host vs client
                            for (int i = 1; i < 3; i++)
                            {
                                // wait for correct action from client
                                while (start && Opponent.Turn == i && continuePlay)
                                {
                                    command = reader.ReadLine();

                                    if (command == null)
                                    {
                                        continuePlay = false;
                                        Logger.AddToLog(AnswerCodes.ConnectionClosed.GetDescription());
                                        break;
                                    }

                                    command = command.Trim().ToUpper();

                                    if (string.IsNullOrEmpty(command))
                                    {
                                        continuePlay = false;
                                        Logger.AddToLog(AnswerCodes.ConnectionClosed.GetDescription());
                                        break;
                                    }

                                    if (command == "QUIT")
                                    {
                                        writer.WriteLine(AnswerCodes.ConnectionClosed.GetDescription());
                                        Logger.AddToLog(AnswerCodes.ConnectionClosed.GetDescription());
                                        Logger.AddToLog("Opponent quit..");
                                        continuePlay = false;
                                        break;
                                    }

                                    if (command == "HELP")
                                    {
                                        writer.WriteLine(
    @"***********************************
Write QUIT to terminate connection.
Write Fire <Coordinate> to fire. 
Write QUIT to terminate connection.
IF opponent misses your boats, write '230 <Message>'.
If your opponent HIT your Carrier, write '241 <Message>'.
If your opponent HIT your Battleship, write '242 <Message>'.
If your opponent HIT your Destroyer, write '243 <Message>'.
If your opponent HIT your Submariner, write '244 <Message>'.
If your opponent HIT your Patrol Boat, write '245 <Message>'.

If your opponent SUNK your Carrier, write '251 <Message>'.
If your opponent SUNK your Battleship, write '252 <Message>'.
If your opponent SUNK your Destroyer, write '253 <Message>'.
If your opponent SUNK your Submariner, write '254 <Message>'.
If your opponent SUNK your Patrol Boat, write '255 <Message>'.

If your opponent wins, write '260 <Message>'
************************************");
                                        continue;
                                    }

                                    if (command.StartsWith("270 ") ||
                                        command == null)
                                    {
                                        Logger.AddToLog(AnswerCodes.ConnectionClosed.GetDescription());
                                        writer.WriteLine(AnswerCodes.ConnectionClosed.GetDescription());
                                        continuePlay = false;
                                        errorCounterClient = 0;
                                        break;
                                    }

                                    //if (command.StartsWith("270 "))
                                    //{
                                    //    writer.WriteLine(AnswerCodes.ConnectionClosed.GetDescription());
                                    //    Logger.AddToLog(command);
                                    //    continuePlay = false;
                                    //    errorCounterClient = 0;
                                    //    break;
                                    //}

                                    if (command.StartsWith("230 ") || command.StartsWith("24") || command.StartsWith("25"))
                                    {
                                        Opponent.Command += command;
                                        Opponent.GetFiredAtForUI();
                                        Logger.AddToLog(command);
                                        Logger.AddToLog("Waiting for opponents action..");
                                        errorCounterClient = 0;
                                        continue;
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
                                            if (Opponent.CheckFiredAt(command))
                                            {
                                                writer.WriteLine(AnswerCodes.Sequence_Error.GetDescription());
                                                continue;
                                            }

                                            // Game logic
                                            Logger.AddToLog($"Klient: {command}");
                                            Opponent.FireAt(command);
                                            player.GetFiredAt(command);
                                            writer.WriteLine(player.GetFiredAtMessage(command));
                                            Logger.AddToLog(player.GetFiredAtMessage(command));

                                            if (player.GetFiredAtMessage(command).StartsWith("260 "))
                                            {
                                                Logger.AddToLog("You lost...");
                                                continuePlay = false;
                                            }
                                            else
                                            {
                                                Logger.AddToLog("It's your turn!");
                                            }
                                            LastAction = "";
                                            errorCounterClient = 0;
                                            break;
                                        }
                                        else
                                        {
                                            // Answer on a fired shot
                                            //writer.WriteLine(player.GetFiredAtMessage(command));
                                            //Logger.AddToLog(player.GetFiredAtMessage(command));
                                            //LastAction = "";
                                            //errorCounterClient = 0;
                                            //break;
                                        }

                                    }

                                    if (errorCounterClient > 3)
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
                                    if (LastAction.ToUpper() == AnswerCodes.ConnectionClosed.GetDescription().ToUpper())
                                    {
                                        writer.WriteLine(AnswerCodes.ConnectionClosed.GetDescription());
                                        Logger.AddToLog(AnswerCodes.ConnectionClosed.GetDescription());
                                        continuePlay = false;
                                        break;
                                    }

                                    if (LastAction == "RestartServer")
                                    {
                                        continuePlay = false;
                                        break;
                                    }

                                    if (!string.IsNullOrEmpty(LastAction))
                                    {
                                        if (!CommandSyntaxCheck(LastAction))
                                        {
                                            Logger.AddToLog(AnswerCodes.Syntax_Error.GetDescription());
                                            errorCounterServer += 1;
                                        }
                                        else if (!CommandSequenceCheck(LastAction, handshake, start))
                                        {
                                            Logger.AddToLog(AnswerCodes.Sequence_Error.GetDescription());
                                            errorCounterServer += 1;
                                        }
                                        else if (LastAction.ToLower().StartsWith("fire "))
                                        {
                                            if (!player.CheckFiredAt(LastAction))
                                            {
                                                writer.WriteLine(LastAction);
                                                Logger.AddToLog(LastAction);

                                                player.FireAt(LastAction);
                                                Opponent.Command = $"{LastAction} ";

                                                Logger.AddToLog("Waiting for opponents response..");
                                                LastAction = "";
                                                errorCounterServer = 0;
                                                break;
                                            }
                                            else
                                            {
                                                Logger.AddToLog(AnswerCodes.Sequence_Error.GetDescription());
                                                errorCounterServer += 1;
                                            }

                                        }

                                        LastAction = "";
                                        if (errorCounterServer > 3)
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
                    if (LastAction == "RestartServer")
                    {

                        Restart();
                        break;
                    }
                }
                catch (IOException)
                {
                    Logger.AddToLog(AnswerCodes.ConnectionClosed.GetDescription());
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
            //LastAction = "RestartServer";
            if (Client != null)
            {
                Client.Close();
            }
            listener?.Stop();
            Logger.ClearLog();
            var manager = new WindowManager();
            manager.ShowWindow(new ShellViewModel(), null);
            Application.Current.Windows[0].Close();
        }

        private void Restart()
        {
            var manager = new WindowManager();
            manager.ShowWindow(new ShellViewModel(), null);
            Application.Current.Windows[0].Close();
        }

    }


}
