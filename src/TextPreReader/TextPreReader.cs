using System;
using System.IO;
using System.Linq;

namespace gjp.io
{
    /// <summary>
    /// Reads data from supplied source and omits lines being consumed according to the rules in the <see cref="Configuration"/>
    /// <para>Based on the article by Mike Stall @ (https://blogs.msdn.microsoft.com/jmstall/2005/08/06/deriving-from-textreader/)</para>
    /// </summary>
    /// <seealso cref="System.IO.TextReader" />
    public class TextPreReader : TextReader
    {

        /// <summary>
        /// Gets or sets the configuration.
        /// </summary>
        /// <value>
        /// The configuration.
        /// </value>
        public Configuration Config { get; set; }

        /// <summary>
        /// The reader
        /// </summary>
        private StreamReader Reader;

        bool disposed = false;


        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="TextPreReader"/> class.
        /// </summary>
        /// <param name="path">The path.</param>
        public TextPreReader(string path) : this(path, new Configuration())
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="TextPreReader"/> class.
        /// </summary>
        /// <param name="reader">The reader.</param>
        public TextPreReader(StreamReader reader) : this(reader, new Configuration())
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="TextPreReader"/> class.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="config">The configuration.</param>
        /// <exception cref="ArgumentNullException">path</exception>
        public TextPreReader(string path, Configuration config)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentNullException(nameof(path));
            }

            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            var reader = File.OpenText(path);

            this.Reader = reader;
            this.Config = config;
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="TextPreReader"/> class.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="config">The configuration.</param>
        /// <exception cref="ArgumentNullException">
        /// reader
        /// or
        /// config
        /// </exception>
        public TextPreReader(StreamReader reader, Configuration config)
        {
            if (reader == null)
            {
                throw new ArgumentNullException(nameof(reader));
            }

            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }


            this.Reader = reader;
            this.Config = config;
        }
        #endregion


        #region Public Methods

        /// <summary>
        /// Reads a line of characters from the text reader and returns the data as a string.
        /// </summary>
        /// <returns>
        /// The next line from the reader, or null if all characters have been read.
        /// </returns>
        public override string ReadLine()
        {
            return GetLine();
        }


        // The default TextReader.Peek() implementation just returns -1. How lame!
        // We can build a real implementation on top of Read().
        /// <summary>
        /// Reads the next character without changing the state of the reader or the character source. Returns the next available character without actually reading it from the reader.
        /// </summary>
        /// <returns>
        /// An integer representing the next character to be read, or -1 if no more characters are available or the reader does not support seeking.
        /// </returns>
        public override int Peek()
        {
            FillCharCache();
            return m_charCache;
        }


        // Reads one character. TextReader() demands this be implemented.
        /// <summary>
        /// Reads the next character from the text reader and advances the character position by one character.
        /// </summary>
        /// <returns>
        /// The next character from the text reader, or -1 if no more characters are available. The default implementation returns -1.
        /// </returns>
        public override int Read()
        {
            FillCharCache();
            int ch = m_charCache;
            ClearCharCache();
            return ch;
        }

        #endregion


        #region Character cache support

        /// <summary>
        /// The m character cache
        /// </summary>
        int m_charCache = -2; // -2 means the cache is empty. -1 means eof.
        /// <summary>
        /// Clears the character cache.
        /// </summary>
        void ClearCharCache()
        {
            m_charCache = -2;
        }
        /// <summary>
        /// Fills the character cache.
        /// </summary>
        void FillCharCache()
        {
            if (m_charCache != -2) return; // cache is already full
            m_charCache = GetNextCharWorker();
        }

        #endregion


        #region Worker to get next single character from a ReadLine()-based source

        // Current buffer
        /// <summary>
        /// The m index
        /// </summary>
        int m_idx = int.MaxValue;
        /// <summary>
        /// The m line
        /// </summary>
        string m_line;

        // The whole point of this helper class is that the derived class is going to 
        // implement ReadLine() instead of Read(). So mark that we don't want to use TextReader's 
        // default implementation of ReadLine(). Null return means eof.
        // public abstract override string ReadLine();


        // Gets the next char and advances the cursor.
        /// <summary>
        /// Gets the next character worker.
        /// </summary>
        /// <returns></returns>
        int GetNextCharWorker()
        {
            // Return the current character
            if (m_line == null)
            {

                m_line = GetLine();

                m_idx = 0;
                if (m_line == null)
                {
                    return -1; // eof
                }
                m_line += "\r\n"; // need to readd the newline that ReadLine() stripped
            }
            char c = m_line[m_idx];
            m_idx++;
            if (m_idx >= m_line.Length)
            {
                m_line = null; // tell us next time around to get a new line.
            }
            return c;
        }

        /// <summary>
        /// Gets the line.
        /// </summary>
        /// <returns></returns>
        private string GetLine()
        {

            do
            {
                var line = Reader.ReadLine();

                if (line == null)
                    return null;

                if (Config.TrimLines)
                    line = line.Trim();


                if (!ShouldSkipLine(line))
                {
                    return line;
                }

            } while (true);

        }

        /// <summary>
        /// Shoulds the skip line.
        /// </summary>
        /// <param name="line">The line.</param>
        /// <returns></returns>
        private bool ShouldSkipLine(string line)
        {
            // Null checks
            if ((line == null))
                return false;

            if (Config.SkipNullOrEmptyLines && string.IsNullOrEmpty(line))
                return true;

            if (Config.SkipNullOrWhiteSpaceLines && string.IsNullOrWhiteSpace(line))
                return true;


            // Line Length Checks
            if (Config.MinLineLength != -1 && line.Length < Config.MinLineLength)
                return true;

            if (Config.MaxLineLength != -1 && line.Length > Config.MaxLineLength)
                return true;


            // Substring checks
            if (Config.SkipLinesThatContain?.Count > 0 && Config.SkipLinesThatContain.Any(line.Contains))
            {
                return true;
            }

            if (Config.SkipLinesThatBeginWith?.Count > 0 && Config.SkipLinesThatBeginWith.Any(line.StartsWith))
            {
                return true;
            }

            if (Config.SkipLinesThatEndWith?.Count > 0 && Config.SkipLinesThatEndWith.Any(line.EndsWith))
            {
                return true;
            }



            return false;

        }

        #endregion


        // Protected implementation of Dispose pattern.
        protected override void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                // Free any other managed objects here.
                this.Reader?.Dispose();
            }

            disposed = true;

            // Call base class implementation.
            base.Dispose(disposing);
        }
    }
}
