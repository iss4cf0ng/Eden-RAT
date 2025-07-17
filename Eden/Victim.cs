using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eden
{
    public class Victim
    {
        public stVictimInfo m_stVictimInfo { get; set; }
        public Client m_clnt { get { return m_stVictimInfo.clnt; } }
        public string m_szID { get { return m_stVictimInfo.ID; } } 
        public string m_szDirectory { get { return m_stVictimInfo.VictimDirectory; } }

        public string[] m_aLoadedPayload { get; set; }

        public void SetLoadedPayload(string[] aLoadedPayload) => m_aLoadedPayload = aLoadedPayload;

        public struct stVictimInfo
        {
            public Client clnt;

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

            public string VictimDirectory;
        }

        public Victim(stVictimInfo st)
        {
            m_stVictimInfo = st;
        }

        public void fnSendCommand(string szMsg) => m_stVictimInfo.clnt.SendVictim(m_szID, szMsg);
    }
}
