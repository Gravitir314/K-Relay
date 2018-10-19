using System.Collections.Generic;
using System.Xml.Linq;

namespace Lib_K_Relay.GameData.DataStructures
{
	public struct AccountStructure : IDataStructure<string>
	{
		internal static Dictionary<string, AccountStructure> Load(XDocument doc)
		{
			Dictionary<string, AccountStructure> map = new Dictionary<string, AccountStructure>();

			doc.Element("Accounts")
				.Elements("Account")
				.ForEach(account =>
				{
					AccountStructure a = new AccountStructure(account);
					map[a.ID] = a;
				});

			return map;
		}

		/// <summary>
		/// The complete name of this account
		/// </summary>
		public string Name
		{
			get;
			private set;
		}

		/// <summary>
		/// The complete guid of this account
		/// </summary>
		public string ID
		{
			get;
			private set;
		}

		/// <summary>
		/// The password of this account
		/// </summary>
		public string Password;

		/// <summary>
		/// The secret of this account
		/// </summary>
		public string Secret;

		public AccountStructure(XElement account)
		{
			Name = account.ElemDefault("NAME", "Unknown");
			ID = account.ElemDefault("GUID", "");
			Password = account.ElemDefault("PASSWORD", "");
			Secret = account.ElemDefault("SECRET", "");
			if (account.HasElement("CHARACTER"))
			{
				// TODO add characters
			}
		}

		public override string ToString()
		{
			Password = Password == "" ? Secret : Password;
			return string.Format("Name: {0}\n GUID: {1}\n Password: {2}]", Name, ID, Password);
		}
	}
}
