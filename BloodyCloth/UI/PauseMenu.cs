using Microsoft.Xna.Framework;

using BloodyCloth.Graphics;

using Iguina.Entities;

namespace BloodyCloth.UI;

public class PauseMenu : UIMenu
{
    protected override void CreateSelf()
    {
        PauseWhileOpen = true;

        CreateMenu();

        base.CreateSelf();
    }

    public override void PreDraw()
    {
        Renderer.SpriteBatch.Draw(Main.OnePixel, new Rectangle(Point.Zero, Main.ScreenSize), Color.Black * 0.5f);
    }

    private void CreateMenu()
    {
        Root.AddChild(
            new Panel(UISystem) {
                Anchor = Iguina.Defs.Anchor.Center,
                AutoHeight = true,
                Identifier = "Root"
            }
            .Builder()
            .SetSizeInPixels(144, 24 * 3 + 8)
            .AddChild(
                new Button(UISystem, "Resume").Builder()
                .SetEventListener(EntityEventType.OnClick, entity => {
                    Main.SetMenu(null);
                })
            )
            .AddChild(
                new Button(UISystem, "Settings").Builder()
                .SetEventListener(EntityEventType.OnClick, entity => {
                    Main.SetMenu(new SettingsMenu());
                })
            )
            .AddChild(
                new Button(UISystem, "Exit").Builder()
                .SetEventListener(EntityEventType.OnClick, entity => {
                    Main.ForceExit();
                })
            )
        );
    }
}
