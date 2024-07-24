namespace BloodyCloth.UI;

public static class IguinaExtensions
{
    public static EntityBuilder<T> Builder<T>(this T entity) where T : Iguina.Entities.Entity
    {
        return new EntityBuilder<T>(entity);
    }

    public static T AddChild<T>(this Iguina.Entities.Entity entity, EntityBuilder<T> builder, int? index = null) where T : Iguina.Entities.Entity
    {
        return entity.AddChild(builder.Build());
    }
}
