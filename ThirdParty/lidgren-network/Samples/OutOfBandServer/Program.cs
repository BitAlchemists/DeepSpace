using System;
using System.Collections.Generic;
using System.Windows.Forms;

using Lidgren.Network;
using SamplesCommon;
using System.Threading;
using System.Net;

namespace OutOfBandServer
{
	static class Program
	{
		private static Form1 s_mainForm;
		private static NetServer s_server;
		private static NetBuffer s_readBuffer;

		[STAThread]
		static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			s_mainForm = new Form1();

			NetConfiguration config = new NetConfiguration("OoBSample");
			config.MaxConnections = 0; // we accept only OoB data
			config.Port = 14242;
			s_server = new NetServer(config);
			s_server.SetMessageTypeEnabled(NetMessageType.OutOfBandData, true);
			s_server.Start();

			s_readBuffer = s_server.CreateBuffer();

			Application.Idle += new EventHandler(OnAppIdle);
			Application.Run(s_mainForm);

			s_server.Shutdown("Bye");
		}

		static void OnAppIdle(object sender, EventArgs e)
		{
			while (NativeMethods.AppStillIdle)
			{
				NetConnection conn;
				NetMessageType tp;
				IPEndPoint ep;
				while (s_server.ReadMessage(s_readBuffer, out tp, out conn, out ep))
				{
					if (s_readBuffer.LengthBytes < 1)
						continue; // no data
					switch (tp)
					{
						case NetMessageType.OutOfBandData:
							NativeMethods.AppendText(s_mainForm.richTextBox1, "Received message: " + s_readBuffer.ReadString() + " from " + ep);
							break;
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
	}
}