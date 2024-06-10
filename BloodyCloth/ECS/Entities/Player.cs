using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using BloodyCloth.Ecs.Components;

namespace BloodyCloth.Ecs.Entities;

public class Player : Entity
{
    Sprite sprite = null;
    PlayerControls playerControls = null;

    List<Texture2D> textures = null;

    Vector2 velocity = Vector2.Zero;

    public Player()
    {
        textures = new List<Texture2D> {
            Main.GetContent<Texture2D>("Images/player")
        };

        GetComponent<Transform>().position = new(100, 100);

        sprite = AddComponent(new Sprite {
            texture = textures[0],
            origin = new Point(16, 32)
        });

        playerControls = AddComponent(new PlayerControls());
    }

    protected override void CleanupUnmanaged()
    {
        sprite = null;
        playerControls = null;

        foreach(var tex in textures)
        {
            tex.Dispose();
        }
        textures.Clear();
        textures = null;
    }
}
