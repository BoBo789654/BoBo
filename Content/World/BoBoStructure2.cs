using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent.Generation;
using Terraria.ID;
using Terraria.IO;
using Terraria.ModLoader;
using Terraria.WorldBuilding;

namespace BoBo.Content.World
{
	public class BoBoStructure2 : ModSystem//别用
	{
		public override void ModifyWorldGenTasks(List<GenPass> tasks, ref double totalWeight)
		{
			int dungeonIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Dungeon"));//在地牢生成后插入宫殿生成
			if (dungeonIndex != -1)
				tasks.Insert(dungeonIndex + 1, new PassLegacy("BoBoStructure2", GeneratePalace));
		}
		private void GeneratePalace(GenerationProgress progress, GameConfiguration config)
		{
			progress.Message = "生成BoBo结构2";//设置进度条显示文本
			int count = WorldGen.genRand.Next(1, 1);
			for (int i = 0; i < count; i++)
			{
				int x = WorldGen.genRand.Next(Main.maxTilesX / 3, Main.maxTilesX * 2 / 3);
				int surfaceHeight = (int)Main.worldSurface;
				int groundY = 0;
				for (int y = surfaceHeight; y < Main.worldSurface + 200; y++)
				{
					if (Main.tile[x, y] != null && Main.tile[x, y].HasTile && Main.tileSolid[Main.tile[x, y].TileType])
					{
						groundY = y;
						break;
					}
				}
				if (groundY > 0)
					GenerateSinglePalace(x, groundY - 5);
			}
		}
		public void GenerateSinglePalace(int originX, int originY)
		{

		}
	}
}