#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2024 Ethan Lee and the MonoGame Team
 *
 * Released under the Microsoft Public License.
 * See LICENSE for details.
 */
#endregion

#region Using Statements
using System;
#endregion

namespace Microsoft.Xna.Framework.Graphics
{
	/// <summary>
	/// Describes the view bounds for render-target surface.
	/// </summary>
	[Serializable]
	public struct Viewport
	{
		#region Public Properties

		/// <summary>
		/// The height of the bounds in pixels.
		/// </summary>
		public int Height
		{
			get
			{
				return viewport.h;
			}
			set
			{
				viewport.h = value;
			}
		}

		/// <summary>
		/// The upper limit of depth of this viewport.
		/// </summary>
		public float MaxDepth
		{
			get
			{
				return viewport.maxDepth;
			}
			set
			{
				viewport.maxDepth = value;
			}
		}

		/// <summary>
		/// The lower limit of depth of this viewport.
		/// </summary>
		public float MinDepth
		{
			get
			{
				return viewport.minDepth;
			}
			set
			{
				viewport.minDepth = value;
			}
		}

		/// <summary>
		/// The width of the bounds in pixels.
		/// </summary>
		public int Width
		{
			get
			{
				return viewport.w;
			}
			set
			{
				viewport.w = value;
			}
		}

		/// <summary>
		/// The y coordinate of the beginning of this viewport.
		/// </summary>
		public int Y
		{
			get
			{
				return viewport.y;

			}
			set
			{
				viewport.y = value;
			}
		}

		/// <summary>
		/// The x coordinate of the beginning of this viewport.
		/// </summary>
		public int X
		{
			get
			{
				return viewport.x;
			}
			set
			{
				viewport.x = value;
			}
		}

		/// <summary>
		/// Gets the aspect ratio of this <see cref="Viewport"/>, which is width / height.
		/// </summary>
		public float AspectRatio
		{
			get
			{
				if ((viewport.h != 0) && (viewport.w != 0))
				{
					return (((float) viewport.w) / ((float) viewport.h));
				}
				return 0.0f;
			}
		}

		/// <summary>
		/// Gets or sets a boundary of this <see cref="Viewport"/>.
		/// </summary>
		public Rectangle Bounds
		{
			get
			{
				return new Rectangle(
					viewport.x,
					viewport.y,
					viewport.w,
					viewport.h
				);
			}

			set
			{
				viewport.x = value.X;
				viewport.y = value.Y;
				viewport.w = value.Width;
				viewport.h = value.Height;
			}
		}

		/// <summary>
		/// Returns the subset of the viewport that is guaranteed to be visible on a lower quality display.
		/// </summary>
		public Rectangle TitleSafeArea
		{
			get
			{
				return Bounds;
			}
		}

		#endregion

		#region Internal FNA3D Variables

		internal FNA3D.FNA3D_Viewport viewport;

		#endregion

		#region Public Constructors

		/// <summary>
		/// Constructs a viewport from the given values. The <see cref="MinDepth"/> will be 0.0 and <see cref="MaxDepth"/> will be 1.0.
		/// </summary>
		/// <param name="x">The x coordinate of the upper-left corner of the view bounds in pixels.</param>
		/// <param name="y">The y coordinate of the upper-left corner of the view bounds in pixels.</param>
		/// <param name="width">The width of the view bounds in pixels.</param>
		/// <param name="height">The height of the view bounds in pixels.</param>
		public Viewport(int x, int y, int width, int height)
		{
			viewport.x = x;
			viewport.y = y;
			viewport.w = width;
			viewport.h = height;
			viewport.minDepth = 0.0f;
			viewport.maxDepth = 1.0f;
		}

		/// <summary>
		/// Constructs a viewport from the given values.
		/// </summary>
		/// <param name="bounds">A <see cref="Rectangle"/> that defines the location and size of the <see cref="Viewport"/> in a render target.</param>
		public Viewport(Rectangle bounds)
		{
			viewport.x = bounds.X;
			viewport.y = bounds.Y;
			viewport.w = bounds.Width;
			viewport.h = bounds.Height;
			viewport.minDepth = 0.0f;
			viewport.maxDepth = 1.0f;
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Projects a <see cref="Vector3"/> from world space into screen space.
		/// </summary>
		/// <param name="source">The <see cref="Vector3"/> to project.</param>
		/// <param name="projection">The projection <see cref="Matrix"/>.</param>
		/// <param name="view">The view <see cref="Matrix"/>.</param>
		/// <param name="world">The world <see cref="Matrix"/>.</param>
		/// <returns></returns>
		public Vector3 Project(
			Vector3 source,
			Matrix projection,
			Matrix view,
			Matrix world
		) {
			Matrix matrix = Matrix.Multiply(
				Matrix.Multiply(world, view),
				projection
			);
			Vector3 vector = Vector3.Transform(source, matrix);

			float a = (((source.X * matrix.M14) + (source.Y * matrix.M24)) + (source.Z * matrix.M34)) + matrix.M44;
			if (!MathHelper.WithinEpsilon(a, 1.0f))
			{
				vector.X = vector.X / a;
				vector.Y = vector.Y / a;
				vector.Z = vector.Z / a;
			}

			vector.X = (((vector.X + 1f) * 0.5f) * Width) + X;
			vector.Y = (((-vector.Y + 1f) * 0.5f) * Height) + Y;
			vector.Z = (vector.Z * (MaxDepth - MinDepth)) + MinDepth;
			return vector;
		}

		/// <summary>
		/// Unprojects a <see cref="Vector3"/> from screen space into world space.
		/// </summary>
		/// <param name="source">The <see cref="Vector3"/> to unproject.</param>
		/// <param name="projection">The projection <see cref="Matrix"/>.</param>
		/// <param name="view">The view <see cref="Matrix"/>.</param>
		/// <param name="world">The world <see cref="Matrix"/>.</param>
		/// <returns></returns>
		public Vector3 Unproject(Vector3 source, Matrix projection, Matrix view, Matrix world)
		{
			Matrix matrix = Matrix.Invert(
				Matrix.Multiply(
					Matrix.Multiply(world, view),
					projection
				)
			);
			source.X = (((source.X - X) / ((float) Width)) * 2f) - 1f;
			source.Y = -((((source.Y - Y) / ((float) Height)) * 2f) - 1f);
			source.Z = (source.Z - MinDepth) / (MaxDepth - MinDepth);
			Vector3 vector = Vector3.Transform(source, matrix);

			float a = (
				((source.X * matrix.M14) + (source.Y * matrix.M24)) +
				(source.Z * matrix.M34)
			) + matrix.M44;
			if (!MathHelper.WithinEpsilon(a, 1.0f))
			{
				vector.X = vector.X / a;
				vector.Y = vector.Y / a;
				vector.Z = vector.Z / a;
			}

			return vector;
		}

		/// <summary>
		/// Returns a <see cref="String"/> representation of this <see cref="Viewport"/> in the format:
		/// {X:[<see cref="X"/>] Y:[<see cref="Y"/>] Width:[<see cref="Width"/>] Height:[<see cref="Height"/>] MinDepth:[<see cref="MinDepth"/>] MaxDepth:[<see cref="MaxDepth"/>]}
		/// </summary>
		/// <returns>A <see cref="String"/> representation of this <see cref="Viewport"/>.</returns>
		public override string ToString()
		{
			return (
				"{" +
				"X:" + viewport.x.ToString() +
				" Y:" + viewport.y.ToString() +
				" Width:" + viewport.w.ToString() +
				" Height:" + viewport.h.ToString() +
				" MinDepth:" + viewport.minDepth.ToString() +
				" MaxDepth:" + viewport.maxDepth.ToString() +
				"}"
			);
		}

		#endregion
	}
}
