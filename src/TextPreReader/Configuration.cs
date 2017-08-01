using System.Collections.ObjectModel;

namespace gjp.io
{
    /// <summary>
    /// Configuration used for determining how lines are read/processed
    /// </summary>
    public class Configuration
    {

        /// <summary>
        /// Gets or sets a value indicating whether to trim whitespace from lines.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [trim lines]; otherwise, <c>false</c>.
        /// </value>
        public bool TrimLines { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether to skip null or empty lines.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [skip null or empty lines]; otherwise, <c>false</c>.
        /// </value>
        public bool SkipNullOrEmptyLines { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether skip null, empty or white space lines.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [skip null or white space lines]; otherwise, <c>false</c>.
        /// </value>
        public bool SkipNullOrWhiteSpaceLines { get; set; } = false;


        /// <summary>
        /// Gets or sets the minimum acceptable length of the line.
        /// <para>Default (not set) value is -1</para>
        /// </summary>
        /// <value>
        /// The minimum length of the line.
        /// </value>
        public int MinLineLength { get; set; } = -1;

        /// <summary>
        /// Gets or sets the maximum acceptable length of the line.
        /// <para>Default (not set) value is -1</para>
        /// </summary>
        /// <value>
        /// The maximum length of the line.
        /// </value>
        public int MaxLineLength { get; set; } = -1;


        /// <summary>
        /// Gets a collection of strings that would result in a containing line being skipped.
        /// </summary>
        /// <value>
        /// The skip lines that contain.
        /// </value>
        public Collection<string> SkipLinesThatContain { get; }

        /// <summary>
        /// Gets a collection of strings that would result in a line beginning with being skipped.
        /// </summary>
        /// <value>
        /// The skip lines that begin with.
        /// </value>
        public Collection<string> SkipLinesThatBeginWith { get; }

        /// <summary>
        /// Gets a collection of strings that would result in a line ending with being skipped.
        /// </summary>
        /// <value>
        /// The skip lines that end with.
        /// </value>
        public Collection<string> SkipLinesThatEndWith { get; }


        /// <summary>
        /// Initializes a new instance of the <see cref="Configuration"/> class.
        /// </summary>
        public Configuration()
        {
            SkipLinesThatContain = new Collection<string>();
            SkipLinesThatBeginWith = new Collection<string>();
            SkipLinesThatEndWith = new Collection<string>();
        }



    }
}