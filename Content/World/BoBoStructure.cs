using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent.Generation;
using Terraria.ID;
using Terraria.IO;
using Terraria.ModLoader;
using Terraria.WorldBuilding;

namespace BoBo.Content.World
{
	public class BoBoStructure : ModSystem
	{
		public override void ModifyWorldGenTasks(List<GenPass> tasks, ref double totalWeight)
		{
			//在液体稳定步骤后插入生成过程
			int settleIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Settle Liquids"));
			if (settleIndex != -1)
			{	
				//在液体稳定后立即插入自定义生成任务
				tasks.Insert(settleIndex + 1, new PassLegacy("BoBo  Structures1", GenerateHellStructures));
			}
		}
		private void GenerateHellStructures(GenerationProgress progress, GameConfiguration config)
		{
			progress.Message = "生成BoBo建筑1";//设置进度条显示文本
			int count = WorldGen.genRand.Next(1, 3);//随机生成随机个建筑
			for (int i = 0; i < count; i++)
			{
				int x = WorldGen.genRand.Next(100, Main.maxTilesX - 100);//随机选择X位置，避免边界生成
				int y = Main.maxTilesY - WorldGen.genRand.Next(80, 200);//确保Y位置在地狱层
				GenerateSingleBuilding(x, y);//在目标位置生成单个建筑
			}
		}
		//已知WorldUtils.Gen()只有三个参数，分别是创建的原点、形状以及对其的改动
		//PlaceTile和SetTile不完全相同，SetTile的效果很不好
		public void GenerateSingleBuilding(int originX, int originY)
		{
			#region 圆形创建
			WorldUtils.Gen(
				new Point(originX, originY),
				new Shapes.Circle(30),//这个圆的半径为30
				new Actions.PlaceTile(TileID.CrystalBlock));//创造方块
			#endregion
			#region 圆形消去
			WorldUtils.Gen(
				new Point(originX, originY),
				new Shapes.Circle(20),//这个圆的半径为20
				new Actions.ClearTile());//消去方块
			WorldUtils.Gen(
				new Point(originX, originY),
				new Shapes.Circle(30),//这个圆的半径为20
				new Actions.ClearWall());//消去方块
			#endregion
			int originYY = originY + 10;
			//创建基底
			int width = 12;
			int height = 4;
			WorldUtils.Gen(
				new Point(originX, originYY),
				new Shapes.Rectangle(new Rectangle(-width / 2, -height / 2, width, height)),
				Actions.Chain(
					new Actions.ClearTile(),
					new Actions.PlaceTile(TileID.DynastyWood)
				)
			);
			//建造砖墙面
			int width2 = 10;
			int height2 = 8;
			WorldUtils.Gen(
				new Point(originX, originYY - 6),
				new Shapes.Rectangle(new Rectangle(-width2 / 2, -height2 / 2, width2, height2)),
				Actions.Chain(
					new Actions.ClearTile(),
					new Actions.PlaceWall(WallID.OrangeStainedGlass)
				)
			);
			//添加建筑上层结构
			int width3 = 12;
			int height3 = 2;
			WorldUtils.Gen(
				new Point(originX - 0, originYY - 10),
				new Shapes.Rectangle(new Rectangle(-width3 / 2, -height3 / 2, width3, height3)),
				new Actions.PlaceTile(TileID.Coralstone)
			);
			int width4 = 10;
			int height4 = 2;
			WorldUtils.Gen(
				new Point(originX - 0, originYY - 12),
				new Shapes.Rectangle(new Rectangle(-width4 / 2, -height4 / 2, width4, height4)),
				new Actions.PlaceTile(TileID.Coralstone)
			);
			int width5 = 8;
			int height5 = 2;
			WorldUtils.Gen(
				new Point(originX - 0, originYY - 14),
				new Shapes.Rectangle(new Rectangle(-width5 / 2, -height5 / 2, width5, height5)),
				new Actions.PlaceTile(TileID.Coralstone)
			);
			//侧面装饰性空洞
			int width6 = 3;
			int height6 = 2;
			WorldUtils.Gen(
				new Point(originX + 3, originYY),//熔炉放这
				new Shapes.Rectangle(new Rectangle(-width6 / 2, -height6 / 2, width6, height6)),
				Actions.Chain(
					new Actions.ClearTile(),
					new Actions.PlaceWall(WallID.Lavafall)
				)
			);
			int width7 = 2;
			int height7 = 1;
			WorldUtils.Gen(
				new Point(originX - 3, originYY),//砧放这
				new Shapes.Rectangle(new Rectangle(-width7 / 2, -height7 / 2, width7, height7)),
				Actions.Chain(
					new Actions.ClearTile(),
					new Actions.PlaceWall(WallID.Waterfall)
				)
			);
			//添加平台
			int width8 = 14;
			int height8 = 1;
			WorldUtils.Gen(
				new Point(originX + 0, originYY - 3),
				new Shapes.Rectangle(new Rectangle(-width8 / 2, -height8 / 2, width8, height8)),
				new Actions.SetTile(TileID.TeamBlockWhitePlatform, true, true)
			);
			//工作站生成
			WorldGen.PlaceTile(originX + 3, originYY, TileID.AdamantiteForge, forced: true);//精金熔炉生成在空洞
			WorldGen.PlaceTile(originX - 4, originYY, TileID.MythrilAnvil, forced: true);//秘银砧生成在空洞
			//箱子生成
			int chestIndex = WorldGen.PlaceChest(
				originX + 3, originYY - 4,
				type: 21,          
				notNearOtherChests: false,
				style: 0
			);
			if (chestIndex != -1)
			{
				Chest chest = Main.chest[chestIndex];
				for (int i = 0; i < 40; i++)
				{
					chest.item[i] = new Item();
				}
				AddItemToChest(chest, ItemID.Hellstone, stack: Main.rand.Next(1000, 3000));//地狱石
				AddItemToChest(chest, ItemID.Obsidian, stack: Main.rand.Next(1000, 3000));//黑曜石
				AddItemToChest(chest, ItemID.FieryGreatsword);//烈焰巨剑
				AddItemToChest(chest, ItemID.BottomlessLavaBucket);//熔岩桶
			}
		}
		private void AddItemToChest(Chest chest, int itemId, int stack = 1, byte prefix = 0)
		{
			for (int i = 0; i < chest.item.Length; i++)
			{
				if (chest.item[i].IsAir)
				{
					chest.item[i] = new Item();
					chest.item[i].SetDefaults(itemId);
					chest.item[i].stack = stack;
					chest.item[i].Prefix(prefix);
					break;
				}
			}
		}

	}
}
/*
#region 矩形创建，左上角原点
WorldUtils.Gen(
	new Point(originX, originY),
	new Shapes.Rectangle(10, 15),//一个宽10格，高15格的矩形
	new Actions.PlaceTile(TileID.WoodBlock));
#endregion
#region 矩形创建，矩形中心原点
int width = 10;
int height = 15;
//实际上就是左移宽度的一半再上移高度的一半
WorldUtils.Gen(
	new Point(originX, originY),
	new Shapes.Rectangle(new Rectangle(-width / 2, -height / 2, width, height)),
	new Actions.PlaceTile(TileID.IceBlock));
#endregion
#region 链式创建Actions.Chain(意思是按顺序创建
WorldUtils.Gen(
	origin,
	new Shapes.Circle(10),
	Actions.Chain(
	    new Actions.ClearTile(),
	    new Actions.PlaceTile(TileID.Coralstone)
    )
);
#endregion
private void SafePlaceObject(int x, int y, int type, int style = 0)
{
	if (!WorldGen.InWorld(x, y)) return;//检查坐标是否在有效世界范围内
	Tile tile = Main.tile[x, y];//获取目标位置的物块数据
	if (tile.HasTile && Main.tileSolid[tile.TileType])//验证：当前位置必须有物块且是固体物块
	{
		WorldGen.PlaceObject(x, y, type, mute: true, style: style);//安全放置对象（type为物块ID，style为变体样式）
		WorldGen.SquareTileFrame(x, y);//刷新物块贴图避免显示错误
	}
	//以下写在GenerateSingleBuilding(int originX, int originY)中
	//放置物体
	SafePlaceObject(originX + 4, originY - 2, TileID.Crystals, style: 1);
	SafePlaceObject(originX + 2, originY - 4, TileID.Hellforge);
	//刷新贴图
	WorldGen.SquareTileFrame(originX + 4, originY - 2);
	WorldGen.SquareTileFrame(originX + 2, originY - 4);
}
*/