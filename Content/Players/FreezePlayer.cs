using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameInput;
using Terraria.ModLoader;

namespace BoBo.Content.Players
{
	public class FreezePlayer : ModPlayer//从蛮荒抄来的时停方法 使用方法：//player.GetModPlayer<FreezePlayer>().FreezeTime = 2;
	{
		public int FreezeTime = 0;
		public int Timer = 0;
		public override void ProcessTriggers(TriggersSet triggersSet)
		{
			if (FreezeTime > 0)
			{
				Player.controlUp = false;
				Player.controlDown = false;
				Player.controlLeft = false;
				Player.controlRight = false;
				Player.controlJump = false;
				Player.controlUseItem = false;
				Player.cursed = true;
				Player.noItems = true;
				Player.lavaImmune = true;
				Player.fireWalk = true;
				Player.breath = Player.breathMax;
				Player.lifeRegen = 0;
			}
		}

		public override void NaturalLifeRegen(ref float regen)
		{
			if (FreezeTime > 0)
			{
				regen = 0;//强制自然回血速度为0
			}
		}
		public override bool CanUseItem(Item item)
		{
			if (item.healLife > 0 && FreezeTime > 0) //检测是否为回血物品
				return false; // 禁止使用
			return base.CanUseItem(item);
		}
		public Vector2 OLDVel_TimeFreeze = Vector2.Zero;
		public override void PreUpdate()
		{
			if (Timer > 0)
			{
				Timer--;
				if (Timer % 10 == 0)
				{
					Player.statLife++;
				}
			}
			if (FreezeTime > 0)
			{
				FreezeTime--;
				if (OLDVel_TimeFreeze == Vector2.Zero)
				{
					OLDVel_TimeFreeze = Player.velocity;
				}
				else
				{
					Player.position = Player.oldPosition;
				}
			}
			else
			{
				if (OLDVel_TimeFreeze != Vector2.Zero)
				{
					Player.velocity = OLDVel_TimeFreeze;
					OLDVel_TimeFreeze = Vector2.Zero;
				}
			}
			if (FreezeTime > 0)
			{
				Player.controlUp = false;
				Player.controlDown = false;
				Player.controlLeft = false;
				Player.controlRight = false;
				Player.controlJump = false;
				Player.controlUseItem = false;
				Player.cursed = true;
				Player.noItems = true;
				Player.lavaImmune = true;
				Player.fireWalk = true;
				Player.breath = Player.breathMax;
			}
		}
		public override bool FreeDodge(Player.HurtInfo info)
		{
			if (FreezeTime > 0)
			{
				return true;
			}
			return false;
		}
		public override void OnRespawn()
		{
			FreezeTime = 0;
			base.OnRespawn();
		}
		public override void OnHitAnything(float x, float y, Entity victim)
		{
			if (FreezeTime > 0)
			{
				Player.immune = true;
			}
			base.OnHitAnything(x, y, victim);
		}
	}
	public class FreezeNPC : GlobalNPC
	{
		public override bool InstancePerEntity => true;
		public Vector2 OLDVel_TimeFreeze = Vector2.Zero;
		public override bool PreAI(NPC npc)
		{
			if (Main.LocalPlayer.GetModPlayer<FreezePlayer>().FreezeTime > 0)
			{
				if (OLDVel_TimeFreeze == Vector2.Zero)
				{
					OLDVel_TimeFreeze = npc.velocity;
				}
				else
				{
					npc.position = npc.oldPosition;
				}
				return false;
			}
			else
			{
				if (OLDVel_TimeFreeze != Vector2.Zero)
				{
					npc.velocity = OLDVel_TimeFreeze;
					OLDVel_TimeFreeze = Vector2.Zero;
				}
			}
			return base.PreAI(npc);
		}
	}
	public class FreezeProjectile : GlobalProjectile
	{
		public override bool InstancePerEntity => true;
		private Vector2 OLDVel_TimeFreeze = Vector2.Zero;
		private int TimeLeft_Freeze = 0;
		public override bool PreAI(Projectile projectile)
		{
			if (Main.LocalPlayer.GetModPlayer<FreezePlayer>().FreezeTime > 0)
			{
				Main.ParticleSystem_World_OverPlayers.Update();
				if (OLDVel_TimeFreeze == Vector2.Zero)
				{
					TimeLeft_Freeze = projectile.timeLeft;
					OLDVel_TimeFreeze = projectile.velocity;

				}
				else
				{
					projectile.timeLeft = 9999;
					projectile.position = projectile.oldPosition;
				}
				return false;
			}
			else
			{
				if (OLDVel_TimeFreeze != Vector2.Zero)
				{
					projectile.velocity = OLDVel_TimeFreeze;
					projectile.timeLeft = TimeLeft_Freeze;
					TimeLeft_Freeze = 0;
					OLDVel_TimeFreeze = Vector2.Zero;
				}
			}
			return base.PreAI(projectile);
		}
	}
	internal abstract class DummyTile : ModTile
	{
		public virtual int DummyType { get; }

		public Projectile Dummytile(int i, int j)
		{
			for (int k = 0; k < Main.maxProjectiles; k++)
			{
				var proj = Main.projectile[k];
				if (proj.active && proj.type == DummyType && (proj.position / 16).ToPoint16() == new Point16(i, j))
					return proj;
			}

			return null;
		}
		public virtual bool SpawnConditions(int i, int j)
		{
			Tile tile = Main.tile[i, j];
			return tile.TileFrameX == 0 && tile.TileFrameY == 0;
		}
		public static bool TileSotersExists(int i, int j, int type)
		{
			for (int k = 0; k < Main.maxProjectiles; k++)
			{
				var proj = Main.projectile[k];
				if (proj.active && proj.type == type && (proj.position / 16).ToPoint16() == new Point16(i, j))
					return true;
			}

			return false;
		}

		public virtual void PostSpawnTileSoters(Projectile tSoters) { }

		public virtual void SafeNearbyEffects(int i, int j, bool closer) { }

		/// <summary>
		/// check the dummy have a vaild condition to spawn(tile framexy = 0)
		/// </summary>
		/// <param name="i"></param>
		/// <param name="j"></param>
		/// <returns></returns>
		public sealed override void NearbyEffects(int i, int j, bool closer)
		{

			if (!Main.tileFrameImportant[Type] || SpawnConditions(i, j))
			{
				int type = DummyType;
				var tSoters = Dummytile(i, j);
				//spawn dummy proj
				if (tSoters is null)
				{
					Projectile p = new Projectile();
					p.SetDefaults(type);
					var spawnPos = new Vector2(i, j) * 16 + p.Size / 2;
					if (Main.LocalPlayer.GetModPlayer<FreezePlayer>().FreezeTime <= 0)
					{
						Projectile.NewProjectile(new EntitySource_Parent(Main.LocalPlayer), spawnPos, Vector2.Zero, type, 1, 0);
					}
					PostSpawnTileSoters(tSoters);
				}
			}
			SafeNearbyEffects(i, j, closer);
		}
	}
	public class InputBlocker : ModSystem
	{

		public override void PreUpdateWorld()
		{
			if (Main.LocalPlayer.GetModPlayer<FreezePlayer>().FreezeTime > 0)
			{
				Main.dayRate = 0; // 时间停止
				Main.time--;      // 抵消引擎自动+1
			}
			else
			{
				Main.dayRate = 1; // 恢复时间
			}
		}
	}
}
