#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2018 Ethan Lee and the MonoGame Team
 *
 * Released under the Microsoft Public License.
 * See LICENSE for details.
 */
#endregion

/* This code is based on the code borrowed from LibGDX: https://github.com/libgdx/libgdx */

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using static Microsoft.Xna.Framework.Graphics.ShaderGLDevice;

namespace Microsoft.Xna.Framework.Graphics
{
	/// <summary>
	/// <p>
	/// A shader program encapsulates a vertex and fragment shader pair linked to form a shader program.
	/// </summary>
	/// <remarks>
	/// <p>
	/// A shader program encapsulates a vertex and fragment shader pair linked to form a shader program.
	/// </p>
	/// <p>
	/// After construction a Shader can be used to draw
	/// <see cref="Mesh"/>
	/// . To make the GPU use a specific Shader the programs
	/// <see cref="Apply"/>
	/// method must be used which effectively binds the program.
	/// </p>
	/// <p>
	/// When a Shader is bound one can set uniforms, vertex attributes and attributes as needed via the respective methods.
	/// </p>
	/// <p>
	/// A Shader can be unbound with a call to
	/// <see cref="End"/>
	/// </p>
	/// <p>
	/// A Shader must be disposed via a call to
	/// <see cref="dispose()"/>
	/// when it is no longer needed
	/// </p>
	/// <p>
	/// Shaders are managed. In case the OpenGL context is lost all shaders get invalidated and have to be reloaded. This
	/// happens on Android when a user switches to another application or receives an incoming call. Managed Shaders are
	/// automatically reloaded when the OpenGL context is recreated so you don't have to do this manually.
	/// </p>
	/// </remarks>
	/// <author>author of the original Java code: mzechner</author>
	public class Shader : GraphicsResource, IGLEffect
	{
		public class Uniform
		{
			public int Location;
			public int Type;
			public int Size;
		}

		/// <summary>the log</summary>
		private string _log = string.Empty;

		/// <summary>uniform lookup</summary>
		private readonly Dictionary<string, Uniform> _uniforms = new Dictionary<string, Uniform>();

		/// <summary>attribute lookup</summary>
		private readonly Dictionary<string, Uniform> _attributes = new Dictionary<string, Uniform>();

		/// <summary>program handle</summary>
		private int _programHandle;

		/// <summary>vertex shader handle</summary>
		private int _vertexShaderHandle;

		/// <summary>fragment shader handle</summary>
		private int _fragmentShaderHandle;

		/// <summary>vertex shader source</summary>
		private readonly string _vertexShaderSource;

		/// <summary>fragment shader source</summary>
		private readonly string _fragmentShaderSource;

		/// <summary>whether this shader was invalidated</summary>
		private bool _invalidated;

		private readonly Dictionary<VertexElementUsage, int> _attributesLocation = new Dictionary<VertexElementUsage, int>();

		/// <summary>code that is always added to the vertex shader code, typically used to inject a #version line.
		/// 	</summary>
		/// <remarks>
		/// code that is always added to the vertex shader code, typically used to inject a #version line. Note that this is added
		/// as-is, you should include a newline (`\n`) if needed.
		/// </remarks>
		public static string PrependVertexCode = string.Empty;

		/// <summary>code that is always added to every fragment shader code, typically used to inject a #version line.
		/// 	</summary>
		/// <remarks>
		/// code that is always added to every fragment shader code, typically used to inject a #version line. Note that this is added
		/// as-is, you should include a newline (`\n`) if needed.
		/// </remarks>
		public static string PrependFragmentCode = string.Empty;

		private readonly ShaderGLDevice _device;

		/// <returns>
		/// the log info for the shader compilation and program linking stage. The shader needs to be bound for this method to
		/// have an effect.
		/// </returns>
		public string Log
		{
			get
			{
				if (string.IsNullOrEmpty(_log))
				{
					_log = GetProgramInfoLog(_programHandle);
				}

				return _log;
			}
		}

		internal int ProgramHandle
		{
			get { return _programHandle; }
		}

		internal int VertexShaderHandle
		{
			get { return _vertexShaderHandle; }
		}

		internal int FragmentShaderHandle
		{
			get { return _fragmentShaderHandle; }
		}

		public string VertexShaderSource
		{
			get { return _vertexShaderSource; }
		}

		public string FragmentShaderSource
		{
			get { return _fragmentShaderSource; }
		}

		public Dictionary<string, Uniform> Uniforms
		{
			get { return _uniforms; }
		}

		public Dictionary<string, Uniform> Attributes
		{
			get { return _attributes; }
		}

		internal Dictionary<VertexElementUsage, int> AttributesLocations
		{
			get { return _attributesLocation; }
		}

		public IntPtr EffectData
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		/// <summary>Constructs a new Shader and immediately compiles it.</summary>
		/// <param name="device"></param>
		/// <param name="vertexShader">the vertex shader</param>
		/// <param name="fragmentShader">the fragment shader</param>
		/// <param name="attributesUsage"></param>
		public Shader(GraphicsDevice device, string vertexShader, string fragmentShader, Dictionary<string, VertexElementUsage> attributesUsage)
		{
			if (device == null)
			{
				throw new ArgumentNullException("graphicsDevice");
			}
			if (vertexShader == null)
			{
				throw new ArgumentNullException("vertexShader");
			}
			if (fragmentShader == null)
			{
				throw new ArgumentNullException("fragmentShader");
			}

			if (attributesUsage == null)
			{
				throw new ArgumentNullException("attributesUsage");
			}

			GraphicsDevice = device;
			_device = (ShaderGLDevice)device.GLDevice;

			if (!string.IsNullOrEmpty(PrependVertexCode))
			{
				vertexShader = PrependVertexCode + vertexShader;
			}
			if (!string.IsNullOrEmpty(PrependFragmentCode))
			{
				fragmentShader = PrependFragmentCode + fragmentShader;
			}
			_vertexShaderSource = vertexShader;
			_fragmentShaderSource = fragmentShader;
			CompileShaders(vertexShader, fragmentShader);
			FetchAttributes();
			FetchUniforms();

			// Build attributes map
			foreach(var pair in attributesUsage)
			{
				var attribute = _attributes[pair.Key];

				_attributesLocation[pair.Value] = attribute.Location;
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (!IsDisposed)
			{
				GraphicsDevice.GLDevice.AddDisposeEffect(this);
			}
			base.Dispose(disposing);
		}

		/// <summary>Loads and compiles the shaders, creates a new program and links the shaders.
		/// 	</summary>
		/// <param name="vertexShader"/>
		/// <param name="fragmentShader"></param>
		private void CompileShaders(string vertexShader, string fragmentShader)
		{
			_vertexShaderHandle = LoadShader(GLenum.GL_VERTEX_SHADER, vertexShader);
			_fragmentShaderHandle = LoadShader(GLenum.GL_FRAGMENT_SHADER, fragmentShader);
			_programHandle = LinkProgram(_device.glCreateProgram());
		}

		private int LoadShader(GLenum type, string source)
		{
			var shader = _device.glCreateShader(type);
			if (shader == 0)
			{
				throw new Exception(string.Format("glCreateShader({0})", type));
			}

			unsafe
			{
				var hGlobal = Marshal.StringToHGlobalAnsi(source);
				var ptr = (sbyte*)hGlobal;
				_device.glShaderSource(shader, 1, &ptr, null);
				Marshal.FreeHGlobal(hGlobal);
			}

			_device.glCompileShader(shader);
			int compiled;

			unsafe
			{
				_device.glGetShaderiv(shader, GLenum.GL_COMPILE_STATUS, &compiled);
			}

			if (compiled == 0)
			{
				var _byteBuffer = new byte[256];
				unsafe
				{
					fixed (byte* ptr = _byteBuffer)
					{
						int length;
						_device.glGetShaderInfoLog(_programHandle, _byteBuffer.Length, &length, (sbyte*)ptr);

						var infoLog = Marshal.PtrToStringAnsi(new IntPtr(ptr), length);
						throw new Exception(infoLog);
					}
				}
			}
			return shader;
		}

		private string GetProgramInfoLog(int program)
		{
			unsafe
			{
				var byteBuffer = new byte[256];

				fixed (byte* ptr = byteBuffer)
				{
					int length;
					_device.glGetProgramInfoLog(program, byteBuffer.Length, &length, (sbyte*)ptr);

					var name = Marshal.PtrToStringAnsi(new IntPtr(ptr), length);

					return name;
				}
			}
		}

		private int LinkProgram(int program)
		{
			_device.glAttachShader(program, _vertexShaderHandle);
			_device.glAttachShader(program, _fragmentShaderHandle);
			_device.glLinkProgram(program);

			int linked;
			unsafe
			{
				_device.glGetProgramiv(program, GLenum.GL_LINK_STATUS, &linked);
			}
			if (linked == 0)
			{
				throw new Exception(GetProgramInfoLog(program));
			}
			return program;
		}

		private unsafe int GetAttribLocation(int program, string name)
		{
			var hGlobal = Marshal.StringToHGlobalAnsi(name);
			var result = _device.glGetAttribLocation(program, (sbyte*)hGlobal);
			Marshal.FreeHGlobal(hGlobal);

			return result;
		}

		public Uniform FetchAttribute(string name)
		{
			Uniform attribute;
			if (_attributes.TryGetValue(name, out attribute))
				return attribute;

			var location = GetAttribLocation(_programHandle, name);
			if (location == -1)
			{
				return null;
			}

			attribute = new Uniform
			{
				Location = location
			};

			_attributes[name] = attribute;
			return attribute;
		}

		private unsafe int GetUniformLocation(int program, string name)
		{
			var hGlobal = Marshal.StringToHGlobalAnsi(name);
			var result = _device.glGetUniformLocation(program, (sbyte*)hGlobal);
			Marshal.FreeHGlobal(hGlobal);

			return result;
		}

		public Uniform FetchUniform(string name)
		{
			Uniform uniform;
			if (_uniforms.TryGetValue(name, out uniform))
				return uniform;

			var location = GetUniformLocation(_programHandle, name);
			if (location == -1)
			{
				return null;
			}

			uniform = new Uniform
			{
				Location = location
			};

			_uniforms[name] = uniform;
			return uniform;
		}

		/// <summary>Sets the uniform with the given name.</summary>
		/// <remarks>
		/// Sets the uniform with the given name. The
		/// <see cref="Shader"/>
		/// must be bound for this to work.
		/// </remarks>
		/// <param name="name">the name of the uniform</param>
		/// <param name="value">the value</param>
		public void SetUniformi(string name, int value)
		{
			CheckManaged();
			var uniform = FetchUniform(name);
			_device.glUniform1i(uniform.Location, value);
		}

		public void SetUniformi(int location, int value)
		{
			CheckManaged();
			_device.glUniform1i(location, value);
		}

		/// <summary>Sets the uniform with the given name.</summary>
		/// <remarks>
		/// Sets the uniform with the given name. The
		/// <see cref="Shader"/>
		/// must be bound for this to work.
		/// </remarks>
		/// <param name="name">the name of the uniform</param>
		/// <param name="value1">the first value</param>
		/// <param name="value2">the second value</param>
		public void SetUniformi(string name, int value1, int value2)
		{
			CheckManaged();
			var uniform = FetchUniform(name);
			_device.glUniform2i(uniform.Location, value1, value2);
		}

		public void SetUniformi(int location, int value1, int value2)
		{
			CheckManaged();
			_device.glUniform2i(location, value1, value2);
		}

		/// <summary>Sets the uniform with the given name.</summary>
		/// <remarks>
		/// Sets the uniform with the given name. The
		/// <see cref="Shader"/>
		/// must be bound for this to work.
		/// </remarks>
		/// <param name="name">the name of the uniform</param>
		/// <param name="value1">the first value</param>
		/// <param name="value2">the second value</param>
		/// <param name="value3">the third value</param>
		public void SetUniformi(string name, int value1, int value2, int value3)
		{
			CheckManaged();
			var uniform = FetchUniform(name);
			_device.glUniform3i(uniform.Location, value1, value2, value3);
		}

		public void SetUniformi(int location, int value1, int value2, int value3)
		{
			CheckManaged();
			_device.glUniform3i(location, value1, value2, value3);
		}

		/// <summary>Sets the uniform with the given name.</summary>
		/// <remarks>
		/// Sets the uniform with the given name. The
		/// <see cref="Shader"/>
		/// must be bound for this to work.
		/// </remarks>
		/// <param name="name">the name of the uniform</param>
		/// <param name="value1">the first value</param>
		/// <param name="value2">the second value</param>
		/// <param name="value3">the third value</param>
		/// <param name="value4">the fourth value</param>
		public void SetUniformi(string name, int value1, int value2, int value3, int value4)
		{
			CheckManaged();
			var uniform = FetchUniform(name);
			_device.glUniform4i(uniform.Location, value1, value2, value3, value4);
		}

		public void SetUniformi(int location, int value1, int value2, int value3, int value4)
		{
			CheckManaged();
			_device.glUniform4i(location, value1, value2, value3, value4);
		}

		/// <summary>Sets the uniform with the given name.</summary>
		/// <remarks>
		/// Sets the uniform with the given name. The
		/// <see cref="Shader"/>
		/// must be bound for this to work.
		/// </remarks>
		/// <param name="name">the name of the uniform</param>
		/// <param name="value">the value</param>
		public void SetUniformf(string name, float value)
		{
			CheckManaged();
			var uniform = FetchUniform(name);
			_device.glUniform1f(uniform.Location, value);
		}

		public void SetUniformf(int location, float value)
		{
			CheckManaged();
			_device.glUniform1f(location, value);
		}

		/// <summary>Sets the uniform with the given name.</summary>
		/// <remarks>
		/// Sets the uniform with the given name. The
		/// <see cref="Shader"/>
		/// must be bound for this to work.
		/// </remarks>
		/// <param name="name">the name of the uniform</param>
		/// <param name="value1">the first value</param>
		/// <param name="value2">the second value</param>
		public void SetUniformf(string name, float value1, float value2)
		{
			CheckManaged();
			var uniform = FetchUniform(name);
			_device.glUniform2f(uniform.Location, value1, value2);
		}

		public void SetUniformf(int location, float value1, float value2)
		{
			CheckManaged();
			_device.glUniform2f(location, value1, value2);
		}

		/// <summary>Sets the uniform with the given name.</summary>
		/// <remarks>
		/// Sets the uniform with the given name. The
		/// <see cref="Shader"/>
		/// must be bound for this to work.
		/// </remarks>
		/// <param name="name">the name of the uniform</param>
		/// <param name="value1">the first value</param>
		/// <param name="value2">the second value</param>
		/// <param name="value3">the third value</param>
		public void SetUniformf(string name, float value1, float value2, float value3)
		{
			CheckManaged();
			var uniform = FetchUniform(name);
			_device.glUniform3f(uniform.Location, value1, value2, value3);
		}

		public void SetUniformf(int location, float value1, float value2, float value3)
		{
			CheckManaged();
			_device.glUniform3f(location, value1, value2, value3);
		}

		/// <summary>Sets the uniform with the given name.</summary>
		/// <remarks>
		/// Sets the uniform with the given name. The
		/// <see cref="Shader"/>
		/// must be bound for this to work.
		/// </remarks>
		/// <param name="name">the name of the uniform</param>
		/// <param name="value1">the first value</param>
		/// <param name="value2">the second value</param>
		/// <param name="value3">the third value</param>
		/// <param name="value4">the fourth value</param>
		public void SetUniformf(string name, float value1, float value2, float value3, float value4)
		{
			CheckManaged();
			var uniform = FetchUniform(name);
			_device.glUniform4f(uniform.Location, value1, value2, value3, value4);
		}

		public void SetUniformf(int location, float value1, float value2, float value3, float value4)
		{
			CheckManaged();
			_device.glUniform4f(location, value1, value2, value3, value4);
		}

		private unsafe void Uniform1fv(int location, int count, float[] v, int offset)
		{
			fixed (float* ptr = &v[offset])
			{
				_device.glUniform1fv(location, count, ptr);
			}
		}

		private unsafe void Uniform2fv(int location, int count, float[] v, int offset)
		{
			fixed (float* ptr = &v[offset])
			{
				_device.glUniform2fv(location, count, ptr);
			}
		}

		private unsafe void Uniform3fv(int location, int count, float[] v, int offset)
		{
			fixed (float* ptr = &v[offset])
			{
				_device.glUniform3fv(location, count, ptr);
			}
		}

		private unsafe void Uniform4fv(int location, int count, float[] v, int offset)
		{
			fixed (float* ptr = &v[offset])
			{
				_device.glUniform4fv(location, count, ptr);
			}
		}

		public void SetUniform1fv(string name, float[] values, int offset, int length)
		{
			CheckManaged();
			var uniform = FetchUniform(name);
			Uniform1fv(uniform.Location, length, values, offset);
		}

		public void SetUniform1fv(int location, float[] values, int offset, int length)
		{
			CheckManaged();
			Uniform1fv(location, length, values, offset);
		}

		public void SetUniform2fv(string name, float[] values, int offset, int length)
		{
			CheckManaged();
			var uniform = FetchUniform(name);
			Uniform2fv(uniform.Location, length / 2, values, offset);
		}

		public void SetUniform2fv(int location, float[] values, int offset, int length)
		{
			CheckManaged();
			Uniform2fv(location, length / 2, values, offset);
		}

		public void SetUniform3fv(string name, float[] values, int offset, int length)
		{
			var uniform = FetchUniform(name);
			Uniform3fv(uniform.Location, length / 3, values, offset);
		}

		public void SetUniform3fv(int location, float[] values, int offset, int length)
		{
			CheckManaged();
			Uniform3fv(location, length / 3, values, offset);
		}

		public void SetUniform4fv(string name, float[] values, int offset, int length)
		{
			CheckManaged();
			var uniform = FetchUniform(name);
			Uniform4fv(uniform.Location, length / 4, values, offset);
		}

		public void SetUniform4fv(int location, float[] values, int offset, int length)
		{
			CheckManaged();
			Uniform4fv(location, length / 4, values, offset);
		}

		private unsafe void UniformMatrix4fv(int location, int count, bool transpose, ref Matrix m)
		{
			fixed (Matrix* ptr = &m)
			{
				_device.glUniformMatrix4fv(location, count, transpose ? (byte)1 : (byte)0, (float*)ptr);
			}
		}

		/// <summary>Sets the uniform matrix with the given name.</summary>
		/// <remarks>
		/// Sets the uniform matrix with the given name. The
		/// <see cref="Shader"/>
		/// must be bound for this to work.
		/// </remarks>
		/// <param name="name">the name of the uniform</param>
		/// <param name="matrix">the matrix</param>
		public void SetUniformMatrix(string name, ref Matrix matrix)
		{
			SetUniformMatrix(name, ref matrix, false);
		}

		/// <summary>Sets the uniform matrix with the given name.</summary>
		/// <remarks>
		/// Sets the uniform matrix with the given name. The
		/// <see cref="Shader"/>
		/// must be bound for this to work.
		/// </remarks>
		/// <param name="name">the name of the uniform</param>
		/// <param name="matrix">the matrix</param>
		/// <param name="transpose">whether the matrix should be transposed</param>
		public void SetUniformMatrix(string name, ref Matrix matrix, bool transpose)
		{
			SetUniformMatrix(FetchUniform(name).Location, ref matrix, transpose);
		}

		public void SetUniformMatrix(int location, ref Matrix matrix)
		{
			SetUniformMatrix(location, ref matrix, false);
		}

		public void SetUniformMatrix(int location, ref Matrix matrix, bool transpose)
		{
			CheckManaged();
			UniformMatrix4fv(location, 1, transpose, ref matrix);
		}

		/// <summary>Sets the uniform with the given name.</summary>
		/// <remarks>
		/// Sets the uniform with the given name. The
		/// <see cref="Shader"/>
		/// must be bound for this to work.
		/// </remarks>
		/// <param name="name">the name of the uniform</param>
		/// <param name="values">x and y as the first and second values respectively</param>
		public void SetUniformf(string name, Vector2 values)
		{
			SetUniformf(name, values.X, values.Y);
		}

		public void SetUniformf(int location, Vector2 values)
		{
			SetUniformf(location, values.X, values.Y);
		}

		/// <summary>Sets the uniform with the given name.</summary>
		/// <remarks>
		/// Sets the uniform with the given name. The
		/// <see cref="Shader"/>
		/// must be bound for this to work.
		/// </remarks>
		/// <param name="name">the name of the uniform</param>
		/// <param name="values">x, y and z as the first, second and third values respectively
		/// 	</param>
		public void SetUniformf(string name, Vector3 values)
		{
			SetUniformf(name, values.X, values.Y, values.Z);
		}

		public void SetUniformf(int location, Vector3 values)
		{
			SetUniformf(location, values.X, values.Y, values.Z);
		}

		/// <summary>Sets the uniform with the given name.</summary>
		/// <remarks>
		/// Sets the uniform with the given name. The
		/// <see cref="Shader"/>
		/// must be bound for this to work.
		/// </remarks>
		/// <param name="name">the name of the uniform</param>
		/// <param name="values">r, g, b and a as the first through fourth values respectively
		/// 	</param>
		public void SetUniformf(string name, Vector4 values)
		{
			SetUniformf(name, values.X, values.Y, values.Z, values.W);
		}

		public void SetUniformf(int location, Vector4 values)
		{
			SetUniformf(location, values.X, values.Y, values.Z, values.W);
		}

		public void Apply()
		{
			CheckManaged();
			_device.ApplyShader(this);
		}

		private void CheckManaged()
		{
			if (!_invalidated)
				return;

			CompileShaders(_vertexShaderSource, _fragmentShaderSource);
			_invalidated = false;
		}

		private void FetchUniforms()
		{
			int numUniforms;
			unsafe
			{
				_device.glGetProgramiv(_programHandle, GLenum.GL_ACTIVE_UNIFORMS, &numUniforms);
			}

			var byteBuffer = new byte[256];
			for (var i = 0; i < numUniforms; i++)
			{
				string name;
				int size, type;

				unsafe
				{
					fixed (byte* ptr = byteBuffer)
					{
						int length, lsize, ltype;
						_device.glGetActiveUniform(_programHandle, i, byteBuffer.Length, &length, &lsize, &ltype, (sbyte*)ptr);

						size = lsize;
						type = ltype;

						name = Marshal.PtrToStringAnsi(new IntPtr(ptr), length);
					}
				}

				var location = GetUniformLocation(_programHandle, name);

				var uniform = new Uniform
				{
					Location = location,
					Type = type,
					Size = size
				};

				_uniforms[name] = uniform;
			}
		}

		private void FetchAttributes()
		{
			int numAttributes;

			unsafe
			{
				_device.glGetProgramiv(_programHandle, GLenum.GL_ACTIVE_ATTRIBUTES, &numAttributes);
			}

			var byteBuffer = new byte[256];
			for (var i = 0; i < numAttributes; i++)
			{
				string name;
				int size, type;

				unsafe
				{
					fixed (byte* ptr = byteBuffer)
					{
						int length, lsize, ltype;
						_device.glGetActiveAttrib(_programHandle, i, byteBuffer.Length, &length, &lsize, &ltype, (sbyte*)ptr);

						size = lsize;
						type = ltype;

						name = Marshal.PtrToStringAnsi(new IntPtr(ptr), length);
					}
				}

				var location = GetAttribLocation(_programHandle, name);

				var uniform = new Uniform
				{
					Location = location,
					Type = type,
					Size = size
				};

				_attributes[name] = uniform;
			}
		}

		public void Invalidate()
		{
			_invalidated = true;
		}
	}
}