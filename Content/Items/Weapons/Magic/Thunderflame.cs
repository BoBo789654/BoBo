using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using BoBo.Content.Projectiles.Weapons.Magic;

namespace BoBo.Content.Items.Weapons.Magic
{
	internal class Thunderflame : ModItem//蛮荒里的武器改的
	{
		public override string Texture => Pictures.Magic + Name;
		private float chargeTime = 0f;      //当前蓄力时间
		private int chargeLevel = 0;        //当前蓄力等级
		private bool isCharging = false;    //是否正在蓄力
		private bool manaConsumed = false;  //标记是否已消耗魔法值，防止连点不耗蓝
		public override void SetStaticDefaults()
		{
			ItemID.Sets.LockOnIgnoresCollision[Type] = true;//允许锁定目标时无视碰撞
			//ItemID.Sets.ItemsThatAllowRepeatedRightClick[Type] = true;//允许重复右键点
		}
		public override void SetDefaults()
		{
			Item.width = 60;          
			Item.height = 60;         
			Item.rare = ItemRarityID.Purple; 
			Item.value = Item.sellPrice(0, 12);
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.useTime = 12;        
			Item.useAnimation = 12;   
			Item.autoReuse = false;   
			Item.noMelee = true;       
			Item.DamageType = DamageClass.Magic; 
			Item.damage = 30;         
			Item.crit = 0;           
			Item.knockBack = 6f;    
			Item.shoot = ModContent.ProjectileType<ThunderflameProj>(); 
			Item.shootSpeed = 0f;     
			Item.mana = 10;           
			Item.channel = true;       
			Item.UseSound = null;   
		}
		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ItemID.NimbusRod);//雨云法杖
			recipe.AddIngredient(ItemID.FragmentNebula, 6);//星云碎片
			recipe.AddTile(TileID.LunarCraftingStation);//远古操纵机
			recipe.Register();
		}
		public override void HoldItem(Player player)
		{
			if (Main.myPlayer == player.whoAmI)//只处理本地玩家
			{
				bool isUsingItem = player.controlUseItem || player.controlUseTile;//检测玩家是否按下了攻击键
				if (isUsingItem)
				{
					
					if (!isCharging)//如果当前不在蓄力状态，则开始新的蓄力
						StartCharging(player);
					else//继续蓄力
						ContinueCharging(player);
					player.itemAnimation = Item.useAnimation;//重置玩家的物品动画和时间，防止动画结束
					player.itemTime = Item.useTime;
				}
				else if (isCharging)//松开按键时释放蓄力攻击
					ReleaseCharge(player);
			}
			if (isCharging)//更新蓄力视觉效果
				UpdateChargingEffects(player);
		}
		private void StartCharging(Player player)//开始蓄力，检查魔法值并初始化蓄力状态
		{
			
			if (player.CheckMana(Item.mana, true))//检查魔法值是否足够
			{
				isCharging = true;		//标记为蓄力中
				manaConsumed = true;	//标记已消耗魔法值
				chargeTime = 0f;		//重置蓄力时间
				chargeLevel = 0;		//重置蓄力等级
				player.itemAnimation = Item.useAnimation;
				player.itemTime = Item.useTime;
			}
			else//魔法值不足，无法开始蓄力
			{
				isCharging = false;
				manaConsumed = false;
			}
		}
		private void ContinueCharging(Player player)//继续蓄力，更新蓄力时间和等级
		{
			chargeTime += 0.0167f;//增加蓄力时间
			int newChargeLevel = 0;
			if (chargeTime >= 2f)//根据蓄力时间计算新的蓄力等级
				newChargeLevel = 3;//三
			else if (chargeTime >= 1f)
				newChargeLevel = 2;//二
			else
				newChargeLevel = 1;//一
			
			if (newChargeLevel != chargeLevel)//如果蓄力等级改变，触发相应效果
			{
				chargeLevel = newChargeLevel;
				OnChargeLevelChanged(player, chargeLevel);
			}
			if (chargeTime > 4f)//限制最大蓄力时间
				chargeTime = 4f;
			player.itemAnimation = Item.useAnimation;//保持动画
			player.itemTime = Item.useTime;
		}
		private void ReleaseCharge(Player player)//释放蓄力攻击，根据蓄力等级发射不同数量和威力的弹幕
		{
			if (!isCharging) return;
			int shootCount = 1;//发射的波次数
			float damageMultiplier = 1f;//伤害倍率
			int projectileType = ModContent.ProjectileType<ThunderflameProj>();
			switch (chargeLevel)
			{
				case 1: //蓄力失败
					shootCount = 1;
					damageMultiplier = 1f;//基础伤害
					break;
				case 2: //第二档
					shootCount = 2;
					damageMultiplier = 2f;//伤害倍率
					player.statMana -= (int)(20 * player.manaCost);//额外消耗
					break;
				case 3: //第三档
					shootCount = 3;
					damageMultiplier = 4f;//伤害倍率
					player.statMana -= (int)(40 * player.manaCost);//额外消耗
					break;
			}
			Vector2 direction = Vector2.Normalize(Main.MouseWorld - player.Center);//计算发射方向
			for (int i = 0; i < shootCount; i++)
			{
				Vector2 spawnPos = player.Center;//生成位置
				float speed = 2f;//弹幕速度
				int damage = (int)(Item.damage * damageMultiplier);//计算最终伤害
				//每次发射4个方向（上、下、左、右）
				for (int j = 0; j < 5; j++)
				{
					if(j != 2)
					Projectile.NewProjectile(
						player.GetSource_ItemUse(Item),						//弹幕来源
						spawnPos,											//生成位置
						direction.RotatedBy(-MathHelper.Pi / 6 + MathHelper.Pi * 15 * j / 180) * speed, //旋转90度发射4个方向
						projectileType,                                     //弹幕类型
						(int)(Item.damage * Main.rand.NextFloat(0.85f, 1.15f)),//伤害
						Item.knockBack,										//击退
						player.whoAmI,										//所属玩家
						ai0: chargeLevel,									//传递蓄力等级
						ai1: i												//传递波次索引
					);
				}
			}
			//重置蓄力状态
			isCharging = false;
			manaConsumed = false;
			chargeTime = 0f;
			chargeLevel = 0;
		}
		private void OnChargeLevelChanged(Player player, int newLevel)//当蓄力等级改变时触发，播放音效、造成伤害和生成粒子效果
		{
			switch (newLevel)
			{
				case 2://第二档
					//SoundEngine.PlaySound(SoundID.Item68, player.Center);//播放雷电音效
					//foreach (NPC n in Main.npc)//蓄力爆炸，对范围内的所有敌人造成伤害
					//{
					//	if (n.CanBeChasedBy()) //检查NPC是否可被锁定
					//	{
					//		Vector2 toN = n.Center - player.Center;
					//		if (toN.Length() <= 300) //距离检查
					//			player.ApplyDamageToNPC(n, (int)(Item.damage * Main.rand.NextFloat(0.85f, 1.15f)), Item.knockBack, -n.direction, true);
					//	}
					//}
					for (int i = 0; i < 40; i++)
					{
						Dust dust = Dust.NewDustDirect(
							player.position,			//生成位置
							player.width,				//宽度
							player.height,				//高度
							DustID.Electric,            //粒子
							0f, 0f, 100, default, 1f	//粒子参数
						);
						dust.velocity = Main.rand.NextVector2Circular(15f, 15f);//随机速度
						dust.noGravity = true;//无重力
					}
					break;
				case 3: //第三档
					//SoundEngine.PlaySound(SoundID.Item68, player.Center);//播放雷电音效
					//foreach (NPC n in Main.npc)//蓄力爆炸，对范围内的所有敌人造成伤害
					//{
					//	if (n.CanBeChasedBy())
					//	{
					//		Vector2 toN = n.Center - player.Center;
					//		if (toN.Length() <= 300)
					//			player.ApplyDamageToNPC(n, (int)(Item.damage * Main.rand.NextFloat(0.85f, 1.15f)), Item.knockBack, -n.direction, true);
					//	}
					//}
					for (int i = 0; i < 60; i++)
					{
						Dust dust = Dust.NewDustDirect(
							player.position,
							player.width,
							player.height,
							DustID.Shadowflame,
							0f, 0f, 100, default, 2.5f
						);
						dust.velocity = Main.rand.NextVector2Circular(18f, 18f);
						dust.noGravity = true;
					}
					break;
			}
		}
		private void UpdateChargingEffects(Player player)//更新蓄力时的视觉效果，包括旋转粒子和发光效果
		{
			
			CreateAbsorptionParticles(player);//生成吸收粒子效果
			Color glowColor = Color.White;//根据蓄力等级计算发光颜色和强度
			float glowIntensity = 0.5f + chargeTime * 0.2f;
			switch (chargeLevel)
			{
				case 1: //一档
					glowColor = Color.LightBlue;
					break;
				case 2: //二档
					glowColor = Color.Blue;
					glowIntensity = 1f;
					break;
				case 3: //三档
					glowColor = Color.Purple;
					glowIntensity = 1.5f;
					break;
			}
			float rotation = Main.GlobalTimeWrappedHourly * 3f;//基于时间的旋转
			float radius = 50f + chargeTime * 10f;//半径随蓄力时间增加
			for (int i = 0; i < 3; i++) //生成3个旋转粒子
			{
				float angle = rotation + MathHelper.TwoPi * i / 3f;//等分
				Vector2 offset = angle.ToRotationVector2() * radius;//计算偏移
				Vector2 particlePos = player.Center + offset;//粒子位置
				int dustType = DustID.WhiteTorch;//根据蓄力等级选择粒子类型
				if (chargeLevel >= 2) dustType = DustID.BlueTorch;
				if (chargeLevel >= 3) dustType = DustID.PurpleTorch;
				Dust dust = Dust.NewDustDirect(
					particlePos, 0, 0,          //位置
					dustType,                   //粒子类型
					0f, 0f, 100, default, 1.5f  //粒子参数
				);
				dust.velocity = Vector2.Zero;	//静止粒子
				dust.noGravity = true;			//无重力
			}
		}
		private void CreateAbsorptionParticles(Player player)//创建吸收粒子效果，粒子从周围向玩家中心汇聚
		{
			int particleCount = 5 + (int)(chargeTime * 10);//粒子数量随蓄力时间增加
			for (int i = 0; i < particleCount; i++)
			{
				float distance = 200f;//粒子生成距离
				Vector2 randomPos = player.Center + Main.rand.NextVector2Circular(distance, distance);//在玩家周围随机位置生成粒子
				Vector2 direction = Vector2.Normalize(player.Center - randomPos);//计算朝向玩家中心的方向
				int dustType = DustID.WhiteTorch;
				if (chargeLevel >= 2) dustType = DustID.BlueTorch;
				if (chargeLevel >= 3) dustType = DustID.PurpleTorch;
				Dust dust = Dust.NewDustDirect(randomPos, 0, 0, dustType, 0f, 0f, 100, default, 1f);
				dust.velocity = direction * 5f;//向玩家中心移动
				dust.noGravity = true;
			}
		}
		public override void UpdateInventory(Player player)//每帧更新物品栏，当玩家不再持有此物品时重置蓄力状态
		{
			if (player.HeldItem.type != Type && isCharging)//当玩家切换武器时，重置蓄力状态
			{
				isCharging = false;
				manaConsumed = false;
				chargeTime = 0f;
				chargeLevel = 0;
			}
		}
		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
		{
			position = player.Center;//从玩家中心发射
		}
		public override bool CanUseItem(Player player)
		{
			return Main.myPlayer == player.whoAmI;//仅限本地玩家可以使用物品
		}
		public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)//在世界中绘制物品后的额外绘制
		{
			//只在蓄力时绘制发光效果
			if (isCharging)
			{
				Texture2D texture = TextureAssets.Item[Type].Value;//获取物品纹理
				Vector2 position = Item.position - Main.screenPosition + texture.Size() * 0.5f;//计算绘制位置
				Color glowColor = Color.Lerp(Color.White, Color.Blue, chargeTime / 3f);//根据蓄力时间计算发光颜色
				if (chargeLevel >= 3) glowColor = Color.Purple;//三档时
				spriteBatch.Draw(
					texture,                    //纹理
					position,                   //位置
					null,                       //源矩形
					glowColor * 0.7f,			//颜色和透明度
					rotation,                   //旋转
					texture.Size() * 0.5f,      //旋转中心
					scale * 1.1f,				//缩放
					SpriteEffects.None,			//无特殊效果
					0f							//图层深度
				);
			}
		}
		public override Vector2? HoldoutOffset()
		{
			return new Vector2(-10, 0);//偏移
		}
	}
}