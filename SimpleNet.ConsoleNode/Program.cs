using SimpleNetNode;
using SimpleNetProtocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SimpleNet.ConsoleNode
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Write node name");
            var nodeName = Console.ReadLine();
            var node = new NetNode(new ConsoleLogger(), nodeName);
            node.RegisterDataType<TextMessage>(); 
            node.CommandReceived += CommandRecieved;
            node.NewConnectionAccepted += NewConnection;
            node.DataReceived += TextMessageReceived;
            node.NodeFound += NodeFound;
            Console.Write("Write port: ");
            var newPort = Console.ReadLine();
            Console.Write("Write inPort: ");
            var inPort = Console.ReadLine();
            Console.Write("Write outPort: ");
            var outPort = Console.ReadLine();
            Console.Write("Write respondPort: ");
            var respondPort = Console.ReadLine();
            node.Start(int.Parse(newPort), int.Parse(inPort),  int.Parse(respondPort));
            
            while (true)
            {
                Console.WriteLine("Write Command or message: ");
                var command = Console.ReadLine();
                switch (command)
                {
                    case "status":
                        foreach (var con in node.Connections)
                        {
                            Console.WriteLine(con.Id + " " + con.Name);
                        }
                        break;
                    case "connect":
                        Console.Write("IP: ");
                        var ip = Console.ReadLine();
                        Console.Write("Port: ");
                        var port = Console.ReadLine();
                        Console.Write("Name: ");
                        var name = Console.ReadLine();
                        node.Connect(IPAddress.Parse(ip), int.Parse(port), name);
                        break;
                    case "turnOff":
                        foreach (var con in node.Connections)
                        {
                            node.WriteCommand(con.Id, CommandType.TurnOff);
                        }
                        break;
                    case "sleep":
                        foreach (var con in node.Connections)
                        {
                            node.WriteCommand(con.Id, CommandType.SleepingMode);
                        }
                        break;
                    case "search":
                        node.SearchForNodes(int.Parse(outPort));
                        break;
                    case "stop":
                        return;
                    default:
                        foreach (var connection in node.Connections)
                        {
                            node.WriteData(connection.Id, new TextMessage
                            {
                                Message = command,
                                Name = connection.Name,
                                SendTime = DateTime.Now
                            });
                        }
                        break;
                }
                command = "";
            }
        }

        public static void TextMessageReceived(Guid id, ObjectTrasnsferData data)
        {
            Console.Beep(124, 2);
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(((TextMessage)data.DataObject).Name);
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(((TextMessage)data.DataObject).Message);
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine(((TextMessage)data.DataObject).SendTime);
            Console.ForegroundColor = ConsoleColor.White;
        }

        public static void NodeFound(IPAddress ip, string name)
        {
            Console.WriteLine("Found name: " + name + " ip: " + ip.ToString());
        }

        public static void CommandRecieved(Guid id, CommandTrasnsferData data)
        {
            Console.WriteLine(data.Command + " recieved from " + id);
        }

        public static void NewConnection(string ip)
        {
            Console.WriteLine("New connection  " + ip);
        }
    }
}
