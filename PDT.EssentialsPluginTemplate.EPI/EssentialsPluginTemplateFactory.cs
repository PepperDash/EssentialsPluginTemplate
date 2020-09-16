using System.Collections.Generic;
using PepperDash.Core;
using PepperDash.Essentials.Core;

namespace EssentialsPluginTemplate
{
	/// <summary>
	/// Plugin device factory
	/// </summary>
	/// <remarks>
	/// Rename the class to match the device plugin being developed
	/// </remarks>
	/// <example>
	/// "EssentialsPluginFactoryTemplate" renamed to "SamsungMdcFactory"
	/// </example>
    public class EssentialsPluginFactoryTemplate : EssentialsPluginDeviceFactory<EssentialsPluginDeviceTemplate>
    {
		/// <summary>
		/// Plugin device factory constructor
		/// </summary>
		/// <remarks>
		/// Update the MinimumEssentialsFrameworkVersion & TypeNames as needed when creating a plugin
		/// </remarks>
		/// <example>
		/// <code>
		/// // Set the minimum Essentials Framework Version
		/// MinimumEssentialsFrameworkVersion = "1.5.5";
		/// // In the constructor we initialize the list with the typenames that will build an instance of this device
#pragma warning disable 1570
		/// TypeNames = new List<string>() { "SamsungMdc", "SamsungMdcDisplay" };
#pragma warning restore 1570
		/// </code>
		/// </example>
        public EssentialsPluginFactoryTemplate()
        {
            // Set the minimum Essentials Framework Version
			// TODO [ ] Update the Essentials minimum framework version which this plugin has been tested against
			MinimumEssentialsFrameworkVersion = "1.6.4";

            // In the constructor we initialize the list with the typenames that will build an instance of this device
			// TODO [ ] Update the TypeNames for the plugin being developed
            TypeNames = new List<string>() { "examplePluginDevice" };
        }
        
		/// <summary>
		/// Builds and returns an instance of EssentialsPluginDeviceTemplate
		/// </summary>
		/// <param name="dc">device configuration</param>
		/// <returns>plugin device or null</returns>
		/// <remarks>		
		/// The example provided below takes the device key, name, properties config and the comms device created.
		/// Modify the EssetnialsPlugingDeviceTemplate constructor as needed to meet the requirements of the plugin device.
		/// </remarks>
		/// <seealso cref="PepperDash.Core.eControlMethod"/>
        public override EssentialsDevice BuildDevice(PepperDash.Essentials.Core.Config.DeviceConfig dc)
        {
            Debug.Console(1, "[{0}] Factory Attempting to create new device from type: {1}", dc.Key, dc.Type);	       

			// get the plugin device properties configuration object & check for null 
            var propertiesConfig = dc.Properties.ToObject<EssentialsPluginConfigObjectTemplate>();
	        if (propertiesConfig == null)
	        {
		        Debug.Console(0, "[{0}] Factory: failed to read properties config for {1}", dc.Key, dc.Name);
		        return null;
	        }

			// TODO [ ] Discuss with Andrew/Neil on recommended best practice 
			
			// Method 1
			//var username = dc.Properties["control"].Value<TcpSshPropertiesConfig>("tcpSshProperties").Username;
			//var password = dc.Properties["control"].Value<TcpSshPropertiesConfig>("tcpSshProperties").Password;
			//var method = dc.Properties["control"].Value<string>("method");
			
			// Method 2 - Returns a JValue, has to be casted to string
	        var username = (string)dc.Properties["control"]["tcpSshProperties"]["username"];
			var password = (string)dc.Properties["control"]["tcpSshProperties"]["password"];
	        var method = (string)dc.Properties["control"]["method"];
	        
			// build the plugin device comms & check for null
			// TODO { ] As of PepperDash Core 1.0.41, HTTP and HTTPS are not valid eControlMethods and will throw an exception.
			var comms = CommFactory.CreateCommForDevice(dc);
			if (comms == null)
			{
				Debug.Console(0, "[{0}] Factory: failed to create comm for {1}", dc.Key, dc.Name);
				return null;
			}

            return new EssentialsPluginDeviceTemplate(dc.Key, dc.Name, propertiesConfig, comms);
        }

    }
}