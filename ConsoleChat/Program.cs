using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleChat
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Select your port");
            var port = Console.ReadLine();
            Console.WriteLine("Write your name");
            var name = Console.ReadLine();
            var user = new User(name, int.Parse(port));

            while (true)
            {
                var command = Console.ReadLine();
                switch (command)
                {
                    case "connect":
                        Console.WriteLine("IP: ");
                        var ip = Console.ReadLine();
                        Console.WriteLine("Port: ");
                        var remoutePort = Console.ReadLine();
                        user.Connect(ip, int.Parse(remoutePort));
                        break;
                    case "names":
                        user.ShowUsers();
                        break;
                    case "text":
                        Console.WriteLine("Whome: ");
                        var remoutename = Console.ReadLine();
                        Console.WriteLine("Message: ");
                        var message = Console.ReadLine();
                        user.SendMessage(message, remoutename, null);
                        break;
                    default:
                        break;
                }
                command = "";
            }
        }
    }
}
