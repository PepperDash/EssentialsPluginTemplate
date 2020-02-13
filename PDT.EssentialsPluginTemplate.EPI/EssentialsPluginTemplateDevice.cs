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
using PepperDash.Essentials.Bridges;

namespace EssentialsPluginTemplateEPI
{
    /// <summary>
    /// Example of a plugin device
    /// </summary>
    public class EssentialsPluginTemplateDevice : Device, IBridge
    {
        /// <summary>
        /// Device Constructor.  Called by BuildDevice
        /// </summary>
        /// <param name="key"></param>
        /// <param name="name"></param>
        /// <param name="config"></param>
        public EssentialsPluginTemplateDevice(string key, string name, EssentialsPluginTemplatePropertiesConfig config)
            : base(key, name)
        {

        }

        /// <summary>
        /// Add items to be executed during the Activaction phase
        /// </summary>
        /// <returns></returns>
        public override bool CustomActivate()
        {

            return base.CustomActivate();
        }

        /// <summary>
        /// This method gets called by the EiscApi bridge and calls your bridge extension method
        /// </summary>
        /// <param name="trilist"></param>
        /// <param name="joinStart"></param>
        /// <param name="joinMapKey"></param>
        public void LinkToApi(BasicTriList trilist, uint joinStart, string joinMapKey)
        {
            this.LinkToApi(trilist, joinStart, joinMapKey);
        }
    }
}