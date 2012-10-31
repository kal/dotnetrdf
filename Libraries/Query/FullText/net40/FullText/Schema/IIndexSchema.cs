/*

Copyright dotNetRDF Project 2009-12
dotnetrdf-develop@lists.sf.net

------------------------------------------------------------------------

This file is part of dotNetRDF.

dotNetRDF is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

dotNetRDF is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with dotNetRDF.  If not, see <http://www.gnu.org/licenses/>.

------------------------------------------------------------------------

dotNetRDF may alternatively be used under the LGPL or MIT License

http://www.gnu.org/licenses/lgpl.html
http://www.opensource.org/licenses/mit-license.php

If these licenses are not suitable for your intended use please contact
us at the above stated email address to discuss alternative
terms.

*/

using System;

namespace VDS.RDF.Query.FullText.Schema
{
    /// <summary>
    /// Interface for Index Schemas
    /// </summary>
    /// <remarks>
    /// Index Schemas are used to provide the set of field names that is used to encode indexed data onto a document in the index
    /// </remarks>
    public interface IFullTextIndexSchema
    {
        /// <summary>
        /// Gets the field in which the full text is indexed
        /// </summary>
        String IndexField
        {
            get;
        }

        /// <summary>
        /// Gets the field in which the Graph URI is indexed
        /// </summary>
        String GraphField
        {
            get;
        }

        /// <summary>
        /// Gets the field in which the hash is stored
        /// </summary>
        /// <remarks>
        /// Used for unindexing
        /// </remarks>
        String HashField
        {
            get;
        }

        /// <summary>
        /// Gets the field in which the Node type is stored
        /// </summary>
        String NodeTypeField
        {
            get;
        }

        /// <summary>
        /// Gets the field in which the Node value is stored
        /// </summary>
        String NodeValueField
        {
            get;
        }

        /// <summary>
        /// Gets the field in which the Node meta is stored
        /// </summary>
        String NodeMetaField
        {
            get;
        }
    }
}
