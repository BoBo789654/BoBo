using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Color = Microsoft.Xna.Framework.Color;

namespace BoBo.Content.Items.Weapons.Magic
{
	public class ColorfulStaff : ModItem//多彩法杖：按顺序发射各种弹幕（伤害随原武器），按时期成长，绘制tooltip
	{
		public override string Texture => Pictures.Magic + Name;
		private int ShotIndex = 0;

		public override void SetDefaults()
		{
			Item.width = 40;
			Item.height = 40;
			Item.useTime = 8;
			Item.useAnimation = 8;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.noMelee = true;
			Item.DamageType = DamageClass.Magic;
			Item.damage = 36;
			Item.knockBack = 4;
			Item.mana = 2;
			Item.crit = 4;
			Item.UseSound = SoundID.Item58;
			Item.autoReuse = true;
			Item.shoot = ProjectileID.Flames;
			Item.shootSpeed = 15f;
			Item.rare = ItemRarityID.Master;
			Item.value = Item.sellPrice(0, 5);
			Item.staff[Type] = true;
		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			var ProjectileList = new (int ProjectileID, int ItemID)[]//这里有个弹幕与原物品伤害的映射表
			{
				(ProjectileID.LaserMachinegunLaser,		ItemID.LaserMachinegun),		//激光，激光机枪
			    (ProjectileID.ChlorophyteBullet,		ItemID.VenusMagnum),			//叶绿弹，维纳斯万能枪
			    (ProjectileID.Flames,					ItemID.Flamethrower),			//火焰，火焰喷射器
			    (ProjectileID.ChargedBlasterOrb,		ItemID.ChargedBlasterCannon),	//充能爆破珠，充能爆破炮
			    (ProjectileID.CrystalStorm,				ItemID.CrystalStorm),			//水晶风暴弹，水晶风暴
			    (ProjectileID.RainbowRodBullet,			ItemID.RainbowRod),				//彩虹弹，彩虹魔杖
			    (ProjectileID.StarWrath,				ItemID.StarWrath),				//狂星之怒，狂星之怒
				(ProjectileID.InfernoFriendlyBlast,		ItemID.InfernoFork),			//狱火爆破弹，狱火叉
				(ProjectileID.NebulaArcanum,			ItemID.NebulaArcanum),			//星云奥秘，星云奥秘
				(ProjectileID.ShadowBeamFriendly,		ItemID.ShadowbeamStaff),		//暗影光束，暗影束法杖
				(ProjectileID.MagnetSphereBall,			ItemID.MagnetSphere),			//磁球珠，磁球
				(ProjectileID.ToxicCloud,				ItemID.ToxicFlask),				//毒云，毒气瓶
				(ProjectileID.HeatRay,					ItemID.HeatRay),				//高温射线枪，高温射线枪
				(ProjectileID.SolarWhipSword,			ItemID.SolarEruption),			//日耀喷发剑，日耀喷发剑
				(ProjectileID.NebulaBlaze1,				ItemID.NebulaBlaze),			//星云烈焰，星云烈焰
				(ProjectileID.VampireKnife,				ItemID.VampireKnives),			//吸血鬼飞刀，吸血鬼刀
				(ProjectileID.Typhoon,					ItemID.RazorbladeTyphoon),		//台风，利刃台风
				(ProjectileID.FrostBoltSword,			ItemID.Frostbrand),				//寒霜矢，寒霜剑
				(ProjectileID.Stake,					ItemID.StakeLauncher),			//尖桩，尖桩发射器
				(ProjectileID.Phantasm,					ItemID.Phantasm),				//幻影弓箭，幻影弓
				(ProjectileID.StardustCellMinionShot,	ItemID.StardustCellStaff)		//星尘细胞弹，星尘细胞法杖
			};
			//var ProjectileList = new (int ProjectileID, int ItemID)[]//这里有个弹幕与原物品伤害的映射表
			//{
			//	(ProjectileID.AmethystBolt, ItemID.AmethystStaff),
			//	(ProjectileID.TopazBolt,    ItemID.TopazStaff),
			//	(ProjectileID.SapphireBolt, ItemID.SapphireStaff),
			//	(ProjectileID.EmeraldBolt,  ItemID.EmeraldStaff),
			//	(ProjectileID.RubyBolt,     ItemID.RubyStaff),
			//	(ProjectileID.DiamondBolt,  ItemID.DiamondStaff),
			//	(ProjectileID.AmberBolt,    ItemID.AmberStaff)
			//};
			Color[] rainbow = new Color[]//定义一组彩虹色
					{
						new Color(255, 0, 0),   //红
						new Color(255, 127, 0), //橙
						new Color(255, 255, 0), //黄
						new Color(0, 255, 0),   //绿
						new Color(0, 0, 255),   //蓝
						new Color(75, 0, 130),  //靛
						new Color(148, 0, 211)  //紫
					};
			int CurrentIndex = ShotIndex % ProjectileList.Length;
			var CurrentPair = ProjectileList[CurrentIndex];
			int OriginalBaseDamage = ContentSamples.ItemsByType[CurrentPair.ItemID].damage;
			float BoostedBaseDamage = OriginalBaseDamage * (NPC.downedPlantBoss ? 1f : (Main.hardMode ? 0.75f : 0.25f));//两段提升
			int FinalDamage = (int)player.GetDamage(DamageClass.Magic).ApplyTo(BoostedBaseDamage);
			// 椭圆参数
			float EllipseWidth = 40f;//椭圆水平半径（X轴方向）
			float EllipseHeight = 20f;//椭圆垂直半径（Y轴方向）
			Vector2 EllipseCenter = player.Center + new Vector2(0, 10);//椭圆中心在玩家下方一点
			// 计算椭圆上的点
			float Angle = CurrentIndex * MathHelper.TwoPi / ProjectileList.Length + (float)Main.time * 0.05f;
			Vector2 SpawnPosition = EllipseCenter + new Vector2(
				EllipseWidth * MathF.Cos(Angle),//X坐标
				EllipseHeight * MathF.Sin(Angle)//Y坐标
			);
			//沿整个椭圆轨迹生成粒子
			int ParticleCount = 36;//粒子数量
			for (int i = 0; i < ParticleCount; i++)
			{
				float ParticleAngle = i * MathHelper.TwoPi / ParticleCount;
				Vector2 ParticlePos = EllipseCenter + new Vector2(EllipseWidth * MathF.Cos(ParticleAngle),
					EllipseHeight * MathF.Sin(ParticleAngle));
				Dust.NewDustPerfect(ParticlePos, DustID.Smoke, new Vector2(0, -5), 0, rainbow[Main.rand.Next(7)], 1f).noGravity = true;
			}
			Vector2 Target = Main.MouseWorld;
			Vector2 Direction = (Target - SpawnPosition).SafeNormalize(Vector2.Zero);
			Vector2 ShootVelocity = Direction * Item.shootSpeed;
			int pIndex = Projectile.NewProjectile(source, SpawnPosition, ShootVelocity, CurrentPair.ProjectileID, FinalDamage, knockback, player.whoAmI);
			Main.projectile[pIndex].tileCollide = false;
			ShotIndex++;
			Terraria.Audio.SoundEngine.PlaySound(Main.rand.NextBool() ? SoundID.Item57 : SoundID.Item58, player.Center);
			return false;
		}
		public override void ModifyTooltips(List<TooltipLine> tooltips)//静态彩虹
		{
			foreach (TooltipLine line in tooltips)
			{
				if (line.Name == "ItemName") // 为您需要扩展的行添加
				{
					// 估算动画所需宽度对应的空格数。例如，预留约200像素宽度（根据字体估算空格宽度）
					int spacesToAdd = 64; // 这个数值需要根据您的动画实际大小进行测试调整
					line.Text += new string(' ', spacesToAdd);
				}
			}
			/*foreach (TooltipLine line in tooltips)//遍历提示中的所有文本行
			{
				if (line.Mod == "Terraria" && line.Name == "Tooltip0")
				{
					Color[] rainbow = new Color[]//定义一组彩虹色
					{
						new Color(255, 0, 0),   //红
						new Color(255, 127, 0), //橙
						new Color(255, 255, 0), //黄
						new Color(0, 255, 0),   //绿
						new Color(0, 0, 255),   //蓝
						new Color(75, 0, 130),  //靛
						new Color(148, 0, 211)  //紫
					};
					string OriginalText = line.Text;//保存原始的提示文本
					StringBuilder ColoredText = new StringBuilder();//构建新的、带颜色代码的文本
					int colorIndex = 0;//循环选取彩虹色数组中的颜色
					for (int i = 0; i < OriginalText.Length; i++)//为每个字符分配一个颜色=
					{
						Color c = rainbow[colorIndex % rainbow.Length];
						//按照泰拉瑞亚原版颜色代码格式拼接。
						//格式为 [c/RRGGBB:要着色的文字]
						//c.Hex3() 将Color结构转换为6位十六进制RGB字符串（如"FF0000"代表红色）
						//这里为每一个字符单独包裹一个颜色代码
						ColoredText.Append($"[c/{c.Hex3()}:{OriginalText[i]}]");
						colorIndex++;
					}
					line.Text = ColoredText.ToString();//将构建好的、带有静态彩虹色代码的文本赋回给提示行
					break;
				}
			}*/
		}
		public override bool PreDrawTooltipLine(DrawableTooltipLine line, ref int yOffset)
		{
			//获取绘制工具
			var SpriteBatch = Main.spriteBatch;

			//计算彩虹色基础偏移
			float HueOffset = (float)Main.timeForVisualEffects * 0.01f % 1f;

			//获取该行文本的原始位置
			Vector2 Position = new Vector2(line.X, line.Y);

			string PrefixTextureName = Pictures.Magic + "ColorfulStaffB";
			Texture2D PrefixTex = ModContent.Request<Texture2D>(PrefixTextureName).Value;

			// ColorfulStaffB 动画参数
			int PrefixFrameCount = 10;
			int PrefixFrameTime = 5;
			int CurrentPrefixFrame = ((int)Main.timeForVisualEffects / PrefixFrameTime) % PrefixFrameCount;

			//计算每帧的高度
			int PrefixFrameHeight = PrefixTex.Height / PrefixFrameCount;
			int PrefixFrameWidth = PrefixTex.Width;
			//当前帧在纹理中的源矩形
			Microsoft.Xna.Framework.Rectangle PrefixSourceRect = new Microsoft.Xna.Framework.Rectangle(
				0,
				CurrentPrefixFrame * PrefixFrameHeight,
				PrefixFrameWidth,
				PrefixFrameHeight
			);

			//绘制行首动画
			float PrefixScale = 0.4f;
			Vector2 PrefixPosition = Position;
			Microsoft.Xna.Framework.Rectangle PrefixDestRect = new Microsoft.Xna.Framework.Rectangle(
				(int)PrefixPosition.X,
				(int)PrefixPosition.Y - 5,
				(int)(PrefixFrameWidth * PrefixScale),
				(int)(PrefixFrameHeight * PrefixScale)
			);
			SpriteBatch.Draw(PrefixTex, PrefixDestRect, PrefixSourceRect, Color.White * 0.9f);
			//计算文字向右移动的距离（行首图片宽度 + 5像素间隔）
			float PrefixWidth = PrefixFrameWidth * PrefixScale;
			Vector2 TextStartPosition = Position + new Vector2(PrefixWidth + 5, 0);

			float TotalTextWidth = 0; //累计文字总宽度

			for (int i = 0; i < line.Text.Length; i++)
			{
				//计算当前字符的色相
				float CharHue = (HueOffset + i * 0.05f) % 1f;
				Color CharColor = Main.hslToRgb(CharHue, 1f, 0.75f);

				char Character = line.Text[i];
				string SubstringBefore = line.Text.Substring(0, i);
				Vector2 Offset = line.Font.MeasureString(SubstringBefore);
				Vector2 CharPos = TextStartPosition + new Vector2(Offset.X, 0);

				Utils.DrawBorderStringFourWay(SpriteBatch, line.Font, Character.ToString(),
					CharPos.X, CharPos.Y, CharColor, Color.Black, line.Origin);

				//如果是最后一个字符，记录整行文字的总宽度
				if (i == line.Text.Length - 1)
				{
					TotalTextWidth = line.Font.MeasureString(line.Text).X;
				}
			}

			string CurrentTextureName = Pictures.Magic + "ColorfulStaffA";
			Texture2D Tex = ModContent.Request<Texture2D>(CurrentTextureName).Value;

			//ColorfulStaffA 动画参数
			int FrameCount = 5;
			int FrameTime = 5;
			int CurrentFrame = ((int)Main.timeForVisualEffects / FrameTime) % FrameCount;

			//计算每帧的高度
			int FrameHeight = Tex.Height / FrameCount;
			int FrameWidth = Tex.Width;

			if (line.Name != "ItemName")
			{
				//当前帧在纹理中的源矩形
				Microsoft.Xna.Framework.Rectangle SourceRect = new Microsoft.Xna.Framework.Rectangle(
					0,
					CurrentFrame * FrameHeight,
					FrameWidth,
					FrameHeight
				);

				//计算纹理绘制位置：文字起始位置 + 文字总宽度 + 间隔
				Vector2 TexturePosition = TextStartPosition + new Vector2(TotalTextWidth + 5, 0);

				//设置目标绘制矩形
				float Scale = 0.6f;
				Microsoft.Xna.Framework.Rectangle DestRect = new Microsoft.Xna.Framework.Rectangle(
					(int)TexturePosition.X,
					(int)Position.Y - 5,
					(int)(FrameWidth * Scale),
					(int)(FrameHeight * Scale)
				);
				SpriteBatch.Draw(Tex, DestRect, SourceRect, Color.White * 0.9f);
			}
			if (line.Name != "Tooltip4")
			{

			}
				return false;
		}
		/*
		public override bool PreDrawTooltipLine(DrawableTooltipLine line, ref int yOffset)
		{
			var spriteBatch = Main.spriteBatch; //获取绘制该行的SpriteBatch
			float HueOffset = (float)Main.timeForVisualEffects * 0.01f % 1f; //计算基础颜色偏移，使其随时间变化
			Vector2 position = new Vector2(line.X, line.Y); //获取该行文本的字体和位置

			//动画参数常量
			const int FRAME_COUNT = 5; //总帧数
			const int FRAME_TIME = 5; //每帧持续时间
			const float FRAME_SCALE = 0.6f; //动画帧缩放
			const int FRAME_PADDING = 5; //帧与文字之间的间距

			//加载动画纹理
			string CurrentTexture = Pictures.Magic + "ColorfulStaffA";
			Texture2D tex = ModContent.Request<Texture2D>(CurrentTexture).Value;

			//计算动画帧参数
			int CurrentFrame = ((int)Main.timeForVisualEffects / FRAME_TIME) % FRAME_COUNT;
			int FrameHeight = tex.Height / FRAME_COUNT;
			int FrameWidth = tex.Width;

			//当前帧在纹理中的源矩形
			Microsoft.Xna.Framework.Rectangle SourceRect = new Microsoft.Xna.Framework.Rectangle(
				0, //X坐标从0开始
				CurrentFrame * FrameHeight, //Y坐标根据当前帧计算
				FrameWidth, FrameHeight
			);

			//计算动画帧的实际绘制尺寸
			int ScaledFrameWidth = (int)(FrameWidth * FRAME_SCALE);
			int ScaledFrameHeight = (int)(FrameHeight * FRAME_SCALE);

			//在行开头绘制动画帧
			Vector2 FramePosition = new Vector2(position.X, position.Y);
			Microsoft.Xna.Framework.Rectangle DestRect = new Microsoft.Xna.Framework.Rectangle(
				(int)FramePosition.X, (int)FramePosition.Y,
				ScaledFrameWidth, ScaledFrameHeight
			);
			spriteBatch.Draw(tex, DestRect, SourceRect, Color.White * 0.9f); //绘制当前帧

			//计算新的文字起始位置（右移动画帧宽度+间距）
			Vector2 NewTextPosition = new Vector2(position.X + ScaledFrameWidth + FRAME_PADDING, position.Y);

			//绘制每个字符（前景文字）
			float TotalTextWidth = 0; //累计文字总宽度
			for (int i = 0; i < line.Text.Length; i++)
			{
				//计算当前字符的色相，在彩虹光谱上循环，每个字符的色相有轻微偏移，形成渐变
				float CharHue = (HueOffset + i * 0.05f) % 1f;
				Color CharColor = Main.hslToRgb(CharHue, 1f, 0.75f); //饱和度和亮度
																	 //获取当前字符
				char Character = line.Text[i];
				//测量之前所有字符的宽度，以确定当前位置
				string substringBefore = line.Text.Substring(0, i);
				Vector2 offset = line.Font.MeasureString(substringBefore);
				//计算当前字符绘制位置
				Vector2 CharPos = NewTextPosition + new Vector2(offset.X, 0);
				//绘制单个字符
				Utils.DrawBorderStringFourWay(spriteBatch, line.Font, Character.ToString(),
					CharPos.X, CharPos.Y, CharColor, Color.Black, line.Origin);

				//如果是最后一个字符，记录整行文字的总宽度
				if (i == line.Text.Length - 1)
				{
					TotalTextWidth = line.Font.MeasureString(line.Text).X;
				}
			}
			return false;
		}*/
	}
}
/*if (line.Name == "ItemName" || line.Name == "Tooltip0")是这样用的，看看源码TooltipLine的说法
/// <summary>
/// This class serves as a way to store information about a line of tooltip for an item. You will create and manipulate objects of this class if you use the ModifyTooltips hook.
/// </summary>
public class TooltipLine
{
	/// <summary>
	/// The name of the mod adding this tooltip line. This will be "Terraria" for all vanilla tooltip lines.
	/// </summary>
	public readonly string Mod;

	/// <summary>
	/// The name of the tooltip, used to help you identify its function.
	/// </summary>
	public readonly string Name;

	/// <summary>
	/// => $"{Mod}/{Name}"
	/// </summary>
	public string FullName => $"{Mod}/{Name}";

	/// <summary>
	/// The actual text that this tooltip displays.
	/// </summary>
	public string Text;

	/// <summary>
	/// Whether or not this tooltip gives prefix information. This will make it so that the tooltip is colored either green or red.
	/// </summary>
	public bool IsModifier;

	/// <summary>
	/// If isModifier is true, this determines whether the tooltip is colored green or red.
	/// </summary>
	public bool IsModifierBad;

	/// <summary>
	/// This completely overrides the color the tooltip is drawn in. If it is set to null (the default value) then the tooltip's color will not be overridden.
	/// </summary>
	public Color? OverrideColor;

	internal bool OneDropLogo;

	/// <summary>
	/// Creates a tooltip line object with the given mod, identifier name, and text.<para />
	/// These are the names of the vanilla tooltip lines, in the order in which they appear, along with their functions. All of them will have a mod name of "Terraria". Remember that most of these tooltip lines will not exist depending on the item.<para />
	/// <list type="bullet">
	/// <item><description>"ItemName" - The name of the item.</description></item>
	/// <item><description>"Favorite" - Tells if the item is favorited.</description></item>
	/// <item><description>"FavoriteDesc" - Tells what it means when an item is favorited.</description></item>
	/// <item><description>"NoTransfer" - Warning that this item cannot be placed inside itself, used by Money Trough and Void Bag/Vault.</description></item>
	/// <item><description>"Social" - Tells if the item is in a social slot.</description></item>
	/// <item><description>"SocialDesc" - Tells what it means for an item to be in a social slot.</description></item>
	/// <item><description>"Damage" - The damage value and type of the weapon.</description></item>
	/// <item><description>"CritChance" - The critical strike chance of the weapon.</description></item>
	/// <item><description>"Speed" - The use speed of the weapon.</description></item>
	/// <item><description>"NoSpeedScaling" - Whether this item does not scale with attack speed, added by tModLoader.</description></item>
	/// <item><description>"SpecialSpeedScaling" - The multiplier this item applies to attack speed bonuses, added by tModLoader.</description></item>
	/// <item><description>"Knockback" - The knockback of the weapon.</description></item>
	/// <item><description>"FishingPower" - Tells the fishing power of the fishing pole.</description></item>
	/// <item><description>"NeedsBait" - Tells that a fishing pole requires bait.</description></item>
	/// <item><description>"BaitPower" - The bait power of the bait.</description></item>
	/// <item><description>"Equipable" - Tells that an item is equipable.</description></item>
	/// <item><description>"WandConsumes" - Tells what item a tile wand consumes.</description></item>
	/// <item><description>"Quest" - Tells that this is a quest item.</description></item>
	/// <item><description>"Vanity" - Tells that this is a vanity item.</description></item>
	/// <item><description>"Defense" - Tells how much defense the item gives when equipped.</description></item>
	/// <item><description>"PickPower" - The item's pickaxe power.</description></item>
	/// <item><description>"AxePower" - The item's axe power.</description></item>
	/// <item><description>"HammerPower" - The item's hammer power.</description></item>
	/// <item><description>"TileBoost" - How much farther the item can reach than normal items.</description></item>
	/// <item><description>"HealLife" - How much health the item recovers when used.</description></item>
	/// <item><description>"HealMana" - How much mana the item recovers when used.</description></item>
	/// <item><description>"UseMana" - Tells how much mana the item consumes upon usage.</description></item>
	/// <item><description>"Placeable" - Tells if the item is placeable.</description></item>
	/// <item><description>"Ammo" - Tells if the item is ammo.</description></item>
	/// <item><description>"Consumable" - Tells if the item is consumable.</description></item>
	/// <item><description>"Material" - Tells if the item can be used to craft something.</description></item>
	/// <item><description>"Tooltip#" - A tooltip line of the item. # will be 0 for the first line, 1 for the second, etc.</description></item>
	/// <item><description>"EtherianManaWarning" - Warning about how the item can't be used without Etherian Mana until the Eternia Crystal has been defeated.</description></item>
	/// <item><description>"WellFedExpert" - In expert mode, tells that food increases life regeneration.</description></item>
	/// <item><description>"BuffTime" - Tells how long the item's buff lasts.</description></item>
	/// <item><description>"OneDropLogo" - The One Drop logo for yoyos.This is a specially-marked tooltip line that has no text.</description></item>
	/// <item><description>"PrefixDamage" - The damage modifier of the prefix.</description></item>
	/// <item><description>"PrefixSpeed" - The usage speed modifier of the prefix.</description></item>
	/// <item><description>"PrefixCritChance" - The critical strike chance modifier of the prefix.</description></item>
	/// <item><description>"PrefixUseMana" - The mana consumption modifier of the prefix.</description></item>
	/// <item><description>"PrefixSize" - The melee size modifier of the prefix.</description></item>
	/// <item><description>"PrefixShootSpeed" - The shootSpeed modifier of the prefix.</description></item>
	/// <item><description>"PrefixKnockback" - The knockback modifier of the prefix.</description></item>
	/// <item><description>"PrefixAccDefense" - The defense modifier of the accessory prefix.</description></item>
	/// <item><description>"PrefixAccMaxMana" - The maximum mana modifier of the accessory prefix.</description></item>
	/// <item><description>"PrefixAccCritChance" - The critical strike chance modifier of the accessory prefix.</description></item>
	/// <item><description>"PrefixAccDamage" - The damage modifier of the accessory prefix.</description></item>
	/// <item><description>"PrefixAccMoveSpeed" - The movement speed modifier of the accessory prefix.</description></item>
	/// <item><description>"PrefixAccMeleeSpeed" - The melee speed modifier of the accessory prefix.</description></item>
	/// <item><description>"SetBonus" - The set bonus description of the armor set.</description></item>
	/// <item><description>"Expert" - Tells whether the item is from expert-mode.</description></item>
	/// <item><description>"Master" - Whether the item is exclusive to Master Mode.</description></item>
	/// <item><description>"JourneyResearch" - How many more items need to be researched to unlock duplication in Journey Mode.</description></item>
	/// <item><description>"ModifiedByMods" - Whether the item has been modified by any mods and what mods when holding shift, added by tModLoader.</description></item>
	/// <item><description>"BestiaryNotes" - Any bestiary notes, used when hovering items in the bestiary.</description></item>
	/// <item><description>"SpecialPrice" - Tells the alternate currency price of an item.</description></item>
	/// <item><description>"Price" - Tells the price of an item.</description></item>
	/// </list>
	/// </summary>
	/// <param name="mod">The mod instance</param>
	/// <param name="name">The name of the tooltip</param>
	/// <param name="text">The content of the tooltip</param>
	public TooltipLine(Mod mod, string name, string text)
	{
		Mod = mod.Name;
		Name = name;
		Text = text;
	}

	internal TooltipLine(string mod, string name, string text)
	{
		Mod = mod;
		Name = name;
		Text = text;
	}

	internal TooltipLine(string name, string text)
	{
		Mod = "Terraria";
		Name = name;
		Text = text;
	}

	public bool Visible { get; private set; } = true;

	public void Hide() => Visible = false;
}
*/