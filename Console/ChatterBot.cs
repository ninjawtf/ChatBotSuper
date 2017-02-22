using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cleverbot.Net;

namespace Console
{
	public static class ChatterBot
	{
		private static readonly CleverbotSession session;

		static ChatterBot()
		{
			session = CleverbotSession.NewSession("7qOe3rwAd7VBxZvW", "DHC0lxN244R3SZaF6LbFxB2DcygEjAFF");
		}

		public async static Task<string> GetAnswer(string message)
		{
			return await session.SendAsync(message);
		}
	}
}
