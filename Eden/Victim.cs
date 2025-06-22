using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eden
{
    internal class Victim
    {
        public string[] m_aLoadedPayload { get; set; }
        public stVictimInfo m_stVictimInfo { get; set; }

        public void SetLoadedPayload(string[] aLoadedPayload) => m_aLoadedPayload = aLoadedPayload;

        public struct stVictimInfo
        {
            public string ID;
            public string IPAddr;
            public string Hostname;
            public string Username;
            public string OS;
            public int uid;
            public bool isRoot;
            public bool ExistsMonitor;
            public bool ExistsWebcam;
            public float Ping;
            public float CPU;
        }

        public Victim()
        {

        }

        public Victim(stVictimInfo st)
        {
            m_stVictimInfo = st;
        }
    }
}
