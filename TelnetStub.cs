using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.IO;
using System.Threading ;

namespace objActiveSolutions
{
	/// <summary>
	/// Telnet 模块
	/// </summary>
	public class TelnetStub
	{
		private IPEndPoint iep ;
		private string address ;
		private int port ;
		private Socket s ;
		Byte[] m_byBuff = new Byte[32767];// 接受到的数据
		private string strFullLog = "";
        private string sendMsg = ""; // 当前发出的指令
        private string reciveMsgForOneCommand = "";// 完整的一条指令回调【包含了发出的指令】

        /// <summary>
        /// 创建一个Telenet连接
        /// </summary>
        /// <param name="Address">IP地址</param>
        /// <param name="Port">端口</param>
		public TelnetStub(string Address, int Port)
		{
			address = Address;
			port = Port;
		}
		
        /// <summary>
        /// 处理 指令回调的 msg
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
                {// 如果包含这个， 则代表当前指令 接受消息已经结束
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
        /// 发送数据---》 第二步: 压缩数据 ---> 发送
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
                Console.WriteLine("发送数据【第二步】失败, 请检查");
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
		/// 连接到telenet服务器
		/// </summary>
		/// <returns>True 表示链接成功, False 表示链接失败</returns>
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
                Console.WriteLine("连接Telenet服务器失败!\n" + ex.Message);
				return false;
			}
			
		}

		/// <summary>
        /// 断开Telenet服务器的连接
        /// </summary>
		public void Disconnect()
		{
			if (s.Connected) s.Close();
		}

		/// <summary>
		/// 发送数据---》 第一步: 记录log
		/// </summary>
		/// <param name="Message">The message to send to the server</param>
		public void SendMessage(string Message)
		{
            sendMsg = Message;
            strFullLog += "\r\nSENDING DATA ====> " + Message.ToUpper() + "\r\n";

			DoSend(Message + "\r");
		}

		/// <summary>
		/// 当前会话的Log
		/// </summary>
		public string SessionLog
		{
			get 
			{
				return strFullLog;
			}
		}


		/// <summary>
		/// 清除当前会话的Log
		/// </summary>
		public void ClearSessionLog()
		{
			strFullLog = "";
		}

	}
}

