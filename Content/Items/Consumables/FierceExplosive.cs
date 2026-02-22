using Microsoft.Xna.Framework;
using System;
using System.ComponentModel;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;
using Terraria.WorldBuilding;

namespace BoBo.Content.Items.Consumables
{
	public enum SlopeTypeEnum//Example Mod里有提到枚举: Enum members can be individually labeled as well: [LabelKey("$Mods.ExampleMod.Configs.SampleEnum.Strange.Label")]
	{
		[LabelKey("$Mods.BoBo.Content.Items.Consumables.SlopeTypeEnum.Solid.Label")]
		Solid,
		[LabelKey("$Mods.BoBo.Content.Items.Consumables.SlopeTypeEnum.SolidDownLeft.Label")]
		SlopeDownLeft,
		[LabelKey("$Mods.BoBo.Content.Items.Consumables.SlopeTypeEnum.SolidDownRigh.Label")]
		SlopeDownRight,
		[LabelKey("$Mods.BoBo.Content.Items.Consumables.SlopeTypeEnum.SolidUpLeft.Label")]
		SlopeUpLeft,
		[LabelKey("$Mods.BoBo.Content.Items.Consumables.SlopeTypeEnum.SolidUpRight.Label")]
		SlopeUpRight
	}
	public class FierceExplosiveConfig : ModConfig//先做这些，后续需要能选择平台样式
	{
		public override ConfigScope Mode => ConfigScope.ClientSide;

		[Label("左右宽度")]
		[Tooltip("清除区域宽度（左右各一半）")]
		[Range(0, 1500)]
		[DefaultValue(700)]
		public int Width;

		[Label("平台间隔")]
		[Tooltip("平台间垂直间隔")]
		[Range(0, 100)]
		[DefaultValue(75)]
		public int PlatformSpacing;

		[Label("清除高度")]
		[Tooltip("清除区域向上清除的高度")]
		[Range(0, 8400)]
		[DefaultValue(8400)]
		public int ClearHeight;

		[Label("斜坡类型")]
		[Tooltip("设置平台斜坡形态")]
		[DefaultValue(SlopeType.Solid)]
		[Slider, DrawTicks]
		public SlopeType SlopeTypeSlider;

		/*[Header("平台设置")]
		public SlopeType SlopeType;
		[Label("斜坡类型(默认)")]
		[Tooltip("设置平台斜坡形态(默认)")]
		[DefaultValue(SlopeType.Solid)]

		[Label("斜坡类型(滑块)")]
		[Tooltip("设置平台斜坡形态(滑块)")]
		[DefaultValue(SlopeType.Solid)]
		[Slider, DrawTicks]
		public SlopeType SlopeTypeSlider;

		[Label("斜坡类型(下拉)")]
		[Tooltip("设置平台斜坡形态(下拉)")]
		[DefaultValue(SlopeType.Solid)]
		[Dropdown]
		public SlopeType SlopeTypeDropdown;

		[Label("斜坡类型(循环)")]
		[Tooltip("设置平台斜坡形态(循环)")]
		[DefaultValue(SlopeType.Solid)]
		[Cycle]
		public SlopeType SlopeTypeCycle;*/
	}
	public class FierceExplosive : ModItem//破坏地形的炸弹
	{
		public override string Texture => Pictures.Consumables + Name;
		public override void SetDefaults()
		{
			Item.CloneDefaults(ItemID.StickyBomb);
			Item.width = 40;
			Item.height = 48;
			Item.shoot = ModContent.ProjectileType<FierceExplosiveProjectile>();
			Item.damage = 0;
		}

		//直接在鼠标位置生成弹幕
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			Vector2 mouseWorld = Main.MouseWorld;//获取鼠标在世界中的位置
			Projectile.NewProjectile(source, mouseWorld, Vector2.Zero, type, damage, knockback, player.whoAmI);//直接在鼠标位置创建弹幕，不发射
			return false;
		}
	}
	public class FierceExplosiveProjectile : ModProjectile//没有创建消耗品的弹幕文件夹，就写这了
	{
		public override string Texture => Pictures.Consumables + "FierceExplosive";
		//清除状态变量
		public bool IsClearing = false;
		public int CurrentSegment;
		public int CenterX;
		public int CenterY;
		public const int SubChunkHeight = 5;//每个子块的高度（每帧清除5行，这样不卡）
		public int CurrentSubChunk;
		//配置相关变量，在Modconfig中有
		public int Width;
		public int PlatformSpacing;
		public int ClearHeight;
		public SlopeType SlopeType;
		public int HalfWidth => Width / 2;
		public override void SetDefaults()
		{
			Projectile.CloneDefaults(ProjectileID.StickyBomb);
			Projectile.width = 40;
			Projectile.height = 48;
			Projectile.tileCollide = false;//直接在鼠标位置生成
			Projectile.timeLeft = 10;//减少存在时间，立即爆炸
			Projectile.aiStyle = -1;//不使用AI样式
		}
		public override void AI()
		{
			if (Projectile.timeLeft == 9)//第一帧触发爆炸
			{
				Projectile.Kill();
			}
		}
		public override void Kill(int timeLeft)
		{
			var config = ModContent.GetInstance<FierceExplosiveConfig>();
			Width = config.Width;
			PlatformSpacing = config.PlatformSpacing;
			ClearHeight = config.ClearHeight;
			SlopeType = config.SlopeTypeSlider;
			if (PlatformSpacing < 1) PlatformSpacing = 1;//确保平台间隔至少为1
			Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero,
				ProjectileID.GrenadeIII, 0, 0, Projectile.owner);//爆炸效果在鼠标位置生成
			Vector2 ExplosionCenter = Projectile.Center;//获取爆炸位置的世界坐标（鼠标位置）
			CenterX = (int)(ExplosionCenter.X / 16);
			CenterY = (int)(ExplosionCenter.Y / 16);
			int MaxHeight = CenterY;//确保清除高度不超过世界顶部
			ClearHeight = Math.Min(ClearHeight, MaxHeight);
			IsClearing = true;//初始化清除状态
			CurrentSegment = 0;
			CurrentSubChunk = 0;
			CreatePlatforms(CenterX, CenterY);//创建平台，一次性完成
		}
		public override void PostAI()
		{
			if (IsClearing)
			{
				ClearSubChunk();//每帧清除一个小区域（5行）
				CurrentSubChunk++;//移动到下一个小区域
				int SubChunksPerSegment = PlatformSpacing / SubChunkHeight;
				if (CurrentSubChunk >= SubChunksPerSegment)
				{
					CurrentSegment++;//移动到下一个区域
					CurrentSubChunk = 0;
					if (CurrentSegment * PlatformSpacing > ClearHeight)//检查是否完成清除
					{
						IsClearing = false;
						Liquid.QuickWater(0, Main.maxTilesX, Main.maxTilesY);//重置液体物理计算
					}
				}
			}
		}
		public void ClearSubChunk()
		{
			//计算当前小区域的Y范围
			int SegmentTop = CurrentSegment * PlatformSpacing;
			int SegmentBottom = (CurrentSegment + 1) * PlatformSpacing - 1;
			if (SegmentBottom > ClearHeight) SegmentBottom = ClearHeight;
			int SubChunkTop = SegmentTop + CurrentSubChunk * SubChunkHeight;
			int SubChunkBottom = SubChunkTop + SubChunkHeight - 1;
			if (SubChunkBottom > SegmentBottom) SubChunkBottom = SegmentBottom;
			if (SubChunkTop > ClearHeight) return;//确保不超过清除高度
			for (int y = SubChunkTop; y <= SubChunkBottom; y++)
			{
				for (int x = CenterX - HalfWidth; x <= CenterX + HalfWidth; x++)
				{
					if (WorldGen.InWorld(x, y))
					{
						WorldGen.KillTile(x, y, noItem: true);//清除物块
						if (Main.tile[x, y].LiquidType > 0)//清除液体
						{
							WorldGen.EmptyLiquid(x, y);
						}
					}
				}
			}
		}
		public void CreatePlatforms(int CenterX, int CenterY)
		{
			//计算平台起始Y位置
			int StartY = CenterY;
			int EndY = CenterY - ClearHeight;
			if (EndY < 0) EndY = 0;//确保结束位置不低于0
			for (int y = StartY; y >= EndY; y -= PlatformSpacing)//使用平台间隔创建平台
			{
				if (WorldGen.InWorld(CenterX - HalfWidth, y))
				{
					for (int x = CenterX - HalfWidth; x <= CenterX + HalfWidth; x++)//创建平台
					{
						if (WorldGen.InWorld(x, y))
						{
							WorldGen.PlaceTile(x, y, TileID.TeamBlockBluePlatform, forced: true);//放置蓝团队平台
							SetSlope(x, y);//根据配置设置斜坡类型
						}
					}
					ClearAbovePlatform(CenterX, y);//在平台上方清除(PlatformSpacing-1)格
				}
			}
		}
		public void ClearAbovePlatform(int CenterX, int PlatformY)
		{
			int ClearTop = PlatformY - (PlatformSpacing - 1);//清除范围是从平台上方1格到(PlatformSpacing-1)格
			int ClearBottom = PlatformY - 1; //清除平台
			if (ClearTop < 0) ClearTop = 0;//确保不超过世界顶部
			if (ClearBottom < ClearTop) return;//确保范围有效
			WorldUtils.Gen(
			   new Point(CenterX - HalfWidth, ClearTop),
			   new Shapes.Rectangle(Width, ClearBottom - ClearTop + 1),
			   Actions.Chain(
				   new Actions.ClearTile(),
				   new Actions.SetLiquid(LiquidID.Lava, 0)
			   )
		   );
		}
		public void SetSlope(int x, int y)//配置斜坡类型
		{
			Console.WriteLine($"当前斜坡类型: {SlopeType}");
			switch (SlopeType)
			{
				case SlopeType.Solid://实心物块，不需要设置斜坡
					break;
				case SlopeType.SlopeDownLeft:
					WorldGen.SlopeTile(x, y, (int)SlopeType.SlopeDownLeft);
					break;
				case SlopeType.SlopeDownRight:
					WorldGen.SlopeTile(x, y, (int)SlopeType.SlopeDownRight);
					break;
				case SlopeType.SlopeUpLeft:
					WorldGen.SlopeTile(x, y, (int)SlopeType.SlopeUpLeft);
					break;
				case SlopeType.SlopeUpRight:
					WorldGen.SlopeTile(x, y, (int)SlopeType.SlopeUpRight);
					break;
				default:
					WorldGen.PoundTile(x, y);//默认使用锤子设置
					break;
			}
		}
	}
}