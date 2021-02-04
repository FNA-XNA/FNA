#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2021 Ethan Lee and the MonoGame Team
 *
 * Released under the Microsoft Public License.
 * See LICENSE for details.
 */
#endregion

#region Using Statements
using System;
using System.Collections.ObjectModel;
#endregion

namespace Microsoft.Xna.Framework
{
	public sealed class GameComponentCollection : Collection<IGameComponent>
	{
		#region Events

		/// <summary>
		/// Event that is triggered when a <see cref="GameComponent"/> is added
		/// to this <see cref="GameComponentCollection"/>.
		/// </summary>
		public event EventHandler<GameComponentCollectionEventArgs> ComponentAdded;

		/// <summary>
		/// Event that is triggered when a <see cref="GameComponent"/> is removed
		/// from this <see cref="GameComponentCollection"/>.
		/// </summary>
		public event EventHandler<GameComponentCollectionEventArgs> ComponentRemoved;

		#endregion

		#region Protected Methods

		/// <summary>
		/// Removes every <see cref="GameComponent"/> from this <see cref="GameComponentCollection"/>.
		/// Triggers <see cref="OnComponentRemoved"/> once for each <see cref="GameComponent"/> removed.
		/// </summary>
		protected override void ClearItems()
		{
			for (int i = 0; i < Count; i += 1)
			{
				OnComponentRemoved(new GameComponentCollectionEventArgs(base[i]));
			}
			base.ClearItems();
		}

		protected override void InsertItem(int index, IGameComponent item)
		{
			if (IndexOf(item) != -1)
			{
				throw new ArgumentException("Cannot Add Same Component Multiple Times");
			}
			base.InsertItem(index, item);
			if (item != null)
			{
				OnComponentAdded(new GameComponentCollectionEventArgs(item));
			}
		}

		protected override void RemoveItem(int index)
		{
			IGameComponent gameComponent = base[index];
			base.RemoveItem(index);
			if (gameComponent != null)
			{
				OnComponentRemoved(
					new GameComponentCollectionEventArgs(gameComponent)
				);
			}
		}

		protected override void SetItem(int index, IGameComponent item)
		{
			throw new NotSupportedException();
		}

		#endregion

		#region Private Methods

		private void OnComponentAdded(GameComponentCollectionEventArgs eventArgs)
		{
			if (ComponentAdded != null)
			{
				ComponentAdded(this, eventArgs);
			}
		}

		private void OnComponentRemoved(GameComponentCollectionEventArgs eventArgs)
		{
			if (ComponentRemoved != null)
			{
				ComponentRemoved(this, eventArgs);
			}
		}

		#endregion
	}
}
