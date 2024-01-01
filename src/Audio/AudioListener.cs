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

namespace Microsoft.Xna.Framework.Audio
{
	// http://msdn.microsoft.com/en-us/library/microsoft.xna.framework.audio.audiolistener.aspx
	public class AudioListener
	{
		#region Public Properties

		public Vector3 Forward
		{
			get
			{
				return new Vector3(
					listenerData.OrientFront.x,
					listenerData.OrientFront.y,
					-listenerData.OrientFront.z
				);
			}
			set
			{
				listenerData.OrientFront.x = value.X;
				listenerData.OrientFront.y = value.Y;
				listenerData.OrientFront.z = -value.Z;
			}
		}

		public Vector3 Position
		{
			get
			{
				return new Vector3(
					listenerData.Position.x,
					listenerData.Position.y,
					-listenerData.Position.z
				);
			}
			set
			{
				listenerData.Position.x = value.X;
				listenerData.Position.y = value.Y;
				listenerData.Position.z = -value.Z;
			}
		}


		public Vector3 Up
		{
			get
			{
				return new Vector3(
					listenerData.OrientTop.x,
					listenerData.OrientTop.y,
					-listenerData.OrientTop.z
				);
			}
			set
			{
				listenerData.OrientTop.x = value.X;
				listenerData.OrientTop.y = value.Y;
				listenerData.OrientTop.z = -value.Z;
			}
		}

		public Vector3 Velocity
		{
			get
			{
				return new Vector3(
					listenerData.Velocity.x,
					listenerData.Velocity.y,
					-listenerData.Velocity.z
				);
			}
			set
			{
				listenerData.Velocity.x = value.X;
				listenerData.Velocity.y = value.Y;
				listenerData.Velocity.z = -value.Z;
			}
		}

		#endregion

		#region Internal Variables

		internal FAudio.F3DAUDIO_LISTENER listenerData;

		#endregion

		#region Public Constructor

		public AudioListener()
		{
			listenerData = new FAudio.F3DAUDIO_LISTENER();
			Forward = Vector3.Forward;
			Position = Vector3.Zero;
			Up = Vector3.Up;
			Velocity = Vector3.Zero;

			/* Unused variables, defaults based on XNA behavior */
			listenerData.pCone = IntPtr.Zero;
		}

		#endregion
	}
}
