using System;
using System.Management.Automation;
using System.IO;
using System.Threading;
using Renci.SshNet;
using Renci.SshNet.Common;
using System.Net.Sockets;
using System.Text;
using System.Runtime.InteropServices;
using System.Security;

namespace Cisco
{
    enum Verbs
    {
        WILL = 251,
        WONT = 252,
        DO = 253,
        DONT = 254,
        IAC = 255
    }

    enum Options
    {
        SGA = 3
    }

    class Telnet
    {
        //**************************************** Class Attributes
        #region Class Attributes
        TcpClient tcpSocket;
        int TimeOutMs = 100;
        #endregion
        //**************************************** Methods
        #region Methods
        public Telnet(string Hostname, int Port)
        {
            tcpSocket = new TcpClient(Hostname, Port);
        }

        public string Login(string Username, string Password, int LoginTimeOutMs)
        {
            int oldTimeOutMs = TimeOutMs;
            TimeOutMs = LoginTimeOutMs;
            string s = Read();
            if (!s.TrimEnd().EndsWith(":"))
                throw new Exception("Failed to connect : no login prompt");
            WriteLine(Username);

            s += Read();
            if (!s.TrimEnd().EndsWith(":"))
                throw new Exception("Failed to connect : no password prompt");
            WriteLine(Password);

            s += Read();
            TimeOutMs = oldTimeOutMs;
            return s;
        }

        public void WriteLine(string cmd)
        {
            Write(cmd + "\n");
        }

        public void Write(string cmd)
        {
            if (!tcpSocket.Connected) return;
            byte[] buf = System.Text.ASCIIEncoding.ASCII.GetBytes(cmd.Replace("\0xFF", "\0xFF\0xFF"));
            tcpSocket.GetStream().Write(buf, 0, buf.Length);
        }

        public string Read()
        {
            if (!tcpSocket.Connected) return null;
            StringBuilder sb = new StringBuilder();
            do
            {
                ParseTelnet(sb);
                System.Threading.Thread.Sleep(TimeOutMs);
            } while (tcpSocket.Available > 0);
            return sb.ToString();
        }

        public bool IsConnected
        {
            get { return tcpSocket.Connected; }
        }

        public void Disconnect()
        {
            tcpSocket.Close();
        }

        void ParseTelnet(StringBuilder sb)
        {
            while (tcpSocket.Available > 0)
            {
                int input = tcpSocket.GetStream().ReadByte();
                switch (input)
                {
                    case -1:
                        break;
                    case (int)Verbs.IAC:
                        // interpret as command
                        int inputverb = tcpSocket.GetStream().ReadByte();
                        if (inputverb == -1) break;
                        switch (inputverb)
                        {
                            case (int)Verbs.IAC:
                                //literal IAC = 255 escaped, so append char 255 to string
                                sb.Append(inputverb);
                                break;
                            case (int)Verbs.DO:
                            case (int)Verbs.DONT:
                            case (int)Verbs.WILL:
                            case (int)Verbs.WONT:
                                // reply to all commands with "WONT", unless it is SGA (suppres go ahead)
                                int inputoption = tcpSocket.GetStream().ReadByte();
                                if (inputoption == -1) break;
                                tcpSocket.GetStream().WriteByte((byte)Verbs.IAC);
                                if (inputoption == (int)Options.SGA)
                                    tcpSocket.GetStream().WriteByte(inputverb == (int)Verbs.DO ? (byte)Verbs.WILL : (byte)Verbs.DO);
                                else
                                    tcpSocket.GetStream().WriteByte(inputverb == (int)Verbs.DO ? (byte)Verbs.WONT : (byte)Verbs.DONT);
                                tcpSocket.GetStream().WriteByte((byte)inputoption);
                                break;
                            default:
                                break;
                        }
                        break;
                    default:
                        sb.Append((char)input);
                        break;
                }
            }

        }
        #endregion
    }

    public class MyCisco : Cmdlet
    {
        public string LogCategory { get; private set; } = "Cisco";
        public string RunCiscoCommand(string deviceName, string connectionType, string command, PSCredential credential = null,
                              int delay = 1000, string terminalName = "", uint column = 100, uint row = 200, int buff = 2048)
        {
            StringReader strReader = new StringReader(command);
            string result = "";
            if (connectionType == "SSH")
            {
                SshClient device = new SshClient(deviceName, credential.UserName, SecureStringToString(credential.Password));
                device.HostKeyReceived += delegate (object sender, HostKeyEventArgs e)
                {
                    e.CanTrust = true;
                };
                device.Connect();

                ShellStream Stream = device.CreateShellStream(terminalName, column, row, column, row, buff);
                while (true)
                {
                    string line = strReader.ReadLine();
                    if (line != null)
                    {
                        result += Stream.ReadLine();
                        Stream.WriteLine(line);
                        Thread.Sleep(delay);
                    }
                    else
                        break;
                }
                result += Stream.Read();
                device.Disconnect();
                result = result.Substring(result.IndexOf(System.Environment.NewLine) + System.Environment.NewLine.Length);
                result = result.Remove(result.TrimEnd().LastIndexOf(Environment.NewLine));
                return result;
            }
            else if (connectionType == "Telnet")
            {
                Telnet telnetClient = new Telnet(deviceName, 23);
                telnetClient.Login(credential.UserName, SecureStringToString(credential.Password), 100);
                while (true)
                {
                    string temp = "";
                    string line = strReader.ReadLine();
                    if (line != null)
                    {
                        telnetClient.WriteLine(line);
                        Thread.Sleep(delay);
                        temp = telnetClient.Read();
                        result += temp;
                        if (temp.IndexOf("--More--") > 0)
                        {
                            do
                            {
                                telnetClient.Write(" ");
                                temp = telnetClient.Read();
                                result += temp;
                                if (temp.IndexOf("--More--") < 0)
                                    break;
                            } while (true);
                        }
                    }
                    else
                        break;
                }
                telnetClient.Disconnect();
                if (result.Split('\n').Length > 2)
                {
                    result = result.TrimStart();
                    result = result.TrimEnd();
                    result = result.Substring(result.IndexOf(Environment.NewLine) + Environment.NewLine.Length);
                    result = result.Remove(result.LastIndexOf(Environment.NewLine));
                }
                else
                    result = null;
                return result;
            }
            return null;
        }

        public string SecureStringToString(SecureString value)
        {
            IntPtr valuePtr = IntPtr.Zero;
            try
            {
                valuePtr = Marshal.SecureStringToGlobalAllocUnicode(value);
                return Marshal.PtrToStringUni(valuePtr);
            }
            finally
            {
                Marshal.ZeroFreeGlobalAllocUnicode(valuePtr);
            }
        }
    }


}
