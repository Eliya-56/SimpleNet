using SimpleNet.Core;
using SimpleNetNode;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleNet.ConsoleNode
{
    public class ConsoleLogger : ILogger
    {
        public void LogError(string message)
        {
            Console.WriteLine(DateTime.Now.ToLocalTime() + "\t ERROR\t\t" + message);
        }

        public void LogMessage(string message)
        {
            Console.WriteLine(DateTime.Now.ToLocalTime() + "\t MESSAGE\t\t" + message);
        }
    }
}
