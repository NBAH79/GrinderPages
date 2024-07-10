using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Grinder;

public interface ISession
{
    public TcpClient client { get; set; }
    //public IPage? template { get; set; }
}
