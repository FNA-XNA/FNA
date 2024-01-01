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

		/* This is the largest array size for a VertexBuffer using VertexPositionColorTexture.
		 * Note that we do NOT change the GPU buffer size since that would break XNA accuracy,
		 * but if you want to optimize your batching you can make this change in a custom SpriteBatch.
		 */
		private const int MAX_ARRAYSIZE = 0x3FFFFFF / 96;

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
		private SpriteInfo[] spriteInfos;
		private IntPtr[] sortedSpriteInfos; // SpriteInfo*[]
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

		// Where are we in the vertex buffer ring?
		private int bufferOffset;
		private bool supportsNoOverwrite;

		// Matrix to be used when creating the projection matrix
		private Matrix transformMatrix;

		// User-provided Effect, if applicable
		private Effect customEffect;

		#endregion

		#region Private Static Variables

		/* If you use this file to make your own SpriteBatch, take the
		 * shader source and binary and load it as a file. Find it in
		 * src/Graphics/Effect/StockEffects/, the HLSL and FXB folders!
		 * -flibit
		 */
		private static readonly byte[] spriteEffectCode = Resources.SpriteEffect;
		private static readonly short[] indexData = GenerateIndexArray();
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
			spriteInfos = new SpriteInfo[MAX_SPRITES];
			sortedSpriteInfos = new IntPtr[MAX_SPRITES];
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
			supportsNoOverwrite = FNA3D.FNA3D_SupportsNoOverwrite(
				GraphicsDevice.GLDevice
			) == 1;
		}

		#endregion

		#region Public Dispose Method

		protected override void Dispose(bool disposing)
		{
			if (!IsDisposed)
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
			Matrix transformMatrix
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
			this.transformMatrix = transformMatrix;

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
				0.0f,
				0.0f,
				1.0f,
				1.0f,
				position.X,
				position.Y,
				texture.Width,
				texture.Height,
				color,
				0.0f,
				0.0f,
				0.0f,
				1.0f,
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
			float sourceX, sourceY, sourceW, sourceH;
			float destW, destH;
			if (sourceRectangle.HasValue)
			{
				sourceX = sourceRectangle.Value.X / (float) texture.Width;
				sourceY = sourceRectangle.Value.Y / (float) texture.Height;
				sourceW = sourceRectangle.Value.Width / (float) texture.Width;
				sourceH = sourceRectangle.Value.Height / (float) texture.Height;
				destW = sourceRectangle.Value.Width;
				destH = sourceRectangle.Value.Height;
			}
			else
			{
				sourceX = 0.0f;
				sourceY = 0.0f;
				sourceW = 1.0f;
				sourceH = 1.0f;
				destW = texture.Width;
				destH = texture.Height;
			}
			CheckBegin("Draw");
			PushSprite(
				texture,
				sourceX,
				sourceY,
				sourceW,
				sourceH,
				position.X,
				position.Y,
				destW,
				destH,
				color,
				0.0f,
				0.0f,
				0.0f,
				1.0f,
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
			float sourceX, sourceY, sourceW, sourceH;
			float destW = scale;
			float destH = scale;
			if (sourceRectangle.HasValue)
			{
				sourceX = sourceRectangle.Value.X / (float) texture.Width;
				sourceY = sourceRectangle.Value.Y / (float) texture.Height;
				sourceW = Math.Sign(sourceRectangle.Value.Width) * Math.Max(
					Math.Abs(sourceRectangle.Value.Width),
					MathHelper.MachineEpsilonFloat
				) / (float) texture.Width;
				sourceH = Math.Sign(sourceRectangle.Value.Height) * Math.Max(
					Math.Abs(sourceRectangle.Value.Height),
					MathHelper.MachineEpsilonFloat
				) / (float) texture.Height;
				destW *= sourceRectangle.Value.Width;
				destH *= sourceRectangle.Value.Height;
			}
			else
			{
				sourceX = 0.0f;
				sourceY = 0.0f;
				sourceW = 1.0f;
				sourceH = 1.0f;
				destW *= texture.Width;
				destH *= texture.Height;
			}
			PushSprite(
				texture,
				sourceX,
				sourceY,
				sourceW,
				sourceH,
				position.X,
				position.Y,
				destW,
				destH,
				color,
				origin.X / sourceW / (float) texture.Width,
				origin.Y / sourceH / (float) texture.Height,
				(float) Math.Sin(rotation),
				(float) Math.Cos(rotation),
				layerDepth,
				(byte) (effects & (SpriteEffects) 0x03)
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
			float sourceX, sourceY, sourceW, sourceH;
			if (sourceRectangle.HasValue)
			{
				sourceX = sourceRectangle.Value.X / (float) texture.Width;
				sourceY = sourceRectangle.Value.Y / (float) texture.Height;
				sourceW = Math.Sign(sourceRectangle.Value.Width) * Math.Max(
					Math.Abs(sourceRectangle.Value.Width),
					MathHelper.MachineEpsilonFloat
				) / (float) texture.Width;
				sourceH = Math.Sign(sourceRectangle.Value.Height) * Math.Max(
					Math.Abs(sourceRectangle.Value.Height),
					MathHelper.MachineEpsilonFloat
				) / (float) texture.Height;
				scale.X *= sourceRectangle.Value.Width;
				scale.Y *= sourceRectangle.Value.Height;
			}
			else
			{
				sourceX = 0.0f;
				sourceY = 0.0f;
				sourceW = 1.0f;
				sourceH = 1.0f;
				scale.X *= texture.Width;
				scale.Y *= texture.Height;
			}
			PushSprite(
				texture,
				sourceX,
				sourceY,
				sourceW,
				sourceH,
				position.X,
				position.Y,
				scale.X,
				scale.Y,
				color,
				origin.X / sourceW / (float) texture.Width,
				origin.Y / sourceH / (float) texture.Height,
				(float) Math.Sin(rotation),
				(float) Math.Cos(rotation),
				layerDepth,
				(byte) (effects & (SpriteEffects) 0x03)
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
				0.0f,
				0.0f,
				1.0f,
				1.0f,
				destinationRectangle.X,
				destinationRectangle.Y,
				destinationRectangle.Width,
				destinationRectangle.Height,
				color,
				0.0f,
				0.0f,
				0.0f,
				1.0f,
				0.0f,
				0
			);
		}

		public void Draw(
			Texture2D texture,
			Rectangle destinationRectangle,
			Rectangle? sourceRectangle,
			Color color
		) {
			CheckBegin("Draw");
			float sourceX, sourceY, sourceW, sourceH;
			if (sourceRectangle.HasValue)
			{
				sourceX = sourceRectangle.Value.X / (float) texture.Width;
				sourceY = sourceRectangle.Value.Y / (float) texture.Height;
				sourceW = sourceRectangle.Value.Width / (float) texture.Width;
				sourceH = sourceRectangle.Value.Height / (float) texture.Height;
			}
			else
			{
				sourceX = 0.0f;
				sourceY = 0.0f;
				sourceW = 1.0f;
				sourceH = 1.0f;
			}
			PushSprite(
				texture,
				sourceX,
				sourceY,
				sourceW,
				sourceH,
				destinationRectangle.X,
				destinationRectangle.Y,
				destinationRectangle.Width,
				destinationRectangle.Height,
				color,
				0.0f,
				0.0f,
				0.0f,
				1.0f,
				0.0f,
				0
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
			float sourceX, sourceY, sourceW, sourceH;
			if (sourceRectangle.HasValue)
			{
				sourceX = sourceRectangle.Value.X / (float) texture.Width;
				sourceY = sourceRectangle.Value.Y / (float) texture.Height;
				sourceW = Math.Sign(sourceRectangle.Value.Width) * Math.Max(
					Math.Abs(sourceRectangle.Value.Width),
					MathHelper.MachineEpsilonFloat
				) / (float) texture.Width;
				sourceH = Math.Sign(sourceRectangle.Value.Height) * Math.Max(
					Math.Abs(sourceRectangle.Value.Height),
					MathHelper.MachineEpsilonFloat
				) / (float) texture.Height;
			}
			else
			{
				sourceX = 0.0f;
				sourceY = 0.0f;
				sourceW = 1.0f;
				sourceH = 1.0f;
			}
			PushSprite(
				texture,
				sourceX,
				sourceY,
				sourceW,
				sourceH,
				destinationRectangle.X,
				destinationRectangle.Y,
				destinationRectangle.Width,
				destinationRectangle.Height,
				color,
				origin.X / sourceW / (float) texture.Width,
				origin.Y / sourceH / (float) texture.Height,
				(float) Math.Sin(rotation),
				(float) Math.Cos(rotation),
				layerDepth,
				(byte) (effects & (SpriteEffects) 0x03)
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

			/* We pull all these internal variables in at once so
			 * anyone who wants to use this file to make their own
			 * SpriteBatch can easily replace these with reflection.
			 * -flibit
			 */
			Texture2D textureValue = spriteFont.textureValue;
			List<Rectangle> glyphData = spriteFont.glyphData;
			List<Rectangle> croppingData = spriteFont.croppingData;
			List<Vector3> kerning = spriteFont.kerning;
			Dictionary<char, int> characterIndexMap = spriteFont.characterIndexMap;

			// FIXME: This needs an accuracy check! -flibit

			// Calculate offsets/axes, using the string size for flipped text
			Vector2 baseOffset = origin;
			float axisDirX = axisDirectionX[(int) effects];
			float axisDirY = axisDirectionY[(int) effects];
			float axisDirMirrorX = 0.0f;
			float axisDirMirrorY = 0.0f;
			if (effects != SpriteEffects.None)
			{
				Vector2 size = spriteFont.MeasureString(text);
				baseOffset.X -= size.X * axisIsMirroredX[(int) effects];
				baseOffset.Y -= size.Y * axisIsMirroredY[(int) effects];
				axisDirMirrorX = axisIsMirroredX[(int) effects];
				axisDirMirrorY = axisIsMirroredY[(int) effects];
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
				int index;
				if (!characterIndexMap.TryGetValue(c, out index))
				{
					if (!spriteFont.DefaultCharacter.HasValue)
					{
						throw new ArgumentException(
							"Text contains characters that cannot be" +
							" resolved by this SpriteFont.",
							"text"
						);
					}
					index = characterIndexMap[spriteFont.DefaultCharacter.Value];
				}

				/* For the first character in a line, always push the width
				 * rightward, even if the kerning pushes the character to the
				 * left.
				 */
				Vector3 cKern = kerning[index];
				if (firstInLine)
				{
					curOffset.X += Math.Abs(cKern.X);
					firstInLine = false;
				}
				else
				{
					curOffset.X += spriteFont.Spacing + cKern.X;
				}

				// Calculate the character origin
				Rectangle cCrop = croppingData[index];
				Rectangle cGlyph = glyphData[index];
				float offsetX = baseOffset.X + (
					curOffset.X + cCrop.X
				) * axisDirX;
				float offsetY = baseOffset.Y + (
					curOffset.Y + cCrop.Y
				) * axisDirY;
				if (effects != SpriteEffects.None)
				{
					offsetX += cGlyph.Width * axisDirMirrorX;
					offsetY += cGlyph.Height * axisDirMirrorY;
				}

				// Draw!
				float sourceW = Math.Sign(cGlyph.Width) * Math.Max(
					Math.Abs(cGlyph.Width),
					MathHelper.MachineEpsilonFloat
				) / (float) textureValue.Width;
				float sourceH = Math.Sign(cGlyph.Height) * Math.Max(
					Math.Abs(cGlyph.Height),
					MathHelper.MachineEpsilonFloat
				) / (float) textureValue.Height;
				PushSprite(
					textureValue,
					cGlyph.X / (float) textureValue.Width,
					cGlyph.Y / (float) textureValue.Height,
					sourceW,
					sourceH,
					position.X,
					position.Y,
					cGlyph.Width * scale.X,
					cGlyph.Height * scale.Y,
					color,
					offsetX / sourceW / (float) textureValue.Width,
					offsetY / sourceH / (float) textureValue.Height,
					(float) Math.Sin(rotation),
					(float) Math.Cos(rotation),
					layerDepth,
					(byte) effects
				);

				/* Add the character width and right-side
				 * bearing to the line width.
				 */
				curOffset.X += cKern.Y + cKern.Z;
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

			/* We pull all these internal variables in at once so
			 * anyone who wants to use this file to make their own
			 * SpriteBatch can easily replace these with reflection.
			 * -flibit
			 */
			Texture2D textureValue = spriteFont.textureValue;
			List<Rectangle> glyphData = spriteFont.glyphData;
			List<Rectangle> croppingData = spriteFont.croppingData;
			List<Vector3> kerning = spriteFont.kerning;
			Dictionary<char, int> characterIndexMap = spriteFont.characterIndexMap;

			// FIXME: This needs an accuracy check! -flibit

			// Calculate offsets/axes, using the string size for flipped text
			Vector2 baseOffset = origin;
			float axisDirX = axisDirectionX[(int) effects];
			float axisDirY = axisDirectionY[(int) effects];
			float axisDirMirrorX = 0.0f;
			float axisDirMirrorY = 0.0f;
			if (effects != SpriteEffects.None)
			{
				Vector2 size = spriteFont.MeasureString(text);
				baseOffset.X -= size.X * axisIsMirroredX[(int) effects];
				baseOffset.Y -= size.Y * axisIsMirroredY[(int) effects];
				axisDirMirrorX = axisIsMirroredX[(int) effects];
				axisDirMirrorY = axisIsMirroredY[(int) effects];
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
				int index;
				if (!characterIndexMap.TryGetValue(c, out index))
				{
					if (!spriteFont.DefaultCharacter.HasValue)
					{
						throw new ArgumentException(
							"Text contains characters that cannot be" +
							" resolved by this SpriteFont.",
							"text"
						);
					}
					index = characterIndexMap[spriteFont.DefaultCharacter.Value];
				}

				/* For the first character in a line, always push the width
				 * rightward, even if the kerning pushes the character to the
				 * left.
				 */
				Vector3 cKern = kerning[index];
				if (firstInLine)
				{
					curOffset.X += Math.Abs(cKern.X);
					firstInLine = false;
				}
				else
				{
					curOffset.X += spriteFont.Spacing + cKern.X;
				}

				// Calculate the character origin
				Rectangle cCrop = croppingData[index];
				Rectangle cGlyph = glyphData[index];
				float offsetX = baseOffset.X + (
					curOffset.X + cCrop.X
				) * axisDirX;
				float offsetY = baseOffset.Y + (
					curOffset.Y + cCrop.Y
				) * axisDirY;
				if (effects != SpriteEffects.None)
				{
					offsetX += cGlyph.Width * axisDirMirrorX;
					offsetY += cGlyph.Height * axisDirMirrorY;
				}

				// Draw!
				float sourceW = Math.Sign(cGlyph.Width) * Math.Max(
					Math.Abs(cGlyph.Width),
					MathHelper.MachineEpsilonFloat
				) / (float) textureValue.Width;
				float sourceH = Math.Sign(cGlyph.Height) * Math.Max(
					Math.Abs(cGlyph.Height),
					MathHelper.MachineEpsilonFloat
				) / (float) textureValue.Height;
				PushSprite(
					textureValue,
					cGlyph.X / (float) textureValue.Width,
					cGlyph.Y / (float) textureValue.Height,
					sourceW,
					sourceH,
					position.X,
					position.Y,
					cGlyph.Width * scale.X,
					cGlyph.Height * scale.Y,
					color,
					offsetX / sourceW / (float) textureValue.Width,
					offsetY / sourceH / (float) textureValue.Height,
					(float) Math.Sin(rotation),
					(float) Math.Cos(rotation),
					layerDepth,
					(byte) effects
				);

				/* Add the character width and right-side
				 * bearing to the line width.
				 */
				curOffset.X += cKern.Y + cKern.Z;
			}
		}

		#endregion

		#region Private Methods

		private unsafe void PushSprite(
			Texture2D texture,
			float sourceX,
			float sourceY,
			float sourceW,
			float sourceH,
			float destinationX,
			float destinationY,
			float destinationW,
			float destinationH,
			Color color,
			float originX,
			float originY,
			float rotationSin,
			float rotationCos,
			float depth,
			byte effects
		) {
			if (numSprites >= vertexInfo.Length)
			{
				if (vertexInfo.Length >= MAX_ARRAYSIZE)
				{
					/* FIXME: We're doing this for safety but it's possible that
					 * XNA just keeps expanding and crashes with OutOfMemory.
					 * Since GraphicsProfile has a buffer cap, we use that for safety.
					 * This might change if someone depends on running out of memory(?!).
					 */
					FlushBatch();
				}
				else
				{
					/* We're out of room, add another batch max
					 * to the total array size. This is required for
					 * sprite sorting accuracy; note that we do NOT
					 * increase the graphics buffer sizes!
					 * -flibit
					 */
					int newMax = Math.Min(vertexInfo.Length * 2, MAX_ARRAYSIZE);
					Array.Resize(ref vertexInfo, newMax);
					Array.Resize(ref textureInfo, newMax);
					Array.Resize(ref spriteInfos, newMax);
					Array.Resize(ref sortedSpriteInfos, newMax);
				}
			}

			if (sortMode == SpriteSortMode.Immediate)
			{
				int offset;
				fixed (VertexPositionColorTexture4* sprite = &vertexInfo[0])
				{
					GenerateVertexInfo(
						sprite,
						sourceX,
						sourceY,
						sourceW,
						sourceH,
						destinationX,
						destinationY,
						destinationW,
						destinationH,
						color,
						originX,
						originY,
						rotationSin,
						rotationCos,
						depth,
						effects
					);

					if (supportsNoOverwrite)
					{
						offset = UpdateVertexBuffer(0, 1);
					}
					else
					{
						/* We do NOT use Discard here because
						 * it would be stupid to reallocate the
						 * whole buffer just for one sprite.
						 *
						 * Unless you're using this to blit a
						 * target, stop using Immediate ya donut
						 * -flibit
						 */
						offset = 0;
						vertexBuffer.SetDataPointerEXT(
							0,
							(IntPtr) sprite,
							VertexPositionColorTexture4.RealStride,
							SetDataOptions.None
						);
					}
				}
				DrawPrimitives(texture, offset, 1);
			}
			else if (sortMode == SpriteSortMode.Deferred)
			{
				fixed (VertexPositionColorTexture4* sprite = &vertexInfo[numSprites])
				{
					GenerateVertexInfo(
						sprite,
						sourceX,
						sourceY,
						sourceW,
						sourceH,
						destinationX,
						destinationY,
						destinationW,
						destinationH,
						color,
						originX,
						originY,
						rotationSin,
						rotationCos,
						depth,
						effects
					);
				}

				textureInfo[numSprites] = texture;
				numSprites += 1;
			}
			else
			{
				fixed (SpriteInfo* spriteInfo = &spriteInfos[numSprites])
				{
					spriteInfo->textureHash = texture.GetHashCode();
					spriteInfo->sourceX = sourceX;
					spriteInfo->sourceY = sourceY;
					spriteInfo->sourceW = sourceW;
					spriteInfo->sourceH = sourceH;
					spriteInfo->destinationX = destinationX;
					spriteInfo->destinationY = destinationY;
					spriteInfo->destinationW = destinationW;
					spriteInfo->destinationH = destinationH;
					spriteInfo->color = color;
					spriteInfo->originX = originX;
					spriteInfo->originY = originY;
					spriteInfo->rotationSin = rotationSin;
					spriteInfo->rotationCos = rotationCos;
					spriteInfo->depth = depth;
					spriteInfo->effects = effects;
				}

				textureInfo[numSprites] = texture;
				numSprites += 1;
			}
		}

		private unsafe void FlushBatch()
		{
			PrepRenderState();

			if (numSprites == 0)
			{
				// Nothing to do.
				return;
			}

			if (sortMode != SpriteSortMode.Deferred)
			{
				IComparer<IntPtr> comparer;
				if (sortMode == SpriteSortMode.Texture)
				{
					comparer = TextureCompare;
				}
				else if (sortMode == SpriteSortMode.BackToFront)
				{
					comparer = BackToFrontCompare;
				}
				else
				{
					comparer = FrontToBackCompare;
				}
				fixed (SpriteInfo* spriteInfo = &spriteInfos[0]) {
				fixed (IntPtr* sortedSpriteInfo = &sortedSpriteInfos[0]) {
				fixed (VertexPositionColorTexture4* sprites = &vertexInfo[0])
				{
					for (int i = 0; i < numSprites; i += 1)
					{
						sortedSpriteInfo[i] = (IntPtr) (&spriteInfo[i]);
					}
					Array.Sort(
						sortedSpriteInfos,
						textureInfo,
						0,
						numSprites,
						comparer
					);
					for (int i = 0; i < numSprites; i += 1)
					{
						SpriteInfo* info = (SpriteInfo*) sortedSpriteInfo[i];
						GenerateVertexInfo(
							&sprites[i],
							info->sourceX,
							info->sourceY,
							info->sourceW,
							info->sourceH,
							info->destinationX,
							info->destinationY,
							info->destinationW,
							info->destinationH,
							info->color,
							info->originX,
							info->originY,
							info->rotationSin,
							info->rotationCos,
							info->depth,
							info->effects
						);
					}
				}}}
			}

			int arrayOffset = 0;
		nextbatch:
			int batchSize = Math.Min(numSprites, MAX_SPRITES);
			int baseOff = UpdateVertexBuffer(arrayOffset, batchSize);
			int offset = 0;

			Texture2D curTexture = textureInfo[arrayOffset];
			for (int i = 1; i < batchSize; i += 1)
			{
				Texture2D tex = textureInfo[arrayOffset + i];
				if (tex != curTexture)
				{
					DrawPrimitives(curTexture, baseOff + offset, i - offset);
					curTexture = tex;
					offset = i;
				}
			}
			DrawPrimitives(curTexture, baseOff + offset, batchSize - offset);

			if (numSprites > MAX_SPRITES)
			{
				numSprites -= MAX_SPRITES;
				arrayOffset += MAX_SPRITES;
				goto nextbatch;
			}
			numSprites = 0;
		}

		private unsafe int UpdateVertexBuffer(int start, int count)
		{
			int offset;
			SetDataOptions options;
			if (	(bufferOffset + count) > MAX_SPRITES ||
				!supportsNoOverwrite	)
			{
				offset = 0;
				options = SetDataOptions.Discard;
			}
			else
			{
				offset = bufferOffset;
				options = SetDataOptions.NoOverwrite;
			}

			fixed (VertexPositionColorTexture4* p = &vertexInfo[start])
			{
				/* We use Discard here because the last batch
				 * may still be executing, and we can't always
				 * trust the driver to use a staging buffer for
				 * buffer uploads that overlap between commands.
				 *
				 * If you aren't using the whole vertex buffer,
				 * that's your own fault. Use the whole buffer!
				 * -flibit
				 */
				vertexBuffer.SetDataPointerEXT(
					offset * VertexPositionColorTexture4.RealStride,
					(IntPtr) p,
					count * VertexPositionColorTexture4.RealStride,
					options
				);
			}
			bufferOffset = offset + count;
			return offset;
		}

		private static unsafe void GenerateVertexInfo(
			VertexPositionColorTexture4* sprite,
			float sourceX,
			float sourceY,
			float sourceW,
			float sourceH,
			float destinationX,
			float destinationY,
			float destinationW,
			float destinationH,
			Color color,
			float originX,
			float originY,
			float rotationSin,
			float rotationCos,
			float depth,
			byte effects
		) {
			float cornerX = -originX * destinationW;
			float cornerY = -originY * destinationH;
			sprite->Position0.X = (
				(-rotationSin * cornerY) +
				(rotationCos * cornerX) +
				destinationX
			);
			sprite->Position0.Y = (
				(rotationCos * cornerY) +
				(rotationSin * cornerX) +
				destinationY
			);
			cornerX = (1.0f - originX) * destinationW;
			cornerY = -originY * destinationH;
			sprite->Position1.X = (
				(-rotationSin * cornerY) +
				(rotationCos * cornerX) +
				destinationX
			);
			sprite->Position1.Y = (
				(rotationCos * cornerY) +
				(rotationSin * cornerX) +
				destinationY
			);
			cornerX = -originX * destinationW;
			cornerY = (1.0f - originY) * destinationH;
			sprite->Position2.X = (
				(-rotationSin * cornerY) +
				(rotationCos * cornerX) +
				destinationX
			);
			sprite->Position2.Y = (
				(rotationCos * cornerY) +
				(rotationSin * cornerX) +
				destinationY
			);
			cornerX = (1.0f - originX) * destinationW;
			cornerY = (1.0f - originY) * destinationH;
			sprite->Position3.X = (
				(-rotationSin * cornerY) +
				(rotationCos * cornerX) +
				destinationX
			);
			sprite->Position3.Y = (
				(rotationCos * cornerY) +
				(rotationSin * cornerX) +
				destinationY
			);
			fixed (float* flipX = &CornerOffsetX[0]) {
			fixed (float* flipY = &CornerOffsetY[0])
			{
				sprite->TextureCoordinate0.X = (flipX[0 ^ effects] * sourceW) + sourceX;
				sprite->TextureCoordinate0.Y = (flipY[0 ^ effects] * sourceH) + sourceY;
				sprite->TextureCoordinate1.X = (flipX[1 ^ effects] * sourceW) + sourceX;
				sprite->TextureCoordinate1.Y = (flipY[1 ^ effects] * sourceH) + sourceY;
				sprite->TextureCoordinate2.X = (flipX[2 ^ effects] * sourceW) + sourceX;
				sprite->TextureCoordinate2.Y = (flipY[2 ^ effects] * sourceH) + sourceY;
				sprite->TextureCoordinate3.X = (flipX[3 ^ effects] * sourceW) + sourceX;
				sprite->TextureCoordinate3.Y = (flipY[3 ^ effects] * sourceH) + sourceY;
			}}
			sprite->Position0.Z = depth;
			sprite->Position1.Z = depth;
			sprite->Position2.Z = depth;
			sprite->Position3.Z = depth;
			sprite->Color0 = color;
			sprite->Color1 = color;
			sprite->Color2 = color;
			sprite->Color3 = color;
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
			if (customEffect != null)
			{
				foreach (EffectPass pass in customEffect.CurrentTechnique.Passes)
				{
					pass.Apply();
					// Set this _after_ Apply, otherwise EffectParameters override it!
					GraphicsDevice.Textures[0] = texture;
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
				GraphicsDevice.Textures[0] = texture;
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

		#region Private SpriteInfo Container Type

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		private struct SpriteInfo
		{
			/* We store the hash instead of the Texture2D because
			 * it allows this to stay an unmanaged type and prevents
			 * us from constantly calling GetHashCode during sorts.
			 */
			public int textureHash;
			public float sourceX;
			public float sourceY;
			public float sourceW;
			public float sourceH;
			public float destinationX;
			public float destinationY;
			public float destinationW;
			public float destinationH;
			public Color color;
			public float originX;
			public float originY;
			public float rotationSin;
			public float rotationCos;
			public float depth;
			public byte effects;
		}

		#endregion

		#region Private Sprite Comparison Classes

		private class TextureComparer : IComparer<IntPtr>
		{
			public unsafe int Compare(IntPtr i1, IntPtr i2)
			{
				SpriteInfo* p1 = (SpriteInfo*) i1;
				SpriteInfo* p2 = (SpriteInfo*) i2;
				return p1->textureHash.CompareTo(p2->textureHash);
			}
		}

		private class BackToFrontComparer : IComparer<IntPtr>
		{
			public unsafe int Compare(IntPtr i1, IntPtr i2)
			{
				SpriteInfo* p1 = (SpriteInfo*) i1;
				SpriteInfo* p2 = (SpriteInfo*) i2;
				return p2->depth.CompareTo(p1->depth);
			}
		}

		private class FrontToBackComparer : IComparer<IntPtr>
		{
			public unsafe int Compare(IntPtr i1, IntPtr i2)
			{
				SpriteInfo* p1 = (SpriteInfo*) i1;
				SpriteInfo* p2 = (SpriteInfo*) i2;
				return p1->depth.CompareTo(p2->depth);
			}
		}

		#endregion
	}
}
