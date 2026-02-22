using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace BoBo.Content.Players
{
	public class stat1AddPlayer : ModPlayer
	{
		public int chiliCount;    //已食用辣椒数量(最大20)
		public int starfruitCount;//已食用杨桃数量(最大15)
		//永久增加玩家最大属性
		public override void ModifyMaxStats(out StatModifier health, out StatModifier mana)
		{
			health = StatModifier.Default;
			health.Base = chiliCount * 10;//每个辣椒+10生命
			mana = StatModifier.Default;
			mana.Base = starfruitCount * 15;//每个杨桃+15魔力
		}
		public void ReceivePlayerSync(System.IO.BinaryReader reader)
		{
			chiliCount = reader.ReadByte();
			starfruitCount = reader.ReadByte();
		}
		//客户端状态复制
		public override void CopyClientState(ModPlayer targetCopy)
		{
			stat1AddPlayer clone = (stat1AddPlayer)targetCopy;
			clone.chiliCount = chiliCount;
			clone.starfruitCount = starfruitCount;
		}
		//检测变化并同步
		public override void SendClientChanges(ModPlayer clientPlayer)
		{
			stat1AddPlayer clone = (stat1AddPlayer)clientPlayer;
			if (chiliCount != clone.chiliCount || starfruitCount != clone.starfruitCount)
				SyncPlayer(toWho: -1, fromWho: Main.myPlayer, newPlayer: false);
		}
		//数据保存
		public override void SaveData(TagCompound tag)
		{
			tag["chiliCount"] = chiliCount;
			tag["starfruitCount"] = starfruitCount;
		}
		//数据加载
		public override void LoadData(TagCompound tag)
		{
			chiliCount = tag.GetInt("chiliCount");
			starfruitCount = tag.GetInt("starfruitCount");
		}
	}
}