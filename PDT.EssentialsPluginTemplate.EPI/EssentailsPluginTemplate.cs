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
    /// <summary>
    /// This class contains the necessary properties and static methods required to function as an Essentials Plugin
    /// </summary>
	public class EssentialsPluginFactory
	{
        /// <summary> 
        /// This string is used to define the minimum version of the
        /// Essentials Framework required for this plugin
        /// </summary>
        public static string MinimumEssentialsFrameworkVersion = "1.4.31";

        /// <summary>
        /// This method will get called by Essentials when this plugin is loaded.  
        /// Use it to add factory methods for all new Device types defined in this plugin
        /// </summary>
		public static void LoadPlugin()
		{
			PepperDash.Essentials.Core.DeviceFactory.AddFactoryForType("EssentialsPluginTemplate", EssentialsPluginFactory.BuildDevice);	

            // Add additional factories for each type here
		}

        /// <summary>
        /// Builds an instance of the device type.  There should be method like this defined for each device type your plugin needs
        /// to be able to build
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

