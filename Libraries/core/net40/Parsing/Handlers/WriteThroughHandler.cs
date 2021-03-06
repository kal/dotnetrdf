/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2012 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is furnished
to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using VDS.RDF.Query;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Parsing.Handlers
{
    /// <summary>
    /// A RDF Handler which writes the handled Triples out to a <see cref="TextWriter">TextWriter</see> using a provided <see cref="ITripleFormatter">ITripleFormatter</see>
    /// </summary>
    public class WriteThroughHandler
        : BaseRdfHandler
    {
        private Type _formatterType;
        private ITripleFormatter _formatter;
        private TextWriter _writer;
        private bool _closeOnEnd = true;
        private INamespaceMapper _formattingMapper = new QNameOutputMapper();
        private int _written = 0;

        private const int FlushInterval = 50000;

        /// <summary>
        /// Creates a new Write-Through Handler
        /// </summary>
        /// <param name="formatter">Triple Formatter to use</param>
        /// <param name="writer">Text Writer to write to</param>
        /// <param name="closeOnEnd">Whether to close the writer at the end of RDF handling</param>
        public WriteThroughHandler(ITripleFormatter formatter, TextWriter writer, bool closeOnEnd)
        {
            if (writer == null) throw new ArgumentNullException("writer", "Cannot use a null TextWriter with the Write Through Handler");
            if (formatter != null)
            {
                this._formatter = formatter;
            }
            else
            {
                this._formatter = new NTriplesFormatter();
            }
            this._writer = writer;
            this._closeOnEnd = closeOnEnd;
        }

        /// <summary>
        /// Creates a new Write-Through Handler
        /// </summary>
        /// <param name="formatter">Triple Formatter to use</param>
        /// <param name="writer">Text Writer to write to</param>
        public WriteThroughHandler(ITripleFormatter formatter, TextWriter writer)
            : this(formatter, writer, true) { }

        /// <summary>
        /// Creates a new Write-Through Handler
        /// </summary>
        /// <param name="formatterType">Type of the formatter to create</param>
        /// <param name="writer">Text Writer to write to</param>
        /// <param name="closeOnEnd">Whether to close the writer at the end of RDF handling</param>
        public WriteThroughHandler(Type formatterType, TextWriter writer, bool closeOnEnd)
        {
            if (writer == null) throw new ArgumentNullException("writer", "Cannot use a null TextWriter with the Write Through Handler");
            if (formatterType == null) throw new ArgumentNullException("formatterType", "Cannot use a null formatter type");
            this._formatterType = formatterType;
            this._writer = writer;
            this._closeOnEnd = closeOnEnd;
        }

        /// <summary>
        /// Creates a new Write-Through Handler
        /// </summary>
        /// <param name="formatterType">Type of the formatter to create</param>
        /// <param name="writer">Text Writer to write to</param>
        public WriteThroughHandler(Type formatterType, TextWriter writer)
            : this(formatterType, writer, true) { }

        /// <summary>
        /// Starts RDF Handling instantiating a Triple Formatter if necessary
        /// </summary>
        protected override void StartRdfInternal()
        {
            if (this._closeOnEnd && this._writer == null) throw new RdfParseException("Cannot use this WriteThroughHandler as an RDF Handler for parsing as you set closeOnEnd to true and you have already used this Handler and so the provided TextWriter was closed");

            if (this._formatterType != null)
            {
                this._formatter = null;
                this._formattingMapper = new QNameOutputMapper();

                //Instantiate a new Formatter
                ConstructorInfo[] cs = this._formatterType.GetConstructors();
                Type qnameMapperType = typeof(QNameOutputMapper);
                Type nsMapperType = typeof(INamespaceMapper);
                foreach (ConstructorInfo c in cs.OrderByDescending(c => c.GetParameters().Count()))
                {
                    ParameterInfo[] ps = c.GetParameters();
                    try
                    {
                        if (ps.Length == 1)
                        {
                            if (ps[0].ParameterType.Equals(qnameMapperType))
                            {
                                this._formatter = Activator.CreateInstance(this._formatterType, new Object[] { this._formattingMapper }) as ITripleFormatter;
                            }
                            else if (ps[0].ParameterType.Equals(nsMapperType))
                            {
                                this._formatter = Activator.CreateInstance(this._formatterType, new Object[] { this._formattingMapper }) as ITripleFormatter;
                            }
                        }
                        else if (ps.Length == 0)
                        {
                            this._formatter = Activator.CreateInstance(this._formatterType) as ITripleFormatter;
                        }

                        if (this._formatter != null) break;
                    }
                    catch
                    {
                        //Suppress errors since we'll throw later if necessary
                    }
                }

                //If we get out here and the formatter is null then we throw an error
                if (this._formatter == null) throw new RdfParseException("Unable to instantiate a ITripleFormatter from the given Formatter Type " + this._formatterType.FullName);
            }

            if (this._formatter is IGraphFormatter)
            {
                this._writer.WriteLine(((IGraphFormatter)this._formatter).FormatGraphHeader(this._formattingMapper));
            }
            this._written = 0;
        }

        /// <summary>
        /// Ends RDF Handling closing the <see cref="TextWriter">TextWriter</see> being used if the setting is enabled
        /// </summary>
        /// <param name="ok">Indicates whether parsing completed without error</param>
        protected override void EndRdfInternal(bool ok)
        {
            if (this._formatter is IGraphFormatter)
            {
                this._writer.WriteLine(((IGraphFormatter)this._formatter).FormatGraphFooter());
            }
            if (this._closeOnEnd)
            {
                this._writer.Close();
                this._writer = null;
            }
        }

        /// <summary>
        /// Handles Namespace Declarations passing them to the underlying formatter if applicable
        /// </summary>
        /// <param name="prefix">Namespace Prefix</param>
        /// <param name="namespaceUri">Namespace URI</param>
        /// <returns></returns>
        protected override bool HandleNamespaceInternal(string prefix, Uri namespaceUri)
        {
            if (this._formattingMapper != null)
            {
                this._formattingMapper.AddNamespace(prefix, namespaceUri);
            }

            if (this._formatter is INamespaceFormatter)
            {
                this._writer.WriteLine(((INamespaceFormatter)this._formatter).FormatNamespace(prefix, namespaceUri));
            }

            return true;
        }

        /// <summary>
        /// Handles Base URI Declarations passing them to the underlying formatter if applicable
        /// </summary>
        /// <param name="baseUri">Base URI</param>
        /// <returns></returns>
        protected override bool HandleBaseUriInternal(Uri baseUri)
        {
            if (this._formatter is IBaseUriFormatter)
            {
                this._writer.WriteLine(((IBaseUriFormatter)this._formatter).FormatBaseUri(baseUri));
            }

            return true;
        }

        /// <summary>
        /// Handles Triples by writing them using the underlying formatter
        /// </summary>
        /// <param name="t">Triple</param>
        /// <returns></returns>
        protected override bool HandleTripleInternal(Triple t)
        {
            this._written++;
            this._writer.WriteLine(this._formatter.Format(t));
            if (this._written >= FlushInterval)
            {
                this._written = 0;
                this._writer.Flush();
            }
            return true;
        }

        /// <summary>
        /// Gets that the Handler accepts all Triples
        /// </summary>
        public override bool AcceptsAll
        {
            get 
            {
                return true;
            }
        }
    }

    /// <summary>
    /// A Results Handler which writes the handled Results out to a <see cref="TextWriter">TextWriter</see> using a provided <see cref="IResultFormatter">IResultFormatter</see>
    /// </summary>
    public class ResultWriteThroughHandler 
        : BaseResultsHandler
    {
        private Type _formatterType;
        private IResultFormatter _formatter;
        private TextWriter _writer;
        private bool _closeOnEnd = true;
        private INamespaceMapper _formattingMapper = new QNameOutputMapper();
        private SparqlResultsType _currentType = SparqlResultsType.Boolean;
        private List<String> _currVariables = new List<String>();
        private bool _headerWritten = false;

        /// <summary>
        /// Creates a new Write-Through Handler
        /// </summary>
        /// <param name="formatter">Triple Formatter to use</param>
        /// <param name="writer">Text Writer to write to</param>
        /// <param name="closeOnEnd">Whether to close the writer at the end of RDF handling</param>
        public ResultWriteThroughHandler(IResultFormatter formatter, TextWriter writer, bool closeOnEnd)
        {
            if (writer == null) throw new ArgumentNullException("writer", "Cannot use a null TextWriter with the Result Write Through Handler");
            if (formatter != null)
            {
                this._formatter = formatter;
            }
            else
            {
                this._formatter = new NTriplesFormatter();
            }
            this._writer = writer;
            this._closeOnEnd = closeOnEnd;
        }

        /// <summary>
        /// Creates a new Write-Through Handler
        /// </summary>
        /// <param name="formatter">Triple Formatter to use</param>
        /// <param name="writer">Text Writer to write to</param>
        public ResultWriteThroughHandler(IResultFormatter formatter, TextWriter writer)
            : this(formatter, writer, true) { }

        /// <summary>
        /// Creates a new Write-Through Handler
        /// </summary>
        /// <param name="formatterType">Type of the formatter to create</param>
        /// <param name="writer">Text Writer to write to</param>
        /// <param name="closeOnEnd">Whether to close the writer at the end of RDF handling</param>
        public ResultWriteThroughHandler(Type formatterType, TextWriter writer, bool closeOnEnd)
        {
            if (writer == null) throw new ArgumentNullException("writer", "Cannot use a null TextWriter with the Result Write Through Handler");
            if (formatterType == null) throw new ArgumentNullException("formatterType", "Cannot use a null formatter type");
            this._formatterType = formatterType;
            this._writer = writer;
            this._closeOnEnd = closeOnEnd;
        }

        /// <summary>
        /// Creates a new Write-Through Handler
        /// </summary>
        /// <param name="formatterType">Type of the formatter to create</param>
        /// <param name="writer">Text Writer to write to</param>
        public ResultWriteThroughHandler(Type formatterType, TextWriter writer)
            : this(formatterType, writer, true) { }

        /// <summary>
        /// Starts writing results
        /// </summary>
        protected override void StartResultsInternal()
        {
            if (this._closeOnEnd && this._writer == null) throw new RdfParseException("Cannot use this ResultWriteThroughHandler as an Results Handler for parsing as you set closeOnEnd to true and you have already used this Handler and so the provided TextWriter was closed");
            this._currentType = SparqlResultsType.Unknown;
            this._currVariables.Clear();
            this._headerWritten = false;

            if (this._formatterType != null)
            {
                this._formatter = null;
                this._formattingMapper = new QNameOutputMapper();

                //Instantiate a new Formatter
                ConstructorInfo[] cs = this._formatterType.GetConstructors();
                Type qnameMapperType = typeof(QNameOutputMapper);
                Type nsMapperType = typeof(INamespaceMapper);
                foreach (ConstructorInfo c in cs.OrderByDescending(c => c.GetParameters().Count()))
                {
                    ParameterInfo[] ps = c.GetParameters();
                    try
                    {
                        if (ps.Length == 1)
                        {
                            if (ps[0].ParameterType.Equals(qnameMapperType))
                            {
                                this._formatter = Activator.CreateInstance(this._formatterType, new Object[] { this._formattingMapper }) as IResultFormatter;
                            }
                            else if (ps[0].ParameterType.Equals(nsMapperType))
                            {
                                this._formatter = Activator.CreateInstance(this._formatterType, new Object[] { this._formattingMapper }) as IResultFormatter;
                            }
                        }
                        else if (ps.Length == 0)
                        {
                            this._formatter = Activator.CreateInstance(this._formatterType) as IResultFormatter;
                        }

                        if (this._formatter != null) break;
                    }
                    catch
                    {
                        //Suppress errors since we'll throw later if necessary
                    }
                }

                //If we get out here and the formatter is null then we throw an error
                if (this._formatter == null) throw new RdfParseException("Unable to instantiate a IResultFormatter from the given Formatter Type " + this._formatterType.FullName);
            }
        }

        /// <summary>
        /// Ends the writing of results closing the <see cref="TextWriter">TextWriter</see> depending on the option set when this instance was instantiated
        /// </summary>
        /// <param name="ok"></param>
        protected override void EndResultsInternal(bool ok)
        {
            if (this._formatter is IResultSetFormatter)
            {
                this._writer.WriteLine(((IResultSetFormatter)this._formatter).FormatResultSetFooter());
            }
            if (this._closeOnEnd)
            {
                this._writer.Close();
                this._writer = null;
            }
            this._currentType = SparqlResultsType.Unknown;
            this._currVariables.Clear();
            this._headerWritten = false;
        }

        /// <summary>
        /// Writes a Boolean Result to the output
        /// </summary>
        /// <param name="result">Boolean Result</param>
        protected override void HandleBooleanResultInternal(bool result)
        {
            if (this._currentType != SparqlResultsType.Unknown) throw new RdfParseException("Cannot handle a Boolean Result when the handler has already handled other types of results");
            this._currentType = SparqlResultsType.Boolean;
            if (!this._headerWritten && this._formatter is IResultSetFormatter)
            {
                this._writer.WriteLine(((IResultSetFormatter)this._formatter).FormatResultSetHeader());
                this._headerWritten = true;
            }

            this._writer.WriteLine(this._formatter.FormatBooleanResult(result));
        }

        /// <summary>
        /// Writes a Variable declaration to the output
        /// </summary>
        /// <param name="var">Variable Name</param>
        /// <returns></returns>
        protected override bool HandleVariableInternal(string var)
        {
            if (this._currentType == SparqlResultsType.Boolean) throw new RdfParseException("Cannot handler a Variable when the handler has already handled a boolean result");
            this._currentType = SparqlResultsType.VariableBindings;
            this._currVariables.Add(var);
            return true;
        }

        /// <summary>
        /// Writes a Result to the output
        /// </summary>
        /// <param name="result">SPARQL Result</param>
        /// <returns></returns>
        protected override bool HandleResultInternal(SparqlResult result)
        {
            if (this._currentType == SparqlResultsType.Boolean) throw new RdfParseException("Cannot handle a Result when the handler has already handled a boolean result");
            this._currentType = SparqlResultsType.VariableBindings;
            if (!this._headerWritten && this._formatter is IResultSetFormatter)
            {
                this._writer.WriteLine(((IResultSetFormatter)this._formatter).FormatResultSetHeader(this._currVariables.Distinct()));
                this._headerWritten = true;
            }
            this._writer.WriteLine(this._formatter.Format(result));
            return true;
        }
    }
}
