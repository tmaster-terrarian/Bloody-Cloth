using Microsoft.Xna.Framework.Graphics;

using var game = new BloodyCloth.Main();

#if true
game.Headless = true;
BloodyCloth.HeadlessRuntimeHelper.Initialize();

try
{
    game.Run();
}
catch(NoSuitableGraphicsDeviceException e)
{
    BloodyCloth.Main.Logger.LogError("woops: " + e);
}
#else
game.Run();
#endif
