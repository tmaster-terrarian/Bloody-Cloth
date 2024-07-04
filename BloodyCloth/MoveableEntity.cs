using System;
using BloodyCloth.Utils;

namespace BloodyCloth;

public abstract class MoveableEntity : Entity
{
    private float yRemainder;
    private float xRemainder;

	public bool NudgeOnMove { get; set; } = true;

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
