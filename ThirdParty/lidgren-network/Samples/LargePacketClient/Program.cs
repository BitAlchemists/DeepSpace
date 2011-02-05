using System;
using System.Collections.Generic;
using System.Windows.Forms;

using Lidgren.Network;
using SamplesCommon;
using System.Net;
using System.Diagnostics;

namespace LargePacketClient
{
	static class Program
	{
		private static Form1 m_mainForm;
		private static NetClient m_client;
		private static NetBuffer m_readBuffer;
		private static int m_nextSize;
		private static double s_nextDisplay;

		[STAThread]
		static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			m_mainForm = new Form1();

			NetConfiguration config = new NetConfiguration("largepacket");
			config.SendBufferSize = 128000;
			config.ThrottleBytesPerSecond = 8192;
			m_client = new NetClient(config);
			m_client.SimulatedLoss = 0.03f; // 3 %
			m_client.SimulatedMinimumLatency = 0.1f; // 100 ms
			m_client.SimulatedLatencyVariance = 0.05f; // 100-150 ms actually

			//m_client.SetMessageTypeEnabled(NetMessageType.VerboseDebugMessage, true);
			m_client.SetMessageTypeEnabled(NetMessageType.Receipt, true);

			m_readBuffer = m_client.CreateBuffer();

			Application.Idle += new EventHandler(OnAppIdle);
			Application.Run(m_mainForm);

			m_client.Shutdown("Application exiting");
		}

		static void OnAppIdle(object sender, EventArgs e)
		{
			while (NativeMethods.AppStillIdle)
			{
				NetMessageType type;
				if (m_client.ReadMessage(m_readBuffer, out type))
				{
					switch (type)
					{
						case NetMessageType.ServerDiscovered:
							IPEndPoint ep = m_readBuffer.ReadIPEndPoint();
							m_client.Connect(ep);
							break;
						case NetMessageType.Receipt:
							NativeMethods.AppendText(m_mainForm.richTextBox1, "Got receipt for packet sized " + m_readBuffer.ReadInt32());
							if (m_client.Status == NetConnectionStatus.Connected)
							{
								m_nextSize *= 2;
								if (m_nextSize > 1000000)
								{
									// 1 meg message is enough
									NativeMethods.AppendText(m_mainForm.richTextBox1, "Done");
									m_client.Disconnect("Done");
									return;
								}
								SendPacket();
							}
							break;
						case NetMessageType.VerboseDebugMessage:
						case NetMessageType.DebugMessage:
						case NetMessageType.BadMessageReceived:
							NativeMethods.AppendText(m_mainForm.richTextBox1, m_readBuffer.ReadString());
							break;
						case NetMessageType.StatusChanged:
							if (m_client.Status == NetConnectionStatus.Connected)
							{
								m_nextSize = 8;
								SendPacket();
							}
							break;
					}
				}

				if (m_client != null && m_client.ServerConnection != null)
					m_mainForm.Text = m_client.ServerConnection.Statistics.CurrentlyUnsentMessagesCount + " unsent messages";

				if (NetTime.Now > s_nextDisplay)
				{
					m_mainForm.label1.Text = m_client.GetStatisticsString(m_client.ServerConnection);
					s_nextDisplay = NetTime.Now + 0.2; // five times per second
				}

				System.Threading.Thread.Sleep(1);
			}
		}

		private static void SendPacket()
		{
			NetBuffer buf = m_client.CreateBuffer();
			buf.EnsureBufferSize(m_nextSize * 8);

			int cnt = m_nextSize / 4;
			for (int i = 0; i < cnt; i++)
				buf.Write(i);

			NativeMethods.AppendText(m_mainForm.richTextBox1, "Sending " + m_nextSize + " byte packet");

			// any receipt data will do
			NetBuffer receipt = new NetBuffer(4);
			receipt.Write(m_nextSize);
			m_client.SendMessage(buf, NetChannel.ReliableInOrder4, receipt);
		}

		internal static void Start()
		{
			m_client.DiscoverLocalServers(14242);
		}

		internal static void Disconnect()
		{
			m_client.Disconnect("Bye!");
		}
	}
}