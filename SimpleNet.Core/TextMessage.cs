using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleNet.Core
{
    public class TextMessage
    {
        public string Name { get; set; }
        public string Message { get; set; }
        public DateTime SendTime { get; set; }
    }
}
