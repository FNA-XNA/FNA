#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2024 Ethan Lee and the MonoGame Team
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

		private static readonly Dictionary<Type, ContentTypeReader> contentReadersCache;

		private static readonly Regex regex;
		private static readonly string regexReplacement;

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

			regex = new Regex(@", (Microsoft.Xna.Framework.Graphics|Microsoft.Xna.Framework.Video|Microsoft.Xna.Framework|MonoGame.Framework), Version=.+?, Culture=.+?, PublicKeyToken=[^\]]+", RegexOptions.Compiled);
			regexReplacement = string.Format(", {0}", typeof(ContentTypeReaderManager).Assembly.FullName);
		}

		#endregion

		#region Internal Constructor

		internal ContentTypeReaderManager()
		{
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

			/* If you got here, you're in a really nasty spot...
			 * In extremely rare cases, a nested type will show up
			 * and it will STILL depend on the Microsoft DLL names.
			 * So, we get to prepare one more time. I bet this is
			 * super annoying for anyone that wants the null case
			 * rather than whatever the hell this mess is.
			 * FIXME: Do we need FullName or can we trust ToString?
			 * -flibit
			 */
			Type fixType = Type.GetType(PrepareType(targetType.FullName), false);
			if (!ReferenceEquals(fixType, null) && contentReaders.TryGetValue(fixType, out reader))
			{
				return reader;
			}

			return null;
		}

		#endregion

		#region Internal Death Defying Method

		internal ContentTypeReader[] LoadAssetReaders(ContentReader reader)
		{
			/* Trick to prevent the linker removing the code, but not actually execute the code */
			if (falseflag)
			{
				/* Dummy variables required for it to work on iDevices ** DO NOT DELETE **
				 * This forces the classes not to be optimized out when PublishTrimmed:true
				 */
				typeof(ByteReader).GetMembers();
				typeof(SByteReader).GetMembers();
				typeof(DateTimeReader).GetMembers();
				typeof(DecimalReader).GetMembers();
				typeof(BoundingSphereReader).GetMembers();
				typeof(BoundingFrustumReader).GetMembers();
				typeof(RayReader).GetMembers();
				typeof(ListReader<Char>).GetMembers();
				typeof(ListReader<Rectangle>).GetMembers();
				typeof(ArrayReader<Rectangle>).GetMembers();
				typeof(ListReader<Vector3>).GetMembers();
				typeof(ListReader<StringReader>).GetMembers();
				typeof(ListReader<Int32>).GetMembers();
				typeof(SpriteFontReader).GetMembers();
				typeof(Texture2DReader).GetMembers();
				typeof(CharReader).GetMembers();
				typeof(RectangleReader).GetMembers();
				typeof(StringReader).GetMembers();
				typeof(Vector2Reader).GetMembers();
				typeof(Vector3Reader).GetMembers();
				typeof(Vector4Reader).GetMembers();
				typeof(CurveReader).GetMembers();
				typeof(IndexBufferReader).GetMembers();
				typeof(BoundingBoxReader).GetMembers();
				typeof(MatrixReader).GetMembers();
				typeof(BasicEffectReader).GetMembers();
				typeof(VertexBufferReader).GetMembers();
				typeof(AlphaTestEffectReader).GetMembers();
				typeof(EnumReader<Graphics.SpriteEffects>).GetMembers();
				typeof(ArrayReader<float>).GetMembers();
				typeof(ArrayReader<Vector2>).GetMembers();
				typeof(ListReader<Vector2>).GetMembers();
				typeof(ArrayReader<Matrix>).GetMembers();
				typeof(EnumReader<Graphics.Blend>).GetMembers();
				typeof(NullableReader<Rectangle>).GetMembers();
				typeof(EffectMaterialReader).GetMembers();
				typeof(ExternalReferenceReader).GetMembers();
				typeof(SoundEffectReader).GetMembers();
				typeof(SongReader).GetMembers();
				typeof(ModelReader).GetMembers();
				typeof(Int32Reader).GetMembers();
			}

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
						if (!ReferenceEquals(l_readerType, null))
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
								catch (NullReferenceException ex)
								{
									/* If you are getting here, you are
									 * probably using .NET AOT and have
									 * an incomplete rd.xml, to aid with
									 * this, show a helpful exception
									 */
									throw new InvalidOperationException(
										"Failed to get default constructor for ContentTypeReader. " +
										"If you're using .NET Native AOT, ensure your rd.xml contains the following type: " +
										originalReaderTypeString,
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

					if (!ReferenceEquals(newReaders[i].TargetType, null))
					{
						contentReaders.Add(newReaders[i].TargetType, newReaders[i]);
					}

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
		private static string PrepareType(string type)
		{
			return regex.Replace(type, regexReplacement);
		}

		#endregion
	}
}
