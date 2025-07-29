using Crestron.SimplSharpPro.DeviceSupport;
using PepperDash.Core;
using PepperDash.Core.Logging;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Bridges;

namespace PepperDash.Essentials.Plugin
{
    /// <summary>
    /// Plugin device template for logic devices that don't communicate outside the program
    /// </summary>
    /// <remarks>
    /// Rename the class to match the device plugin being developed.
    /// </remarks>
    /// <example>
    /// "EssentialsPluginTemplateLogicDevice" renamed to "SamsungMdcDevice"
    /// </example>
    public class MakeModelLogicDevice : EssentialsBridgeableDevice
    {
        /// <summary>
        /// It is often desirable to store the config
        /// </summary>
        private readonly MakeModelConfig config;

        /// <summary>
        /// Plugin device constructor
        /// </summary>
        /// <param name="key"></param>
        /// <param name="name"></param>
        /// <param name="config"></param>
        public MakeModelLogicDevice(string key, string name, MakeModelConfig config)
            : base(key, name)
        {
            this.LogInformation("Constructing new {0} instance", name);

            // TODO [ ] Update the constructor as needed for the plugin device being developed

            this.config = config;
        }

        #region Overrides of EssentialsBridgeableDevice

        /// <summary>
        /// Links the plugin device to the EISC bridge
        /// </summary>
        /// <param name="trilist"></param>
        /// <param name="joinStart"></param>
        /// <param name="joinMapKey"></param>
        /// <param name="bridge"></param>
        public override void LinkToApi(BasicTriList trilist, uint joinStart, string joinMapKey, EiscApiAdvanced bridge)
        {
            var joinMap = new EssentialsPluginTemplateBridgeJoinMap(joinStart);

            // This adds the join map to the collection on the bridge
            bridge?.AddJoinMap(Key, joinMap);

            var customJoins = JoinMapHelper.TryGetJoinMapAdvancedForDevice(joinMapKey);

            if (customJoins != null)
            {
                joinMap.SetCustomJoinData(customJoins);
            }

            this.LogDebug("Linking to Trilist {id}", trilist.ID.ToString("X"));
            this.LogInformation("Linking to Bridge Type {type}", GetType().Name);

            // TODO [ ] Implement bridge links as needed

            // links to bridge
            trilist.SetString(joinMap.DeviceName.JoinNumber, Name);

            trilist.OnlineStatusChange += (o, a) =>
            {
                if (!a.DeviceOnLine) return;

                trilist.SetString(joinMap.DeviceName.JoinNumber, Name);
            };
        }

        #endregion

    }
}

