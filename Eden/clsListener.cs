using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace Eden
{
    internal class Listener
    {
        public string szIP { get { return _szIP; } }
        private string _szIP;
        public int nPort { get { return _nPort; } }
        private int _nPort;

        public Socket socket;

        public Listener(string szIP, int nPort)
        {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _szIP = szIP;
            _nPort = nPort;
        }

        public (int, string) Start()
        {
            int code = 1;
            string msg = string.Empty;

            try
            {
                socket.Bind(new IPEndPoint(IPAddress.Parse(szIP), nPort));
                socket.Listen();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                msg = ex.Message;
                code = 0;
            }

            return (code, msg);
        }

        public (int, string) Stop()
        {
            int code = 1;
            string msg = string.Empty;

            return (code, msg);
        }
    }
}
