using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.IO;
using System.Threading ;

namespace objActiveSolutions
{
	/// <summary>
	/// Telnet ģ��
	/// </summary>
	public class TelnetStub
	{
		private IPEndPoint iep ;
		private string address ;
		private int port ;
		private Socket s ;
		Byte[] m_byBuff = new Byte[32767];// ���ܵ�������
		private string strFullLog = "";
        private string sendMsg = ""; // ��ǰ������ָ��
        private string reciveMsgForOneCommand = "";// ������һ��ָ��ص��������˷�����ָ�

        /// <summary>
        /// ����һ��Telenet����
        /// </summary>
        /// <param name="Address">IP��ַ</param>
        /// <param name="Port">�˿�</param>
		public TelnetStub(string Address, int Port)
		{
			address = Address;
			port = Port;
		}
		
        /// <summary>
        /// ���� ָ��ص��� msg
        /// </summary>
        /// <param name="ar"></param>
		private void OnRecievedData( IAsyncResult ar )
		{
			// Get The connection socket from the callback
			Socket sock = (Socket)ar.AsyncState;

			// Get The data , if any
			int nBytesRec = sock.EndReceive( ar );

            if( nBytesRec > 0 )
			{
                // Decode the received data
                string sRecieved = Encoding.ASCII.GetString(m_byBuff, 0, nBytesRec);
				//Console.Write(sRecieved);
                reciveMsgForOneCommand += sRecieved;
                if (reciveMsgForOneCommand.Contains(">>>"))
                {// ������������ �����ǰָ�� ������Ϣ�Ѿ�����
                    if(reciveMsgForOneCommand.StartsWith(sendMsg))
                    {
                        reciveMsgForOneCommand = reciveMsgForOneCommand.Substring(Encoding.Default.GetByteCount(sendMsg + "\r\n"));
                        Console.Write(reciveMsgForOneCommand);
                        sendMsg = String.Empty;
                        reciveMsgForOneCommand = String.Empty;
                    }
                }
				strFullLog += sRecieved;

				// Launch another callback to listen for data
				AsyncCallback recieveData = new AsyncCallback(OnRecievedData);
				sock.BeginReceive( m_byBuff, 0, m_byBuff.Length, SocketFlags.None, recieveData , sock );
				
			}
			else
			{
				// If no data was recieved then the connection is probably dead
				Console.WriteLine( "Disconnected", sock.RemoteEndPoint );
				sock.Shutdown( SocketShutdown.Both );
				sock.Close();
				//Application.Exit();
			}
        }

        /// <summary>
        /// ��������---�� �ڶ���: ѹ������ ---> ����
        /// </summary>
        /// <param name="strText"></param>
        private void DoSend(string strText)
		{
			try
			{
				Byte[] smk = new Byte[strText.Length];
				for ( int i=0; i < strText.Length ; i++)
				{
					Byte ss = Convert.ToByte(strText[i]);
					smk[i] = ss ;
				}

				s.Send(smk,0 , smk.Length , SocketFlags.None);
			}
			catch(Exception ers)
			{
                //MessageBox.Show("");
                Console.WriteLine("�������ݡ��ڶ�����ʧ��, ����");
                sendMsg = "";
            }
		}


		private string CleanDisplay(string input)
		{
			
			input = input.Replace("(0x (B","|");
			input = input.Replace("(0 x(B","|");
			input = input.Replace(")0=>","");
			input = input.Replace("[0m>","");
			input = input.Replace("7[7m","[");
			input = input.Replace("[0m*8[7m","]");
			input = input.Replace("[0m","");
			return input;
		}

		/// <summary>
		/// ���ӵ�telenet������
		/// </summary>
		/// <returns>True ��ʾ���ӳɹ�, False ��ʾ����ʧ��</returns>
		public bool Connect()
		{
			IPHostEntry IPHost = Dns.GetHostEntry(address); 
			string []aliases = IPHost.Aliases; 
			IPAddress[] addr = IPHost.AddressList; 
		
			try
			{
				// Try a blocking connection to the server
				s				= new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
				iep				= new IPEndPoint(addr[0],port);  
				s.Connect(iep) ;		

				// If the connect worked, setup a callback to start listening for incoming data
				AsyncCallback recieveData = new AsyncCallback( OnRecievedData );
				s.BeginReceive( m_byBuff, 0, m_byBuff.Length, SocketFlags.None, recieveData , s );
		
				// All is good
				return true;
			}
			catch(Exception ex )
			{
                Console.WriteLine("����Telenet������ʧ��!\n" + ex.Message);
				return false;
			}
			
		}

		/// <summary>
        /// �Ͽ�Telenet������������
        /// </summary>
		public void Disconnect()
		{
			if (s.Connected) s.Close();
		}

		/// <summary>
		/// ��������---�� ��һ��: ��¼log
		/// </summary>
		/// <param name="Message">The message to send to the server</param>
		public void SendMessage(string Message)
		{
            sendMsg = Message;
            strFullLog += "\r\nSENDING DATA ====> " + Message.ToUpper() + "\r\n";

			DoSend(Message + "\r");
		}

		/// <summary>
		/// ��ǰ�Ự��Log
		/// </summary>
		public string SessionLog
		{
			get 
			{
				return strFullLog;
			}
		}


		/// <summary>
		/// �����ǰ�Ự��Log
		/// </summary>
		public void ClearSessionLog()
		{
			strFullLog = "";
		}

	}
}

