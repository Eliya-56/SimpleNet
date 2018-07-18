using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleNetProtocol
{
    public enum TrasnsferDataType
    {
        Command = 1,
        DataObject,
        File
    }

    public enum CommandType
    {
        TurnOff = 1,
        SleepingMode 
    }

    public abstract class TrasnsferData
    {
        public TrasnsferDataType Type { get; set; }
    }

    public class CommandTrasnsferData : TrasnsferData
    {
        public CommandType Command { get; set; }
    }

    public class ObjectTrasnsferData : TrasnsferData
    {
        public object DataObject { get; set; }
        public string TypeName { get; set; }
    }

    public class FileTransferData : TrasnsferData
    {
        public byte[] FileBytes { get; set; }
    }
}
