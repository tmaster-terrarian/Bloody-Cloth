namespace BloodyCloth.UI;

public class EntityBuilder<T>(T _entity) where T : Iguina.Entities.Entity
{
    public EntityBuilder<T> SetEventListener(EntityEventType type, Iguina.Entities.EntityEvent entityEvent)
    {
        switch(type)
        {
            case EntityEventType.OnValueChanged: _entity.Events.OnValueChanged = entityEvent; break;
            case EntityEventType.OnChecked: _entity.Events.OnChecked = entityEvent; break;
            case EntityEventType.OnUnchecked: _entity.Events.OnUnchecked = entityEvent; break;
            case EntityEventType.BeforeDraw: _entity.Events.BeforeDraw = entityEvent; break;
            case EntityEventType.AfterDraw: _entity.Events.AfterDraw = entityEvent; break;
            case EntityEventType.BeforeUpdate: _entity.Events.BeforeUpdate = entityEvent; break;
            case EntityEventType.AfterUpdate: _entity.Events.AfterUpdate = entityEvent; break;
            case EntityEventType.OnMouseWheelScrollUp: _entity.Events.OnMouseWheelScrollUp = entityEvent; break;
            case EntityEventType.OnMouseWheelScrollDown: _entity.Events.OnMouseWheelScrollDown = entityEvent; break;
            case EntityEventType.OnLeftMouseDown: _entity.Events.OnLeftMouseDown = entityEvent; break;
            case EntityEventType.OnLeftMousePressed: _entity.Events.OnLeftMousePressed = entityEvent; break;
            case EntityEventType.OnLeftMouseReleased: _entity.Events.OnLeftMouseReleased = entityEvent; break;
            case EntityEventType.OnRightMouseDown: _entity.Events.OnRightMouseDown = entityEvent; break;
            case EntityEventType.OnRightMousePressed: _entity.Events.OnRightMousePressed = entityEvent; break;
            case EntityEventType.OnRightMouseReleased: _entity.Events.OnRightMouseReleased = entityEvent; break;
            case EntityEventType.WhileMouseHover: _entity.Events.WhileMouseHover = entityEvent; break;
            case EntityEventType.OnClick: _entity.Events.OnClick = entityEvent; break;
            default: throw new System.ArgumentOutOfRangeException(nameof(type));
        }
        return this;
    }

    public EntityBuilder<T> SetSizeInPixels(int? x = null, int? y = null)
    {
        if(x.HasValue) _entity.Size.X.SetPixels(x.Value);
        if(y.HasValue) _entity.Size.Y.SetPixels(y.Value);
        return this;
    }

    public EntityBuilder<T> SetSizeInPercents(float? x = null, float? y = null)
    {
        if(x.HasValue) _entity.Size.X.SetPercents(x.Value);
        if(y.HasValue) _entity.Size.Y.SetPercents(y.Value);
        return this;
    }

    public EntityBuilder<T> AddChild<K>(K child, int? index = null) where K : Iguina.Entities.Entity
    {
        _entity.AddChild(child, index);
        return this;
    }

    public EntityBuilder<T> AddChild<K>(EntityBuilder<K> builder, int? index = null) where K : Iguina.Entities.Entity
    {
        _entity.AddChild(builder, index);
        return this;
    }

    public delegate void OverrideStylesDelegate(Iguina.Defs.StyleSheetState styles);

    public delegate void StyleSheetDelegate(Iguina.Defs.StyleSheet styles);

    public EntityBuilder<T> OverrideStyles(OverrideStylesDelegate overrideStylesDelegate)
    {
        System.ArgumentNullException.ThrowIfNull(overrideStylesDelegate);

        overrideStylesDelegate(_entity.OverrideStyles);
        return this;
    }

    public EntityBuilder<T> StyleSheet(StyleSheetDelegate styleSheetDelegate)
    {
        System.ArgumentNullException.ThrowIfNull(styleSheetDelegate);

        styleSheetDelegate(_entity.StyleSheet);
        return this;
    }

    public EntityBuilder<T> Modify(Iguina.Entities.EntityEvent entity)
    {
        System.ArgumentNullException.ThrowIfNull(entity);

        entity(_entity);
        return this;
    }

    public EntityBuilder<T> SetVisible(bool visible)
    {
        _entity.Visible = visible;
        return this;
    }

    public EntityBuilder<T> SetLocked(bool locked)
    {
        _entity.Locked = locked;
        return this;
    }

    public T Build()
    {
        return _entity;
    }

    // public static implicit operator Iguina.Entities.Entity(EntityBuilder builder)
    // {
    //     return builder.Build();
    // }
}
