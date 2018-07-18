using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SimpleNetProtocol
{
    public static class WriterExtensions
    {
        public static void WriteCommand(this BinaryWriter writer, CommandType command)
        {
            writer.Write((int)TrasnsferDataType.Command);
            writer.Write((int)command);
        }

        public static void WriteData<T>(this BinaryWriter writer, T data)
        {
            var dataType = typeof(T);
            writer.Write((int)TrasnsferDataType.DataObject);
            writer.Write(nameof(T));
            foreach (var prop in dataType.GetProperties())
            {
                if (prop.PropertyType == typeof(string))
                {
                    writer.Write((string)prop.GetValue(data));
                }
                else if (prop.PropertyType == typeof(int))
                {
                    writer.Write((int)prop.GetValue(data));
                }
                else if (prop.PropertyType == typeof(bool))
                {
                    writer.Write((bool)prop.GetValue(data));
                }
                else if (prop.PropertyType == typeof(DateTime))
                {
                    long time = ((DateTime)prop.GetValue(data)).ToBinary();
                    writer.Write(time);
                }
                else if (prop.PropertyType == typeof(string[]))
                {
                    var strs = (string[])prop.GetValue(data);
                    var size = strs.Length;
                    writer.Write(size);
                    for (int i = 0; i < size; i++)
                    {
                        writer.Write(strs[i]);
                    }
                }
            }
        }

        public static void WriteFile(this BinaryWriter writer)
        {
            throw new NotImplementedException("Transfer of files haven't implemented yet"); 
        }
    }
}
