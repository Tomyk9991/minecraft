using System;
using ArithmeticParser.Exceptions;

namespace ArithmeticParser
{
    public class Compiler
    {
        // Grammatik:m
        // expression = term | expression `+` term | expression `-` term
        // term = factor | term `*` factor | term `/` factor
        // factor = `+` factor | `-` factor | `(` expression `)` | number | functionName factor | factor `^` factor

        private string sourceCode = "";
        private int pos = -1;
        private int ch;

        public Compiler(string sourceCode)
        {
            InitCompiler(sourceCode);
        }

        
        public IExpression SyntaxTree
        {
            get;
            private set;
        }

        private void InitCompiler(string sourceCode)
        {
            this.sourceCode = sourceCode;
            pos = -1;
            ch = 0;
        }

        public double Evaluate()
        {
            if (!string.IsNullOrEmpty(this.sourceCode))
            {
                return Parse().Evaluate();
            }

            throw new Exception("No string for evaluation present");
        }
        
        public double Evaluate(string sourceCode)
        {
            InitCompiler(sourceCode);
            return Parse().Evaluate();
        }

        void nextChar()
        {
            ch = ++pos < this.sourceCode.Length ? this.sourceCode[pos] : -1;
        }

        private bool Eat(int charToEat)
        {
            while (ch == ' ') nextChar();
            if (ch == charToEat)
            {
                nextChar();
                return true;
            }

            return false;
        }

        private IExpression Parse()
        {
            this.sourceCode = this.sourceCode.Replace('.', ',');
            
            nextChar();
            this.SyntaxTree = ParseExpression();
            if (pos < this.sourceCode.Length) throw new SyntaxException("Unexpected: " + "\"" + (char) ch + "\"");
            return this.SyntaxTree;
        }

        private IExpression ParseExpression()
        {
            IExpression x = ParseTerm();
            while (true)
            {
                if (Eat('+')) x = x.Add(ParseTerm());
                else if (Eat('-')) x = x.Sub(ParseTerm());
                else return x;
            }
        }

        IExpression ParseTerm()
        {
            IExpression x = ParseFactor();
            while (true)
            {
                if (Eat('*')) x = x.Mul(ParseTerm());
                else if (Eat('/')) x = x.Div(ParseTerm());
                else return x;
            }
        }

        private IExpression ParseFactor()
        {
            IExpression x;
            if (Eat('+'))
            {
                x = ParseFactor();
                return x;
            }

            if (Eat('-'))
            {
                x = ParseFactor();
                x.Value *= -1;
                return x;
            }

            int startPos = this.pos;
            if (Eat('('))
            {
                x = ParseExpression();
                if (!Eat(')'))
                {
                    throw new SyntaxException("Expected: \")\"");
                }
            }
            else if (ch >= '0' && ch <= '9' || ch == '.')
            {
                while (ch >= '0' && ch <= '9' || ch == ',' || ch == '.') nextChar();
                x = new Expression(double.Parse(this.sourceCode.Substring(startPos, this.pos - startPos)));
            }
            else if (ch >= 'a' && ch <= 'z')
            {
                // functions
                while (ch >= 'a' && ch <= 'z') nextChar();
                String func = this.sourceCode.Substring(startPos, this.pos - startPos);
                x = ParseFactor();

                switch (func)
                {
                    case "sqrt":
                        x = new Expression("Sqrt", x, Math.Sqrt(x.Evaluate()));
                        break;
                    case "sin":
                        x = new Expression("Sin", x, Math.Sin(x.Evaluate()));
                        break;
                    case "cos":
                        x = new Expression("Cos", x, Math.Cos(x.Evaluate()));
                        break;
                    case "tan":
                        x = new Expression("Tan", x, Math.Tan(x.Evaluate()));
                        break;
                    default:
                        throw new Exception("Unknown function: " + func);
                }
            }
            else
            {
                throw new SyntaxException("Unexpected: " + "\"" + (char) ch + "\"");
            }

            if (Eat('^'))
            {
                x = x.Pow(ParseFactor());
            }

            return x;
        }
    }
}