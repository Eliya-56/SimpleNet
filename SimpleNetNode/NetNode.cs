using SimpleNet.Core;
using SimpleNetProtocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace SimpleNetNode
{
    public class NetNode
    {
        private List<Connection> _connections;
        private SearchProtocol searching;
        private Dictionary<string, Type> _registeredTypes = new Dictionary<string, Type>();

        public List<PublicConnection> Connections
        {
            get
            {
                return _connections.Select(c => new PublicConnection { Id = c.Id, Name = c.Name }).ToList();
            }
        }
        public string NodeName { get; set; }

        public event Action<string> NewConnectionAccepted;
        public event Action<Guid, CommandTrasnsferData> CommandReceived;
        public event Action<Guid, ObjectTrasnsferData> DataReceived;
        public event Action<Guid, FileTransferData> FileReceived;
        public event Action<IPAddress, string> NodeFound;

        private ILogger _logger;
        private TcpListener _listener;
        private bool _continueListening = false;
        private Task _listening;

        public NetNode(ILogger logger, string nodeName = "Unrecognized node")
        {
            NodeName = nodeName;
            _logger = logger;
            _connections = new List<Connection>();
        }

        public void Start(int port, int inPort, int respondePort)
        {
            var ip = new IPEndPoint(IPAddress.Parse("235.5.5.11"), inPort);
            searching = new SearchProtocol(_logger, ip, respondePort,  NodeName);
            searching.IPFound += nodeFound;
            Task.Run(() => searching.ListenForRequests());
            Task.Run(() => searching.ListenForResponses());
            _logger.LogMessage("Starting");
            _listener = new TcpListener(LocalConfig.GetLocalIP(), port);
            _listener.Start();
            _listening = new Task(ListenConnections);
            _listening.Start();
        }

        public void SearchForNodes(int outPort)
        {
            searching.StartSearch(outPort);
        }

        public void RegisterDataType<T>()
        {
            _registeredTypes.Add(nameof(T), typeof(T));
            foreach (var connection in _connections)
            {
                connection.TransferProtocol.RegistrateType<T>(nameof(T));
            }
        }

        public void WriteCommand(Guid id, CommandType command)
        {
            var connection =_connections.FirstOrDefault(c => c.Id == id);
            if(connection == null)
            {
                _logger.LogError("No connection with Id: " + id);
                return;
            }
            connection.TransferProtocol.WriteCommand(command);
            _logger.LogMessage("Command " + command.ToString() + " sent");
        }

        public void WriteData<T>(Guid id, T data) where T :class
        {
            var connection = _connections.FirstOrDefault(c => c.Id == id);
            if (connection == null)
            {
                _logger.LogError("No connection with Id: " + id);
                return;
            }
            connection.TransferProtocol.WriteData(data);
            _logger.LogMessage("Data  sent");
        }

        public void Connect(IPAddress address, int port, string name = null)
        {
            var newConnection = new TcpClient();
            try
            {
                newConnection.Connect(new IPEndPoint(address, port));
            }
            catch(Exception ex)
            {
                _logger.LogError("Can't connect to " + address.AddressFamily.ToString() + ". Error: " + ex.Message);
            }
            if (string.IsNullOrEmpty(name))
            {
                name = "New connection " + _connections.Count;
            }
            var id = Guid.NewGuid();
            var connection = new Connection
            {
                Id = id,
                Name = name,
                TransferProtocol = new Protocol(newConnection)
            };
            connection.TransferProtocol.RegisteredTypes = _registeredTypes;
            _connections.Add(connection);
            Task.Run(() => ListenData(connection));
        }

        private void ListenConnections()
        {
            _continueListening = true;
            while (_continueListening)
            {
                try
                {
                    var newConnection = _listener.AcceptTcpClient();
                    _logger.LogMessage("New incoming connection " + ((IPEndPoint)newConnection.Client.RemoteEndPoint).Address);
                    var id = Guid.NewGuid();
                    var name = "New connection " + _connections.Count + 1;
                    var connection = new Connection
                    {
                        Id = id,
                        Name = name,
                        TransferProtocol = new Protocol(newConnection)
                    };
                    connection.TransferProtocol.RegisteredTypes = _registeredTypes;
                    _connections.Add(connection);
                    Task.Run(() => ListenData(connection));
                    NewConnectionAccepted?.Invoke(name);
                }
                catch (Exception ex)
                {
                    _logger.LogError("Error while listening: " + ex.Message);
                }
            }
        }

        private void ListenData(Connection connection)
        {
            while (true)
            {
                try
                {
                    var data = connection.TransferProtocol.Read();
                    if (data.Type == TrasnsferDataType.Command)
                    {
                        _logger.LogMessage("Command received from " + connection.Id + " " + connection.Name);
                        CommandReceived?.Invoke(connection.Id, (CommandTrasnsferData)data);
                    }
                    else if (data.Type == TrasnsferDataType.DataObject)
                    {
                        _logger.LogMessage("Data received from " + connection.Id + " " + connection.Name);
                        DataReceived?.Invoke(connection.Id, (ObjectTrasnsferData)data);
                    }
                    else
                    {
                        _logger.LogMessage("File received from " + connection.Id + " " + connection.Name);
                        FileReceived?.Invoke(connection.Id, (FileTransferData)data);
                    }
                }
                catch(Exception ex)
                {
                    _logger.LogError("Disconected: " + connection.Id + " " + connection.Name + " Error connection: " + ex.Message);
                    Disconect(connection);
                    return;
                }
            }
        }

        private void Disconect(Connection connection)
        {
            _connections.Remove(connection);
        }

        private void nodeFound(IPAddress ip, string name)
        {
            NodeFound?.Invoke(ip, name);
        }
    }
}
