using Microsoft.Xna.Framework;
using System.IO;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace BoBo.Content.Projectiles.Tools
{
	///<summary>
	///自定义浮标弹射物 - 决定钓线颜色并实现多人同步
	///</summary>
	public class VastWaveBobber : ModProjectile
	{
		public override string Texture => Pictures.ToolProj + Name;
		public static readonly Color[] LineColors = {
			new Color(255, 215, 0),
			new Color(215, 255, 0),
            new Color(0, 215, 255),
			new Color(0, 255, 215),
			new Color(215, 0, 255),
			new Color(255, 0, 215),
        };//钓线颜色库
		private int ColorIndex;//当前颜色索引
		public Color CurrentLineColor => LineColors[ColorIndex];//获取钓线颜色
		public override void SetDefaults()
		{
			Projectile.CloneDefaults(ProjectileID.BobberGolden);//复制原版浮标属性
			DrawOriginOffsetY = -8;//垂直偏移调整绘制位置
		}
		public static void AddLineColor(Color newColor)//颜色配置方法
		{
			Color[] newColors = new Color[LineColors.Length + 1];//扩展颜色数组
			LineColors.CopyTo(newColors, 0);
			newColors[1] = newColor;
		}
		public override void OnSpawn(IEntitySource source)
		{
			ColorIndex = Main.rand.Next(LineColors.Length);//随机选择钓线颜色
		}
		#region 多人同步
		public override void AI()
		{
			if (!Main.dedServ)//仅在客户端生成光影效果（服务端不渲染）
			{
				Lighting.AddLight(Projectile.Center, CurrentLineColor.ToVector3());
			}
		}
		public override void SendExtraAI(BinaryWriter writer)
		{
			writer.Write((byte)ColorIndex);//压缩传输字节
		}

		public override void ReceiveExtraAI(BinaryReader reader)
		{
			ColorIndex = reader.ReadByte();//解码颜色索引
		}
		#endregion
	}
}