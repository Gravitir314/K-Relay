using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Mila
{
	internal sealed partial class MilaConfig : ApplicationSettingsBase
	{
		private void SettingChangingEventHandler(object sender, SettingChangingEventArgs e)
		{
		}
		private void SettingsSavingEventHandler(object sender, CancelEventArgs e)
		{
		}
	}
}
