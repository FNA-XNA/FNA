#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2016 Ethan Lee and the MonoGame Team
 *
 * Released under the Microsoft Public License.
 * See LICENSE for details.
 */
#endregion

#region Using Statements
using System;
using System.Runtime.InteropServices;
#endregion

namespace Microsoft.Xna.Framework.Graphics
{
	[Serializable]
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct VertexPositionColorTexture : IVertexType
	{
		#region Private Properties

		VertexDeclaration IVertexType.VertexDeclaration
		{
			get
			{
				return VertexDeclaration;
			}
		}

		#endregion

		#region Public Variables

		public Vector3 Position;
		public Color Color;
		public Vector2 TextureCoordinate;

		#endregion

		#region Public Static Variables

		public static readonly VertexDeclaration VertexDeclaration;

		#endregion

		#region Private Static Constructor

		static VertexPositionColorTexture()
		{
			VertexDeclaration = new VertexDeclaration(
				new VertexElement[]
				{
					new VertexElement(
						0,
						VertexElementFormat.Vector3,
						VertexElementUsage.Position,
						0
					),
					new VertexElement(
						12,
						VertexElementFormat.Color,
						VertexElementUsage.Color,
						0
					),
					new VertexElement(
						16,
						VertexElementFormat.Vector2,
						VertexElementUsage.TextureCoordinate,
						0
					)
				}
			);
		}

		#endregion

		#region Public Constructor

		public VertexPositionColorTexture(
			Vector3 position,
			Color color,
			Vector2 textureCoordinate
		) {
			Position = position;
			Color = color;
			TextureCoordinate = textureCoordinate;
		}

		#endregion

		#region Public Static Operators and Override Methods

		public override int GetHashCode()
		{
			// TODO: Fix GetHashCode
			return 0;
		}

		public override string ToString()
		{
			return (
				"{{Position:" + Position.ToString() +
				" Color:" + Color.ToString() +
				" TextureCoordinate:" + TextureCoordinate.ToString() +
				"}}"
			);
		}

		public static bool operator ==(VertexPositionColorTexture left, VertexPositionColorTexture right)
		{
			return (	(left.Position == right.Position) &&
					(left.Color == right.Color) &&
					(left.TextureCoordinate == right.TextureCoordinate)	);
		}

		public static bool operator !=(VertexPositionColorTexture left, VertexPositionColorTexture right)
		{
			return !(left == right);
		}

		public override bool Equals(object obj)
		{
			if (obj == null)
			{
				return false;
			}

			if (obj.GetType() != base.GetType())
			{
				return false;
			}

			return (this == ((VertexPositionColorTexture) obj));
		}

		#endregion
	}
}
