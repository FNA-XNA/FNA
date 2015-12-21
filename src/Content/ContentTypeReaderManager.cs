#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2015 Ethan Lee and the MonoGame Team
 *
 * Released under the Microsoft Public License.
 * See LICENSE for details.
 */

/* Derived from code by the Mono.Xna Team (Copyright 2006).
 * Released under the MIT License. See monoxna.LICENSE for details.
 */
#endregion

#region Using Statements
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
#endregion

namespace Microsoft.Xna.Framework.Content
{
	public sealed class ContentTypeReaderManager
	{
		#region Private Variables

		private Dictionary<Type, ContentTypeReader> contentReaders;

		#endregion

		#region Private Static Variables

		private static readonly object locker;

		private static readonly string assemblyName;

		private static readonly Dictionary<Type, ContentTypeReader> contentReadersCache;

		// Trick to prevent the linker removing the code, but not actually execute the code
		private static bool falseflag = false;

		/* Static map of type names to creation functions. Required as iOS requires all
		 * types at compile time
		 */
		private static Dictionary<string, Func<ContentTypeReader>> typeCreators =
			new Dictionary<string, Func<ContentTypeReader>>();

		#endregion

		#region Private Static Constructor

		static ContentTypeReaderManager()
		{
			locker = new object();
			contentReadersCache = new Dictionary<Type, ContentTypeReader>(255);
			assemblyName = typeof(ContentTypeReaderManager).Assembly.FullName;
		}

		#endregion

		#region Public Methods

		public ContentTypeReader GetTypeReader(Type targetType)
		{
			ContentTypeReader reader;
			if (contentReaders.TryGetValue(targetType, out reader))
			{
				return reader;
			}
			return null;
		}

		#endregion

		#region Internal Death Defying Method

		internal ContentTypeReader[] LoadAssetReaders(ContentReader reader)
		{
#pragma warning disable 0219, 0649
			/* Trick to prevent the linker removing the code, but not actually execute the code
			 * FIXME: Do we really need this in FNA?
			 */
			if (falseflag)
			{
				/* Dummy variables required for it to work on iDevices ** DO NOT DELETE **
				 * This forces the classes not to be optimized out when deploying to iDevices
				 */
				ByteReader hByteReader = new ByteReader();
				SByteReader hSByteReader = new SByteReader();
				DateTimeReader hDateTimeReader = new DateTimeReader();
				DecimalReader hDecimalReader = new DecimalReader();
				BoundingSphereReader hBoundingSphereReader = new BoundingSphereReader();
				BoundingFrustumReader hBoundingFrustumReader = new BoundingFrustumReader();
				RayReader hRayReader = new RayReader();
				ListReader<char> hCharListReader = new ListReader<Char>();
				ListReader<Rectangle> hRectangleListReader = new ListReader<Rectangle>();
				ArrayReader<Rectangle> hRectangleArrayReader = new ArrayReader<Rectangle>();
				ListReader<Vector3> hVector3ListReader = new ListReader<Vector3>();
				ListReader<StringReader> hStringListReader = new ListReader<StringReader>();
				ListReader<int> hIntListReader = new ListReader<Int32>();
				SpriteFontReader hSpriteFontReader = new SpriteFontReader();
				Texture2DReader hTexture2DReader = new Texture2DReader();
				CharReader hCharReader = new CharReader();
				RectangleReader hRectangleReader = new RectangleReader();
				StringReader hStringReader = new StringReader();
				Vector2Reader hVector2Reader = new Vector2Reader();
				Vector3Reader hVector3Reader = new Vector3Reader();
				Vector4Reader hVector4Reader = new Vector4Reader();
				CurveReader hCurveReader = new CurveReader();
				IndexBufferReader hIndexBufferReader = new IndexBufferReader();
				BoundingBoxReader hBoundingBoxReader = new BoundingBoxReader();
				MatrixReader hMatrixReader = new MatrixReader();
				BasicEffectReader hBasicEffectReader = new BasicEffectReader();
				VertexBufferReader hVertexBufferReader = new VertexBufferReader();
				AlphaTestEffectReader hAlphaTestEffectReader = new AlphaTestEffectReader();
				EnumReader<Microsoft.Xna.Framework.Graphics.SpriteEffects> hEnumSpriteEffectsReader = new EnumReader<Graphics.SpriteEffects>();
				ArrayReader<float> hArrayFloatReader = new ArrayReader<float>();
				ArrayReader<Vector2> hArrayVector2Reader = new ArrayReader<Vector2>();
				ListReader<Vector2> hListVector2Reader = new ListReader<Vector2>();
				ArrayReader<Matrix> hArrayMatrixReader = new ArrayReader<Matrix>();
				EnumReader<Microsoft.Xna.Framework.Graphics.Blend> hEnumBlendReader = new EnumReader<Graphics.Blend>();
				NullableReader<Rectangle> hNullableRectReader = new NullableReader<Rectangle>();
				EffectMaterialReader hEffectMaterialReader = new EffectMaterialReader();
				ExternalReferenceReader hExternalReferenceReader = new ExternalReferenceReader();
				SoundEffectReader hSoundEffectReader = new SoundEffectReader();
				SongReader hSongReader = new SongReader();
				ModelReader hModelReader = new ModelReader();
				Int32Reader hInt32Reader = new Int32Reader();
			}
#pragma warning restore 0219, 0649

			/* The first content byte i read tells me the number of
			 * content readers in this XNB file.
			 */
			int numberOfReaders = reader.Read7BitEncodedInt();
			ContentTypeReader[] newReaders = new ContentTypeReader[numberOfReaders];
			BitArray needsInitialize = new BitArray(numberOfReaders);
			contentReaders = new Dictionary<Type, ContentTypeReader>(numberOfReaders);

			/* Lock until we're done allocating and initializing any new
			 * content type readers... this ensures we can load content
			 * from multiple threads and still cache the readers.
			 */
			lock (locker)
			{
				/* For each reader in the file, we read out the
				 * length of the string which contains the type
				 * of the reader, then we read out the string.
				 * Finally we instantiate an instance of that
				 * reader using reflection.
				 */
				for (int i = 0; i < numberOfReaders; i += 1)
				{
					/* This string tells us what reader we
					 * need to decode the following data.
					 */
					string originalReaderTypeString = reader.ReadString();

					Func<ContentTypeReader> readerFunc;
					if (typeCreators.TryGetValue(originalReaderTypeString, out readerFunc))
					{
						newReaders[i] = readerFunc();
						needsInitialize[i] = true;
					}
					else
					{
						// Need to resolve namespace differences
						string readerTypeString = originalReaderTypeString;
						readerTypeString = PrepareType(readerTypeString);

						Type l_readerType = Type.GetType(readerTypeString);
						if (l_readerType != null)
						{
							ContentTypeReader typeReader;
							if (!contentReadersCache.TryGetValue(l_readerType, out typeReader))
							{
								try
								{
									typeReader = l_readerType.GetDefaultConstructor().Invoke(null) as ContentTypeReader;
								}
								catch (TargetInvocationException ex)
								{
									/* If you are getting here, the Mono runtime
									 * is most likely not able to JIT the type.
									 * In particular, MonoTouch needs help
									 * instantiating types that are only defined
									 * in strings in Xnb files.
									 */
									throw new InvalidOperationException(
										"Failed to get default constructor for ContentTypeReader. " +
										"To work around, add a creation function to ContentTypeReaderManager.AddTypeCreator() " +
										"with the following failed type string: " + originalReaderTypeString,
										ex
									);
								}

								needsInitialize[i] = true;

								contentReadersCache.Add(l_readerType, typeReader);
							}

							newReaders[i] = typeReader;
						}
						else
						{
							throw new ContentLoadException(
									"Could not find ContentTypeReader Type. " +
									"Please ensure the name of the Assembly that " +
									"contains the Type matches the assembly in the full type name: " +
									originalReaderTypeString + " (" + readerTypeString + ")"
							);
						}
					}

					contentReaders.Add(newReaders[i].TargetType, newReaders[i]);

					/* I think the next 4 bytes refer to the "Version" of the type reader,
					 * although it always seems to be zero.
					 */
					reader.ReadInt32();
				}

				// Initialize any new readers.
				for (int i = 0; i < newReaders.Length; i += 1)
				{
					if (needsInitialize.Get(i))
					{
						newReaders[i].Initialize(this);
					}
				}
			} // lock (locker)

			return newReaders;
		}

		#endregion

		#region Internal Static Methods

		/// <summary>
		/// Adds the type creator.
		/// </summary>
		/// <param name='typeString'>
		/// Type string.
		/// </param>
		/// <param name='createFunction'>
		/// Create function.
		/// </param>
		internal static void AddTypeCreator(
			string typeString,
			Func<ContentTypeReader> createFunction
		) {
			if (!typeCreators.ContainsKey(typeString))
			{
				typeCreators.Add(typeString, createFunction);
			}
		}

		internal static void ClearTypeCreators()
		{
			typeCreators.Clear();
		}

		/// <summary>
		/// Removes Version, Culture and PublicKeyToken from a type string.
		/// </summary>
		/// <remarks>
		/// Supports multiple generic types (e.g. Dictionary&lt;TKey,TValue&gt;)
		/// and nested generic types (e.g. List&lt;List&lt;int&gt;&gt;).
		/// </remarks>
		/// <param name="type">
		/// A <see cref="System.String"/>
		/// </param>
		/// <returns>
		/// A <see cref="System.String"/>
		/// </returns>
		internal static string PrepareType(string type)
		{
			// Needed to support nested types
			int count = type.Split(
				new[] {"[["},
				StringSplitOptions.None
			).Length - 1;
			string preparedType = type;
			for (int i = 0; i < count; i += 1)
			{
				preparedType = Regex.Replace(
					preparedType,
					@"\[(.+?), Version=.+?\]",
					"[$1]"
				);
			}
			// Handle non generic types
			if (preparedType.Contains("PublicKeyToken"))
			{
				preparedType = Regex.Replace(
					preparedType,
					@"(.+?), Version=.+?$",
					"$1"
				);
			}
			preparedType = preparedType.Replace(
				", Microsoft.Xna.Framework.Graphics",
				string.Format(
					", {0}",
					assemblyName
				)
			);
			preparedType = preparedType.Replace(
				", Microsoft.Xna.Framework.Video",
				string.Format(
					", {0}",
					assemblyName
				)
			);
			preparedType = preparedType.Replace(
				", Microsoft.Xna.Framework",
				string.Format(
					", {0}",
					assemblyName
				)
			);
			preparedType = preparedType.Replace(
				", MonoGame.Framework",
				string.Format(
					", {0}",
					assemblyName
				)
			);
			return preparedType;
		}

		#endregion
	}
}
