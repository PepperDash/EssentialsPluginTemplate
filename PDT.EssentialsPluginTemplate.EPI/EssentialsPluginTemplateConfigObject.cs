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
    /// Example of a config class that represents the structure of the Properties object of a DeviceConfig
    /// </summary>
	public class EssentialsPluginTemplatePropertiesConfig 
	{
        /// <summary>
        /// Control properties if needed to communicate with device
        /// </summary>
        [JsonProperty("control")]
        ControlPropertiesConfig Control { get; set; }


	}
}