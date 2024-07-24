using Microsoft.Xna.Framework;

using BloodyCloth.Graphics;

using Iguina.Entities;

namespace BloodyCloth.UI;

public class SettingsMenu : UIMenu
{
    private Iguina.Entities.Entity settingsSubmenuHolder;
    private Iguina.Entities.Entity settingsSubmenu;

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

    public override void HandleBackButton()
    {
        if(!Main.InMenu)
            Main.SetMenu(new PauseMenu());
        else
            base.HandleBackButton();
    }

    private void CreateMenu()
    {
        int padding = 4;

        settingsSubmenuHolder = new Panel(UISystem) {
            Anchor = Iguina.Defs.Anchor.CenterRight
        };
        settingsSubmenuHolder.Size.SetPercents((1 - 2/12f) * 100 * (1 - 4f/Main.ScreenSize.X), 100);

        SettingsSubmenus[] menuOrder = [
            SettingsSubmenus.Gameplay,
            SettingsSubmenus.Audio,
            SettingsSubmenus.Display
        ];

        SetPage(
            new Panel(UISystem) {
                Anchor = Iguina.Defs.Anchor.Center,
                Identifier = "Settings"
            }
            .Builder()
            .SetSizeInPixels(Main.ScreenSize.X, Main.ScreenSize.Y)
            .OverrideStyles(styles => {
                styles.Padding = new(padding, padding, padding, padding);
                styles.TintColor = new(0, 0, 0, 0);
            })
            .AddChild(
                new Panel(UISystem) {
                    Anchor = Iguina.Defs.Anchor.AutoInlineLTR
                }
                .Builder()
                .SetSizeInPercents(2/12f * 100, 100)
                .AddChild(CreateSettingsSubmenuOption(menuOrder[0], true))
                .AddChild(CreateSettingsSubmenuOption(menuOrder[1]))
                .AddChild(CreateSettingsSubmenuOption(menuOrder[2]))
                .AddChild(
                    new Button(UISystem, "Back") {
                        Anchor = Iguina.Defs.Anchor.BottomCenter,
                        ExclusiveSelection = false
                    }
                    .Builder()
                    .SetEventListener(EntityEventType.OnClick, entity => {
                        Main.SetMenu(new PauseMenu());
                    })
                )
            )
            .AddChild(settingsSubmenuHolder)
        );

        CreateSettingsSubmenu(menuOrder[0]);
    }

    enum SettingsSubmenus
    {
        Gameplay,
        Audio,
        Display
    }

    private SettingsSubmenus _lastSelectedSettingsSubmenu = (SettingsSubmenus)(-1);

    private EntityBuilder<Button> CreateSettingsSubmenuOption(SettingsSubmenus menu, bool first = false)
    {
        return new Button(UISystem, menu.ToString())
        {
            Anchor = Iguina.Defs.Anchor.AutoLTR,
            ToggleCheckOnClick = true,
            CanClickToUncheck = false,
            ExclusiveSelection = true,
            Checked = first
        }
        .Builder()
        .SetEventListener(EntityEventType.OnClick, entity => {
            CreateSettingsSubmenu(menu);
        });
    }

    private void CreateSettingsSubmenu(SettingsSubmenus menu)
    {
        if(_lastSelectedSettingsSubmenu == menu) return;
        _lastSelectedSettingsSubmenu = menu;

        settingsSubmenu?.RemoveSelf();
        settingsSubmenu = settingsSubmenuHolder.AddChild(
            menu switch
            {
                SettingsSubmenus.Gameplay =>
                    new Iguina.Entities.Entity(UISystem, null).Builder()
                    .AddChild(new Button(UISystem, "test"))
                    .AddChild(new Button(UISystem, "test2")),
                SettingsSubmenus.Audio =>
                    new Iguina.Entities.Entity(UISystem, null).Builder()
                    .AddChild(new Paragraph(UISystem, "hello whats up"))
                    .AddChild(new Button(UISystem, "what")),
                SettingsSubmenus.Display =>
                    new Iguina.Entities.Entity(UISystem, null).Builder()
                    .AddChild(new RowsSpacer(UISystem))
                    .AddChild(new Paragraph(UISystem, "idk")),
                _ =>
                    new Iguina.Entities.Entity(UISystem, null).Builder(),
            }
        );
    }
}
