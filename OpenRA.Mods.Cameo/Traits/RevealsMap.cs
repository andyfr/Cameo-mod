﻿#region Copyright & License Information
/*
 * Copyright 2007-2020 The OpenRA Developers (see AUTHORS)
 * This file is part of OpenRA, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */
#endregion

using System.Linq;
using OpenRA.Traits;

namespace OpenRA.Mods.Common.Traits
{
	[Desc("Reveals shroud and fog across the whole map while active.")]
	public class RevealsMapInfo : ConditionalTraitInfo
	{
		[Desc("Should the map also be revealed for the allies of the actor's owner?")]
		public readonly bool IncludeAllies = false;

		[Desc("Can this actor reveal shroud generated by the `GeneratesShroud` trait?")]
		public readonly bool RevealGeneratedShroud = true;

		public override object Create(ActorInitializer init) { return new RevealsMap(this); }
	}

	public class RevealsMap : ConditionalTrait<RevealsMapInfo>, INotifyKilled, INotifyActorDisposing
	{
		readonly Shroud.SourceType type;

		public RevealsMap(RevealsMapInfo info)
			: base(info)
		{
			type = info.RevealGeneratedShroud ? Shroud.SourceType.Visibility
				: Shroud.SourceType.PassiveVisibility;
		}

		protected void AddCellsToPlayerShroud(Actor self, Player p, PPos[] uv)
		{
			if (Info.IncludeAllies)
			{
				foreach (var player in self.World.Players)
					if (self.Owner.IsAlliedWith(player))
						player.Shroud.AddSource(this, type, uv);
			}
			else
			{
				self.Owner.Shroud.AddSource(this, type, uv);
			}
		}

		protected void RemoveCellsFromPlayerShroud(Actor self, Player p)
		{
			if (Info.IncludeAllies)
			{
				foreach (var player in self.World.Players)
				if (self.Owner.IsAlliedWith(player))
					player.Shroud.RemoveSource(this);
		}
			else
			{
				self.Owner.Shroud.RemoveSource(this);
			}
		}

		protected PPos[] ProjectedCells(Actor self)
		{
			var map = self.World.Map;
			return map.ProjectedCellBounds.ToArray();
		}

		void INotifyActorDisposing.Disposing(Actor self)
		{
			RemoveCellsFromPlayerShroud(self, self.Owner);
		}

		void INotifyKilled.Killed(Actor self, AttackInfo e)
		{
			RemoveCellsFromPlayerShroud(self, self.Owner);
		}

		protected override void TraitEnabled(Actor self)
		{
			AddCellsToPlayerShroud(self, self.Owner, ProjectedCells(self));
		}

		protected override void TraitDisabled(Actor self)
		{
			RemoveCellsFromPlayerShroud(self, self.Owner);
		}
	}
}
