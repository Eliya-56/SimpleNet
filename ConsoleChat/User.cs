using SimpleNet.Core;
using SimpleNetNode;
using SimpleNetProtocol;
using System;
using System.Linq;
using System.Net;

namespace ConsoleChat
{
    public class User
    {
        private readonly NetNode _node;
        public string Name { get; set; }

        public User(string name, int port)
        {
            Name = name;
            _node = new NetNode(new FakeLogger(), name);
            _node.Start(port, 1111, 1122);
            _node.RegisterDataType<TextMessage>();
            _node.DataReceived += TextMessageReceived;
        }

        private void TextMessageReceived(Guid id, ObjectTrasnsferData data)
        {
            var message = (TextMessage)data.DataObject;
            Console.Beep(124, 2);
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message.Name + "(" + _node.Connections.FirstOrDefault(c => c.Id == id).Name + ")");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(message.Message);
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine(message.SendTime);
            Console.ForegroundColor = ConsoleColor.White;
        }

        public void SendMessage(string message, string remouteName, string localName)
        {
            if (string.IsNullOrEmpty(localName))
                localName = Name;
            Guid id;
            try
            {
                id = _node.Connections.First(c => c.Name == Name).Id;
            }
            catch
            {
                Console.WriteLine("ERROR: Bad name");
                return;
            }
            _node.WriteData(id, new TextMessage
            {
                Name = localName,
                Message = message,
                SendTime = DateTime.Now
            });
        }

        public void Connect(string ip, int port)
        {
            _node.Connect(IPAddress.Parse(ip), port);
        }

        public void ShowUsers()
        {
            foreach (var connection in _node.Connections)
            {
                Console.WriteLine(connection.Name);
            }
        }
    }
}
