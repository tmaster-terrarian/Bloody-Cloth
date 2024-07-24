using BloodyCloth.Graphics;
using Iguina.Entities;
using Microsoft.Xna.Framework;

namespace BloodyCloth.UI;

public class PauseMenu : UIMenu
{
    public override void CreateSelf()
    {
        PauseWhileOpen = true;

        var panel = new Panel(UISystem)
        {
            Anchor = Iguina.Defs.Anchor.Center,
            AutoHeight = true,
        };
        panel.Size.SetPixels(144, 86);

        var btnResume = new Button(UISystem, "Resume");
        btnResume.OverrideStyles.TextAlignment = Iguina.Defs.TextAlignment.Center;
        btnResume.Events.OnLeftMouseReleased = entity => {
            Main.SetMenu(null);
        };
        panel.AddChild(btnResume);

        var btnSettings = new Button(UISystem, "Settings");
        btnSettings.OverrideStyles.TextAlignment = Iguina.Defs.TextAlignment.Center;
        btnSettings.Events.OnLeftMouseReleased = entity => {
            // open settings
        };
        panel.AddChild(btnSettings);

        var btnExit = new Button(UISystem, "Exit");
        btnExit.OverrideStyles.TextAlignment = Iguina.Defs.TextAlignment.Center;
        btnExit.Events.OnLeftMouseReleased = entity => {
            Main.ForceExit();
        };
        panel.AddChild(btnExit);

        Root.AddChild(panel);

        base.CreateSelf();
    }

    public override void PreDraw()
    {
        Renderer.SpriteBatch.Draw(Main.OnePixel, new Rectangle(Point.Zero, Main.ScreenSize), Color.Black * 0.5f);
    }
}
