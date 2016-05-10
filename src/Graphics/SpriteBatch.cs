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
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
#endregion

namespace Microsoft.Xna.Framework.Graphics
{
	/* MSDN Docs:
	 * http://msdn.microsoft.com/en-us/library/microsoft.xna.framework.graphics.spritebatch.aspx
	 * Other References:
	 * http://directxtk.codeplex.com/SourceControl/changeset/view/17079#Src/SpriteBatch.cpp
	 * http://gamedev.stackexchange.com/questions/21220/how-exactly-does-xnas-spritebatch-work
	 */
	public class SpriteBatch : GraphicsResource
	{
		#region Private Constant Values

		// As defined by the HiDef profile spec
		private const int MAX_SPRITES = 2048;
		private const int MAX_VERTICES = MAX_SPRITES * 4;
		private const int MAX_INDICES = MAX_SPRITES * 6;

		// Used to quickly flip text for DrawString
		private static readonly float[] axisDirectionX = new float[]
		{
			-1.0f,
			1.0f,
			-1.0f,
			1.0f
		};
		private static readonly float[] axisDirectionY = new float[]
		{
			-1.0f,
			-1.0f,
			1.0f,
			1.0f
		};
		private static readonly float[] axisIsMirroredX = new float[]
		{
			0.0f,
			1.0f,
			0.0f,
			1.0f
		};
		private static readonly float[] axisIsMirroredY = new float[]
		{
			0.0f,
			0.0f,
			1.0f,
			1.0f
		};

		// Used to calculate texture coordinates
		private static readonly float[] CornerOffsetX = new float[]
		{
			0.0f,
			1.0f,
			0.0f,
			1.0f
		};
		private static readonly float[] CornerOffsetY = new float[]
		{
			0.0f,
			0.0f,
			1.0f,
			1.0f
		};

		#endregion

		#region Private Variables

		// Buffer objects used for actual drawing
		private DynamicVertexBuffer vertexBuffer;
		private IndexBuffer indexBuffer;

		// Local data stored before buffering to GPU
		private VertexPositionColorTexture4[] vertexInfo;
		private Texture2D[] textureInfo;

		// Default SpriteBatch Effect
		private Effect spriteEffect;
		private IntPtr spriteMatrixTransform;
		private EffectPass spriteEffectPass;

		// Tracks Begin/End calls
		private bool beginCalled;

		// Current sort mode
		private SpriteSortMode sortMode;

		// Keep render state for non-Immediate modes.
		private BlendState blendState;
		private SamplerState samplerState;
		private DepthStencilState depthStencilState;
		private RasterizerState rasterizerState;

		// How many sprites are in the current batch?
		private int numSprites;

		// Matrix to be used when creating the projection matrix
		private Matrix transformMatrix;

		// User-provided Effect, if applicable
		private Effect customEffect;

		#endregion

		#region Private Static Variables

		private static readonly short[] indexData = GenerateIndexArray();
		private static readonly byte[] spriteEffectCode = Resources.SpriteEffect;
		private static readonly TextureComparer TextureCompare = new TextureComparer();
		private static readonly BackToFrontComparer BackToFrontCompare = new BackToFrontComparer();
		private static readonly FrontToBackComparer FrontToBackCompare = new FrontToBackComparer();

		#endregion

		#region Public Constructor

		public SpriteBatch(GraphicsDevice graphicsDevice)
		{
			if (graphicsDevice == null)
			{
				throw new ArgumentNullException("graphicsDevice");
			}
			GraphicsDevice = graphicsDevice;

			vertexInfo = new VertexPositionColorTexture4[MAX_SPRITES];
			textureInfo = new Texture2D[MAX_SPRITES];
			vertexBuffer = new DynamicVertexBuffer(
				graphicsDevice,
				typeof(VertexPositionColorTexture),
				MAX_VERTICES,
				BufferUsage.WriteOnly
			);
			indexBuffer = new IndexBuffer(
				graphicsDevice,
				IndexElementSize.SixteenBits,
				MAX_INDICES,
				BufferUsage.WriteOnly
			);
			indexBuffer.SetData(indexData);

			spriteEffect = new Effect(
				graphicsDevice,
				spriteEffectCode
			);
			spriteMatrixTransform = spriteEffect.Parameters["MatrixTransform"].values;
			spriteEffectPass = spriteEffect.CurrentTechnique.Passes[0];

			beginCalled = false;
			numSprites = 0;
		}

		#endregion

		#region Public Dispose Method

		protected override void Dispose(bool disposing)
		{
			if (!IsDisposed && disposing)
			{
				spriteEffect.Dispose();
				indexBuffer.Dispose();
				vertexBuffer.Dispose();
			}
			base.Dispose(disposing);
		}

		#endregion

		#region Public Begin Methods

		public void Begin()
		{
			Begin(
				SpriteSortMode.Deferred,
				BlendState.AlphaBlend,
				SamplerState.LinearClamp,
				DepthStencilState.None,
				RasterizerState.CullCounterClockwise,
				null,
				Matrix.Identity
			);
		}

		public void Begin(
			SpriteSortMode sortMode,
			BlendState blendState
		) {
			Begin(
				sortMode,
				blendState,
				SamplerState.LinearClamp,
				DepthStencilState.None,
				RasterizerState.CullCounterClockwise,
				null,
				Matrix.Identity
			);
		}

		public void Begin(
			SpriteSortMode sortMode,
			BlendState blendState,
			SamplerState samplerState,
			DepthStencilState depthStencilState,
			RasterizerState rasterizerState
		) {
			Begin(
				sortMode,
				blendState,
				samplerState,
				depthStencilState,
				rasterizerState,
				null,
				Matrix.Identity
			);
		}

		public void Begin(
			SpriteSortMode sortMode,
			BlendState blendState,
			SamplerState samplerState,
			DepthStencilState depthStencilState,
			RasterizerState rasterizerState,
			Effect effect
		) {
			Begin(
				sortMode,
				blendState,
				samplerState,
				depthStencilState,
				rasterizerState,
				effect,
				Matrix.Identity
			);
		}

		public void Begin(
			SpriteSortMode sortMode,
			BlendState blendState,
			SamplerState samplerState,
			DepthStencilState depthStencilState,
			RasterizerState rasterizerState,
			Effect effect,
			Matrix transformationMatrix
		) {
			if (beginCalled)
			{
				throw new InvalidOperationException(
					"Begin has been called before calling End" +
					" after the last call to Begin." +
					" Begin cannot be called again until" +
					" End has been successfully called."
				);
			}
			beginCalled = true;

			this.sortMode = sortMode;

			this.blendState = blendState ?? BlendState.AlphaBlend;
			this.samplerState = samplerState ?? SamplerState.LinearClamp;
			this.depthStencilState = depthStencilState ?? DepthStencilState.None;
			this.rasterizerState = rasterizerState ?? RasterizerState.CullCounterClockwise;

			customEffect = effect;
			transformMatrix = transformationMatrix;

			if (sortMode == SpriteSortMode.Immediate)
			{
				PrepRenderState();
			}
		}

		#endregion

		#region Public End Method

		public void End()
		{
			if (!beginCalled)
			{
				throw new InvalidOperationException(
					"End was called, but Begin has not yet" +
					" been called. You must call Begin " +
					" successfully before you can call End."
				);
			}
			beginCalled = false;

			if (sortMode != SpriteSortMode.Immediate)
			{
				FlushBatch();
			}
			customEffect = null;
		}

		#endregion

		#region Public Draw Methods

		public void Draw(
			Texture2D texture,
			Vector2 position,
			Color color
		) {
			CheckBegin("Draw");
			PushSprite(
				texture,
				null,
				position.X,
				position.Y,
				1.0f,
				1.0f,
				color,
				Vector2.Zero,
				0.0f,
				0.0f,
				0,
				false
			);
		}

		public void Draw(
			Texture2D texture,
			Vector2 position,
			Rectangle? sourceRectangle,
			Color color
		) {
			CheckBegin("Draw");
			PushSprite(
				texture,
				sourceRectangle,
				position.X,
				position.Y,
				1.0f,
				1.0f,
				color,
				Vector2.Zero,
				0.0f,
				0.0f,
				0,
				false
			);
		}

		public void Draw(
			Texture2D texture,
			Vector2 position,
			Rectangle? sourceRectangle,
			Color color,
			float rotation,
			Vector2 origin,
			float scale,
			SpriteEffects effects,
			float layerDepth
		) {
			CheckBegin("Draw");
			PushSprite(
				texture,
				sourceRectangle,
				position.X,
				position.Y,
				scale,
				scale,
				color,
				origin,
				rotation,
				layerDepth,
				(byte) (effects & (SpriteEffects) 0x03),
				false
			);
		}

		public void Draw(
			Texture2D texture,
			Vector2 position,
			Rectangle? sourceRectangle,
			Color color,
			float rotation,
			Vector2 origin,
			Vector2 scale,
			SpriteEffects effects,
			float layerDepth
		) {
			CheckBegin("Draw");
			PushSprite(
				texture,
				sourceRectangle,
				position.X,
				position.Y,
				scale.X,
				scale.Y,
				color,
				origin,
				rotation,
				layerDepth,
				(byte) (effects & (SpriteEffects) 0x03),
				false
			);
		}

		public void Draw(
			Texture2D texture,
			Rectangle destinationRectangle,
			Color color
		) {
			CheckBegin("Draw");
			PushSprite(
				texture,
				null,
				destinationRectangle.X,
				destinationRectangle.Y,
				destinationRectangle.Width,
				destinationRectangle.Height,
				color,
				Vector2.Zero,
				0.0f,
				0.0f,
				0,
				true
			);
		}

		public void Draw(
			Texture2D texture,
			Rectangle destinationRectangle,
			Rectangle? sourceRectangle,
			Color color
		) {
			CheckBegin("Draw");
			PushSprite(
				texture,
				sourceRectangle,
				destinationRectangle.X,
				destinationRectangle.Y,
				destinationRectangle.Width,
				destinationRectangle.Height,
				color,
				Vector2.Zero,
				0.0f,
				0.0f,
				0,
				true
			);
		}

		public void Draw(
			Texture2D texture,
			Rectangle destinationRectangle,
			Rectangle? sourceRectangle,
			Color color,
			float rotation,
			Vector2 origin,
			SpriteEffects effects,
			float layerDepth
		) {
			CheckBegin("Draw");
			PushSprite(
				texture,
				sourceRectangle,
				destinationRectangle.X,
				destinationRectangle.Y,
				destinationRectangle.Width,
				destinationRectangle.Height,
				color,
				origin,
				rotation,
				layerDepth,
				(byte) (effects & (SpriteEffects) 0x03),
				true
			);
		}

		#endregion

		#region Public DrawString Methods

		public void DrawString(
			SpriteFont spriteFont,
			StringBuilder text,
			Vector2 position,
			Color color
		) {
			if (text == null)
			{
				throw new ArgumentNullException("text");
			}
			DrawString(
				spriteFont,
				text,
				position,
				color,
				0.0f,
				Vector2.Zero,
				Vector2.One,
				SpriteEffects.None,
				0.0f
			);
		}

		public void DrawString(
			SpriteFont spriteFont,
			StringBuilder text,
			Vector2 position,
			Color color,
			float rotation,
			Vector2 origin,
			float scale,
			SpriteEffects effects,
			float layerDepth
		) {
			if (text == null)
			{
				throw new ArgumentNullException("text");
			}
			DrawString(
				spriteFont,
				text,
				position,
				color,
				rotation,
				origin,
				new Vector2(scale),
				effects,
				layerDepth
			);
		}

		public void DrawString(
			SpriteFont spriteFont,
			StringBuilder text,
			Vector2 position,
			Color color,
			float rotation,
			Vector2 origin,
			Vector2 scale,
			SpriteEffects effects,
			float layerDepth
		) {
			/* FIXME: This method is a duplicate of DrawString(string)!
			 * The only difference is how we iterate through the StringBuilder.
			 * We don't use ToString() since it generates garbage.
			 * -flibit
			 */
			CheckBegin("DrawString");
			if (text == null)
			{
				throw new ArgumentNullException("text");
			}
			if (text.Length == 0)
			{
				return;
			}
			effects &= (SpriteEffects) 0x03;

			// FIXME: This needs an accuracy check! -flibit

			// Calculate offset, using the string size for flipped text
			Vector2 baseOffset = origin;
			if (effects != SpriteEffects.None)
			{
				Vector2 size = spriteFont.MeasureString(text);
				baseOffset.X -= size.X * axisIsMirroredX[(int) effects];
				baseOffset.Y -= size.Y * axisIsMirroredY[(int) effects];
			}

			Vector2 curOffset = Vector2.Zero;
			bool firstInLine = true;
			for (int i = 0; i < text.Length; i += 1)
			{
				char c = text[i];

				// Special characters
				if (c == '\r')
				{
					continue;
				}
				if (c == '\n')
				{
					curOffset.X = 0.0f;
					curOffset.Y += spriteFont.LineSpacing;
					firstInLine = true;
					continue;
				}

				/* Get the List index from the character map, defaulting to the
				 * DefaultCharacter if it's set.
				 */
				int index = spriteFont.characterMap.IndexOf(c);
				if (index == -1)
				{
					if (!spriteFont.DefaultCharacter.HasValue)
					{
						throw new ArgumentException(
							"Text contains characters that cannot be" +
							" resolved by this SpriteFont.",
							"text"
						);
					}
					index = spriteFont.characterMap.IndexOf(
						spriteFont.DefaultCharacter.Value
					);
				}

				/* For the first character in a line, always push the width
				 * rightward, even if the kerning pushes the character to the
				 * left.
				 */
				if (firstInLine)
				{
					curOffset.X += Math.Abs(spriteFont.kerning[index].X);
					firstInLine = false;
				}
				else
				{
					curOffset.X += spriteFont.Spacing + spriteFont.kerning[index].X;
				}

				// Calculate the character origin
				Vector2 offset = baseOffset;
				offset.X += (curOffset.X + spriteFont.croppingData[index].X) * axisDirectionX[(int) effects];
				offset.Y += (curOffset.Y + spriteFont.croppingData[index].Y) * axisDirectionY[(int) effects];
				if (effects != SpriteEffects.None)
				{
					offset.X += spriteFont.glyphData[index].Width * axisIsMirroredX[(int) effects];
					offset.Y += spriteFont.glyphData[index].Height * axisIsMirroredY[(int) effects];
				}

				// Draw!
				PushSprite(
					spriteFont.textureValue,
					spriteFont.glyphData[index],
					position.X,
					position.Y,
					scale.X,
					scale.Y,
					color,
					offset,
					rotation,
					layerDepth,
					(byte) effects,
					false
				);

				/* Add the character width and right-side bearing to the line
				 * width.
				 */
				curOffset.X += spriteFont.kerning[index].Y + spriteFont.kerning[index].Z;
			}
		}

		public void DrawString(
			SpriteFont spriteFont,
			string text,
			Vector2 position,
			Color color
		) {
			DrawString(
				spriteFont,
				text,
				position,
				color,
				0.0f,
				Vector2.Zero,
				Vector2.One,
				SpriteEffects.None,
				0.0f
			);
		}

		public void DrawString(
			SpriteFont spriteFont,
			string text,
			Vector2 position,
			Color color,
			float rotation,
			Vector2 origin,
			float scale,
			SpriteEffects effects,
			float layerDepth
		) {
			DrawString(
				spriteFont,
				text,
				position,
				color,
				rotation,
				origin,
				new Vector2(scale),
				effects,
				layerDepth
			);
		}

		public void DrawString(
			SpriteFont spriteFont,
			string text,
			Vector2 position,
			Color color,
			float rotation,
			Vector2 origin,
			Vector2 scale,
			SpriteEffects effects,
			float layerDepth
		) {
			/* FIXME: This method is a duplicate of DrawString(StringBuilder)!
			 * The only difference is how we iterate through the string.
			 * -flibit
			 */
			CheckBegin("DrawString");
			if (text == null)
			{
				throw new ArgumentNullException("text");
			}
			if (text.Length == 0)
			{
				return;
			}
			effects &= (SpriteEffects) 0x03;

			// FIXME: This needs an accuracy check! -flibit

			// Calculate offset, using the string size for flipped text
			Vector2 baseOffset = origin;
			if (effects != SpriteEffects.None)
			{
				Vector2 size = spriteFont.MeasureString(text);
				baseOffset.X -= size.X * axisIsMirroredX[(int) effects];
				baseOffset.Y -= size.Y * axisIsMirroredY[(int) effects];
			}

			Vector2 curOffset = Vector2.Zero;
			bool firstInLine = true;
			foreach (char c in text)
			{
				// Special characters
				if (c == '\r')
				{
					continue;
				}
				if (c == '\n')
				{
					curOffset.X = 0.0f;
					curOffset.Y += spriteFont.LineSpacing;
					firstInLine = true;
					continue;
				}

				/* Get the List index from the character map, defaulting to the
				 * DefaultCharacter if it's set.
				 */
				int index = spriteFont.characterMap.IndexOf(c);
				if (index == -1)
				{
					if (!spriteFont.DefaultCharacter.HasValue)
					{
						throw new ArgumentException(
							"Text contains characters that cannot be" +
							" resolved by this SpriteFont.",
							"text"
						);
					}
					index = spriteFont.characterMap.IndexOf(
						spriteFont.DefaultCharacter.Value
					);
				}

				/* For the first character in a line, always push the width
				 * rightward, even if the kerning pushes the character to the
				 * left.
				 */
				if (firstInLine)
				{
					curOffset.X += Math.Abs(spriteFont.kerning[index].X);
					firstInLine = false;
				}
				else
				{
					curOffset.X += spriteFont.Spacing + spriteFont.kerning[index].X;
				}

				// Calculate the character origin
				Vector2 offset = baseOffset;
				offset.X += (curOffset.X + spriteFont.croppingData[index].X) * axisDirectionX[(int) effects];
				offset.Y += (curOffset.Y + spriteFont.croppingData[index].Y) * axisDirectionY[(int) effects];
				if (effects != SpriteEffects.None)
				{
					offset.X += spriteFont.glyphData[index].Width * axisIsMirroredX[(int) effects];
					offset.Y += spriteFont.glyphData[index].Height * axisIsMirroredY[(int) effects];
				}

				// Draw!
				PushSprite(
					spriteFont.textureValue,
					spriteFont.glyphData[index],
					position.X,
					position.Y,
					scale.X,
					scale.Y,
					color,
					offset,
					rotation,
					layerDepth,
					(byte) effects,
					false
				);

				/* Add the character width and right-side bearing to the line
				 * width.
				 */
				curOffset.X += spriteFont.kerning[index].Y + spriteFont.kerning[index].Z;
			}
		}

		#endregion

		#region Private Methods

		private void PushSprite(
			Texture2D texture,
			Rectangle? sourceRectangle,
			float destinationX,
			float destinationY,
			float destinationW,
			float destinationH,
			Color color,
			Vector2 origin,
			float rotation,
			float depth,
			byte effects,
			bool destSizeInPixels
		) {
			if (numSprites >= MAX_SPRITES)
			{
				// Oh crap, we're out of space, flush!
				FlushBatch();
			}

			// Source/Destination/Origin Calculations
			float sourceX, sourceY, sourceW, sourceH;
			float originX, originY;
			if (sourceRectangle.HasValue)
			{
				float inverseTexW = 1.0f / (float) texture.Width;
				float inverseTexH = 1.0f / (float) texture.Height;

				sourceX = sourceRectangle.Value.X * inverseTexW;
				sourceY = sourceRectangle.Value.Y * inverseTexH;
				sourceW = Math.Max(
					sourceRectangle.Value.Width,
					MathHelper.MachineEpsilonFloat
				) * inverseTexW;
				sourceH = Math.Max(
					sourceRectangle.Value.Height,
					MathHelper.MachineEpsilonFloat
				) * inverseTexH;

				originX = (origin.X / sourceW) * inverseTexW;
				originY = (origin.Y / sourceH) * inverseTexH;

				if (!destSizeInPixels)
				{
					destinationW *= sourceRectangle.Value.Width;
					destinationH *= sourceRectangle.Value.Height;
				}
			}
			else
			{
				sourceX = 0.0f;
				sourceY = 0.0f;
				sourceW = 1.0f;
				sourceH = 1.0f;

				originX = origin.X * (1.0f / (float) texture.Width);
				originY = origin.Y * (1.0f / (float) texture.Height);

				if (!destSizeInPixels)
				{
					destinationW *= texture.Width;
					destinationH *= texture.Height;
				}
			}

			// Rotation Calculations
			float rotationMatrix1X;
			float rotationMatrix1Y;
			float rotationMatrix2X;
			float rotationMatrix2Y;
			if (!MathHelper.WithinEpsilon(rotation, 0.0f))
			{
				float sin = (float) Math.Sin(rotation);
				float cos = (float) Math.Cos(rotation);
				rotationMatrix1X = cos;
				rotationMatrix1Y = sin;
				rotationMatrix2X = -sin;
				rotationMatrix2Y = cos;
			}
			else
			{
				rotationMatrix1X = 1.0f;
				rotationMatrix1Y = 0.0f;
				rotationMatrix2X = 0.0f;
				rotationMatrix2Y = 1.0f;
			}

			// Calculate vertices, finally.
			float cornerX = (CornerOffsetX[0] - originX) * destinationW;
			float cornerY = (CornerOffsetY[0] - originY) * destinationH;
			vertexInfo[numSprites].Position0.X = (
				(rotationMatrix2X * cornerY) +
				(rotationMatrix1X * cornerX) +
				destinationX
			);
			vertexInfo[numSprites].Position0.Y = (
				(rotationMatrix2Y * cornerY) +
				(rotationMatrix1Y * cornerX) +
				destinationY
			);
			cornerX = (CornerOffsetX[1] - originX) * destinationW;
			cornerY = (CornerOffsetY[1] - originY) * destinationH;
			vertexInfo[numSprites].Position1.X = (
				(rotationMatrix2X * cornerY) +
				(rotationMatrix1X * cornerX) +
				destinationX
			);
			vertexInfo[numSprites].Position1.Y = (
				(rotationMatrix2Y * cornerY) +
				(rotationMatrix1Y * cornerX) +
				destinationY
			);
			cornerX = (CornerOffsetX[2] - originX) * destinationW;
			cornerY = (CornerOffsetY[2] - originY) * destinationH;
			vertexInfo[numSprites].Position2.X = (
				(rotationMatrix2X * cornerY) +
				(rotationMatrix1X * cornerX) +
				destinationX
			);
			vertexInfo[numSprites].Position2.Y = (
				(rotationMatrix2Y * cornerY) +
				(rotationMatrix1Y * cornerX) +
				destinationY
			);
			cornerX = (CornerOffsetX[3] - originX) * destinationW;
			cornerY = (CornerOffsetY[3] - originY) * destinationH;
			vertexInfo[numSprites].Position3.X = (
				(rotationMatrix2X * cornerY) +
				(rotationMatrix1X * cornerX) +
				destinationX
			);
			vertexInfo[numSprites].Position3.Y = (
				(rotationMatrix2Y * cornerY) +
				(rotationMatrix1Y * cornerX) +
				destinationY
			);
			vertexInfo[numSprites].TextureCoordinate0.X = (CornerOffsetX[0 ^ effects] * sourceW) + sourceX;
			vertexInfo[numSprites].TextureCoordinate0.Y = (CornerOffsetY[0 ^ effects] * sourceH) + sourceY;
			vertexInfo[numSprites].TextureCoordinate1.X = (CornerOffsetX[1 ^ effects] * sourceW) + sourceX;
			vertexInfo[numSprites].TextureCoordinate1.Y = (CornerOffsetY[1 ^ effects] * sourceH) + sourceY;
			vertexInfo[numSprites].TextureCoordinate2.X = (CornerOffsetX[2 ^ effects] * sourceW) + sourceX;
			vertexInfo[numSprites].TextureCoordinate2.Y = (CornerOffsetY[2 ^ effects] * sourceH) + sourceY;
			vertexInfo[numSprites].TextureCoordinate3.X = (CornerOffsetX[3 ^ effects] * sourceW) + sourceX;
			vertexInfo[numSprites].TextureCoordinate3.Y = (CornerOffsetY[3 ^ effects] * sourceH) + sourceY;
			vertexInfo[numSprites].Position0.Z = depth;
			vertexInfo[numSprites].Position1.Z = depth;
			vertexInfo[numSprites].Position2.Z = depth;
			vertexInfo[numSprites].Position3.Z = depth;
			vertexInfo[numSprites].Color0 = color;
			vertexInfo[numSprites].Color1 = color;
			vertexInfo[numSprites].Color2 = color;
			vertexInfo[numSprites].Color3 = color;

			if (sortMode == SpriteSortMode.Immediate)
			{
				vertexBuffer.SetData(
					0,
					vertexInfo,
					0,
					1,
					VertexPositionColorTexture4.RealStride,
					SetDataOptions.None
				);
				DrawPrimitives(texture, 0, 1);
			}
			else
			{
				textureInfo[numSprites] = texture;
				numSprites += 1;
			}
		}

		private void FlushBatch()
		{
			int offset = 0;
			Texture2D curTexture = null;

			PrepRenderState();

			if (numSprites == 0)
			{
				// Nothing to do.
				return;
			}

			// FIXME: OPTIMIZATION POINT: Speed up sprite sorting! -flibit
			if (sortMode == SpriteSortMode.Texture)
			{
				Array.Sort(
					textureInfo,
					vertexInfo,
					0,
					numSprites,
					TextureCompare
				);
			}
			else if (sortMode == SpriteSortMode.BackToFront)
			{
				Array.Sort(
					vertexInfo,
					textureInfo,
					0,
					numSprites,
					BackToFrontCompare
				);
			}
			else if (sortMode == SpriteSortMode.FrontToBack)
			{
				Array.Sort(
					vertexInfo,
					textureInfo,
					0,
					numSprites,
					FrontToBackCompare
				);
			}

			vertexBuffer.SetData(
				0,
				vertexInfo,
				0,
				numSprites,
				VertexPositionColorTexture4.RealStride,
				SetDataOptions.None
			);

			curTexture = textureInfo[0];
			for (int i = 1; i < numSprites; i += 1)
			{
				if (textureInfo[i] != curTexture)
				{
					DrawPrimitives(curTexture, offset, i - offset);
					curTexture = textureInfo[i];
					offset = i;
				}
			}
			DrawPrimitives(curTexture, offset, numSprites - offset);

			numSprites = 0;
		}

		private void PrepRenderState()
		{
			GraphicsDevice.BlendState = blendState;
			GraphicsDevice.SamplerStates[0] = samplerState;
			GraphicsDevice.DepthStencilState = depthStencilState;
			GraphicsDevice.RasterizerState = rasterizerState;

			GraphicsDevice.SetVertexBuffer(vertexBuffer);
			GraphicsDevice.Indices = indexBuffer;

			Viewport viewport = GraphicsDevice.Viewport;

			// Inlined CreateOrthographicOffCenter * transformMatrix
			float tfWidth = (float) (2.0 / (double) viewport.Width);
			float tfHeight = (float) (-2.0 / (double) viewport.Height);
			unsafe
			{
				float* dstPtr = (float*) spriteMatrixTransform;
				dstPtr[0] = (tfWidth * transformMatrix.M11) - transformMatrix.M14;
				dstPtr[1] = (tfWidth * transformMatrix.M21) - transformMatrix.M24;
				dstPtr[2] = (tfWidth * transformMatrix.M31) - transformMatrix.M34;
				dstPtr[3] = (tfWidth * transformMatrix.M41) - transformMatrix.M44;
				dstPtr[4] = (tfHeight * transformMatrix.M12) + transformMatrix.M14;
				dstPtr[5] = (tfHeight * transformMatrix.M22) + transformMatrix.M24;
				dstPtr[6] = (tfHeight * transformMatrix.M32) + transformMatrix.M34;
				dstPtr[7] = (tfHeight * transformMatrix.M42) + transformMatrix.M44;
				dstPtr[8] = transformMatrix.M13;
				dstPtr[9] = transformMatrix.M23;
				dstPtr[10] = transformMatrix.M33;
				dstPtr[11] = transformMatrix.M43;
				dstPtr[12] = transformMatrix.M14;
				dstPtr[13] = transformMatrix.M24;
				dstPtr[14] = transformMatrix.M34;
				dstPtr[15] = transformMatrix.M44;
			}

			// FIXME: When is this actually applied? -flibit
			spriteEffectPass.Apply();
		}

		private void DrawPrimitives(Texture texture, int baseSprite, int batchSize)
		{
			GraphicsDevice.Textures[0] = texture;
			if (customEffect != null)
			{
				foreach (EffectPass pass in customEffect.CurrentTechnique.Passes)
				{
					pass.Apply();
					GraphicsDevice.DrawIndexedPrimitives(
						PrimitiveType.TriangleList,
						baseSprite * 4,
						0,
						batchSize * 4,
						0,
						batchSize * 2
					);
				}
			}
			else
			{
				GraphicsDevice.DrawIndexedPrimitives(
					PrimitiveType.TriangleList,
					baseSprite * 4,
					0,
					batchSize * 4,
					0,
					batchSize * 2
				);
			}
		}

		[System.Diagnostics.Conditional("DEBUG")]
		private void CheckBegin(string method)
		{
			if (!beginCalled)
			{
				throw new InvalidOperationException(
					method + " was called, but Begin has" +
					" not yet been called. Begin must be" +
					" called successfully before you can" +
					" call " + method + "."
				);
			}
		}

		#endregion

		#region Private Static Methods

		private static short[] GenerateIndexArray()
		{
			short[] result = new short[MAX_INDICES];
			for (int i = 0, j = 0; i < MAX_INDICES; i += 6, j += 4)
			{
				result[i] = (short) (j);
				result[i + 1] = (short) (j + 1);
				result[i + 2] = (short) (j + 2);
				result[i + 3] = (short) (j + 3);
				result[i + 4] = (short) (j + 2);
				result[i + 5] = (short) (j + 1);
			}
			return result;
		}

		#endregion

		#region Private Sprite Data Container Class

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		private struct VertexPositionColorTexture4 : IVertexType
		{
			public const int RealStride = 96;

			VertexDeclaration IVertexType.VertexDeclaration
			{
				get
				{
					throw new NotImplementedException();
				}
			}

			public Vector3 Position0;
			public Color Color0;
			public Vector2 TextureCoordinate0;
			public Vector3 Position1;
			public Color Color1;
			public Vector2 TextureCoordinate1;
			public Vector3 Position2;
			public Color Color2;
			public Vector2 TextureCoordinate2;
			public Vector3 Position3;
			public Color Color3;
			public Vector2 TextureCoordinate3;
		}

		#endregion

		#region Private Sprite Comparison Classes

		private class TextureComparer : IComparer<Texture2D>
		{
			public int Compare(Texture2D x, Texture2D y)
			{
				return x.GetHashCode().CompareTo(y.GetHashCode());
			}
		}

		private class BackToFrontComparer : IComparer<VertexPositionColorTexture4>
		{
			public int Compare(VertexPositionColorTexture4 x, VertexPositionColorTexture4 y)
			{
				return y.Position0.Z.CompareTo(x.Position0.Z);
			}
		}

		private class FrontToBackComparer : IComparer<VertexPositionColorTexture4>
		{
			public int Compare(VertexPositionColorTexture4 x, VertexPositionColorTexture4 y)
			{
				return x.Position0.Z.CompareTo(y.Position0.Z);
			}
		}

		#endregion
	}
}
