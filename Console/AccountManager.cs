using System.Collections.Generic;
using System.IO;

namespace Console
{
	public static class AccountManager
	{
		public static List<Account> GetAccountsFromFile(string filePath)
		{
			var accountsList = new List<Account>();

			foreach (var line in File.ReadAllLines(filePath))
			{
				var splitedLine = line.Split(';');

				accountsList.Add(new Account {Login = splitedLine[0].Trim(), OUathToken = splitedLine[1].Trim()});
			}

			return accountsList;
		}
	}
}
