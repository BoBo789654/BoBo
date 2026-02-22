using System.Collections;
using System.IO;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace BoBo.Content.Systems
{
	//作为"已击败Boss"标志的容器
	//在 Boss 的 OnKill 钩子中设置标志（示例）：
	//NPC.SetEventFlagCleared(ref DownedBossSystem.downedEvilPumpking, -1);
	//保存和加载标志需使用 TagCompound，详见 Wiki 指南：
	//https://github.com/tModLoader/tModLoader/wiki/Saving-and-loading-using-TagCompound
	public class DownedBossSystem : ModSystem
	{
		public static bool downedEvilPumpking = false; //标记特定 Boss 是否被击败
		public static bool downedEvilWorm = false; //标记邪恶蠕虫 Boss 是否被击败
		//创建新世界时重置所有标志
		public override void ClearWorld()
		{
			downedEvilPumpking = false;
			downedEvilWorm = false;
		}
		//使用TagCompound保存世界数据，注意：默认提供的tag实例为空
		public override void SaveWorldData(TagCompound tag)
		{
			if (downedEvilPumpking)
			{
				tag["downedEvilPumpking"] = true;//仅当Boss被击败时保存标志
			}
			if (downedEvilWorm)
			{
				tag["downedEvilWorm"] = true;//仅当邪恶蠕虫 Boss 被击败时保存标志
			}
		}
		//从TagCompound加载世界数据
		public override void LoadWorldData(TagCompound tag)
		{
			downedEvilPumpking = tag.ContainsKey("downedEvilPumpking"); //检查保存的标志
			downedEvilWorm = tag.ContainsKey("downedEvilWorm"); //检查邪恶蠕虫 Boss 的保存标志
		}
		//网络发送数据（多人模式同步）
		public override void NetSend(BinaryWriter writer)
		{
			//参数顺序必须与NetReceive一致
			writer.WriteFlags(downedEvilPumpking, downedEvilWorm); //打包布尔值，WriteFlags最多支持8个标志，超过需多次调用，若需同步大量标志（如按物品类型），可使用BitArray高效处理（参考 Utils.SendBitArray）
		}
		//网络接收数据（多人模式同步）
		public override void NetReceive(BinaryReader reader)
		{
			//参数顺序必须与 NetSend 一致
			reader.ReadFlags(out downedEvilPumpking, out downedEvilWorm); //解包布尔值，ReadFlags 最多支持 8 个标志，超过需多次调用
		}
	}
}