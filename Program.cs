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
    game.Headless = true;
    game.ForcedInit();
    while(true)
    {
        game.ForcedUpdate();
        System.Threading.Thread.Sleep(170);
    }
}
#else
game.Run();
#endif
