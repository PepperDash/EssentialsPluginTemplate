using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Crestron.SimplSharp;
using Crestron.SimplSharpPro.DeviceSupport;

using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Bridges;

namespace EssentialsPluginTemplateEPI
{
	public static class EssentialsPluginTemplateBridge
	{
		public static void LinkToApiExt(this EssentialsPluginTemplate DspDevice, BasicTriList trilist, uint joinStart, string joinMapKey)
		{
			var joinMap = JoinMapHelper.GetJoinMapForDevice(joinMapKey) as EssentialsPluginTemplateBridgeJoinMap;

			if (joinMap == null)
				joinMap = new EssentialsPluginTemplateBridgeJoinMap();

		}
	}
	public class EssentialsPluginTemplateBridgeJoinMap : JoinMapBase
	{
		public EssentialsPluginTemplateBridgeJoinMap()
		{
		}

		public override void OffsetJoinNumbers(uint joinStart)
		{
		}
	}
}