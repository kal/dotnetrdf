﻿using System;
using System.Collections.Generic;
using System.Linq;
using VDS.RDF.Collections;
using VDS.RDF.Query.Engine.Medusa;
using VDS.RDF.Query.Expressions;

namespace VDS.RDF.Query.Engine.Join.Workers
{
    public class LeftJoinWorker
        : WrapperJoinWorker
    {
        public LeftJoinWorker(IJoinWorker worker, IEnumerable<IExpression> expressions)
            : base(worker)
        {
            if (expressions == null) throw new ArgumentNullException();
            this.Expressions = expressions.ToList().AsReadOnly();
        }

        public IList<IExpression> Expressions { get; private set; }

        public override IEnumerable<ISet> Find(ISet lhs)
        {
            IEnumerable<ISet> rhs = base.Find(lhs);
            // TODO Need to get the execution context from somewhere
            if (this.Expressions.Count > 0) rhs = new FilterEnumerable(rhs, this.Expressions, null);
            return rhs.AddIfEmpty(new Set());
        }
    }
}
