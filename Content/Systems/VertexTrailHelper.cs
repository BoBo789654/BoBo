using BoBo.Content.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;

namespace BoBo.Content.Projectiles.Weapons.Melee
{
	public static class VertexTrailHelper//顶点拖尾辅助类
	{
		/// <summary>
		/// 弹幕拖尾(弹幕，拖尾贴图，拖尾偏移，拖尾颜色1，拖尾颜色2, 拖尾宽度，是否采用拖尾逐渐缩小)
		/// </summary>
		public static void ProjectileDrawTail(Projectile Projectile, Texture2D Tail, Vector2 DrawOrigin, Color TailColor1, Color TailColor2, float Width, bool Lerp)
		{
			Vector2 drawOrigin = DrawOrigin;
			List<CustomVertexInfo> bars = new List<CustomVertexInfo>();
			for (int i = 1; i < Projectile.oldPos.Length; ++i)
			{
				if (Projectile.oldPos[i] == Vector2.Zero) break;
				var normalDir = Projectile.oldPos[i - 1] - Projectile.oldPos[i];
				normalDir = Vector2.Normalize(new Vector2(-normalDir.Y, normalDir.X));
				float scale = Projectile.scale * ((float)(Projectile.oldPos.Length - i) / (float)Projectile.oldPos.Length);
				float width = Width;
				if (Lerp)
				{
					width = Width * scale;
				}
				var factor = i / (float)Projectile.oldPos.Length;
				var w = MathHelper.Lerp(1f, 0.05f, factor);
				var color = Color.Lerp(TailColor1, TailColor2, factor);
				color.A = 0;
				Vector2 offset = Vector2.Zero;
				bars.Add(new CustomVertexInfo(Projectile.oldPos[i] + normalDir * width + drawOrigin - Main.screenPosition + offset, color, new Vector3(factor, 1, w)));
				bars.Add(new CustomVertexInfo(Projectile.oldPos[i] + normalDir * -width + drawOrigin - Main.screenPosition + offset, color, new Vector3(factor, 0, w)));
			}
			List<CustomVertexInfo> Vx = new List<CustomVertexInfo>();
			if (bars.Count > 2)
			{
				Vx.Add(bars[0]);
				Vx.Add(bars[1]);
				Vx.Add(bars[2]);
				for (int i = 0; i < bars.Count - 2; i += 2)
				{
					Vx.Add(bars[i]);
					Vx.Add(bars[i + 2]);
					Vx.Add(bars[i + 1]);

					Vx.Add(bars[i + 1]);
					Vx.Add(bars[i + 2]);
					Vx.Add(bars[i + 3]);
				}
			}
			Main.graphics.GraphicsDevice.Textures[0] = Tail;
			Main.graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, Vx.ToArray(), 0, Vx.Count / 3);
		}
	}
}