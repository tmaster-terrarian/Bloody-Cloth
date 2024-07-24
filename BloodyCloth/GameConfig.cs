using Microsoft.Xna.Framework;

namespace BloodyCloth;

public static class GameConfig
{
    public static class Audio
    {
        public static bool MuteWhenUnfocused { get; set; } = true;

        static float masterVolume = 1;
        public static float MasterVolume
        {
            get => masterVolume;
            set => masterVolume = MathHelper.Clamp(value, 0, 1);
        }

        static float soundVolume = 1;
        public static float SoundVolume
        {
            get => soundVolume;
            set => soundVolume = MathHelper.Clamp(value, 0, 1);
        }

        public static float CalculatedSoundVolume => SoundVolume * MasterVolume;

        static float musicVolume = 1;
        public static float MusicVolume
        {
            get => musicVolume;
            set => musicVolume = MathHelper.Clamp(value, 0, 1);
        }

        public static float CalculatedMusicVolume => MusicVolume * MasterVolume;
    }
}
