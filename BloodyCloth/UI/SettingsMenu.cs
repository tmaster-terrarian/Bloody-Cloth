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
        switch(menu)
        {
            case SettingsSubmenus.Gameplay:
                settingsSubmenu = settingsSubmenuHolder.AddChild(
                    new Iguina.Entities.Entity(UISystem, null).Builder()

                    .AddChild(new Button(UISystem, "test"))

                    .AddChild(new Button(UISystem, "test2"))
                );
                break;
            case SettingsSubmenus.Audio:
                var masterPercent = new Paragraph(UISystem, $"{(int)(GameConfig.Audio.MasterVolume * 100)}%") {
                    ShrinkWidthToMinimalSize = false,
                    AutoWidth = false,
                    Anchor = Iguina.Defs.Anchor.AutoInlineLTR
                };

                var soundPercent = new Paragraph(UISystem, $"{(int)(GameConfig.Audio.SoundVolume * 100)}%") {
                    ShrinkWidthToMinimalSize = false,
                    AutoWidth = false,
                    Anchor = Iguina.Defs.Anchor.AutoInlineLTR
                };

                var musicPercent = new Paragraph(UISystem, $"{(int)(GameConfig.Audio.MusicVolume * 100)}%") {
                    ShrinkWidthToMinimalSize = false,
                    AutoWidth = false,
                    Anchor = Iguina.Defs.Anchor.AutoInlineLTR
                };

                settingsSubmenu = settingsSubmenuHolder.AddChild(
                    new Iguina.Entities.Entity(UISystem, null).Builder()

                    .AddChild(
                        new Checkbox(UISystem, "Mute when unfocused") {
                            Anchor = Iguina.Defs.Anchor.AutoInlineLTR,
                            Checked = GameConfig.Audio.MuteWhenUnfocused
                        }
                        .Builder()
                        .SetEventListener(EntityEventType.OnValueChanged, entity => {
                            GameConfig.Audio.MuteWhenUnfocused = (entity as Checkbox).Checked;
                        })
                    )

                    .AddChild(new HorizontalLine(UISystem))

                    .AddChild(
                        new Paragraph(UISystem, "Master Volume") {
                            ShrinkWidthToMinimalSize = false,
                            AutoWidth = false
                        }
                        .Builder()
                        .SetSizeInPercents(19, null)
                    )
                    .AddChild(
                        new Slider(UISystem) {
                            Anchor = Iguina.Defs.Anchor.AutoInlineLTR,
                            MaxValue = 100,
                            StepsCount = 100,
                            MouseWheelStep = 10,
                            ValueSafe = (int)(GameConfig.Audio.MasterVolume * 100),
                        }
                        .Builder()
                        .Modify(entity => {
                            entity.Offset.Y.SetPixels(4);
                        })
                        .SetSizeInPercents(70, null)
                        .SetEventListener(EntityEventType.OnValueChanged, entity => {
                            GameConfig.Audio.MasterVolume = (entity as Slider).ValuePercent;
                            masterPercent.Text = $"{(entity as Slider).Value}%";
                        })
                    )
                    .AddChild(
                        masterPercent.Builder()
                        .Modify(entity => {
                            entity.Offset.Y.SetPixels(-4);
                        })
                        .SetSizeInPercents(9, null)
                        .OverrideStyles(styles => {
                            styles.TextAlignment = Iguina.Defs.TextAlignment.Right;
                        })
                    )

                    .AddChild(new RowsSpacer(UISystem))

                    .AddChild(
                        new Paragraph(UISystem, "Sound Volume") {
                            ShrinkWidthToMinimalSize = false,
                            AutoWidth = false
                        }
                        .Builder()
                        .SetSizeInPercents(19, null)
                    )
                    .AddChild(
                        new Slider(UISystem) {
                            Anchor = Iguina.Defs.Anchor.AutoInlineLTR,
                            MaxValue = 100,
                            StepsCount = 100,
                            MouseWheelStep = 10,
                            ValueSafe = (int)(GameConfig.Audio.SoundVolume * 100),
                        }
                        .Builder()
                        .Modify(entity => {
                            entity.Offset.Y.SetPixels(4);
                        })
                        .SetSizeInPercents(70, null)
                        .SetEventListener(EntityEventType.OnValueChanged, entity => {
                            GameConfig.Audio.SoundVolume = (entity as Slider).ValuePercent;
                            soundPercent.Text = $"{(entity as Slider).Value}%";
                        })
                    )
                    .AddChild(
                        soundPercent.Builder()
                        .Modify(entity => {
                            entity.Offset.Y.SetPixels(-4);
                        })
                        .SetSizeInPercents(9, null)
                        .OverrideStyles(styles => {
                            styles.TextAlignment = Iguina.Defs.TextAlignment.Right;
                        })
                    )

                    .AddChild(new RowsSpacer(UISystem))

                    .AddChild(
                        new Paragraph(UISystem, "Music Volume") {
                            ShrinkWidthToMinimalSize = false,
                            AutoWidth = false
                        }
                        .Builder()
                        .SetSizeInPercents(19, null)
                    )
                    .AddChild(
                        new Slider(UISystem) {
                            Anchor = Iguina.Defs.Anchor.AutoInlineLTR,
                            MaxValue = 100,
                            StepsCount = 100,
                            MouseWheelStep = 10,
                            ValueSafe = (int)(GameConfig.Audio.MusicVolume * 100)
                        }
                        .Builder()
                        .Modify(entity => {
                            entity.Offset.Y.SetPixels(4);
                        })
                        .SetSizeInPercents(70, null)
                        .SetEventListener(EntityEventType.OnValueChanged, entity => {
                            GameConfig.Audio.MusicVolume = (entity as Slider).ValuePercent;
                            musicPercent.Text = $"{(entity as Slider).Value}%";
                        })
                    )
                    .AddChild(
                        musicPercent.Builder()
                        .Modify(entity => {
                            entity.Offset.Y.SetPixels(-4);
                        })
                        .SetSizeInPercents(9, null)
                        .OverrideStyles(styles => {
                            styles.TextAlignment = Iguina.Defs.TextAlignment.Right;
                        })
                    )
                );
                break;
            case SettingsSubmenus.Display:
                settingsSubmenu = settingsSubmenuHolder.AddChild(
                    new Iguina.Entities.Entity(UISystem, null).Builder()
                    .AddChild(new Paragraph(UISystem, "idk"))
                );
                break;
            default:
                settingsSubmenu = settingsSubmenuHolder.AddChild(new Iguina.Entities.Entity(UISystem, null).Builder());
                break;
        }
    }
}
