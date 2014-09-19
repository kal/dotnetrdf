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

using VDS.RDF.Specifications;

namespace VDS.RDF.Query.Expressions.Functions.Sparql.Numeric
{
    /// <summary>
    /// Represents the SPARQL FLOOR() Function
    /// </summary>
    public class FloorFunction
        : XPath.Numeric.FloorFunction
    {
        /// <summary>
        /// Creates a new SPARQL FLOOR() Function
        /// </summary>
        /// <param name="expr">Argument Expression</param>
        public FloorFunction(IExpression expr)
            : base(expr) { }

        /// <summary>
        /// Gets the Functor of this Expression
        /// </summary>
        public override string Functor
        {
            get
            {
                return SparqlSpecsHelper.SparqlKeywordFloor;
            }
        }

        public override bool Equals(IExpression other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (other == null) return false;
            if (!(other is FloorFunction)) return false;

            FloorFunction func = (FloorFunction)other;
            return this.Argument.Equals(func.Argument);
        }
    }
}
