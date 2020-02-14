
namespace CMI.Processor.DAL
{
    /// <summary>
    /// Processor Type
    /// </summary>
    public enum ProcessorType
    {
        /// <summary>
        /// Both processor type
        /// </summary>
        Both = 0,

        /// <summary>
        /// Inbound processor type
        /// </summary>
        Inbound = 1,

        /// <summary>
        /// Outbound processor type
        /// </summary>
        Outbound = 2
    }

    /// <summary>
    /// Crud Action Type
    /// </summary>
    public enum CrudActionType
    {
        /// <summary>
        /// Crud Action Type None
        /// </summary>
        None = 0,

        /// <summary>
        /// Crud Action Type Add
        /// </summary>
        Add = 1,

        /// <summary>
        /// Crud Action Type Update
        /// </summary>
        Update = 2,

        /// <summary>
        /// Crud Action Type Delete
        /// </summary>
        Delete = 3
    }
}
