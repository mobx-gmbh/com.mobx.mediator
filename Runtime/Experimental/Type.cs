namespace MobX.Mediator.Experimental
{
    internal enum Type
    {
        /// <summary>
        ///     The value is serialized in the inspector and treated like a constant value.
        /// </summary>
        Serialized = 0,

        /// <summary>
        ///     The value is modified during runtime and reset after leaving edit mode.
        /// </summary>
        Runtime = 1,

        /// <summary>
        ///     The value is saved persistent between sessions and part of a save game.
        /// </summary>
        Persistent = 2,

        /// <summary>
        ///     The value is provided by an external getter and routed to an external setter.
        /// </summary>
        Property = 3
    }
}