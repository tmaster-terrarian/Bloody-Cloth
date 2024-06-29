using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using BloodyCloth.Utils;

namespace BloodyCloth.Graphics;

public static class NineSlice
{
	public static void DrawNineSlice(Texture2D texture, Rectangle destinationRectangle, Rectangle? sourceRectangle, Point topLeft, Point bottomRight, Color color, Vector2 origin, SpriteEffects effects = SpriteEffects.None, float layerDepth = 0)
	{
		if(texture == null)
		{
			throw new System.ArgumentNullException(nameof(texture));
		}

		var dest = destinationRectangle;
		var src = sourceRectangle ?? texture.Bounds;

		if(dest.Size == Point.Zero) return;
		if(src.Size == Point.Zero) return;

		// if the nineslice would end up being pointless, just draw the texture directly
		if(dest.Size == src.Size)
		{
			Renderer.SpriteBatch.Draw(texture, dest, src, color, 0, origin, effects, layerDepth);
			return;
		}

		int left = topLeft.X;
		int top = topLeft.Y;
		int right = bottomRight.X;
		int bottom = bottomRight.Y;

		Rectangle topLeftRect =		new(dest.X,							dest.Y,							left,						top);

		Rectangle topRect =			new(dest.X + left,					dest.Y,							dest.Width - left - right,	top);

		Rectangle topRightRect =	new(dest.X + dest.Width - right,	dest.Y,							right,						top);

		Rectangle leftRect =		new(dest.X,							dest.Y + top,					left,						dest.Height - top - bottom);

		Rectangle centerRect =		new(dest.X + left,					dest.Y + top,					dest.Width - left - right,	dest.Height - top - bottom);

		Rectangle rightRect =		new(dest.X + dest.Width - right,	dest.Y + top,					right,						dest.Height - top - bottom);

		Rectangle bottomLeftRect =	new(dest.X, 						dest.Y + dest.Height - bottom,	left,						bottom);

		Rectangle bottomRect =		new(dest.X + left,					dest.Y + dest.Height - bottom,	dest.Width - left - right,	bottom);

		Rectangle bottomRightRect =	new(dest.X + dest.Width - right,	dest.Y + dest.Height - bottom,	right,						bottom);


		Rectangle topLeftSrc =		new(src.X,							src.Y,							left,						top);

		Rectangle topSrc =			new(src.X + left,					src.Y,							src.Width - left - right,	top);

		Rectangle topRightSrc =		new(src.X + src.Width - right,		src.Y,							right,						top);

		Rectangle leftSrc =			new(src.X,							src.Y + top,					left,						src.Height - top - bottom);

		Rectangle centerSrc =		new(src.X + left,					src.Y + top,					src.Width - left - right,	src.Height - top - bottom);

		Rectangle rightSrc =		new(src.X + src.Width - right,		src.Y + top,					right,						src.Height - top - bottom);

		Rectangle bottomLeftSrc =	new(src.X, 							src.Y + src.Height - bottom,	left,						bottom);

		Rectangle bottomSrc =		new(src.X + left,					src.Y + src.Height - bottom,	src.Width - left - right,	bottom);

		Rectangle bottomRightSrc =	new(src.X + src.Width - right,		src.Y + src.Height - bottom,	right,						bottom);


		if(topLeftRect.Size != Point.Zero)
			Renderer.SpriteBatch.Draw(texture, topLeftRect.Shift((-origin).ToPoint()),		topLeftSrc,		color, 0, Vector2.Zero, effects, layerDepth);

		if(topRect.Size != Point.Zero)
			Renderer.SpriteBatch.Draw(texture, topRect.Shift((-origin).ToPoint()),			topSrc,			color, 0, Vector2.Zero, effects, layerDepth);

		if(topRightRect.Size != Point.Zero)
			Renderer.SpriteBatch.Draw(texture, topRightRect.Shift((-origin).ToPoint()),		topRightSrc,	color, 0, Vector2.Zero, effects, layerDepth);

		if(leftRect.Size != Point.Zero)
			Renderer.SpriteBatch.Draw(texture, leftRect.Shift((-origin).ToPoint()),			leftSrc,		color, 0, Vector2.Zero, effects, layerDepth);

		if(centerRect.Size != Point.Zero)
			Renderer.SpriteBatch.Draw(texture, centerRect.Shift((-origin).ToPoint()),		centerSrc,		color, 0, Vector2.Zero, effects, layerDepth);

		if(rightRect.Size != Point.Zero)
			Renderer.SpriteBatch.Draw(texture, rightRect.Shift((-origin).ToPoint()),		rightSrc,		color, 0, Vector2.Zero, effects, layerDepth);

		if(bottomLeftRect.Size != Point.Zero)
			Renderer.SpriteBatch.Draw(texture, bottomLeftRect.Shift((-origin).ToPoint()),	bottomLeftSrc,	color, 0, Vector2.Zero, effects, layerDepth);

		if(bottomRect.Size != Point.Zero)
			Renderer.SpriteBatch.Draw(texture, bottomRect.Shift((-origin).ToPoint()),		bottomSrc,		color, 0, Vector2.Zero, effects, layerDepth);

		if(bottomRightRect.Size != Point.Zero)
			Renderer.SpriteBatch.Draw(texture, bottomRightRect.Shift((-origin).ToPoint()),	bottomRightSrc,	color, 0, Vector2.Zero, effects, layerDepth);
	}

	public static void DrawNineSlice(Texture2D texture, Rectangle destinationRectangle, Rectangle? sourceRectangle, Point topLeft, Point bottomRight, Color color)
	{
		DrawNineSlice(texture, destinationRectangle, sourceRectangle, topLeft, bottomRight, color, Vector2.Zero);
	}

	public static void DrawNineSlice(Texture2D texture, Rectangle destinationRectangle, Rectangle? sourceRectangle, Rectangle centerSliceBounds, Color color)
	{
		if(texture == null)
		{
			throw new System.ArgumentNullException(nameof(texture));
		}

		DrawNineSlice(texture, destinationRectangle, sourceRectangle, centerSliceBounds.Location, (sourceRectangle ?? texture.Bounds).Size - (centerSliceBounds.Location + centerSliceBounds.Size), color);
	}
}
