using System;
using System.Collections.Generic;
using System.Text;
using Lidgren.Network;
using System.Threading;
using System.Diagnostics;
using System.IO;

namespace DurableClient
{
	class Program
	{
		static void Main(string[] args)
		{
			NetConfiguration config = new NetConfiguration("durable");
			NetClient client = new NetClient(config);

			client.SimulatedMinimumLatency = 0.05f;
			client.SimulatedLatencyVariance = 0.025f;
			client.SimulatedLoss = 0.03f;

			// wait half a second to allow server to start up in Visual Studio
			Thread.Sleep(500);

			// create a buffer to read data into
			NetBuffer buffer = client.CreateBuffer();

			// connect to localhost
			client.Connect("127.0.0.1", 14242, Encoding.ASCII.GetBytes("Hail from client"));

			// enable some library messages
			client.SetMessageTypeEnabled(NetMessageType.BadMessageReceived, true);
			//client.SetMessageTypeEnabled(NetMessageType.VerboseDebugMessage, true);
			client.SetMessageTypeEnabled(NetMessageType.ConnectionRejected, true);

			FileStream fs = new FileStream("./clientlog.txt", FileMode.Create, FileAccess.Write, FileShare.Read);
			StreamWriter wrt = new StreamWriter(fs);
			Output(wrt, "Log started at " + DateTime.Now);
			wrt.Flush();

			// create a stopwatch
			Stopwatch sw = new Stopwatch();
			sw.Start();
			int loops = 0;

			while (!Console.KeyAvailable)
			{
				NetMessageType type;
				if (client.ReadMessage(buffer, out type))
				{
					switch (type)
					{
						case NetMessageType.StatusChanged:
							string statusMessage = buffer.ReadString();
							NetConnectionStatus newStatus = (NetConnectionStatus)buffer.ReadByte();
							if (client.ServerConnection.RemoteHailData != null)
								Output(wrt, "New status: " + newStatus + " (" + statusMessage + ") Remote hail is: " + Encoding.ASCII.GetString(client.ServerConnection.RemoteHailData));
							else
								Output(wrt, "New status: " + newStatus + " (" + statusMessage + ") Remote hail hasn't arrived.");
							break;
						case NetMessageType.BadMessageReceived:
						case NetMessageType.ConnectionRejected:
						case NetMessageType.DebugMessage:
						case NetMessageType.VerboseDebugMessage:
							//
							// These types of messages all contain a string in the buffer; display it.
							//
							Output(wrt, buffer.ReadString());
							break;
						case NetMessageType.Data:
						default:
							//
							// For this application; server doesn't send any data... so Data messages are unhandled
							//
							Output(wrt, "Unhandled: " + type + " " + buffer.ToString());
							break;
					}
				}

				// send a message every second
				if (client.Status == NetConnectionStatus.Connected && sw.Elapsed.TotalMilliseconds >= 516)
				{
					loops++;
					//Console.WriteLine("Sending message #" + loops);
					Console.Title = "Client; Messages sent: " + loops;

					Output(wrt, "Sending #" + loops + " at " + NetTime.ToMillis(NetTime.Now));
					NetBuffer send = client.CreateBuffer();
					send.Write("Message #" + loops);
					client.SendMessage(send, NetChannel.ReliableInOrder14);

					sw.Reset();
					sw.Start();
				}

				Thread.Sleep(1);
			}

			// clean shutdown
			client.Shutdown("Application exiting");
			wrt.Close();
		}

		private static void Output(StreamWriter wrt, string str)
		{
			Console.WriteLine(str);
			wrt.WriteLine(str);
		}
	}
}