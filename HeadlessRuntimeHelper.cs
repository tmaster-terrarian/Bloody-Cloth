using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace BloodyCloth;

public static class HeadlessRuntimeHelper
{
    private static ContentManager _contentManager;

    public static void Initialize()
    {
        var cache = (Dictionary<Type, ContentTypeReader>) typeof(ContentTypeReaderManager).GetField(
                "_contentReadersCache",
                BindingFlags.Static | BindingFlags.NonPublic)
            .GetValue(null);

        Type readerType = typeof(Game).Assembly.GetType("Microsoft.Xna.Framework.Content.Texture2DReader");
        cache.Add(readerType, new HeadlessTexture2DReader());
    }

    public static ContentManager GetContentManager(string contentPath = "Content")
    {
        return _contentManager ??= new ContentManager(new GameServiceContainer(), contentPath);
    }

    private class HeadlessTexture2DReader : ContentTypeReader<Texture2D>
    {
        private readonly FieldInfo _heightFieldInfo =
            typeof(Texture2D).GetField("height", BindingFlags.Instance | BindingFlags.NonPublic);
        private readonly FieldInfo _widthFieldInfo =
            typeof(Texture2D).GetField("width", BindingFlags.Instance | BindingFlags.NonPublic);

        protected override Texture2D Read(ContentReader reader, Texture2D existingInstance)
        {
            reader.ReadInt32();
            var width = reader.ReadInt32();
            var height = reader.ReadInt32();
            var levelCount = reader.ReadInt32();
            for (var level = 0; level < levelCount; level++)
            {
                var levelDataSizeInBytes = reader.ReadInt32();
                reader.BaseStream.Position += levelDataSizeInBytes;
            }

            Texture2D texture = existingInstance;
            if (texture == null)
            {
                texture = (Texture2D) FormatterServices.GetUninitializedObject(typeof(Texture2D));
                _widthFieldInfo.SetValue(texture, width);
                _heightFieldInfo.SetValue(texture, height);
            }
            return texture;
        }
    }
}
