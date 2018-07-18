using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;

namespace SimpleNetProtocol
{
    public class Protocol
    {
        private TcpClient _remouteNode;
        private BinaryReader _reader;
        private BinaryWriter _writer;

        public Dictionary<string, Type> RegisteredTypes { get; set; } = new Dictionary<string, Type>();


        public Protocol(TcpClient remouteNode)
        {
            _remouteNode = remouteNode;
            var stream = remouteNode.GetStream();
            _reader = new BinaryReader(stream);
            _writer = new BinaryWriter(stream);
        }

        public void RegistrateType<T>(string name)
        {
            RegisteredTypes.Add(name, typeof(T));
        }

        public TrasnsferData Read()
        {
            var transferType = (TrasnsferDataType)_reader.ReadInt32();
            if (transferType == TrasnsferDataType.Command)
            {
                return _reader.ReadCommand();
            }
            else if (transferType == TrasnsferDataType.DataObject)
            {
                return _reader.ReadData(RegisteredTypes);
            }
            else
            {
#warning !!!NOT IMPLEMENTED READ FILE
                return _reader.ReadFile();
            }
        }

        public void WriteCommand(CommandType command)
        {
            _writer.WriteCommand(command);
        }

        public void WriteData<T>(T data) where T : class
        {
            if (!RegisteredTypes.ContainsValue(typeof(T)))
            {
                throw new Exception(nameof(T) + " is not registered");
            }
            _writer.WriteData(data);
        }

#warning !!!NOT IMPLEMENTED WRITE FILE
        public void WriteFile()
        {
            _writer.WriteFile();
        }
    }
}
