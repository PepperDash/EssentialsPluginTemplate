using System;
using System.Collections.Generic;
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
    /// This class contains the necessary properties and methods required to function as an Essentials Plugin
    /// </summary>
	public class EssentialsPluginFactory:EssentialsPluginDeviceFactory<EssentialsPluginTemplateDevice>
    {
        public EssentialsPluginFactory()
        {
            // This string is used to define the minimum version of the
            // Essentials Framework required for this plugin
            MinimumEssentialsFrameworkVersion = "1.6.1";

            //The strings defined in this list will be used in the configuration file to build the device in this plugin.
            TypeNames = new List<string> {"essentialsPluginTemplateDevice"};
        }

        #region Overrides of EssentialsDeviceFactory<EssentialsPluginTemplateDevice>

        public override EssentialsDevice BuildDevice(DeviceConfig dc)
        {
            var config = dc.Properties.ToObject<EssentialsPluginTemplatePropertiesConfig>();
			var newDevice = new EssentialsPluginTemplateDevice(dc.Key, dc.Name, config);
			return newDevice;
        }

        #endregion
    }
}

