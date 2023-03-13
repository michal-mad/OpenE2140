﻿#region Copyright & License Information

/*
 * Copyright 2007-2023 The OpenE2140 Developers (see AUTHORS)
 * This file is part of OpenE2140, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */

#endregion

using System.Reflection;
using JetBrains.Annotations;
using OpenRA.Graphics;
using OpenRA.Mods.Common.Traits.Render;
using OpenRA.Traits;

namespace OpenRA.Mods.OpenE2140.Traits.Rendering;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
[Desc("Custom trait that generates FactionImages from actor name")]
public class FactionRenderSpritesInfo : TraitInfo
{
	[Desc(
		"List of factions to generate faction images for. Faction image is not generated for faction, which name is prefix of actor's name"
		+ "(e.g. 'ucs_vehicles_tiger_assault' is UCS unit by default, so it's considered as default.)"
	)]
	public readonly List<string> Factions = new List<string>();

	public override object Create(ActorInitializer init)
	{
		return new FactionRenderSprites(this);
	}
}

public class FactionRenderSprites : IWorldLoaded
{
	private readonly FactionRenderSpritesInfo info;

	public FactionRenderSprites(FactionRenderSpritesInfo info)
	{
		this.info = info;
	}

	void IWorldLoaded.WorldLoaded(World world, WorldRenderer worldRenderer)
	{
		foreach (var actorInfo in world.Map.Rules.Actors.Values)
		{
			var renderSpritesInfo = actorInfo.TraitInfoOrDefault<RenderSpritesInfo>();

			if (renderSpritesInfo == null)
				continue;

			if (renderSpritesInfo.FactionImages == null)
			{
				renderSpritesInfo.GetType()
					.GetField("FactionImages", BindingFlags.Instance | BindingFlags.Public)
					?.SetValue(renderSpritesInfo, new Dictionary<string, string>());
			}

			foreach (var faction in this.info.Factions)
			{
				var factionImageName = $"{actorInfo.Name}.{faction}";

				try
				{
					world.Map.Sequences.Sequences(factionImageName);
				}
				catch
				{
					continue;
				}

				renderSpritesInfo.FactionImages?.TryAdd(faction, factionImageName);
			}
		}
	}
}
