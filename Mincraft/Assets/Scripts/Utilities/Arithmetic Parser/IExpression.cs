namespace ArithmeticParser
{
    public interface IExpression
    {
        IExpression LHS { get; set; }
        IExpression RHS { get; set; }
        Operation OP { get; set; }

        double Value { get; set; }
        
        double Evaluate();
        IExpression Add(IExpression other);
        IExpression Sub(IExpression other);
        IExpression Mul(IExpression other);
        IExpression Div(IExpression other);
        IExpression Pow(IExpression other);

        string TreeView(string indent = "", bool last = true);
    }
}