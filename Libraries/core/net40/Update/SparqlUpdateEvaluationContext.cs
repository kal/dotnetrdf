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
using System.Diagnostics;
using VDS.RDF.Query;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Datasets;

namespace VDS.RDF.Update
{
    /// <summary>
    /// Evaluation Context for SPARQL Updates evaluated by the libraries Leviathan SPARQL Engine
    /// </summary>
    public class SparqlUpdateEvaluationContext
    {
        private ISparqlDataset _data;
        private SparqlUpdateCommandSet _commands;
#if !NO_STOPWATCH
        private Stopwatch _timer = new Stopwatch();
#else
        private DateTime _start, _end;
#endif
        private long _timeout;

        /// <summary>
        /// Creates a new SPARQL Update Evaluation Context
        /// </summary>
        /// <param name="commands">Command Set</param>
        /// <param name="data">SPARQL Dataset</param>
        /// <param name="processor">Query Processor for WHERE clauses</param>
        public SparqlUpdateEvaluationContext(SparqlUpdateCommandSet commands, ISparqlDataset data, ISparqlQueryAlgebraProcessor<BaseMultiset, SparqlEvaluationContext> processor)
            : this(commands, data)
        {
            this.QueryProcessor = processor;
        }

        /// <summary>
        /// Creates a new SPARQL Update Evaluation Context
        /// </summary>
        /// <param name="commands">Command Set</param>
        /// <param name="data">SPARQL Dataset</param>
        public SparqlUpdateEvaluationContext(SparqlUpdateCommandSet commands, ISparqlDataset data)
            : this(data)
        {
            this._commands = commands;
        }

        /// <summary>
        /// Creates a new SPARQL Update Evaluation Context
        /// </summary>
        /// <param name="data">SPARQL Dataset</param>
        /// <param name="processor">Query Processor for WHERE clauses</param>
        public SparqlUpdateEvaluationContext(ISparqlDataset data, ISparqlQueryAlgebraProcessor<BaseMultiset, SparqlEvaluationContext> processor)
            : this(data)
        {
            this.QueryProcessor = processor;
        }

        /// <summary>
        /// Creates a new SPARQL Update Evaluation Context
        /// </summary>
        /// <param name="data">SPARQL Dataset</param>
        public SparqlUpdateEvaluationContext(ISparqlDataset data)
        {
            this._data = data;
        }

        /// <summary>
        /// Gets the Command Set (if any) that this context pertains to
        /// </summary>
        public SparqlUpdateCommandSet Commands
        {
            get
            {
                return this._commands;
            }
        }

        /// <summary>
        /// Dataset upon which the Updates are applied
        /// </summary>
        public ISparqlDataset Data
        {
            get
            {
                return this._data;
            }
        }

        /// <summary>
        /// Gets the Query Processor used to process the WHERE clauses of DELETE or INSERT commands
        /// </summary>
        public ISparqlQueryAlgebraProcessor<BaseMultiset, SparqlEvaluationContext> QueryProcessor
        {
            get;
            private set;
        }

        /// <summary>
        /// Retrieves the Time in milliseconds the update took to evaluate
        /// </summary>
        public long UpdateTime
        {
            get
            {
#if !NO_STOPWATCH
                return this._timer.ElapsedMilliseconds;
#else
                if (this._end == null || this._end < this._start) this._end = DateTime.Now;
                return (this._end - this._start).Milliseconds;
#endif
            }
        }

        /// <summary>
        /// Retrieves the Time in ticks the updates took to evaluate
        /// </summary>
        public long UpdateTimeTicks
        {
            get
            {
#if !NO_STOPWATCH
                return this._timer.ElapsedTicks;
#else
                if (this._end == null || this._end < this._start) this._end = DateTime.Now;
                return (this._end - this._start).Ticks;
#endif
            }
        }

        private void CalculateTimeout()
        {
            if (this._commands != null)
            {
                if (this._commands.Timeout > 0)
                {
                    if (Options.UpdateExecutionTimeout == 0 || (this._commands.Timeout <= Options.UpdateExecutionTimeout && Options.UpdateExecutionTimeout > 0))
                    {
                        //Update Timeout is used provided it is less than global timeout unless global timeout is zero
                        this._timeout = this._commands.Timeout;
                    }
                    else
                    {
                        //Update Timeout cannot be set higher than global timeout
                        this._timeout = Options.UpdateExecutionTimeout;
                    }
                }
                else
                {
                    //If Update Timeout set to zero (i.e. no timeout) then global timeout is used
                    this._timeout = Options.UpdateExecutionTimeout;
                }
            }
            else
            {
                //If no updates then global timeout is used
                this._timeout = Options.UpdateExecutionTimeout;
            }
        }

        /// <summary>
        /// Gets the Remaining Timeout i.e. the Timeout taking into account time already elapsed
        /// </summary>
        /// <remarks>
        /// If there is no timeout then this is always zero, if there is a timeout this is always >= 1 since any operation that wants to respect the timeout must have a non-zero timeout to actually timeout properly.
        /// </remarks>
        public long RemainingTimeout
        {
            get
            {
                if (this._timeout <= 0)
                {
                    return 0;
                }
                else
                {
                    long timeout = this._timeout - this.UpdateTime;
                    if (timeout <= 0)
                    {
                        return 1;
                    }
                    else
                    {
                        return timeout;
                    }
                }
            }
        }

        /// <summary>
        /// Gets the Update Timeout used for the Command Set
        /// </summary>
        /// <remarks>
        /// <para>
        /// This is taken either from the <see cref="SparqlUpdateCommandSet.Timeout">Timeout</see> property of the <see cref="SparqlUpdateCommandSet">SparqlUpdateCommandSet</see> to which this evaluation context pertains or from the global option <see cref="Options.UpdateExecutionTimeout">Options.UpdateExecutionTimeout</see>.  To set the Timeout to be used set whichever of those is appropriate prior to evaluating the updates.  If there is a Command Set present then it's timeout takes precedence unless it is set to zero (no timeout) in which case the global timeout setting is applied.  You cannot set the Update Timeout to be higher than the global timeout unless the global timeout is set to zero (i.e. no global timeout)
        /// </para>
        /// </remarks>
        public long UpdateTimeout
        {
            get
            {
                return this._timeout;
            }
        }

        /// <summary>
        /// Checks whether Execution should Time out
        /// </summary>
        /// <exception cref="SparqlUpdateTimeoutException">Thrown if the Update has exceeded the Execution Timeout</exception>
        public void CheckTimeout()
        {
            if (this._timeout > 0)
            {
#if !NO_STOPWATCH
                if (this._timer.ElapsedMilliseconds > this._timeout)
                {
                    this._timer.Stop();
                    throw new SparqlUpdateTimeoutException("Update Execution Time exceeded the Timeout of " + this._timeout + "ms, updates aborted after " + this._timer.ElapsedMilliseconds + "ms");
                }
#else
                TimeSpan elapsed = DateTime.Now - this._start;
                if (elapsed.Milliseconds > this._timeout)
                {
                    this._end = DateTime.Now;
                    throw new SparqlUpdateTimeoutException("Update Execution Time exceeded the Timeout of " + this._timeout + "ms, updates aborted after " + elapsed.Milliseconds + "ms");
                }
#endif
            }
        }

        /// <summary>
        /// Starts the Execution Timer
        /// </summary>
        public void StartExecution()
        {
            this.CalculateTimeout();
#if !NO_STOPWATCH
            this._timer.Start();
#else
            this._start = DateTime.Now;
#endif
        }

        /// <summary>
        /// Ends the Execution Timer
        /// </summary>
        public void EndExecution()
        {
#if !NO_STOPWATCH
            this._timer.Stop();
#else
            this._end = DateTime.Now;
#endif
        }
    }
}
