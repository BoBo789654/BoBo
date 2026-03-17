using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
namespace BoBo.Content.Systems
{
	public struct CustomVertexInfo : IVertexType//自定义顶点信息结构体，用于顶点绘制
	{
		private static VertexDeclaration _vertexDeclaration = new VertexDeclaration(new VertexElement[3]
		{
				new VertexElement(0, VertexElementFormat.Vector2, VertexElementUsage.Position, 0),
				new VertexElement(8, VertexElementFormat.Color, VertexElementUsage.Color, 0),
				new VertexElement(12, VertexElementFormat.Vector3, VertexElementUsage.TextureCoordinate, 0)
		});

		/// <summary>
		/// 顶点位置(世界坐标)
		/// </summary>
		public Vector2 Position;
		/// <summary>
		/// 绘制的颜色
		/// </summary>
		public Color Color;

		/// <summary>
		/// 前两个是纹理坐标，x,y是UV坐标，z用于自定义数据
		/// </summary>
		public Vector3 TexCoord;

		public CustomVertexInfo(Vector2 position, Color color, Vector3 texCoord)
		{
			this.Position = position;
			this.Color = color;
			this.TexCoord = texCoord;
		}
		public VertexDeclaration VertexDeclaration => _vertexDeclaration;
	}
}