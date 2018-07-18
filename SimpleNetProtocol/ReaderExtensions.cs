using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SimpleNetProtocol
{
    public static class ReaderExtensions
    {
        public static TrasnsferData ReadCommand(this BinaryReader reader)
        {
            return new CommandTrasnsferData
            {
                Type = TrasnsferDataType.Command,
                Command = (CommandType)reader.ReadInt32()
            };
        }

        public static TrasnsferData ReadData(this BinaryReader reader, Dictionary<string, Type> registeredTypes)
        {
            var typeName = reader.ReadString();
            var data = reader.ReadData(registeredTypes[typeName]);

            return new ObjectTrasnsferData
            {
                Type = TrasnsferDataType.DataObject,
                TypeName = typeName,
                DataObject = data
            };
        }

        public static TrasnsferData ReadFile(this BinaryReader reader)
        {
            throw new NotImplementedException("Transfer of files haven't implemented yet");
        }

        private static object ReadData(this BinaryReader reader, Type dataType)
        {
            var data = Activator.CreateInstance(dataType);
            foreach (var prop in dataType.GetProperties())
            {
                if (prop.PropertyType == typeof(string))
                {
                    prop.SetValue(data, reader.ReadString());
                }
                else if (prop.PropertyType == typeof(int))
                {
                    prop.SetValue(data, reader.ReadInt32());
                }
                else if (prop.PropertyType == typeof(bool))
                {
                    prop.SetValue(data, reader.ReadBoolean());
                }
                else if (prop.PropertyType == typeof(DateTime))
                {
                    var time = reader.ReadInt64();
                    prop.SetValue(data, DateTime.FromBinary(time));
                }
                else if (prop.PropertyType == typeof(string[]))
                {
                    var size = reader.ReadInt32();
                    List<string> strs = new List<string>();
                    for (int i = 0; i < size; i++)
                    {
                        strs.Add(reader.ReadString());
                    }
                    prop.SetValue(data, strs.ToArray());
                }
            }
            return data;
        }
    }
}
