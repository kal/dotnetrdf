﻿using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Nodes;
using VDS.RDF.Query.Aggregation;
using VDS.RDF.Query.Engine;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Query.Expressions.Aggregates
{
    public abstract class BaseAggregate 
        : BaseNAryExpression, IAggregateExpression
    {
        protected BaseAggregate()
            : base(Enumerable.Empty<IExpression>()) {}

        protected BaseAggregate(IEnumerable<IExpression> args) 
            : base(args) {}

        public override IValuedNode Evaluate(ISolution set, IExpressionContext context)
        {
            throw new RdfQueryException("Aggregates cannot be invoked on a single solution");
        }

        public override bool CanParallelise
        {
            get { return false; }
        }

        public override bool IsDeterministic
        {
            get { return true; }
        }

        public override bool IsConstant
        {
            get { return false; }
        }

        public override void Accept(IExpressionVisitor visitor)
        {
            visitor.Visit((IAggregateExpression)this);
        }

        public abstract IAccumulator CreateAccumulator();

        public override string ToString(IAlgebraFormatter formatter)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(this.Functor.ToLowerInvariant());
            builder.Append('(');
            for (int i = 0; i < this.Arguments.Count; i++)
            {
                if (i > 0) builder.Append(", ");
                builder.Append(this.Arguments[i].ToString(formatter));
            }
            builder.Append(")");
            return builder.ToString();
        }

        public override string ToPrefixString(IAlgebraFormatter formatter)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append('(');
            builder.Append(this.Functor.ToLowerInvariant());
            foreach (IExpression expr in this.Arguments)
            {
                builder.Append(' ');
                builder.Append(expr.ToPrefixString(formatter));
            }
            builder.Append(')');
            return builder.ToString();
        }

        public override bool Equals(IExpression other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (other == null) return false;
            if (!(other is INAryExpression)) return false;
            if (!this.Functor.Equals(other.Functor)) return false;
            if (other is BaseDistinctAggregate) return false;

            INAryExpression expr = (INAryExpression)other;
            if (this.Arguments.Count != expr.Arguments.Count) return false;
            for (int i = 0; i < this.Arguments.Count; i++)
            {
                if (!this.Arguments[i].Equals(expr.Arguments[i])) return false;
            }
            return true;
        }
    }
}