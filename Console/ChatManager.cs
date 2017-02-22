using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Meebey.SmartIrc4net;

namespace Console
{
	public class ChatManager
	{
		private string _login;
		private readonly string _password;
		private static string _channel;

		private static IrcClient _ircClient;
		private static byte[] data;

		private NetworkStream stream;

		private object locker = new object();
		
		public ChatManager(string login, string password, string channel)
		{
			_login = login;
			_password = password;
			_channel = channel;

			int port = 80;
			TcpClient client = new TcpClient("irc.chat.twitch.tv", port);

			// Get a client stream for reading and writing.
			//  Stream stream = client.GetStream();
			stream = client.GetStream();

			connect(stream);

			Task.Run(() => readChat(stream));
		}

		private void readChat(NetworkStream stream)
		{
			lock (locker)
			{

				while (true)
				{
					// build a buffer to read the incoming TCP stream to, convert to a string

					byte[] myReadBuffer = new byte[1024];
					StringBuilder myCompleteMessage = new StringBuilder();
					int numberOfBytesRead = 0;

					// Incoming message may be larger than the buffer size.
					if(stream == null)
						continue;

					try
					{
						numberOfBytesRead = stream.Read(myReadBuffer, 0, myReadBuffer.Length);
					}
					catch (Exception e)
					{
						System.Console.WriteLine("OH SHIT SOMETHING WENT WRONG\r\n", e);
					}

					myCompleteMessage.AppendFormat("{0}", Encoding.UTF8.GetString(myReadBuffer, 0, numberOfBytesRead));


					if (numberOfBytesRead == 0)
						continue;

					// when we've received data, do Things
					
					// Print out the received message to the console.
					System.Console.WriteLine(myCompleteMessage.ToString());
					switch (myCompleteMessage.ToString())
					{
						// Every 5 minutes the Twitch server will send a PING, this is to respond with a PONG to keepalive

						case "PING :tmi.twitch.tv\r\n":
							try
							{
								Byte[] say = System.Text.Encoding.ASCII.GetBytes("PONG :tmi.twitch.tv\r\n");
								stream.Write(say, 0, say.Length);
								System.Console.WriteLine("Ping? Pong!");
							}
							catch (Exception e)
							{
								System.Console.WriteLine("OH SHIT SOMETHING WENT WRONG\r\n", e);
							}
							break;

						// If it's not a ping, it's probably something we care about.  Try to parse it for a message.

						default:
							try
							{
								string messageParser = myCompleteMessage.ToString();
								string[] message = messageParser.Split(':');

								if (message.Length <= 1)
								{
									continue;
								}

								string[] preamble = message[1].Split(' ');
								string tochat;

								// This means it's a message to the channel.  Yes, PRIVMSG is IRC for messaging a channel too
								if (preamble[1] == "PRIVMSG")
								{
									string[] sendingUser = preamble[0].Split('!');
									tochat = sendingUser[0] + ": " + message[2];

									// sometimes the carriage returns get lost (??)
									if (tochat.Contains("\n") == false)
									{
										tochat = tochat + "\n";
									}

									var nickYour = messageParser.Split(':').Last();
									var nickForSend = messageParser.Split('!').FirstOrDefault().Remove(0, 1);

									if (messageParser.Split(':').Last().Contains(_login))
									{
										sendMessage(nickForSend + " " + ChatterBot.GetAnswer(parseMessage(messageParser).Replace(_login, "")).Result);
										continue;
									}

									getAnswer(parseMessage(messageParser));

								}
							}
							// This is a disgusting catch for something going wrong that keeps it all running.  I'm sorry.
							catch (Exception e)
							{
								System.Console.WriteLine("OH SHIT SOMETHING WENT WRONG\r\n", e);
							}

							// Uncomment the following for raw message output for debugging
							//
							// Console.WriteLine("Raw output: " + message[0] + "::" + message[1] + "::" + message[2]);
							// Console.WriteLine("You received the following message : " + myCompleteMessage);
							break;
					}
				}
			}
		}

		public static void OnRawMessage(object sender, IrcEventArgs e)
		{
			System.Console.WriteLine("Received: " + e.Data.RawMessage);
		}

		private void getAnswer(string message)
		{
			switch (message)
			{
				case "ты\r\n":
					sendMessage("не тычь!");
					break;

				case "кто такой Макс\r\n":
					var rnd = new Random().Next(0, 10);
					var str = new StringBuilder("хуй");
					for (int i = 0; i <= rnd; i++)
					{
						str.Append(')');
					}
					sendMessage(str.ToString());
					break;
				case "бунт\r\n":
					var rnd2 = new Random().Next(0,10);
					var str2 = new StringBuilder("SMOrc");
					for (int i = 0; i <= rnd2; i++)
					{
						str2.Append(" SMOrc");
					}

					sendMessage(str2.ToString());
					break;
			}
		}

		public static string parseMessage(string data)
		{
			//:uwnara!uwnara@uwnara.tmi.twitch.tv PRIVMSG #skimmitar :z
			

			return data.Split(new []{_channel}, StringSplitOptions.None).Last().Split(':').Last();
		}

		public void sendMessage(string message)
		{
			Byte[] say = System.Text.Encoding.UTF8.GetBytes($"PRIVMSG #{_channel} :{message}\r\n");
			stream.Write(say, 0, say.Length);
			
		}

		public static void ReadCommands()
		{
			// here we read the commands from the stdin and send it to the IRC API
			// WARNING, it uses WriteLine() means you need to enter RFC commands
			// like "JOIN #test" and then "PRIVMSG #test :hello to you"
			while (true)
			{
				string cmd = System.Console.ReadLine();
				if (cmd.StartsWith("/list"))
				{
					int pos = cmd.IndexOf(" ");
					string channel = null;
					if (pos != -1)
					{
						channel = cmd.Substring(pos + 1);
					}

					IList<ChannelInfo> channelInfos = _ircClient.GetChannelList(channel);
					System.Console.WriteLine("channel count: {0}", channelInfos.Count);
					foreach (ChannelInfo channelInfo in channelInfos)
					{
						System.Console.WriteLine("channel: {0} user count: {1} topic: {2}",
										  channelInfo.Channel,
										  channelInfo.UserCount,
										  channelInfo.Topic);
					}
				}
				else
				{
					_ircClient.WriteLine(cmd);
				}
			}
		}

		public static void Exit()
		{
			// we are done, lets exit...
			System.Console.WriteLine($"Exiting...");
		}

		private void connect(NetworkStream stream)
		{
			string loginstring = $"PASS {_password}\r\nNICK {_login}\r\n";
			Byte[] login = System.Text.Encoding.ASCII.GetBytes(loginstring);
			stream.Write(login, 0, login.Length);

			data = new Byte[512];

			// String to store the response ASCII representation.
			String responseData = String.Empty;

			// Read the first batch of the TcpServer response bytes.
			Int32 bytes = stream.Read(data, 0, data.Length);
			responseData = System.Text.Encoding.ASCII.GetString(data, 0, bytes);

			// send message to join channel

			string joinstring = "JOIN " + "#" + _channel + "\r\n";
			Byte[] join = System.Text.Encoding.ASCII.GetBytes(joinstring);
			stream.Write(join, 0, join.Length);
		}
	}

}
