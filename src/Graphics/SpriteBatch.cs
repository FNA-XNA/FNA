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
		private static readonly Vector2[] axisDirection = new Vector2[]
		{
			new Vector2(-1, -1),
			new Vector2( 1, -1),
			new Vector2(-1,  1),
			new Vector2( 1,  1)
		};
		private static readonly Vector2[] axisIsMirrored = new Vector2[]
		{
			new Vector2(0, 0),
			new Vector2(1, 0),
			new Vector2(0, 1),
			new Vector2(1, 1)
		};

		#endregion

		#region Private Variables

		// Buffer objects used for actual drawing
		private DynamicVertexBuffer vertexBuffer;
		private IndexBuffer indexBuffer;

		// Local data stored before buffering to GPU
		private VertexPositionColorTexture[] vertexInfo;
		private SpriteInfo[] spriteData;

		// Default SpriteBatch Effect
		private Effect spriteEffect;
		private EffectParameter spriteMatrixTransform;
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

			vertexInfo = new VertexPositionColorTexture[MAX_VERTICES];
			spriteData = new SpriteInfo[MAX_SPRITES];
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
			spriteMatrixTransform = spriteEffect.Parameters["MatrixTransform"];
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
				new Vector4(
					position.X,
					position.Y,
					1.0f,
					1.0f
				),
				color,
				Vector2.Zero,
				0.0f,
				0.0f,
				0
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
				new Vector4(
					position.X,
					position.Y,
					1.0f,
					1.0f
				),
				color,
				Vector2.Zero,
				0.0f,
				0.0f,
				0
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
				new Vector4(
					position.X,
					position.Y,
					scale,
					scale
				),
				color,
				origin,
				rotation,
				layerDepth,
				(byte) effects
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
				new Vector4(
					position.X,
					position.Y,
					scale.X,
					scale.Y
				),
				color,
				origin,
				rotation,
				layerDepth,
				(byte) effects
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
				new Vector4(
					destinationRectangle.X,
					destinationRectangle.Y,
					destinationRectangle.Width,
					destinationRectangle.Height
				),
				color,
				Vector2.Zero,
				0.0f,
				0.0f,
				SpriteInfo.DestSizeInPixels
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
				new Vector4(
					destinationRectangle.X,
					destinationRectangle.Y,
					destinationRectangle.Width,
					destinationRectangle.Height
				),
				color,
				Vector2.Zero,
				0.0f,
				0.0f,
				SpriteInfo.DestSizeInPixels
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
				new Vector4(
					destinationRectangle.X,
					destinationRectangle.Y,
					destinationRectangle.Width,
					destinationRectangle.Height
				),
				color,
				origin,
				rotation,
				layerDepth,
				(byte) ((int) effects | SpriteInfo.DestSizeInPixels)
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
				new Vector2(1.0f),
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

			// FIXME: This needs an accuracy check! -flibit

			// Calculate offset, using the string size for flipped text
			Vector2 baseOffset = origin;
			if (effects != SpriteEffects.None)
			{
				baseOffset -= spriteFont.MeasureString(text) * axisIsMirrored[(int) effects];
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
				offset.X += (curOffset.X + spriteFont.croppingData[index].X) * axisDirection[(int) effects].X;
				offset.Y += (curOffset.Y + spriteFont.croppingData[index].Y) * axisDirection[(int) effects].Y;
				if (effects != SpriteEffects.None)
				{
					offset += new Vector2(
						spriteFont.glyphData[index].Width,
						spriteFont.glyphData[index].Height
					) * axisIsMirrored[(int) effects];
				}

				// Draw!
				PushSprite(
					spriteFont.textureValue,
					spriteFont.glyphData[index],
					new Vector4(
						position.X,
						position.Y,
						scale.X,
						scale.Y
					),
					color,
					offset,
					rotation,
					layerDepth,
					(byte) effects
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
				new Vector2(1.0f),
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

			// FIXME: This needs an accuracy check! -flibit

			// Calculate offset, using the string size for flipped text
			Vector2 baseOffset = origin;
			if (effects != SpriteEffects.None)
			{
				baseOffset -= spriteFont.MeasureString(text) * axisIsMirrored[(int) effects];
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
				offset.X += (curOffset.X + spriteFont.croppingData[index].X) * axisDirection[(int) effects].X;
				offset.Y += (curOffset.Y + spriteFont.croppingData[index].Y) * axisDirection[(int) effects].Y;
				if (effects != SpriteEffects.None)
				{
					offset += new Vector2(
						spriteFont.glyphData[index].Width,
						spriteFont.glyphData[index].Height
					) * axisIsMirrored[(int) effects];
				}

				// Draw!
				PushSprite(
					spriteFont.textureValue,
					spriteFont.glyphData[index],
					new Vector4(
						position.X,
						position.Y,
						scale.X,
						scale.Y
					),
					color,
					offset,
					rotation,
					layerDepth,
					(byte) effects
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
			Vector4 destination,
			Color color,
			Vector2 origin,
			float rotation,
			float depth,
			byte effects
		) {
			if (numSprites >= MAX_SPRITES)
			{
				// Oh crap, we're out of space, flush!
				FlushBatch();
			}

			// Calculate source/destination
			spriteData[numSprites].destination = destination;
			if (sourceRectangle.HasValue)
			{
				spriteData[numSprites].source = sourceRectangle.Value;
				if ((effects & SpriteInfo.DestSizeInPixels) != SpriteInfo.DestSizeInPixels)
				{
					spriteData[numSprites].destination.Z *= sourceRectangle.Value.Width;
					spriteData[numSprites].destination.W *= sourceRectangle.Value.Height;
				}
				effects |= SpriteInfo.SourceInTexels | SpriteInfo.DestSizeInPixels;
			}
			else
			{
				spriteData[numSprites].source.X = 0;
				spriteData[numSprites].source.Y = 0;
				spriteData[numSprites].source.Width = 1;
				spriteData[numSprites].source.Height = 1;
			}

			// Everything else passed is just copied
			spriteData[numSprites].texture = texture;
			spriteData[numSprites].color = color;
			spriteData[numSprites].origin = origin;
			spriteData[numSprites].rotation = rotation;
			spriteData[numSprites].depth = depth;
			spriteData[numSprites].effects = effects;

			if (sortMode == SpriteSortMode.Immediate)
			{
				RenderBatch(0, 1);
			}
			else
			{
				numSprites += 1;
			}
		}

		private void RenderBatch(int offset, int batchSize)
		{
			GraphicsDevice.Textures[0] = spriteData[offset].texture;
			for (int i = 0; i < batchSize; i += 1)
			{
				/* FIXME: OPTIMIZATION POINT: This method
				 * allocates like fuck right now! In general,
				 * it's by far the slowest block of code in the
				 * whole file, and needs fixing.
				 * -flibit
				 */

				// Current sprite being calculated
				SpriteInfo info = spriteData[i + offset];

				// Calculate initial sprite information
				Vector2 source = new Vector2(
					info.source.X,
					info.source.Y
				);
				Vector2 destination = new Vector2(
					info.destination.X,
					info.destination.Y
				);
				Vector2 sourceSize = new Vector2(
					Math.Max(
						info.source.Width,
						MathHelper.MachineEpsilonFloat
					),
					Math.Max(
						info.source.Height,
						MathHelper.MachineEpsilonFloat
					)
				);
				Vector2 destinationSize = new Vector2(
					info.destination.Z,
					info.destination.W
				);
				Vector2 origin = info.origin / sourceSize;

				// Calculations performed with inverse texture size
				Vector2 inverseTexSize = new Vector2(
					(1.0f / (float) spriteData[offset].texture.Width),
					(1.0f / (float) spriteData[offset].texture.Height)
				);
				if ((info.effects & SpriteInfo.SourceInTexels) == SpriteInfo.SourceInTexels)
				{
					source *= inverseTexSize;
					sourceSize *= inverseTexSize;
				}
				else
				{
					origin *= inverseTexSize;
				}

				// Calculations done with texture size
				if ((info.effects & SpriteInfo.DestSizeInPixels) != SpriteInfo.DestSizeInPixels)
				{
					destinationSize.X *= spriteData[offset].texture.Width;
					destinationSize.Y *= spriteData[offset].texture.Height;
				}

				// Calculations performed with rotation
				Vector2 rotationMatrix1;
				Vector2 rotationMatrix2;
				if (!MathHelper.WithinEpsilon(info.rotation, 0.0f))
				{
					float sin = (float) Math.Sin(info.rotation);
					float cos = (float) Math.Cos(info.rotation);
					rotationMatrix1.X = cos;
					rotationMatrix1.Y = sin;
					rotationMatrix2.X = -sin;
					rotationMatrix2.Y = cos;
				}
				else
				{
					rotationMatrix1.X = 1.0f;
					rotationMatrix1.Y = 0.0f;
					rotationMatrix2.X = 0.0f;
					rotationMatrix2.Y = 1.0f;
				}

				// Calculate vertices, finally.
				for (int j = 0, curVertex = i * 4; j < 4; j += 1, curVertex += 1)
				{
					Vector2 cornerOffset = (
						SpriteInfo.CornerOffsets[j] - origin
					) * destinationSize;

					Vector2 position = Vector2.Add(
						Vector2.Multiply(
							rotationMatrix2,
							cornerOffset.Y
						),
						Vector2.Add(
							Vector2.Multiply(
								rotationMatrix1,
								cornerOffset.X
							),
							destination
						)
					);
					vertexInfo[curVertex].Position.X = position.X;
					vertexInfo[curVertex].Position.Y = position.Y;
					vertexInfo[curVertex].Position.Z = info.depth;
					vertexInfo[curVertex].Color = info.color;
					vertexInfo[curVertex].TextureCoordinate = Vector2.Add(
						Vector2.Multiply(
							SpriteInfo.CornerOffsets[j ^ (info.effects & 0x3)],
							sourceSize
						),
						source
					);
				}
			}
			vertexBuffer.SetData(vertexInfo, 0, batchSize * 4, SetDataOptions.None);

			if (customEffect != null)
			{
				foreach (EffectPass pass in customEffect.CurrentTechnique.Passes)
				{
					pass.Apply();
					GraphicsDevice.DrawIndexedPrimitives(
						PrimitiveType.TriangleList,
						0,
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
					0,
					0,
					batchSize * 4,
					0,
					batchSize * 2
				);
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
					spriteData,
					0,
					numSprites,
					TextureCompare
				);
			}
			else if (sortMode == SpriteSortMode.BackToFront)
			{
				Array.Sort(
					spriteData,
					0,
					numSprites,
					BackToFrontCompare
				);
			}
			else if (sortMode == SpriteSortMode.FrontToBack)
			{
				Array.Sort(
					spriteData,
					0,
					numSprites,
					FrontToBackCompare
				);
			}

			for (int i = 0; i < numSprites; i += 1)
			{
				if (spriteData[i].texture != curTexture)
				{
					if (i > offset)
					{
						RenderBatch(offset, i - offset);
					}
					curTexture = spriteData[i].texture;
					offset = i;
				}
			}
			RenderBatch(offset, numSprites - offset);

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

			// Inlined CreateOrthographicOffCenter
			Matrix projection = new Matrix(
				(float) (2.0 / (double) viewport.Width),
				0.0f,
				0.0f,
				0.0f,
				0.0f,
				(float) (-2.0 / (double) viewport.Height),
				0.0f,
				0.0f,
				0.0f,
				0.0f,
				1.0f,
				0.0f,
				-1.0f,
				1.0f,
				0.0f,
				1.0f
			);
			Matrix.Multiply(
				ref transformMatrix,
				ref projection,
				out projection
			);
			spriteMatrixTransform.SetValue(projection);

			// FIXME: When is this actually applied? -flibit
			spriteEffectPass.Apply();
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

		private struct SpriteInfo
		{
			public Rectangle source;
			public Vector4 destination;
			public Color color;
			public Vector2 origin;
			public float rotation;
			public float depth;
			public Texture2D texture;
			public byte effects;

			public const int SourceInTexels = 0x4;
			public const int DestSizeInPixels = 0x8;
			public static readonly Vector2[] CornerOffsets = new Vector2[]
			{
				new Vector2(0.0f, 0.0f),
				new Vector2(1.0f, 0.0f),
				new Vector2(0.0f, 1.0f),
				new Vector2(1.0f, 1.0f)
			};
		}

		#endregion

		#region Private Sprite Comparison Classes

		private class TextureComparer : IComparer<SpriteInfo>
		{
			public int Compare(SpriteInfo x, SpriteInfo y)
			{
				return x.texture.GetHashCode().CompareTo(y.texture.GetHashCode());
			}
		}

		private class BackToFrontComparer : IComparer<SpriteInfo>
		{
			public int Compare(SpriteInfo x, SpriteInfo y)
			{
				return y.depth.CompareTo(x.depth);
			}
		}

		private class FrontToBackComparer : IComparer<SpriteInfo>
		{
			public int Compare(SpriteInfo x, SpriteInfo y)
			{
				return x.depth.CompareTo(y.depth);
			}
		}

		#endregion
	}
}
