using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Crestron.SimplSharp;
using Crestron.SimplSharpPro.DeviceSupport;
using Crestron.SimplSharp.Reflection;

using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Bridges;

using Newtonsoft.Json;

namespace EssentialsPluginTemplateEPI
{
	public static class EssentialsPluginTemplateBridge
	{
		public static void LinkToApiExt(this EssentialsPluginFactory DspDevice, BasicTriList trilist, uint joinStart, string joinMapKey)
		{
            // Construct the default join map
            EssentialsPluginTemplateBridgeJoinMap joinMap = new EssentialsPluginTemplateBridgeJoinMap();

            // Attempt to get a custom join map if specified in config
            var joinMapSerialized = JoinMapHelper.GetJoinMapForDevice(joinMapKey);

            // If we find a custom join map, deserialize it
            if (!string.IsNullOrEmpty(joinMapSerialized))
                joinMap = JsonConvert.DeserializeObject<EssentialsPluginTemplateBridgeJoinMap>(joinMapSerialized);

            // Offset the joins based on the join start
            joinMap.OffsetJoinNumbers(joinStart);


            // Set all your join actions here


            // Link all your feedbacks to joins here

		}
	}
	public class EssentialsPluginTemplateBridgeJoinMap : JoinMapBase
	{
        // Specify your joins here


		public EssentialsPluginTemplateBridgeJoinMap()
		{
            // Set the values of your joins here
		}

		public override void OffsetJoinNumbers(uint joinStart)
		{
			GetType()
				.GetCType()
				.GetProperties()
				.Where(x => x.PropertyType == typeof(uint))
				.ToList()
				.ForEach(prop => prop.SetValue(this, (uint)prop.GetValue(this, null) + joinStart - 1, null));
		}
	}
}