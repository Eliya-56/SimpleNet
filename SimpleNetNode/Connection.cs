using SimpleNetProtocol;
using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleNetNode
{
    public class Connection
    {
        public Guid Id;
        public string Name;
        public Protocol TransferProtocol { get; set; }
    }
}
