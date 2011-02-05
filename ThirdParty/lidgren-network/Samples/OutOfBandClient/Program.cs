using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Lidgren.Network;
using SamplesCommon;
using System.Threading;
using System.Net;

namespace OutOfBandClient
{
	static class Program
	{
		private static Form1 s_mainForm;
		private static NetClient s_client;
		private static NetBuffer s_readBuffer;

		[STAThread]
		static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			s_mainForm = new Form1();

			NetConfiguration config = new NetConfiguration("OoBSample");
			s_client = new NetClient(config);
			s_client.SetMessageTypeEnabled(NetMessageType.OutOfBandData, true);
			s_client.Start();

			s_readBuffer = s_client.CreateBuffer();

			Application.Idle += new EventHandler(OnAppIdle);
			Application.Run(s_mainForm);

			s_client.Shutdown("Bye");
		}

		static void OnAppIdle(object sender, EventArgs e)
		{
			while (NativeMethods.AppStillIdle)
			{
				// NetConnection conn;
				NetMessageType tp;
				IPEndPoint ep;
				while (s_client.ReadMessage(s_readBuffer, out tp, out ep))
				{
					switch (tp)
					{
						case NetMessageType.DebugMessage:
							NativeMethods.AppendText(s_mainForm.richTextBox1, "Debug message: " + s_readBuffer.ReadString());
							break;
						default:
							NativeMethods.AppendText(s_mainForm.richTextBox1, "Unhandled type: " + tp);
							break;
					}
				}
				Thread.Sleep(1);
			}
		}

		public static void SendMessage(string str)
		{
			//
			// Send Out-of-band message to 127.0.0.1 14242 (hardcoded)
			//
			// Notice: There is no connection in place at this point,
			// trying to use SendMessage() will throw an exception
			//

			NetBuffer buf = s_client.CreateBuffer();
			buf.Write(str);

			IPEndPoint ep = new IPEndPoint(NetUtility.Resolve("127.0.0.1"), 14242);
			s_client.SendOutOfBandMessage(buf, ep);
		}
	}
}
