using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Console
{
	class Program
	{
		static void Main(string[] args)
		{
			var accounts = AccountManager.GetAccountsFromFile("Accounts.txt");

			//var chat = new ChatManager(accounts.Login, accounts.OUathToken, "skimmitar");

			List<Task> tasks = new List<Task>();


			foreach (var acc in accounts)
			{
				tasks.Add(Task.Run(() => new ChatManager(acc.Login, acc.OUathToken, "skimmitar")));
			}

			while (true)
			{
				
			}
		}
	}
}
