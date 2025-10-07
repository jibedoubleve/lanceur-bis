namespace Lanceur.Core.Decorators;

public sealed class EntityDecorator<TEntity>
{
    #region Constructors

    public EntityDecorator(TEntity entity) => Entity = entity;

    #endregion

    #region Properties

    /// <summary>
    ///     This object keep the state of this entity.
    /// </summary>
    public TEntity Entity { get; }

    /// <summary>
    ///     Indicates whether the contained <see cref="Entity" /> has been
    ///     updated in the database or not.
    /// </summary>
    public bool IsDirty { get; private set; }

    #endregion

    #region Methods

    /// <summary>
    ///     Mark the contained <see cref="Entity" /> as to be updated in the database
    /// </summary>
    public void MarkAsDirty() => IsDirty = true;

    #endregion
}