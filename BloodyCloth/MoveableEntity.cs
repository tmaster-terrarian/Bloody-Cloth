using System;

using Microsoft.Xna.Framework;

using BloodyCloth.Utils;

namespace BloodyCloth;

public abstract class MoveableEntity : Entity
{
    private float yRemainder;
    private float xRemainder;

	public bool NudgeOnMove { get; set; } = true;

	public virtual bool NoCollide { get; set; }
	public virtual bool CollidesWithSolids { get; set; } = true;
	public virtual bool CollidesWithJumpthroughs { get; set; } = true;

	public bool OnGround { get; protected set; }

	public bool CheckOnGround()
	{
		return CheckColliding(Hitbox.Shift(0, 1));
	}

	public bool CheckColliding(Rectangle rectangle, bool ignoreJumpThroughs = false)
	{
		if(NoCollide) return false;

		if(Main.World.TileMeeting(rectangle)) return true;
		if(CollidesWithSolids && Main.World.SolidMeeting(rectangle)) return true;

		if(!ignoreJumpThroughs)
		{
			return CheckCollidingJumpthrough(rectangle);
		}

		return false;
	}

	public bool CheckCollidingJumpthrough(Rectangle rectangle)
	{
		if(NoCollide) return false;
		if(!CollidesWithJumpthroughs) return false;

		Rectangle newRect = new(rectangle.Left, rectangle.Bottom - 1, rectangle.Width, 1);

		Rectangle rect = Main.World.JumpThroughPlace(newRect) ?? Rectangle.Empty;
		Rectangle rect2 = Main.World.JumpThroughPlace(newRect.Shift(0, -1)) ?? Rectangle.Empty;

		if(rect != Rectangle.Empty) return rect != rect2;

		Line line = Main.World.JumpThroughSlopePlace(newRect) ?? Line.Empty;
		Line line2 = Main.World.JumpThroughSlopePlace(newRect.Shift(0, -1)) ?? Line.Empty;

		if(line != Line.Empty) return line != line2;

		return false;
	}

    public virtual void MoveX(float amount, Action? onCollide)
    {
        xRemainder += amount;
        int move = Extensions.Round(xRemainder);
        xRemainder -= move;

        if(move != 0)
        {
            int sign = Math.Sign(move);
            while(move != 0)
            {
                bool col1 = CheckColliding((sign >= 0 ? RightEdge : LeftEdge).Shift(new(sign, 0)));
                if(col1 && !CheckColliding((sign >= 0 ? RightEdge : LeftEdge).Shift(new(sign, -1)), true) && NudgeOnMove)
                {
                    // slope up
                    position.X += sign;
                    position.Y -= 1;
                    move -= sign;
                }
                else if(!col1)
                {
                    if(OnGround && NudgeOnMove)
                    {
                        // slope down
                        if(!CheckColliding(BottomEdge.Shift(new(sign, 1))) && CheckColliding(BottomEdge.Shift(new(sign, 2))))
                            position.Y += 1;
                    }
                    position.X += sign;
                    move -= sign;
                }
                else
                {
                    onCollide?.Invoke();
                    break;
                }
            }
        }
    }

    public virtual void MoveY(float amount, Action? onCollide)
    {
        yRemainder += amount;
        int move = Extensions.Round(yRemainder);
        yRemainder -= move;

        if(move != 0)
        {
            int sign = Math.Sign(move);
            while(move != 0)
            {
                if(!CheckColliding((sign >= 0 ? BottomEdge : TopEdge).Shift(new(0, sign)), sign < 0))
                {
                    position.Y += sign;
                    move -= sign;
                    continue;
                }

                onCollide?.Invoke();
                break;
            }
        }
    }
}
