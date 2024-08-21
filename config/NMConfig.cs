using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace NeatMediumcore.Config
{
	public class NMConfig : ModConfig
	{
		public override ConfigScope Mode => ConfigScope.ClientSide;

        [DefaultValue(true)]
        public bool ShowButtonsToggle;

        [DefaultValue(false)]
		public bool ItemsGlowToggle;
    }
}