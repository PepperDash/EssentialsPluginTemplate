using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

using PepperDash.Core;

using Newtonsoft.Json;

namespace EssentialsPluginTemplateEPI
{
    /// <summary>
    /// Example of a config class that represents the structure of the Properties object of a DeviceConfig.
    /// The BuildDevice method will attempt to deserialize the Properties object into this class.
    /// Populate with any necssary properties for your device
    /// </summary>
	public class EssentialsPluginTemplatePropertiesConfig 
	{
        // Below are some example properties


        /// <summary>
        /// Control properties if needed to communicate with device.
        /// The JsonProperty attribute has been added to specify the name
        /// of the object and that it is required
        /// </summary>
        [JsonProperty("control", Required = Required.Always)]
        ControlPropertiesConfig Control { get; set; }

        /// <summary>
        /// Add custom properties here 
        /// </summary>
        [JsonProperty("myDeviceProperty")]
        string MyDeviceProperty { get; set; }
	}
}