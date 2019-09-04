using System;
using Crestron.SimplSharp;                          				// For Basic SIMPL# Classes
using Crestron.SimplSharpPro;                       				// For Basic SIMPL#Pro classes

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using PepperDash.Essentials;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;
using PepperDash.Core;

namespace EssentialsPluginTemplateEPI 
{
	public class EssentialsPluginTemplate : Device

	{
		public static void LoadPlugin()
		{
			PepperDash.Essentials.Core.DeviceFactory.AddFactoryForType("EssentialsPluginTemplate", EssentialsPluginTemplate.BuildDevice);	
		}

		public static EssentialsPluginTemplate BuildDevice(DeviceConfig dc)
		{
			var config = JsonConvert.DeserializeObject<EssentialsPluginTemplateConfigObject>(dc.Properties.ToString());
			var newMe = new EssentialsPluginTemplate(dc.Key, dc.Name, config);
			return newMe;
		}


		GenericSecureTcpIpClient_ForServer Client;

		public EssentialsPluginTemplate(string key, string name, EssentialsPluginTemplateConfigObject config)
			: base(key, name)
		{
			
		}
	}
}

