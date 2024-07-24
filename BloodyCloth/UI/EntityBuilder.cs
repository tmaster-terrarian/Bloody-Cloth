namespace BloodyCloth.UI;

public class EntityBuilder<T>(T entity) where T : Iguina.Entities.Entity
{
    public EntityBuilder<T> SetEventListener(EntityEventType type, Iguina.Entities.EntityEvent entityEvent)
    {
        switch(type)
        {
            case EntityEventType.OnValueChanged: entity.Events.OnValueChanged = entityEvent; break;
            case EntityEventType.OnChecked: entity.Events.OnChecked = entityEvent; break;
            case EntityEventType.OnUnchecked: entity.Events.OnUnchecked = entityEvent; break;
            case EntityEventType.BeforeDraw: entity.Events.BeforeDraw = entityEvent; break;
            case EntityEventType.AfterDraw: entity.Events.AfterDraw = entityEvent; break;
            case EntityEventType.BeforeUpdate: entity.Events.BeforeUpdate = entityEvent; break;
            case EntityEventType.AfterUpdate: entity.Events.AfterUpdate = entityEvent; break;
            case EntityEventType.OnMouseWheelScrollUp: entity.Events.OnMouseWheelScrollUp = entityEvent; break;
            case EntityEventType.OnMouseWheelScrollDown: entity.Events.OnMouseWheelScrollDown = entityEvent; break;
            case EntityEventType.OnLeftMouseDown: entity.Events.OnLeftMouseDown = entityEvent; break;
            case EntityEventType.OnLeftMousePressed: entity.Events.OnLeftMousePressed = entityEvent; break;
            case EntityEventType.OnLeftMouseReleased: entity.Events.OnLeftMouseReleased = entityEvent; break;
            case EntityEventType.OnRightMouseDown: entity.Events.OnRightMouseDown = entityEvent; break;
            case EntityEventType.OnRightMousePressed: entity.Events.OnRightMousePressed = entityEvent; break;
            case EntityEventType.OnRightMouseReleased: entity.Events.OnRightMouseReleased = entityEvent; break;
            case EntityEventType.WhileMouseHover: entity.Events.WhileMouseHover = entityEvent; break;
            case EntityEventType.OnClick: entity.Events.OnClick = entityEvent; break;
            default: throw new System.ArgumentOutOfRangeException(nameof(type));
        }
        return this;
    }

    public EntityBuilder<T> SetSizeInPixels(int? x = null, int? y = null)
    {
        if(x.HasValue) entity.Size.X.SetPixels(x.Value);
        if(y.HasValue) entity.Size.Y.SetPixels(y.Value);
        return this;
    }

    public EntityBuilder<T> SetSizeInPercents(float? x = null, float? y = null)
    {
        if(x.HasValue) entity.Size.X.SetPercents(x.Value);
        if(y.HasValue) entity.Size.Y.SetPercents(y.Value);
        return this;
    }

    public EntityBuilder<T> AddChild<K>(K child, int? index = null) where K : Iguina.Entities.Entity
    {
        entity.AddChild(child, index);
        return this;
    }

    public EntityBuilder<T> AddChild<K>(EntityBuilder<K> builder, int? index = null) where K : Iguina.Entities.Entity
    {
        entity.AddChild(builder, index);
        return this;
    }

    public delegate void OverrideStylesDelegate(Iguina.Defs.StyleSheetState styles);

    public delegate void StyleSheetDelegate(Iguina.Defs.StyleSheet styles);

    public EntityBuilder<T> OverrideStyles(OverrideStylesDelegate overrideStylesDelegate)
    {
        System.ArgumentNullException.ThrowIfNull(overrideStylesDelegate);

        overrideStylesDelegate(entity.OverrideStyles);
        return this;
    }

    public EntityBuilder<T> StyleSheet(StyleSheetDelegate styleSheetDelegate)
    {
        System.ArgumentNullException.ThrowIfNull(styleSheetDelegate);

        styleSheetDelegate(entity.StyleSheet);
        return this;
    }

    public EntityBuilder<T> Modify(Iguina.Entities.EntityEvent entityDelegate)
    {
        System.ArgumentNullException.ThrowIfNull(entityDelegate);

        entityDelegate(entity);
        return this;
    }

    public EntityBuilder<T> SetVisible(bool visible)
    {
        entity.Visible = visible;
        return this;
    }

    public EntityBuilder<T> SetLocked(bool locked)
    {
        entity.Locked = locked;
        return this;
    }

    public T Build()
    {
        return entity;
    }

    // public static implicit operator Iguina.Entities.Entity(EntityBuilder builder)
    // {
    //     return builder.Build();
    // }
}
