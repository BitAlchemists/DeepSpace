using System;
using System.Collections.Generic;
using System.Text;

using Lidgren.Network;
using System.Diagnostics;
using System.Net;

namespace UnitTests
{
	class Program
	{
		static unsafe void Main(string[] args)
		{
			// JIT stuff
			NetBuffer msg = new NetBuffer(20);
			msg.Write((short)short.MaxValue);
			
			// Go
			double timeStart = NetTime.Now;

			msg = new NetBuffer(20);
			for (int n = 0; n < 10000; n++)
			{
				msg.Reset();

				msg.Write((short)short.MaxValue);
				msg.Write((short)short.MinValue);
				msg.Write((short)-42);

				msg.Write(421);
				msg.Write((byte)7);
				msg.Write(-42.8f);

				if (msg.LengthBytes != 15)
					throw new Exception("Bad message length");

				msg.Write("duke of earl");

				int bytesWritten;
				bytesWritten = msg.WriteVariableInt32(-1);
				bytesWritten = msg.WriteVariableInt32(5);
				bytesWritten = msg.WriteVariableInt32(-18);
				bytesWritten = msg.WriteVariableInt32(42);
				bytesWritten = msg.WriteVariableInt32(-420);

				msg.Write((uint)9991);

				// byte boundary kept until here

				msg.Write(true);
				msg.Write((uint)3, 5);
				msg.Write(8.111f);
				msg.Write("again");
				byte[] arr = new byte[] { 1, 6, 12, 24 };
				msg.Write(arr);
				msg.Write((byte)7, 7);
				msg.Write(Int32.MinValue);
				msg.Write(UInt32.MaxValue);
				msg.WriteRangedSingle(21.0f, -10, 50, 12);

				// test reduced bit signed writing
				msg.Write(15, 5);
				msg.Write(2, 5);
				msg.Write(0, 5);
				msg.Write(-1, 5);
				msg.Write(-2, 5);
				msg.Write(-15, 5);

				msg.Write(UInt64.MaxValue);
				msg.Write(Int64.MaxValue);
				msg.Write(Int64.MinValue);

				msg.Write(42);
				msg.WritePadBits();

				int numBits = msg.WriteRangedInteger(0, 10, 5);
				if (numBits != 4)
					throw new Exception("Ack WriteRangedInteger failed");

				// verify
				msg.Position = 0;

				short a = msg.ReadInt16();
				short b = msg.ReadInt16();
				short c = msg.ReadInt16();

				if (a != short.MaxValue || b != short.MinValue || c != -42)
					throw new Exception("Ack thpth short failed");

				if (msg.ReadInt32() != 421)
					throw new Exception("Ack thphth 1");
				if (msg.ReadByte() != (byte)7)
					throw new Exception("Ack thphth 2");
				if (msg.ReadSingle() != -42.8f)
					throw new Exception("Ack thphth 3");
				if (msg.ReadString() != "duke of earl")
					throw new Exception("Ack thphth 4");

				if (msg.ReadVariableInt32() != -1) throw new Exception("ReadVariableInt32 failed 1");
				if (msg.ReadVariableInt32() != 5) throw new Exception("ReadVariableInt32 failed 2");
				if (msg.ReadVariableInt32() != -18) throw new Exception("ReadVariableInt32 failed 3");
				if (msg.ReadVariableInt32() != 42) throw new Exception("ReadVariableInt32 failed 4");
				if (msg.ReadVariableInt32() != -420) throw new Exception("ReadVariableInt32 failed 5");

				if (msg.ReadUInt32() != 9991)
					throw new Exception("Ack thphth 4.5");

				if (msg.ReadBoolean() != true)
					throw new Exception("Ack thphth 5");
				if (msg.ReadUInt32(5) != (uint)3)
					throw new Exception("Ack thphth 6");
				if (msg.ReadSingle() != 8.111f)
					throw new Exception("Ack thphth 7");
				if (msg.ReadString() != "again")
					throw new Exception("Ack thphth 8");
				byte[] rrr = msg.ReadBytes(4);
				if (rrr[0] != arr[0] || rrr[1] != arr[1] || rrr[2] != arr[2] || rrr[3] != arr[3])
					throw new Exception("Ack thphth 9");
				if (msg.ReadByte(7) != 7)
					throw new Exception("Ack thphth 10");
				if (msg.ReadInt32() != Int32.MinValue)
					throw new Exception("Ack thphth 11");
				if (msg.ReadUInt32() != UInt32.MaxValue)
					throw new Exception("Ack thphth 12");

				float v = msg.ReadRangedSingle(-10, 50, 12);
				// v should be close to, but not necessarily exactly, 21.0f
				if ((float)Math.Abs(21.0f - v) > 0.1f)
					throw new Exception("Ack thphth *RangedSingle() failed");

				if (msg.ReadInt32(5) != 15)
					throw new Exception("Ack thphth ReadInt32 1");
				if (msg.ReadInt32(5) != 2)
					throw new Exception("Ack thphth ReadInt32 2");
				if (msg.ReadInt32(5) != 0)
					throw new Exception("Ack thphth ReadInt32 3");
				if (msg.ReadInt32(5) != -1)
					throw new Exception("Ack thphth ReadInt32 4");
				if (msg.ReadInt32(5) != -2)
					throw new Exception("Ack thphth ReadInt32 5");
				if (msg.ReadInt32(5) != -15)
					throw new Exception("Ack thphth ReadInt32 6");

				UInt64 longVal = msg.ReadUInt64();
				if (longVal != UInt64.MaxValue)
					throw new Exception("Ack thphth UInt64");
				if (msg.ReadInt64() != Int64.MaxValue)
					throw new Exception("Ack thphth Int64");
				if (msg.ReadInt64() != Int64.MinValue)
					throw new Exception("Ack thphth Int64");

				if (msg.ReadInt32() != 42)
					throw new Exception("Ack thphth end");

				msg.SkipPadBits();

				if (msg.ReadRangedInteger(0, 10) != 5)
					throw new Exception("Ack thphth ranged integer");
			}

			// test writevariableuint64
			NetBuffer largeBuffer = new NetBuffer(100 * 8);
			UInt64[] largeNumbers = new ulong[100];
			for (int i = 0; i < 100; i++)
			{
				largeNumbers[i] = NetRandom.Instance.NextUInt() << 32;
				largeNumbers[i] |= NetRandom.Instance.NextUInt();
				largeBuffer.WriteVariableUInt64(largeNumbers[i]);
			}

			largeBuffer.Position = 0;
			for (int i = 0; i < 100; i++)
			{
				UInt64 ln = largeBuffer.ReadVariableUInt64();
				if (ln != largeNumbers[i])
					throw new Exception("large fail");
			}

			//
			// Extended tests on padbits
			//
			for (int i = 1; i < 31; i++)
			{
				NetBuffer buf = new NetBuffer();
				buf.Write((int)1, i);

				if (buf.LengthBits != i)
					throw new Exception("Bad length!");

				buf.WritePadBits();
				int wholeBytes = buf.LengthBits / 8;
				if (wholeBytes * 8 != buf.LengthBits)
					throw new Exception("WritePadBits failed! Length is " + buf.LengthBits);
			}

			NetBuffer small = new NetBuffer(100);
			byte[] rnd = new byte[24];
			int[] bits = new int[24];
			for (int i = 0; i < 24; i++)
			{
				rnd[i] = (byte)NetRandom.Instance.Next(0, 65);
				bits[i] = NetUtility.BitsToHoldUInt((uint)rnd[i]);

				small.Write(rnd[i], bits[i]);
			}

			small.Position = 0;
			for (int i = 0; i < 24; i++)
			{
				byte got = small.ReadByte(bits[i]);
				if (got != rnd[i])
					throw new Exception("Failed small allocation test");
			}

			double timeEnd = NetTime.Now;
			double timeSpan = timeEnd - timeStart;

			Console.WriteLine("Trivial tests passed in " + (timeSpan * 1000.0) + " milliseconds");

			Console.WriteLine("Creating client and server for live testing...");

			NetConfiguration config = new NetConfiguration("unittest");
			config.Port = 14242;
			NetServer server = new NetServer(config);
			NetBuffer serverBuffer = new NetBuffer();
			server.Start();

			config = new NetConfiguration("unittest");
			NetClient client = new NetClient(config);
			client.SetMessageTypeEnabled(NetMessageType.Receipt, true);
			NetBuffer clientBuffer = client.CreateBuffer();
			client.Start();

			client.Connect("127.0.0.1", 14242);

			List<string> events = new List<string>();

			double end = double.MaxValue;
			double disconnect = double.MaxValue;

			while (NetTime.Now < end)
			{
				double now = NetTime.Now;

				NetMessageType nmt;
				NetConnection sender;

				//
				// client
				//
				if (client.ReadMessage(clientBuffer, out nmt))
				{
					switch (nmt)
					{
						case NetMessageType.StatusChanged:
							Console.WriteLine("Client: " + client.Status + " (" + clientBuffer.ReadString() + ")");
							events.Add("CStatus " + client.Status);
							if (client.Status == NetConnectionStatus.Connected)
							{
								// send reliable message
								NetBuffer buf = client.CreateBuffer();
								buf.Write(true);
								buf.Write((int)52, 7);
								buf.Write("Hallon");

								client.SendMessage(buf, NetChannel.ReliableInOrder1, new NetBuffer("kokos"));
							}

							if (client.Status == NetConnectionStatus.Disconnected)
								end = NetTime.Now + 1.0; // end in one second

							break;
						case NetMessageType.Receipt:
							events.Add("CReceipt " + clientBuffer.ReadString());
							break;
						case NetMessageType.ConnectionRejected:
						case NetMessageType.BadMessageReceived:
							throw new Exception("Failed: " + nmt);
						case NetMessageType.DebugMessage:
							// silently ignore
							break;
						default:
							// ignore
							Console.WriteLine("Ignored: " + nmt);
							break;
					}
				}

				//
				// server
				//
				if (server.ReadMessage(serverBuffer, out nmt, out sender))
				{
					switch (nmt)
					{
						case NetMessageType.StatusChanged:
							events.Add("SStatus " + sender.Status);
							Console.WriteLine("Server: " + sender.Status + " (" + serverBuffer.ReadString() + ")");
							break;
						case NetMessageType.ConnectionRejected:
						case NetMessageType.BadMessageReceived:
							throw new Exception("Failed: " + nmt);
						case NetMessageType.Data:
							events.Add("DataRec " + serverBuffer.LengthBits);
							bool shouldBeTrue = serverBuffer.ReadBoolean();
							int shouldBeFifthTwo = serverBuffer.ReadInt32(7);
							string shouldBeHallon = serverBuffer.ReadString();

							if (shouldBeTrue != true ||
								shouldBeFifthTwo != 52 ||
								shouldBeHallon != "Hallon")
								throw new Exception("Bad data transmission");

							disconnect = now + 1.0;
							break;
						case NetMessageType.DebugMessage:
							// silently ignore
							break;
						default:
							// ignore
							Console.WriteLine("Ignored: " + nmt);
							break;
					}
				}

				if (now > disconnect)
				{
					server.Connections[0].Disconnect("Bye", 0.1f);
					disconnect = double.MaxValue;
				}
			}

			// verify events
			string[] expected = new string[] {
				"CStatus Connecting",
				"SStatus Connecting",
				"CStatus Connected",
				"SStatus Connected",
				"DataRec 64",
				"CReceipt kokos",
				"SStatus Disconnecting",
				"CStatus Disconnecting",
				"SStatus Disconnected",
				"CStatus Disconnected"
			};

			if (events.Count != expected.Length)
				throw new Exception("Mismatch in events count! Expected " + expected.Length + ", got " + events.Count);

			for(int i=0;i<expected.Length;i++)
			{
				if (events[i] != expected[i])
					throw new Exception("Event " + i + " (" + expected[i] + ") mismatched!");
			}

			Console.WriteLine("All tests successful");

			Console.ReadKey();

			server.Shutdown("App exiting");
			client.Shutdown("App exiting");
		}
	}
}
