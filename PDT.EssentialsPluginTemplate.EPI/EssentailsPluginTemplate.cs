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
	public class EssentialsPluginTemplate
	{
        /// <summary> 
        /// This string is used to define the minimum version of the
        /// Essentials Framework required for this plugin
        /// </summary>
        public static string MinimumEssentialsFrameworkVersion = "1.4.23";

        /// <summary>
        /// This method will get called by Essentials when this plugin is loaded.  
        /// Use it to add factory methods for all new Device types defined in this plugin
        /// </summary>
		public static void LoadPlugin()
		{
			PepperDash.Essentials.Core.DeviceFactory.AddFactoryForType("EssentialsPluginTemplate", EssentialsPluginTemplate.BuildDevice);	
		}

        /// <summary>
        /// Builds an instance of the device type
        /// </summary>
        /// <param name="dc">The device configuration</param>
        /// <returns>The device</returns>
		public static EssentialsPluginTemplateDevice BuildDevice(DeviceConfig dc)
		{
			var config = JsonConvert.DeserializeObject<EssentialsPluginTemplatePropertiesConfig>(dc.Properties.ToString());
			var newDevice = new EssentialsPluginTemplateDevice(dc.Key, dc.Name, config);
			return newDevice;
		}

	}
}

