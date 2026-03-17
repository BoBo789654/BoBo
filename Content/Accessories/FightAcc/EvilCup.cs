using BoBo.Content.Common;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

namespace BoBo.Content.Accessories.FightAcc
{
	/// <summary>
	/// [c/D4AF37:牺牲召唤栏时，获得等量“牺牲”标记，上限100层]
	/// [c/FF4500:自身造成伤害时，获得“游魂”标记，该标记会衰减]
	/// [c/2C3E50:每个攻击一个目标为自己增加1点游魂标记]
	/// [c/CD7F32:免疫所有异常和状态]
	/// [c/2C3E50:每层游魂层数增加30点伤害]
	/// [c/E67E22:增幅技：]
	/// [c/8B0000:自身防御增加30 + 牺牲召唤栏 * 20，最高70]
	/// [c/8B0000:自身伤害增加20 + 牺牲召唤栏 * 3]
	/// [c/8B0000:提升生命恢复15%]
	/// [c/8B0000:鞭子长度与鞭子攻速随牺牲召唤栏数增加]
	/// </summary>
	public class EvilCup : ModItem
	{
		public override string Texture => Pictures.FightAcc + Name;
		public override void SetDefaults()
		{
			Item.width = 32;
			Item.height = 36;
			Item.accessory = true;
			Item.rare = ItemRarityID.Orange;
			Item.value = Item.sellPrice(1, 0, 0, 0);
		}
		public override void UpdateAccessory(Player player, bool hideVisual)
		{
			var modPlayer = player.GetModPlayer<EvilCupPlayer>();
			modPlayer.HasEvilCup = true;
			int sacrificedSlots = ModContent.GetInstance<EvilCupConfig>().SacrificedMinionSlots;//直接使用配置中的牺牲值
			if (sacrificedSlots > player.maxMinions)
				sacrificedSlots = player.maxMinions;
			float whipSpeedBonus = sacrificedSlots * 0.01f;//攻速
			float whipRangeBonus = sacrificedSlots * 0.005f;//鞭长
			player.GetAttackSpeed(DamageClass.SummonMeleeSpeed) += whipSpeedBonus;//攻速
			player.whipRangeMultiplier += whipRangeBonus;//鞭长
			player.maxMinions -= sacrificedSlots;//最大召唤栏
			int defenseBonus = 30 + sacrificedSlots * 20;//防御加成: 30 + 牺牲召唤栏*20 (最高70)
			player.statDefense += Math.Min(defenseBonus, 70);//防御加成: 30 + 牺牲召唤栏*20 (最高70)
			player.GetDamage(DamageClass.Summon) += 0.2f + sacrificedSlots * 0.03f;//伤害加成: 20% + 牺牲召唤栏 * 3%
			player.lifeRegen *= (int)11.5f;//治疗效果提升
			player.potionDelayTime -= 50;
			player.potionDelay = (int)(player.potionDelay * 0.85f);//治疗间隔减少
			player.statLifeMax2 += modPlayer.GhostMarks * 10;
			foreach (int buffID in BaseDeBuff.DeBuffIDs)//免疫所有负面状态
			player.buffImmune[buffID] = true;
		}
		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ItemID.Ruby, 15);//红玉
			recipe.AddIngredient(ItemID.GolfTrophyGold, 1);//高尔夫金奖杯	
			recipe.AddIngredient(ItemID.GolfTrophySilver, 1);//高尔夫银奖杯
			recipe.AddIngredient(ItemID.GolfTrophyBronze, 1);//高尔夫铜奖杯
			recipe.AddTile(TileID.TinkerersWorkbench);//工匠作坊
			recipe.Register();
		}
	}
	public class EvilCupConfig : ModConfig
	{
		public override ConfigScope Mode => ConfigScope.ClientSide;

		[Label("牺牲的召唤栏数")]
		[Tooltip("实际扣除的召唤栏数量，牺牲召唤栏 = 最大召唤栏 - 当前可召唤数量")]
		[Range(0, 100)]
		[DefaultValue(0)]
		public int SacrificedMinionSlots;
	}
	public class EvilCupPlayer : ModPlayer
	{
		public bool HasEvilCup;
		public int GhostMarks;//游魂标记
		private int[] AttackedNPC;//记录已攻击的NPC
		private int Time;//计时器，用于每秒扣除标记
		public int GhostMarksDecline = 30;//掉游魂标记的时间
		public int MaxGhostMarks = 51;//最大游魂标记数
		public override void Initialize()
		{
			GhostMarks = 0;
			AttackedNPC = [];
			Time = 0;
		}
		public override void ResetEffects()
		{
			HasEvilCup = false;
		}
		public override void PostUpdate()
		{
			if (!HasEvilCup) return;
			Time++;
			if (Time >= GhostMarksDecline)
			{
				Time = 0;
				if (GhostMarks > 0)
				{
					GhostMarks--;//每秒扣除1点游魂标记
					int maxLife = Player.statLifeMax2 + GhostMarks * 10;
					if (Player.statLife > maxLife)
						Player.statLife = maxLife;
				}
			}
		}
		public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
		{
			if (!HasEvilCup) return;
			if (Array.IndexOf(AttackedNPC, target.whoAmI) == -1)
			{
				if (GhostMarks < MaxGhostMarks)
					GhostMarks++;
				Array.Resize(ref AttackedNPC, AttackedNPC.Length + 1);
			}
		}
		public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone)
		{
			if (!HasEvilCup) return;
			if (Array.IndexOf(AttackedNPC, target.whoAmI) == -1)
			{
				if (GhostMarks < MaxGhostMarks)
					GhostMarks += 2;
				Array.Resize(ref AttackedNPC, AttackedNPC.Length + 1);
			}
		}
		/*public override void ModifyHitNPCWithItem(Item item, NPC target, ref NPC.HitModifiers modifiers)
		{
			if (HasEvilCup)
				modifiers.FlatBonusDamage += GhostMarks;
		}*/
		public override void ModifyHitNPCWithProj(Projectile proj, NPC target, ref NPC.HitModifiers modifiers)
		{
			if (HasEvilCup)
			{
				if (ProjectileID.Sets.IsAWhip[proj.type])
					modifiers.FlatBonusDamage += GhostMarks * 30;//每层游魂标记+30点鞭子伤害
				else
					modifiers.FlatBonusDamage += 0;
				if (proj.minion || proj.sentry)
				{
					modifiers.FlatBonusDamage += GhostMarks * 1;//每层游魂标记+1点召唤伤害
				}
				else
					modifiers.FlatBonusDamage += 0;
			}
		}
	}
	public class EvilCupUI : ModSystem
	{
		private const int MarkSize = 32; //标记图标尺寸
		private string CurrentTooltip = "";
		private const int TooltipPadding = 8;
		private const int TooltipOffset = 20;

		public override void PostDrawInterface(SpriteBatch SpriteBatch)
		{
			Player Player = Main.LocalPlayer;
			var ModPlayer = Player.GetModPlayer<EvilCupPlayer>();
			if (!ModPlayer.HasEvilCup) return;
			Vector2 BasePos = GetDisplayPosition();//获取位置
			Vector2 MarkPos = BasePos - new Vector2(0, MarkSize + 8);//计算游魂标记位置
			Rectangle MarkRect = DrawGhostMark(SpriteBatch, MarkPos, ModPlayer.GhostMarks, ModPlayer.MaxGhostMarks);//绘制游魂标记
			Point MousePos = new(Main.mouseX, Main.mouseY);//检测鼠标悬停
			CurrentTooltip = "";
			if (MarkRect.Contains(MousePos))
			{
				CurrentTooltip = $"游魂标记: {ModPlayer.GhostMarks}/{ModPlayer.MaxGhostMarks}" +
					$"\n每层增加1点召唤物伤害\n鞭子每层额外增加30点伤害\n每层增加10点最大生命值\n" +
					$"{ModPlayer.GhostMarksDecline / 60}秒未攻击减少1层";
				Main.LocalPlayer.mouseInterface = true;
			}
			if (!string.IsNullOrEmpty(CurrentTooltip))//显示提示框
				DrawTooltip(SpriteBatch);
		}

		private Vector2 GetDisplayPosition()
		{
			//如果有LifeQuartzNecklace，就在蓝盾上方显示
			Player player = Main.LocalPlayer;
			var quartzPlayer = player.GetModPlayer<LifeQuartzPlayer>();
			if (quartzPlayer.HasNecklace)
			{
				//LifeQuartzNecklace同款位置计算
				Vector2 armorPos = GetArmorPosition();
				return armorPos - new Vector2(0, MarkSize * 2 + 12); //在蓝盾上方
			}
			else
			{
				//没有LifeQuartzNecklace时使用默认位置
				if (!Main.playerInventory || Main.hideUI)
					return new Vector2(50, Main.screenHeight - 100 / Main.UIScale);
				else
					return new Vector2(Main.instance.invBottom + 315, Main.instance.invBottom);
			}
		}
		private static Vector2 GetArmorPosition()
		{
			if (!Main.playerInventory || Main.hideUI)
				return new Vector2(50, Main.screenHeight - 100 / Main.UIScale);
			else
				return new Vector2(Main.instance.invBottom + 315, Main.instance.invBottom);
		}

		private static Rectangle DrawGhostMark(SpriteBatch SpriteBatch, Vector2 Position, int CurrentMarks, int MaxMarks)
		{
			Texture2D Texture = TextureAssets.Extra[152].Value;//使用现有纹理
			Rectangle Rect = new Rectangle((int)Position.X, (int)Position.Y, MarkSize, MarkSize);
			float Ratio = (float)CurrentMarks / MaxMarks;//计算填充比例和颜色
			Color Color = Color.Lerp(Color.Gray, Color.Purple, Ratio);
			SpriteBatch.Draw(Texture, Rect, Color.Black * 0.5f);//绘制背景
			float opacity = MathHelper.Clamp(Ratio, 0.3f, 1f);//绘制填充
			SpriteBatch.Draw(Texture, Rect, Color * opacity);
			if (Ratio > 0.7f)//绘制发光效果（层数高时）
			{
				float glowIntensity = (Ratio - 0.7f) / 0.3f;
				Color glowColor = Color.Lerp(Color.Transparent, Color.White, glowIntensity);
				SpriteBatch.Draw(Texture, Rect, glowColor * opacity * 0.5f);
			}
			//绘制层数文本
			string Text = $"{CurrentMarks}";
			Vector2 TextSize = FontAssets.ItemStack.Value.MeasureString(Text);
			Vector2 CenterPos = new Vector2(Position.X + MarkSize / 2, Position.Y + MarkSize / 2);
			Vector2 TextPos = CenterPos - TextSize / 2;
			Color TextColor = (Ratio > 0.8f) ? Color.Gold : Color.White;
			Utils.DrawBorderString(SpriteBatch, Text, TextPos, TextColor);
			return Rect;
		}

		private void DrawTooltip(SpriteBatch SpriteBatch)
		{
			float opacity = 0.35f;
			Vector2 TextSize = FontAssets.MouseText.Value.MeasureString(CurrentTooltip);
			Vector2 BasePosition = new Vector2(Main.mouseX + TooltipOffset, Main.mouseY + TooltipOffset);
			if (BasePosition.X + TextSize.X > Main.screenWidth)
				BasePosition.X = Main.screenWidth - TextSize.X - TooltipPadding;
			if (BasePosition.Y + TextSize.Y > Main.screenHeight)
				BasePosition.Y = Main.screenHeight - TextSize.Y - TooltipPadding;
			Rectangle BackgroundRect = new Rectangle((int)BasePosition.X - TooltipPadding,
				(int)BasePosition.Y - TooltipPadding - 8, (int)TextSize.X + TooltipPadding * 2, (int)TextSize.Y + TooltipPadding * 2);
			SpriteBatch.Draw(TextureAssets.MagicPixel.Value, BackgroundRect, Color.Black * opacity);
			Utils.DrawBorderString(SpriteBatch, CurrentTooltip, BasePosition, Color.White, scale: 1.0f);
		}
	}
}