using Lib_K_Relay.GameData;
using System;
using System.Linq;
using System.Windows.Forms;

namespace K_Relay
{
	partial class FrmMainMetro
	{
		private void InitAccounts()
		{
			Invoke((MethodInvoker)delegate
			{
				if (GameData.Accounts != null)
				{
					listAccounts.ListBox.Items.AddRange(GameData.Accounts.Map.Select(x => x.Value.Name).ToArray());
				}
				else
				{
					listAccounts.ListBox.Items.Insert(0, "Account list not found.");
				}
			});
		}

		private void listAccounts_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (listAccounts.ListBox.SelectedItem != null && listAccounts.ListBox.SelectedItem.ToString() != "Account list not found.")
			{
				string data = GameData.Accounts.ByName((string)listAccounts.ListBox.SelectedItem).ToString();
				tbxAccountInfo.Text = data;
			}
		}
	}
}
