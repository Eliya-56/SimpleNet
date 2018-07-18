using SimpleNet.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace SimpleNetProtocol
{
    public class SearchProtocol
    {
        private UdpClient _sendClient;
        private UdpClient _receiverClient;
        private bool _listenRequests = false;
        private bool _listenResponses = false;
        private IPAddress _multicastAddress;
        private readonly ILogger _logger;

        public string NodeName { get; set; }
        /// <summary>
        /// This port that would listen for incoming search requests
        /// </summary>
        public int PortIncomingRequests { get; private set; }
        /// <summary>
        /// This port that would listen for responses of outcoming requests
        /// </summary>
        public int ListenRespondPort { get; private set; }

        public event Action<IPEndPoint> IPRequested;
        public event Action<IPAddress, string> IPFound;

        public SearchProtocol(ILogger logger, IPAddress ip, int inPort, int responsePort, string nodeName = "Unrecognized node") : this(logger, new IPEndPoint(ip, inPort), responsePort, nodeName)
        { }

        public SearchProtocol(ILogger logger, IPEndPoint endPoint, int responsePort, string nodeName = "Unrecognized node")
        {
            _logger = logger;
            NodeName = nodeName;
            PortIncomingRequests = endPoint.Port;
            ListenRespondPort = responsePort;
            _receiverClient = new UdpClient(endPoint.Port);
            _receiverClient.JoinMulticastGroup(endPoint.Address);
            _multicastAddress = endPoint.Address;
        }

        /// <summary>
        /// Listening if someone node searching and respond
        /// </summary>
        public void ListenForRequests()
        {
            _listenRequests = true;
            IPEndPoint remoteIp = null;
            while (_listenRequests)
            {
                try
                {
                    var message = _receiverClient.Receive(ref remoteIp);
                    _logger.LogMessage("Search request was got");
                    int port = int.Parse(Encoding.Unicode.GetString(message));
                    IPRequested?.Invoke(remoteIp);
                    remoteIp.Port = port;
                    Respond(remoteIp);
                }
                catch (Exception ex)
                {
                    _logger.LogError("Can't listen requst, message: " + ex.Message);
                }
            }
        }

        /// <summary>
        /// Listen for responses for searching requests 
        /// </summary>
        public void ListenForResponses()
        {
            _listenResponses = true;
            var listener = new TcpListener(LocalConfig.GetLocalIP(), ListenRespondPort);
            listener.Start();
            while (_listenResponses)
            {
                try
                {
                    var remouteClient = listener.AcceptTcpClient();
                    _logger.LogMessage("Got response from " + ((IPEndPoint)remouteClient.Client.RemoteEndPoint).Address.ToString());
                    var stream = new BinaryReader(remouteClient.GetStream());
                    var ip = IPAddress.Parse(stream.ReadString());
                    var name = stream.ReadString();
                    IPFound?.Invoke(ip, name);
                }
                catch (Exception ex)
                {
                    _logger.LogError("Can't listen response, message: " + ex.Message);
                }
            }
            _logger.LogMessage("Stop listen responses");
        }

        private void Respond(IPEndPoint ip)
        {
            var responseClient = new TcpClient();
            try
            {
                responseClient.Connect(ip);
                var stream = new BinaryWriter(responseClient.GetStream());
                stream.Write(LocalConfig.GetLocalIP().ToString());
                stream.Write(NodeName);
            }
            catch (Exception ex)
            {
                _logger.LogError("Can't send response, message: " + ex.Message);
            }
        }

        /// <summary>
        /// Stop Listening
        /// </summary>
        public void StopListen()
        {
            _listenRequests = false;
            _listenResponses = false;
            _logger.LogMessage("Stop listen for search requests");
        }

        /// <summary>
        /// Start searching nodes in local net, if found event would be generated
        /// </summary>
        /// <param name="outPort">port that remoute nodes listen</param>
        public void StartSearch(int outPort)
        {
            _sendClient = new UdpClient();
            try
            {
                _logger.LogMessage("Start search nodes on port: " + outPort);
                byte[] message = Encoding.Unicode.GetBytes(ListenRespondPort.ToString());
                _sendClient.Send(message, message.Length, new IPEndPoint(_multicastAddress, outPort));
            }
            catch (Exception ex)
            {
                _logger.LogError("Can't send search request, message: " + ex.Message);
            }
        }
    }
}
