using System;
using System.Collections.Generic;
using System.Text;
using Lidgren.Network;
using System.Threading;
using System.IO;

namespace DurableServer
{
	class Program
	{
		static void Main(string[] args)
		{
			NetConfiguration config = new NetConfiguration("durable");
			config.MaxConnections = 128;
			config.Port = 14242;
			NetServer server = new MyDurableServer(config);

			server.SetMessageTypeEnabled(NetMessageType.ConnectionApproval, true);
			server.SetMessageTypeEnabled(NetMessageType.DebugMessage, true);
			//server.SetMessageTypeEnabled(NetMessageType.VerboseDebugMessage, true);
			server.SetMessageTypeEnabled(NetMessageType.StatusChanged, true);

			server.SimulatedMinimumLatency = 0.05f;
			server.SimulatedLatencyVariance = 0.025f;
			server.SimulatedLoss = 0.03f;

			server.Start();

			FileStream fs = new FileStream("./serverlog.txt", FileMode.Create, FileAccess.Write, FileShare.Read);
			StreamWriter wrt = new StreamWriter(fs);
			Output(wrt, "Log started at " + DateTime.Now);
			wrt.Flush();

			NetBuffer buffer = server.CreateBuffer();

			int expected = 1;

			Console.WriteLine("Press any key to quit");
			while (!Console.KeyAvailable)
			{
				NetMessageType type;
				NetConnection sender;
				if (server.ReadMessage(buffer, out type, out sender))
				{
					switch (type)
					{
						case NetMessageType.StatusChanged:
							string statusMessage = buffer.ReadString();
							NetConnectionStatus newStatus = (NetConnectionStatus)buffer.ReadByte();
							if (sender.RemoteHailData != null)
								Output(wrt, "New status: " + newStatus + " (" + statusMessage + ") Remote hail is: " + Encoding.ASCII.GetString(sender.RemoteHailData));
							else
								Output(wrt, "New status: " + newStatus + " (" + statusMessage + ") Remote hail hasn't arrived.");
							break;
						case NetMessageType.BadMessageReceived:
						case NetMessageType.ConnectionRejected:
						case NetMessageType.DebugMessage:
							//
							// All these types of messages all contain a single string in the buffer; display it
							//
							Output(wrt, buffer.ReadString());
							break;
						case NetMessageType.VerboseDebugMessage:
							wrt.WriteLine(buffer.ReadString()); // don't output to console
							break;
						case NetMessageType.ConnectionApproval:
							if (sender.RemoteHailData != null &&
								Encoding.ASCII.GetString(sender.RemoteHailData) == "Hail from client")
							{
								Output(wrt, "Hail ok!");
								sender.Approve(Encoding.ASCII.GetBytes("Hail from server"));
							}
							else
							{
								sender.Disapprove("Wrong hail!");
							}
							break;
						case NetMessageType.Data:
							
							// verify ProcessReceived has done its work
							int len = (int)buffer.Tag;
							if (len != buffer.LengthBytes)
								Output(wrt, "OUCH! ProcessReceived hasn't done its job!");

							string str = buffer.ReadString();

							// parse it
							int nr = Int32.Parse(str.Substring(9));

							if (nr != expected)
							{
								Output(wrt, "Warning! Expected " + expected + "; received " + nr + " str is ---" + str + "---");
							}
							else
							{
								expected++;
								Console.Title = "Server; received " + nr + " messages";
							}

							break;
						default:
							Output(wrt, "Unhandled: " + type + " " + buffer.ToString());
							break;
					}
				}

				// we're not doing anything but reading; to suspend this thread until there's something to read
				server.DataReceivedEvent.WaitOne(1000);
			}

			// clean shutdown
			wrt.Close();
			server.Shutdown("Application exiting");
			System.Threading.Thread.Sleep(500); // give network thread time to exit
		}

		private static void Output(StreamWriter wrt, string str)
		{
			Console.WriteLine(str);
			wrt.WriteLine(str);
		}
	}

	public class MyDurableServer : NetServer
	{
		public MyDurableServer(NetConfiguration config)
			: base(config)
		{
		}

		public override void ProcessReceived(NetBuffer buffer)
		{
			//
			// This is run on the networking thread; so there are lots of things we can't do without proper locking
			// A real application can do some processing here and store the result in the NetBuffer.Tag
			// For now; just look at the length of the buffer
			//
			buffer.Tag = (object)buffer.LengthBytes;
		}
	}
}
