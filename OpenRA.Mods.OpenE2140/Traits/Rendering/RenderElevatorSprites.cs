﻿#region Copyright & License Information

/*
 * Copyright (c) The OpenE2140 Developers and Contributors
 * This file is part of OpenE2140, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */

#endregion

using JetBrains.Annotations;
using OpenRA.Graphics;
using OpenRA.Mods.Common.Traits.Render;
using OpenRA.Mods.OpenE2140.Graphics;
using OpenRA.Mods.OpenE2140.Helpers.Reflection;
using OpenRA.Primitives;

namespace OpenRA.Mods.OpenE2140.Traits.Rendering;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
[Desc("Cutoff sprites before rendering, used for the elevator.")]
public class RenderElevatorSpritesInfo : RenderSpritesInfo
{
	public override object Create(ActorInitializer init)
	{
		return new RenderElevatorSprites(init, this);
	}
}

public class RenderElevatorSprites : RenderSprites
{
	private static readonly TypeFieldHelper<Sprite> SpriteFieldHelper = ReflectionHelper.GetTypeFieldHelper<Sprite>(typeof(SpriteRenderable), "sprite");

	private readonly RenderSpritesReflectionHelper reflectionHelper;

	public RenderElevatorSprites(ActorInitializer init, RenderSpritesInfo info)
		: base(init, info)
	{
		this.reflectionHelper = new RenderSpritesReflectionHelper(this);
	}

	public override IEnumerable<IRenderable> Render(Actor self, WorldRenderer worldRenderer)
	{
		var renderables = this.reflectionHelper.RenderAnimations(
			self,
			worldRenderer,
			this.reflectionHelper.GetVisibleAnimations(),
			(anim, renderables) =>
			{
				if (anim is CutOffAnimationWithOffset elevatorAnimation)
					RenderElevatorSprites.PostProcess(renderables, elevatorAnimation.CutOff());
			}
		);

		return renderables;
	}

	public static void PostProcess(IEnumerable<IRenderable> renderables, int bottom)
	{
		foreach (var renderable in renderables.OfType<SpriteRenderable>())
		{
			var sprite = RenderElevatorSprites.SpriteFieldHelper.GetValue(renderable);

			if (sprite == null)
				continue;

			var height = sprite.Bounds.Height;
			var offset = sprite.Offset.Y;
			var current = renderable.Offset.Y - renderable.Offset.Z + height * 16 / 2;

			if (current > bottom)
			{
				height = Math.Max(height - (current - bottom) / 16, 0);
				offset -= (sprite.Bounds.Height - height) / 2f;
			}

			RenderElevatorSprites.SpriteFieldHelper.SetValue(
				renderable,
				new Sprite(
					sprite.Sheet,
					new Rectangle(sprite.Bounds.X, sprite.Bounds.Y, sprite.Bounds.Width, height),
					sprite.ZRamp,
					new float3(sprite.Offset.X, offset, sprite.Offset.Z),
					sprite.Channel,
					sprite.BlendMode
				)
			);
		}
	}
}
