#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2015 Ethan Lee and the MonoGame Team
 *
 * Released under the Microsoft Public License.
 * See LICENSE for details.
 */
#endregion

namespace Microsoft.Xna.Framework.Audio
{
	// http://msdn.microsoft.com/en-us/library/microsoft.xna.framework.audio.audiolistener.aspx
	public class AudioListener
	{
		#region Public Properties

		public Vector3 Forward
		{
			get;
			set;
		}

		public Vector3 Position
		{
			get;
			set;
		}


		public Vector3 Up
		{
			get;
			set;
		}

		public Vector3 Velocity
		{
			get;
			set;
		}

		#endregion

		#region Public Constructor

		public AudioListener()
		{
			Forward = Vector3.Forward;
			Position = Vector3.Zero;
			Up = Vector3.Up;
			Velocity = Vector3.Zero;
		}

		#endregion
	}
}
