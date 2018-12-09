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

        public MainViewModel(string address, int port)
        {
            if (string.IsNullOrEmpty(address))

            {
                Task.Run(() => StartServer(port));
            }
            else
            {
                Task.Run(() => StartClient(address, port));
            }
        }

        public static Logger Logger { get; set; } = new Logger();

        public event PropertyChangedEventHandler PropertyChanged;

        static TcpListener listener;

        public string LastAction { get; set; }

        public bool ClientReady { get; set; } // ha kvaR?

        public bool ServerReady { get; set; } // ha kvar??

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
                Logger.AddToLog($"Server: Starts listening on port: {port}");
            }
            catch (SocketException)
            {
                Logger.AddToLog("Misslyckades att öppna socket. Troligtvis upptagen.");
                Environment.Exit(1);
            }
        }

        private async Task StartServer(int port)
        {
            Logger.AddToLog("Server: Välkommen till servern");

            StartListen(port);

            while (true)
            {
                Logger.AddToLog("Server: Väntar på att någon ska ansluta sig...");


                using (var client = await listener.AcceptTcpClientAsync())
                using (var networkStream = client.GetStream())
                using (var reader = new StreamReader(networkStream, Encoding.UTF8))
                using (var writer = new StreamWriter(networkStream, Encoding.UTF8) { AutoFlush = true })
                {
                    Logger.AddToLog($"Server: Klient har anslutit sig {client.Client.RemoteEndPoint}!");

                    while (client.Connected)
                    {
                        var command = "";

                        // wait for correct action from client
                        while (true)
                        {
                            command = reader.ReadLine();

                            if (command.Contains("fire"))
                            {
                                Logger.AddToLog($"Klient: {command}");
                                writer.WriteLine($"Waiting for opponents action..");
                                LastAction = "";
                                ServerReady = true;
                                ClientReady = false;
                                break;
                            }
                            else if (!ClientReady)
                            {
                                // do nothing??
                            }
                            else
                            {
                                writer.WriteLine("501 Sequence error");
                                //Logger.AddToLog("501 Sequence error");
                            }
                        }

                        // Wait for correct action from server
                        while (true)
                        {
                            if (!string.IsNullOrEmpty(LastAction))
                            {
                                if (LastAction.Contains("fire"))
                                {
                                    writer.WriteLine(LastAction + " LastAction");
                                    Logger.AddToLog(LastAction + " LastAction");
                                    Logger.AddToLog("Waiting for opponents action..");
                                    LastAction = "";
                                    ClientReady = true;
                                    ServerReady = false;
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
