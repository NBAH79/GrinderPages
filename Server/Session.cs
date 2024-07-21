using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Server;

    public class Session
    {
        public Page? page;
        public TcpClient client=new ();

        public Session(TcpClient client, Page? template)
        {
            this.client = client;
            this.page = null;
        }
    }
