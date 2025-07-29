// For Basic SIMPL# Classes
// For Basic SIMPL#Pro classes

using Crestron.SimplSharpPro.DeviceSupport;
using PepperDash.Core;
using PepperDash.Core.Logging;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Bridges;
using PepperDash.Essentials.Core.Queues;

namespace PepperDash.Essentials.Plugin
{
	/// <summary>
	/// Plugin device template for third party devices that use IBasicCommunication
	/// </summary>
	/// <remarks>
	/// Rename the class to match the device plugin being developed.
	/// </remarks>
	/// <example>
	/// "EssentialsPluginDeviceTemplate" renamed to "SamsungMdcDevice"
	/// </example>
	public class MakeModelDevice : EssentialsBridgeableDevice
	{
		/// <summary>
		/// It is often desirable to store the config
		/// </summary>
		private readonly MakeModelConfig config;

		/// <summary>
		/// Provides a queue and dedicated worker thread for processing feedback messages from a device.
		/// </summary>
		private readonly GenericQueue receiveQueue;

		#region IBasicCommunication Properties and Constructor.  Remove if not needed.

		// TODO [ ] Add, modify, remove properties and fields as needed for the plugin being developed
		private readonly IBasicCommunication comms;
		private readonly GenericCommunicationMonitor commsMonitor;

		// comms gather for ASCII based API's
		// TODO [ ] If not using an ASCII based API, delete the properties below
		private readonly CommunicationGather commsGather;

		/// <summary>
		/// Set this value to that of the delimiter used by the API (if applicable)
		/// </summary>
		private const string commsDelimiter = "\r";

		// comms byte buffer for HEX/byte based API's
		// TODO [ ] If not using an HEX/byte based API, delete the properties below
		private readonly byte[] commsByteBuffer = { };



		/// <summary>
		/// Connects/disconnects the comms of the plugin device
		/// </summary>
		/// <remarks>
		/// triggers the comms.Connect/Disconnect as well as thee comms monitor start/stop
		/// </remarks>
		public bool Connect
		{
			get { return comms.IsConnected; }
			set
			{
				if (value)
				{
					comms.Connect();
					commsMonitor.Start();
				}
				else
				{
					comms.Disconnect();
					commsMonitor.Stop();
				}
			}
		}

		/// <summary>
		/// Reports connect feedback through the bridge
		/// </summary>
		public BoolFeedback ConnectFeedback { get; private set; }

		/// <summary>
		/// Reports online feedback through the bridge
		/// </summary>
		public BoolFeedback OnlineFeedback { get; private set; }

		/// <summary>
		/// Reports socket status feedback through the bridge
		/// </summary>
		public IntFeedback StatusFeedback { get; private set; }

		/// <summary>
		/// Plugin device constructor for devices that need IBasicCommunication
		/// </summary>
		/// <param name="key"></param>
		/// <param name="name"></param>
		/// <param name="config"></param>
		/// <param name="comms"></param>
		public MakeModelDevice(string key, string name, MakeModelConfig config, IBasicCommunication comms)
	: base(key, name)
		{
			this.LogInformation("Constructing new {0} instance", name);

			// TODO [ ] Update the constructor as needed for the plugin device being developed

			this.config = config;

			receiveQueue = new GenericQueue(key + "-rxqueue");  // If you need to set the thread priority, use one of the available overloaded constructors.

			ConnectFeedback = new BoolFeedback("connect", () => Connect);
			OnlineFeedback = new BoolFeedback("online", () => commsMonitor.IsOnline);
			StatusFeedback = new IntFeedback("status", () => (int)commsMonitor.Status);

			this.comms = comms;
			commsMonitor = new GenericCommunicationMonitor(this, this.comms, this.config.PollTimeMs, this.config.WarningTimeoutMs, this.config.ErrorTimeoutMs, Poll);

			var socket = this.comms as ISocketStatus;
			if (socket != null)
			{
				// device comms is IP **ELSE** device comms is RS232
				socket.ConnectionChange += socket_ConnectionChange;
				Connect = true;
			}

			#region Communication data event handlers.  Comment out any that don't apply to the API type

			// Only one of the below handlers should be necessary.  

			// comms gather for any API that has a defined delimiter
			// TODO [ ] If not using an ASCII based API, remove the line below
			commsGather = new CommunicationGather(this.comms, commsDelimiter);
			commsGather.LineReceived += Handle_LineRecieved;

			// comms byte buffer for HEX/byte based API's with no delimiter
			// TODO [ ] If not using an HEX/byte based API, remove the line below
			this.comms.BytesReceived += Handle_BytesReceived;

			// comms byte buffer for HEX/byte based API's with no delimiter
			// TODO [ ] If not using an HEX/byte based API, remove the line below
			this.comms.TextReceived += Handle_TextReceived;

			#endregion
		}


		private void socket_ConnectionChange(object sender, GenericSocketStatusChageEventArgs args)
		{
			ConnectFeedback?.FireUpdate();

			StatusFeedback?.FireUpdate();
		}

		// TODO [ ] If not using an API with a delimeter, delete the method below
		private void Handle_LineRecieved(object sender, GenericCommMethodReceiveTextArgs args)
		{
			// TODO [ ] Implement method 

			// Enqueues the message to be processed in a dedicated thread, but the specified method
			receiveQueue.Enqueue(new ProcessStringMessage(args.Text, ProcessFeedbackMessage));
		}

		// TODO [ ] If not using an HEX/byte based API with no delimeter,  delete the method below
		private void Handle_BytesReceived(object sender, GenericCommMethodReceiveBytesArgs args)
		{
			// TODO [ ] Implement method 
			throw new System.NotImplementedException();
		}

		// TODO [ ] If not using an ASCII based API with no delimeter, delete the method below
		void Handle_TextReceived(object sender, GenericCommMethodReceiveTextArgs e)
		{
			// TODO [ ] Implement method 
			throw new System.NotImplementedException();
		}

		/// <summary>
		/// This method should perform any necessary parsing of feedback messages from the device
		/// </summary>
		/// <param name="message"></param>
		void ProcessFeedbackMessage(string message)
		{

		}


		// TODO [ ] If not using an ACII based API, delete the properties below
		/// <summary>
		/// Sends text to the device plugin comms
		/// </summary>
		/// <remarks>
		/// Can be used to test commands with the device plugin using the DEVPROPS and DEVJSON console commands
		/// </remarks>
		/// <param name="text">Command to be sent</param>		
		public void SendText(string text)
		{
			if (string.IsNullOrEmpty(text)) return;

			comms.SendText(string.Format("{0}{1}", text, commsDelimiter));
		}

		// TODO [ ] If not using an HEX/byte based API, delete the properties below
		/// <summary>
		/// Sends bytes to the device plugin comms
		/// </summary>
		/// <remarks>
		/// Can be used to test commands with the device plugin using the DEVPROPS and DEVJSON console commands
		/// </remarks>
		/// <param name="bytes">Bytes to be sent</param>		
		public void SendBytes(byte[] bytes)
		{
			if (bytes == null) return;

			comms.SendBytes(bytes);
		}

		/// <summary>
		/// Polls the device
		/// </summary>
		/// <remarks>
		/// Poll method is used by the communication monitor.  Update the poll method as needed for the plugin being developed
		/// </remarks>
		public void Poll()
		{
			// TODO [ ] Update Poll method as needed for the plugin being developed
			// Example: SendText("getstatus");
			throw new System.NotImplementedException();
		}

		#endregion


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

			trilist.SetBoolSigAction(joinMap.Connect.JoinNumber, sig => Connect = sig);
			ConnectFeedback.LinkInputSig(trilist.BooleanInput[joinMap.Connect.JoinNumber]);

			StatusFeedback.LinkInputSig(trilist.UShortInput[joinMap.Status.JoinNumber]);
			OnlineFeedback.LinkInputSig(trilist.BooleanInput[joinMap.IsOnline.JoinNumber]);

			UpdateFeedbacks();

			trilist.OnlineStatusChange += (o, a) =>
			{
				if (!a.DeviceOnLine) return;

				trilist.SetString(joinMap.DeviceName.JoinNumber, Name);
				UpdateFeedbacks();
			};
		}

		private void UpdateFeedbacks()
		{
			// TODO [ ] Update as needed for the plugin being developed
			ConnectFeedback.FireUpdate();
			OnlineFeedback.FireUpdate();
			StatusFeedback.FireUpdate();
		}

		#endregion

	}
}

