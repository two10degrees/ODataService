namespace RazorEngine
{
    using System.Text;

    /// <summary>
    /// Provides a base implementation of a template.
    /// </summary>
    public abstract class TemplateBase : ITemplate
    {
        #region Fields
        private readonly StringBuilder builder = new StringBuilder();
        #endregion

        #region Properties
        /// <summary>
        /// Gets the parsed result of the template.
        /// </summary>
        public string Result { get { return builder.ToString(); } }
        #endregion

        #region Methods
        /// <summary>
        /// Clears the template.
        /// </summary>
        public void Clear()
        {
            builder.Clear();
        }

        /// <summary>
        /// Executes the template.
        /// </summary>
        public virtual void Execute() { }

        /// <summary>
        /// Writes the specified object to the template.
        /// </summary>
        /// <param name="object"></param>
        public void Write(object @object)
        {
            if (@object == null)
                return;

            builder.Append(@object);
        }

        /// <summary>
        /// Writes a literal to the template.
        /// </summary>
        /// <param name="literal"></param>
        public void WriteLiteral(string literal)
        {
            if (literal == null)
                return;

            builder.Append(literal);
        }
        #endregion
    }

    /// <summary>
    /// Provides a base implementation of a template.
    /// </summary>
    /// <typeparam name="TModel">The model type.</typeparam>
    public abstract class TemplateBase<TModel> : TemplateBase, ITemplate<TModel>
    {
        #region Properties
        /// <summary>
        /// Gets or sets the model.
        /// </summary>
        public TModel Model { get; set; }
        #endregion
    }
}
