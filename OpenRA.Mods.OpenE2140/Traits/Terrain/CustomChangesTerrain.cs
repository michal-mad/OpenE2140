﻿using OpenRA.Traits;

namespace OpenRA.Mods.OpenE2140.Traits.Terrain;

[Desc("Modifies the terrain type underneath the actors location.")]
class CustomChangesTerrainInfo : TraitInfo
{
	[FieldLoader.Require]
	public readonly string TerrainType = string.Empty;

	public override object Create(ActorInitializer init) { return new CustomChangesTerrain(this); }
}

class CustomChangesTerrain : INotifyAddedToWorld, INotifyRemovedFromWorld
{
	readonly CustomChangesTerrainInfo info;
	byte? previousTerrain;

	public CustomChangesTerrain(CustomChangesTerrainInfo info)
	{
		this.info = info;
	}

	void INotifyAddedToWorld.AddedToWorld(Actor self)
	{
		var cell = self.Location;
		var map = self.World.Map;
		var terrain = map.Rules.TerrainInfo.GetTerrainIndex(this.info.TerrainType);
		this.previousTerrain = map.CustomTerrain[cell];
		map.CustomTerrain[cell] = terrain;
	}

	void INotifyRemovedFromWorld.RemovedFromWorld(Actor self)
	{
		if (this.previousTerrain == null)
			return;

		var cell = self.Location;
		var map = self.World.Map;
		map.CustomTerrain[cell] = this.previousTerrain.Value;
	}
}
