using BoBo.Content.Buffs.Good;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Runtime.InteropServices;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
namespace BoBo.Content.Accessories.FightAcc
{
	/// <summary>
	///	[c/FFD700:★★★★][c/DAA520:☆]
	///	[c/FF4500:♥][c/FF4500:获得红盾，上限100，+5护盾/s]
	///	[c/87CEFA:♥][c/87CEFA:获得蓝盾，上限100，-1护盾/s，每造成1000伤害获得1护盾]
	///	[c/BBFFFF:◒][c/BBFFFF:两种护盾被破坏时][c/FF4500:补偿][c/BBFFFF:本次损失护盾点数的][c/FF4500:血量][c/BBFFFF:给玩家]
	///	[c/00FF7F:✿][c/00FF7F:复活技：玩家受到致命伤害抵挡该次伤害，并给予][c/FF4500:7秒无敌][c/00FF7F:✿]
	///	[c/00FF7F:✿][c/FF4500:恢复][c/00FF7F:该次伤害][c/FF4500:200%的血量][c/00FF7F:，复活有5分钟冷却][c/00FF7F:✿]
	/// </summary>
	public class LifeQuartzNecklace : ModItem
	{
		public override string Texture => Pictures.FightAcc + Name;
		public override void SetDefaults()
		{
			Item.width = 30;
			Item.height = 32;
			Item.accessory = true;
			Item.rare = ItemRarityID.Green;
			Item.value = Item.sellPrice(1, 0, 0, 0);
		}

		public override void UpdateAccessory(Player player, bool hideVisual)
		{
			player.AddImmuneTime(0, 300);
			var modPlayer = player.GetModPlayer<LifeQuartzPlayer>();
			modPlayer.HasNecklace = true;
		}
		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ItemID.CrossNecklace, 1);//十字项链	
			recipe.AddIngredient(ItemID.CharmofMyths, 1);//神话护身符	
			recipe.AddIngredient(ItemID.LifeFruit, 5);//生命果
			recipe.AddTile(TileID.TinkerersWorkbench);//制作站：工匠作坊
			recipe.Register();
		}
	}
	public class LifeQuartzPlayer : ModPlayer
	{
		public bool HasNecklace;
		public RedShield ShieldRed = new RedShield();
		public BlueShield ShieldBlue = new BlueShield();
		public int ResurrectionCooldown = 0;
		private const int CooldownTime = 300 * 60;//5分钟冷却
		public override void ResetEffects() => HasNecklace = false;

		public override void PostUpdate()
		{
			if (ResurrectionCooldown > 0)
			{
				ResurrectionCooldown--;
				if (ResurrectionCooldown == 0)
					Main.NewText("复活冷却已重置!", Color.Green);
			}
			if (!HasNecklace) return;
			ShieldRed.Update(Player);
			ShieldBlue.Update(Player);
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			if (HasNecklace)
				ShieldBlue.ConvertDamage(hit.Damage);//使用实际伤害值
		}
		public override void UpdateDead()//玩家死亡时强制重置冷却
		{
			ResurrectionCooldown = 0;
		}
		public void ConvertDamage(int damage)
		{
			//添加伤害类型过滤，避免误触发
			if (damage <= 0 || Player.whoAmI != Main.myPlayer)
				return;
			//添加转换系数配置（便于后续平衡调整）
			float ConversionRate = 0.01f;//默认100伤害=1点
			ShieldBlue.AddValue(damage * ConversionRate);
		}
		public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
		{
			if (HasNecklace)
				ShieldBlue.ConvertDamage(hit.Damage);
		}
		public override void OnHurt(Player.HurtInfo Info)
		{
			if (!HasNecklace) return;
			int OriginalDamage = Info.Damage;//记录原始伤害值（用于复活判定）
			bool DamageFullyBlocked = false;
			//红盾优先吸收
			if (ShieldRed.Value > 0)
			{
				float AbsorbedByRed = ShieldRed.AbsorbDamage(Info.Damage);
				Info.Damage -= (int)AbsorbedByRed;
				DamageFullyBlocked = (AbsorbedByRed >= OriginalDamage);
				int HealAmount = (int)(AbsorbedByRed * 1.0f);
				Player.Heal(HealAmount);//回血
				//Player.AddBuff(ModContent.BuffType<LifeQuartzNecklaceBuff>(), 180);
			}
			//红盾未完全吸收时，蓝盾继续吸收
			if (Info.Damage > 0 && ShieldBlue.Value > 0)
			{
				float AbsorbedByBlue = ShieldBlue.AbsorbDamage(Info.Damage);
				Info.Damage -= (int)AbsorbedByBlue;
				DamageFullyBlocked = (AbsorbedByBlue >= Info.Damage);
				int HealAmount = (int)(AbsorbedByBlue * 1.0f);
				Player.Heal(HealAmount);//回血
				//Player.AddBuff(ModContent.BuffType<LifeQuartzNecklaceBuff>(), 180);
			}
			//复活判定
			bool isLethal = (Player.statLife - OriginalDamage) <= 0;
			if (isLethal && ResurrectionCooldown <= 0)
			{
				int Resurrection = Info.Damage;
				//触发复活：完全抵挡此次伤害
				Info.Damage = 0;
				//给予长无敌帧避免连续死亡
				Player.AddBuff(ModContent.BuffType<LifeQuartzNecklaceBuff>(), 420);//无敌时间
				Player.Heal((int)(Resurrection * 10000));
				ResurrectionCooldown = CooldownTime;
				//复活特效
				for (int i = 0; i < 30; i++)
					Dust.NewDust(Player.position, Player.width, Player.height, DustID.RedStarfish, Main.rand.NextFloat(-3, 2), -5, 150, Color.Pink, 1.5f);
			}
		}
	}
	public abstract class ShieldHeart
	{
		public float Value { get; protected set; }
		public float MaxValue { get; protected set; } = 100f;
		public abstract void Update(Player player);
		public virtual void AddValue(float amount)
		{
			Value = MathHelper.Clamp(Value + amount, 0f, MaxValue);
		}
		public float AbsorbDamage(float damage)
		{
			float absorb = Math.Min(Value, damage);
			Value -= absorb;
			return absorb;
		}
	}
	//红盾心：自然恢复
	public class RedShield : ShieldHeart
	{
		public override void Update(Player player)
		{
			AddValue(5f / 60f);//每秒恢复5点
		}
	}
	//蓝盾心：伤害转化
	public class BlueShield : ShieldHeart
	{
		public override void Update(Player player)
		{
			AddValue(-1f / 60f);//每秒减1点
		}

		public void ConvertDamage(int damage)
		{
			AddValue(damage / 1000f);//每1000伤害=1点盾心
		}
	}
	public class ShieldUI : ModSystem
	{
		private const int ShieldSize = 32;//盾心图标尺寸
		private string CurrentTooltip = "";
		private const int TooltipPadding = 8;
		private const int TooltipOffset = 20;

		public override void PostDrawInterface(SpriteBatch SpriteBatch)
		{
			Player Player = Main.LocalPlayer;
			var ModPlayer = Player.GetModPlayer<LifeQuartzPlayer>();
			if (!ModPlayer.HasNecklace) return;
			//获取一个位置
			Vector2 ArmorPos = GetArmorPosition();
			//计算盾心位置
			Vector2 RedShieldPos = ArmorPos - new Vector2(0, ShieldSize + 8);
			Vector2 BlueShieldPos = RedShieldPos - new Vector2(0, ShieldSize + 4);
			//绘制盾心并获取碰撞区域
			Rectangle RedRect = DrawShield(SpriteBatch, RedShieldPos, ModPlayer.ShieldRed.Value, Color.Red);
			Rectangle BlueRect = DrawShield(SpriteBatch, BlueShieldPos, ModPlayer.ShieldBlue.Value, Color.Blue);
			//检测鼠标悬停
			Point MousePos = new Point(Main.mouseX, Main.mouseY);
			CurrentTooltip = "";
			if (RedRect.Contains(MousePos))
			{
				CurrentTooltip = "红盾：护盾恢复2点/s\n比蓝盾优先损失";
				Main.LocalPlayer.mouseInterface = true;
			}
			else if (BlueRect.Contains(MousePos))
			{
				CurrentTooltip = "蓝盾：护盾-1点/s\n每造成1000伤害=1点护盾";
				Main.LocalPlayer.mouseInterface = true;
			}
			//显示提示框
			if (!string.IsNullOrEmpty(CurrentTooltip))
			{
				DrawTooltip(SpriteBatch);
			}
		}
		private Vector2 GetArmorPosition()
		{
			if (!Main.playerInventory || Main.hideUI)//优先判断ESC关闭背包的状态，ESC隐藏背包或玩家主动隐藏UI
				return new Vector2(50, Main.screenHeight - 100 / Main.UIScale);//左下角位置
			else 
				return new Vector2(Main.instance.invBottom + 315, Main.instance.invBottom);//以物品栏中间的底部为标准
		}
		private Rectangle DrawShield(SpriteBatch SpriteBatch, Vector2 Position, float Value, Color Color)
		{
			Texture2D Texture = TextureAssets.Extra[58].Value;
			Rectangle Rect = new Rectangle((int)Position.X, (int)Position.Y, ShieldSize, ShieldSize);
			//绘制盾心背景
			SpriteBatch.Draw(Texture, Rect, Color.Black * 0.5f);
			//盾心填充（与透明度、亮度有关）
			float Opacity = MathHelper.Clamp(Value / 100f, 0.2f, 1f);
			float Brightness = MathHelper.Clamp(Value / 100f, 0.2f, 0.8f);
			Color FillColor = new Color((int)(Color.R * Brightness),
				(int)(Color.G * Brightness), (int)(Color.B * Brightness), Color.A);
			//绘制盾心填充
			SpriteBatch.Draw(Texture, Rect, FillColor * Opacity);
			//绘制发光效果
			float GlowIntensity = MathHelper.Clamp((Value - 80) / 20f, 0f, 1f);
			if (GlowIntensity > 0)
			{
				Color GlowColor = Color.Lerp(Color.Transparent, Color.White, GlowIntensity);
				SpriteBatch.Draw(Texture, Rect, GlowColor * Opacity);
			}
			//绘制数值
			string Text = $"{(int)Value}";
			Vector2 TextSize = FontAssets.ItemStack.Value.MeasureString(Text);
			Vector2 CenterPos = new Vector2(Position.X + ShieldSize / 2, Position.Y + ShieldSize / 2);
			Vector2 TextPos = CenterPos - TextSize / 2;
			Color TextColor = (Value > 70) ? Color.LightGoldenrodYellow : Color.White;
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