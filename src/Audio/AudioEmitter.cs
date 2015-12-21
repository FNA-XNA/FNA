#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2015 Ethan Lee and the MonoGame Team
 *
 * Released under the Microsoft Public License.
 * See LICENSE for details.
 */
#endregion

#region Using Statements
using System;
#endregion

namespace Microsoft.Xna.Framework.Audio
{
	// http://msdn.microsoft.com/en-us/library/microsoft.xna.framework.audio.audioemitter.aspx
	public class AudioEmitter
	{
		#region Public Properties

		private float INTERNAL_dopplerScale;
		public float DopplerScale
		{
			get
			{
				return INTERNAL_dopplerScale;
			}
			set
			{
				if (value < 0.0f)
				{
					throw new ArgumentOutOfRangeException("AudioEmitter.DopplerScale must be greater than or equal to 0.0f");
				}
				INTERNAL_dopplerScale = value;
			}
		}

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

		public AudioEmitter()
		{
			DopplerScale = 1.0f;
			Forward = Vector3.Forward;
			Position = Vector3.Zero;
			Up = Vector3.Up;
			Velocity = Vector3.Zero;
		}

		#endregion
	}
}
