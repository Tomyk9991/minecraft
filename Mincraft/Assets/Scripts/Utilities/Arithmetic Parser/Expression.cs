using System;
using System.Collections.Generic;
using System.Text;

namespace ArithmeticParser
{
    public class Expression : IExpression
    {
        public Expression(IExpression e)
        {
            Value = e.Evaluate();
        }

        public Expression(double val)
        {
            Value = val;
        }

        public Expression(string func, IExpression lhs, double value)
        {
            this.Func = func;
            this.LHS = lhs;
            this.Value = value;
        }

        private Expression(IExpression lhs, Operation op, IExpression rhs, double value)
        {
            this.LHS = lhs;
            this.OP = op;
            this.RHS = rhs;

            this.Value = value;
        }
        
        public bool Defined =>
            Value == double.NaN &&
            this.OP != Operation.NOOP &&
            this.LHS != null && this.RHS != null;

        public IExpression LHS { get; set; } = null;
        public IExpression RHS { get; set; } = null;
        public Operation OP { get; set; } = Operation.NOOP;
        public double Value { get; set; } = Double.NaN;

        public string Func { get; set; } = "";
        
        
        public double Evaluate() => Value;

        public IExpression Add(IExpression other)
        {
            return new Expression(this, Operation.ADD, other, this.Value + other.Evaluate());
        }

        public IExpression Sub(IExpression other)
        {
            return new Expression(this, Operation.SUB, other, this.Value - other.Evaluate());
        }

        public IExpression Mul(IExpression other)
        {
            return new Expression(this, Operation.MUL, other, this.Value * other.Evaluate());
        }

        public IExpression Div(IExpression other)
        {
            double val2 = other.Evaluate();
            
            if (val2 == 0)
                throw new DivideByZeroException();
            
            return new Expression(this, Operation.DIV, other, this.Value / val2);
        }

        public IExpression Pow(IExpression other)
        {
            return new Expression(this, Operation.POW, other, Math.Pow(this.Value, other.Evaluate()));
        }

        public string TreeView(string indent = "", bool last = true)
        {
            StringBuilder builder = new StringBuilder(indent);
            if (last)
            {
                builder.Append("└─");
                indent += "  ";
            }
            else
            {
                builder.Append("├─");
                indent += "| ";
            }

            string inner = this.ToInnerString();
            builder.Append(inner).Append('\n');
            
            var children = new List<IExpression>();
            
            if (this.LHS != null) children.Add(this.LHS);
            if (this.RHS != null) children.Add(this.RHS);
            
            for (int i = 0; i < children.Count; i++)
            {
                builder.Append(children[i].TreeView(indent, i == children.Count - 1));
            }

            return builder.ToString();
        }
        
        public override string ToString()
        {
            string lhs = this.LHS?.ToString() ?? "NULL";
            string rhs = this.RHS?.ToString() ?? "NULL";


            if (this.Value != double.NaN && lhs == "NULL" && rhs == "NULL")
                return this.Value % 1 == 0 ? this.Value.ToString() : this.Value.ToString("F");

            if (this.OP != Operation.NOOP && lhs == "NULL" && rhs == "NULL")
                return this.OP.ToString();

            return string.Join(" ", lhs, this.OP.ToString(), rhs);
        }
        
        private string ToInnerString()
        {
            if (!string.IsNullOrEmpty(this.Func))
            {
                return (this.Value % 1 == 0 ? this.Value.ToString() : this.Value.ToString("F")) + ", " + this.Func.ToUpper();
            }
            
            string lhs = this.LHS?.ToString() ?? "NULL";
            string rhs = this.RHS?.ToString() ?? "NULL";
            


            if (this.Value != double.NaN && this.LHS == null && this.RHS == null)
                return this.Value % 1 == 0 ? this.Value.ToString() : this.Value.ToString("F");

            if (this.OP != Operation.NOOP && this.LHS == null && this.RHS == null)
                return this.OP.ToString();

            return string.Join(", ", this.Value % 1 == 0 ? this.Value.ToString() : this.Value.ToString("F"), this.OP);
        }
    }
}