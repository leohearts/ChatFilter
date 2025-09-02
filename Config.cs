using System.ComponentModel;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

namespace ChatFilter
{
	public class Config : ModConfig
	{
		public override ConfigScope Mode => ConfigScope.ClientSide;

		public static Config Instance => ModContent.GetInstance<Config>();

		[DefaultValue(true)]
		public bool ChatFilterEnabled;

		[DefaultValue(true)]
		public bool BlockEntireMessage;

		public List<string> BlockedWords;
	}
}
