using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro.DeviceSupport;

using PepperDash.Essentials;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;
using PepperDash.Core;

namespace EssentialsPluginTemplateEPI
{
    /// <summary>
    /// Example of a plugin device
    /// </summary>
    public class EssentialsPluginTemplateDevice : Device, IBridge
    {
        /// <summary>
        /// Device Constructor.  Called by 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="name"></param>
        /// <param name="config"></param>
        public EssentialsPluginTemplateDevice(string key, string name, EssentialsPluginTemplatePropertiesConfig config)
            : base(key, name)
        {

        }

        public void LinkToApi(BasicTriList trilist, uint joinStart, string joinMapKey)
        {


        }
    }
}