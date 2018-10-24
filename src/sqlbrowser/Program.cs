using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Sockets;
using System.Text;
using McMaster.Extensions.CommandLineUtils;

namespace sqlbrowser
{
    [Command(Description = "SqlBrowser")]
    public class Program
    {
        private const string CLNT_UCAST_EX = "\x03";
        public static int Main(string[] args) => CommandLineApplication.Execute<Program>(args);

        [Argument(0, Description = "A positional parameter that must be specified.\nThe server name.")]
        [Required]
        public string Server { get; } = null;

        [Option(Description = "An optional parameter, with a default value.\nThe port to connect in sqlbrowser.")]
        [Range(1, 64000)]
        public int Port { get; } = 1434;

        private int OnExecute()
        {
            Console.WriteLine();
            Console.WriteLine($"Sqlbrowser");
            Console.WriteLine();
            foreach (string server in GetServers())
            {
                try
                {
                    UdpClient udpClient = new UdpClient();
                    udpClient.Connect(server, Port);
                    Byte[] data = Encoding.ASCII.GetBytes(CLNT_UCAST_EX);
                    udpClient.Send(data, data.Length);
                    IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, 0);
                    Byte[] dataReceived = udpClient.Receive(ref endPoint);
                    string dataReceivedText = Encoding.ASCII.GetString(dataReceived);
                    string[] sqlservers = dataReceivedText.Substring(3).Split(";;");
                    Console.WriteLine($"Server: {server}");
                    foreach (string sqlserver in sqlservers)
                    {
                        Console.WriteLine();
                        string[] sqlserverProperties = sqlserver.Split(";");
                        for (int i = 0; i < (sqlserverProperties.Length - 1); i = i + 2)
                        {
                            Console.WriteLine($"{sqlserverProperties[i]} : {sqlserverProperties[i + 1]}");
                        }
                    }
                    Console.WriteLine();
                } catch{
                    Console.WriteLine();
                    Console.WriteLine($"Error connecting to {server}");
                }
            }
            return 0;
        }

        private List<string> GetServers()
        {
            List<string> servers = new List<string>();
            //TODO: work over here. Allow a range of servers.
            servers.Add(Server);
            return (servers);
        }
    }
}
