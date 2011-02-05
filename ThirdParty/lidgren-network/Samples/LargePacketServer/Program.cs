using System;
using System.Collections.Generic;
using System.Windows.Forms;
using SamplesCommon;
using Lidgren.Network;

namespace LargePacketServer
{
	static class Program
	{
		private static NetServer s_server;
		private static NetBuffer s_readBuffer;
		private static Form1 s_mainForm;
		private static double s_nextDisplay;
 
		[STAThread]
		static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			s_mainForm = new Form1();

			NetConfiguration config = new NetConfiguration("largepacket");
			config.Port = 14242;
			config.MaxConnections = 16;
			s_server = new NetServer(config);
			s_server.SimulatedLoss = 0.03f; // 3 %
			s_server.SimulatedMinimumLatency = 0.1f; // 100 ms
			s_server.SimulatedLatencyVariance = 0.05f; // 100-150 ms actually

			//m_server.SetMessageTypeEnabled(NetMessageType.VerboseDebugMessage, true);
			s_server.Start();

			s_readBuffer = s_server.CreateBuffer();

			Application.Idle += new EventHandler(OnAppIdle);
			Application.Run(s_mainForm);

			s_server.Shutdown("Application exiting");
		}

		static void OnAppIdle(object sender, EventArgs e)
		{
			while (NativeMethods.AppStillIdle)
			{
				NetMessageType type;
				NetConnection source;
				if (s_server.ReadMessage(s_readBuffer, out type, out source))
				{
					switch (type)
					{
						case NetMessageType.VerboseDebugMessage:
						case NetMessageType.DebugMessage:
						case NetMessageType.BadMessageReceived:
						case NetMessageType.StatusChanged:
							NativeMethods.AppendText(s_mainForm.richTextBox1, s_readBuffer.ReadString());
							break;
						case NetMessageType.Data:
							int cnt = s_readBuffer.LengthBytes / 4;
							if (cnt * 4 != s_readBuffer.LengthBytes)
								throw new NetException("Bad size!");

							for (int i = 0; i < cnt; i++)
							{
								int a = s_readBuffer.ReadInt32();
								if (a != i)
									throw new NetException("Bad data!");
							}

							NativeMethods.AppendText(s_mainForm.richTextBox1, "Verified " + s_readBuffer.LengthBytes + " bytes in a single message");
							
							break;
					}
				}

				if (NetTime.Now > s_nextDisplay)
				{
					if (s_server.Connections.Count > 0)
						s_mainForm.label1.Text = s_server.GetStatisticsString(s_server.Connections[0]);
					s_nextDisplay = NetTime.Now + 0.2; // five times per second
				}

				System.Threading.Thread.Sleep(1);
			}
		}
	}
}