using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using PepperDash.Essentials.Core;

namespace PDT.EssentialsPluginTemplate.EPI
{
    public class EssentialsPluginTemplateBridgeJoinMap : JoinMapBaseAdvanced
    {
        [JoinName("IsOnline")]
        public JoinDataComplete IsOnline = new JoinDataComplete(new JoinData {JoinNumber = 1, JoinSpan = 1},
            new JoinMetadata
            {
                Description = "Device is Online",
                JoinType = eJoinType.Digital,
                JoinCapabilities = eJoinCapabilities.ToSIMPL
            });


        public EssentialsPluginTemplateBridgeJoinMap(uint joinStart):base(joinStart, typeof(EssentialsPluginTemplateBridgeJoinMap))
        {
            
        }
    }
}