using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Globalization;

class Solution
{
    static void Main(string[] args)
    {
        string F_str = Console.ReadLine();
        string vsStr = Console.ReadLine();
        string dictStr = Console.ReadLine();

        var tokenizer = new Tokenizer(F_str);
        var tokens = tokenizer.Tokenize();
        var parser = new Parser(tokens);
        Expression exprTree = parser.Parse();

        string[] varsToDifferentiate = vsStr.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        Expression currentExpression = exprTree;
        foreach (string varName in varsToDifferentiate)
            currentExpression = currentExpression.Differentiate(varName);

        var variableValues = new Dictionary<string, double>();
        string[] valuePairs = dictStr.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        for (int i = 0; i < valuePairs.Length; i += 2)
            variableValues[valuePairs[i]] = double.Parse(valuePairs[i + 1], CultureInfo.InvariantCulture);

        double finalResult = currentExpression.Evaluate(variableValues);
        finalResult = Math.Round(finalResult, 2, MidpointRounding.AwayFromZero);
        if (finalResult == -0.0) finalResult = 0.0;
        Console.WriteLine(finalResult.ToString("F2", CultureInfo.InvariantCulture));
    }
}

public abstract class Expression
{
    public abstract double Evaluate(Dictionary<string, double> varValues);
    public abstract Expression Differentiate(string varName);
    public abstract override string ToString();
    public static Expression Const(double val) => new Constant(val);
    public static Expression Var(string name) => new Variable(name);
}

public class Constant : Expression
{
    public double Value { get; }
    public Constant(double value) { Value = value; }
    public override double Evaluate(Dictionary<string, double> varValues) => Value;
    public override Expression Differentiate(string varName) => Const(0);
    public override string ToString() => Value.ToString(CultureInfo.InvariantCulture);
}

public class Variable : Expression
{
    public string Name { get; }
    public Variable(string name) { Name = name; }
    public override double Evaluate(Dictionary<string, double> varValues)
    {
        if (varValues.TryGetValue(Name, out double value)) return value;
        throw new ArgumentException($"Variable '{Name}' not found in dictionary. Vars: {string.Join(",", varValues.Keys)}");
    }
    public override Expression Differentiate(string varName)
    {
        if (Name.ToLowerInvariant() == "e" || Name.ToLowerInvariant() == "pi") return Const(0);
        return Name == varName ? Const(1) : Const(0);
    }
    public override string ToString() => Name;
}

public class UnaryOperation : Expression
{
    public string Op { get; }
    public Expression Operand { get; }
    public UnaryOperation(string op, Expression operand)
    {
        Op = op.ToLowerInvariant();
        Operand = operand;
    }
    public override double Evaluate(Dictionary<string, double> varValues)
    {
        double val = Operand.Evaluate(varValues);
        switch (Op)
        {
            case "ln": return Math.Log(val);
            case "sin": return Math.Sin(val);
            case "cos": return Math.Cos(val);
            case "neg": return -val;
            default: throw new ArgumentException($"Unknown unary operator: {Op}");
        }
    }
    public override Expression Differentiate(string varName)
    {
        Expression du = Operand.Differentiate(varName);
        if (du is Constant c && c.Value == 0) return Const(0);
        switch (Op)
        {
            case "ln":
                return new BinaryOperation("*", du, new BinaryOperation("^", Operand, Const(-1)));
            case "sin":
                return new BinaryOperation("*", du, new UnaryOperation("cos", Operand));
            case "cos":
                return new BinaryOperation("*", Const(-1), new BinaryOperation("*", du, new UnaryOperation("sin", Operand)));
            case "neg":
                return new UnaryOperation("neg", du);
            default: throw new ArgumentException($"Unknown unary operator for differentiation: {Op}");
        }
    }
    public override string ToString() => $"{Op}({Operand})";
}

public class BinaryOperation : Expression
{
    public string Op { get; }
    public Expression Left { get; }
    public Expression Right { get; }
    public BinaryOperation(string op, Expression left, Expression right)
    {
        Op = op;
        Left = left;
        Right = right;
    }
    public override double Evaluate(Dictionary<string, double> varValues)
    {
        double lVal = Left.Evaluate(varValues);
        if (Op == "*" && (lVal == 0.0 || double.IsNaN(lVal))) return 0.0;
        double rVal = Right.Evaluate(varValues);
        if (Op == "*" && (rVal == 0.0 || double.IsNaN(rVal))) return 0.0;
        switch (Op)
        {
            case "+": return lVal + rVal;
            case "*": return lVal * rVal;
            case "^": return Math.Pow(lVal, rVal);
            default: throw new ArgumentException($"Unknown binary operator: {Op}");
        }
    }
    public override Expression Differentiate(string varName)
    {
        Expression du = Left.Differentiate(varName);
        Expression dv = Right.Differentiate(varName);
        bool du_is_zero = du is Constant cdu && cdu.Value == 0;
        bool dv_is_zero = dv is Constant cdv && cdv.Value == 0;
        switch (Op)
        {
            case "+":
                if (du_is_zero && dv_is_zero) return Const(0);
                if (du_is_zero) return dv;
                if (dv_is_zero) return du;
                return new BinaryOperation("+", du, dv);
            case "*":
                Expression term1 = new BinaryOperation("*", du, Right);
                Expression term2 = new BinaryOperation("*", Left, dv);
                if (du_is_zero && dv_is_zero) return Const(0);
                if (du_is_zero) return term2;
                if (dv_is_zero) return term1;
                return new BinaryOperation("+", term1, term2);
            case "^":
                if (du_is_zero && dv_is_zero) return Const(0);
                if (dv_is_zero)
                {
                    if (Right is Constant c_exp && c_exp.Value == 0) return Const(0);
                    if (Right is Constant c_exp1 && c_exp1.Value == 1) return du;
                    Expression C_minus_1 = new BinaryOperation("+", Right, Const(-1));
                    Expression u_pow_C_minus_1 = new BinaryOperation("^", Left, C_minus_1);
                    Expression C_times_u_pow = new BinaryOperation("*", Right, u_pow_C_minus_1);
                    return new BinaryOperation("*", C_times_u_pow, du);
                }
                if (du_is_zero)
                {
                    if (Left is Constant c_base && (c_base.Value == 1 || c_base.Value == 0)) return Const(0);
                    Expression ln_C = new UnaryOperation("ln", Left);
                    Expression C_pow_v = new BinaryOperation("^", Left, Right);
                    Expression term_ln_mul_pow = new BinaryOperation("*", C_pow_v, ln_C);
                    return new BinaryOperation("*", term_ln_mul_pow, dv);
                }
                Expression original_power_term = new BinaryOperation("^", Left, Right);
                Expression term_v_prime_ln_u = new BinaryOperation("*", dv, new UnaryOperation("ln", Left));
                Expression u_inverse = new BinaryOperation("^", Left, Const(-1));
                Expression v_u_prime_u_inv = new BinaryOperation("*", Right, new BinaryOperation("*", du, u_inverse));
                Expression sum_in_parentheses = new BinaryOperation("+", term_v_prime_ln_u, v_u_prime_u_inv);
                return new BinaryOperation("*", original_power_term, sum_in_parentheses);
            default: throw new ArgumentException($"Unknown binary operator for differentiation: {Op}");
        }
    }
    public override string ToString() => $"({Left} {Op} {Right})";
}

public enum TokenType { Number, Identifier, Plus, Minus, Multiply, Power, Ln, Sin, Cos, LParen, RParen, Eof }
public class Token
{
    public TokenType Type { get; }
    public string Value { get; }
    public Token(TokenType type, string value) { Type = type; Value = value; }
    public override string ToString() => $"{Type}: {Value}";
}
public class Tokenizer
{
    private readonly string _text;
    private int _pos;
    public Tokenizer(string text) { _text = text; _pos = 0; }
    public List<Token> Tokenize()
    {
        var tokens = new List<Token>();
        while (_pos < _text.Length)
        {
            char currentChar = _text[_pos];
            if (char.IsWhiteSpace(currentChar)) { _pos++; continue; }
            if (char.IsDigit(currentChar) || (currentChar == '.' && _pos + 1 < _text.Length && char.IsDigit(_text[_pos + 1])))
            {
                var sb = new StringBuilder();
                bool hasDecimal = currentChar == '.';
                sb.Append(currentChar); _pos++;
                while (_pos < _text.Length && (char.IsDigit(_text[_pos]) || (!hasDecimal && _text[_pos] == '.')))
                {
                    if (_text[_pos] == '.') hasDecimal = true;
                    sb.Append(_text[_pos]);
                    _pos++;
                }
                tokens.Add(new Token(TokenType.Number, sb.ToString()));
                continue;
            }
            if (char.IsLetter(currentChar))
            {
                var sb = new StringBuilder();
                while (_pos < _text.Length && (char.IsLetterOrDigit(_text[_pos]) || _text[_pos] == '_'))
                {
                    sb.Append(_text[_pos]);
                    _pos++;
                }
                string id = sb.ToString();
                string idLower = id.ToLowerInvariant();
                switch (idLower)
                {
                    case "ln": tokens.Add(new Token(TokenType.Ln, "ln")); break;
                    case "sin": tokens.Add(new Token(TokenType.Sin, "sin")); break;
                    case "cos": tokens.Add(new Token(TokenType.Cos, "cos")); break;
                    default: tokens.Add(new Token(TokenType.Identifier, id)); break;
                }
                continue;
            }
            switch (currentChar)
            {
                case '+': tokens.Add(new Token(TokenType.Plus, "+")); break;
                case '-': tokens.Add(new Token(TokenType.Minus, "-")); break;
                case '*': tokens.Add(new Token(TokenType.Multiply, "*")); break;
                case '^': tokens.Add(new Token(TokenType.Power, "^")); break;
                case '(': tokens.Add(new Token(TokenType.LParen, "(")); break;
                case ')': tokens.Add(new Token(TokenType.RParen, ")")); break;
                default: throw new ArgumentException($"Unexpected character: {currentChar} at pos {_pos}");
            }
            _pos++;
        }
        tokens.Add(new Token(TokenType.Eof, ""));
        return tokens;
    }
}
public class Parser
{
    private List<Token> _tokens;
    private int _pos;
    private Token CurrentToken => _tokens[_pos];
    private Token PreviousToken => _pos > 0 ? _tokens[_pos - 1] : new Token(TokenType.Eof, "");
    public Parser(List<Token> tokens) { _tokens = tokens; _pos = 0; }
    private void Consume(TokenType expectedType)
    {
        if (CurrentToken.Type == expectedType) _pos++;
        else throw new ArgumentException($"Expected token {expectedType} but got {CurrentToken.Type} ('{CurrentToken.Value}') at pos {_pos}. Previous: {PreviousToken}");
    }
    public Expression Parse() => ParseExpression();
    private Expression ParseExpression()
    {
        Expression expr = ParseTerm();
        while (CurrentToken.Type == TokenType.Plus || CurrentToken.Type == TokenType.Minus)
        {
            Token opToken = CurrentToken;
            _pos++;
            Expression right = ParseTerm();
            if (opToken.Type == TokenType.Minus)
                expr = new BinaryOperation("+", expr, new UnaryOperation("neg", right));
            else
                expr = new BinaryOperation(opToken.Value, expr, right);
        }
        return expr;
    }
    private Expression ParseTerm()
    {
        Expression expr = ParsePower();
        while (CurrentToken.Type == TokenType.Multiply)
        {
            Token opToken = CurrentToken;
            _pos++;
            Expression right = ParsePower();
            expr = new BinaryOperation(opToken.Value, expr, right);
        }
        return expr;
    }
    private Expression ParsePower()
    {
        Expression left = ParseUnary();
        if (CurrentToken.Type == TokenType.Power)
        {
            Token opToken = CurrentToken;
            _pos++;
            Expression right = ParsePower();
            return new BinaryOperation(opToken.Value, left, right);
        }
        return left;
    }
    private Expression ParseUnary()
    {
        if (CurrentToken.Type == TokenType.Minus)
        {
            _pos++;
            return new UnaryOperation("neg", ParsePower());
        }
        if (CurrentToken.Type == TokenType.Ln || CurrentToken.Type == TokenType.Sin || CurrentToken.Type == TokenType.Cos)
        {
            Token funcToken = CurrentToken;
            _pos++;
            Expression arg;
            if (CurrentToken.Type == TokenType.LParen)
            {
                _pos++;
                arg = ParseExpression();
                Consume(TokenType.RParen);
            }
            else
            {
                arg = ParseUnary();
            }
            return new UnaryOperation(funcToken.Value, arg);
        }
        return ParseAtom();
    }
    private Expression ParseAtom()
    {
        Token token = CurrentToken;
        if (token.Type == TokenType.Number)
        {
            _pos++;
            return Expression.Const(double.Parse(token.Value, CultureInfo.InvariantCulture));
        }
        if (token.Type == TokenType.Identifier)
        {
            _pos++;
            if (token.Value.ToLowerInvariant() == "e") return Expression.Const(Math.E);
            if (token.Value.ToLowerInvariant() == "pi") return Expression.Const(Math.PI);
            return Expression.Var(token.Value);
        }
        if (token.Type == TokenType.LParen)
        {
            _pos++;
            Expression expr = ParseExpression();
            Consume(TokenType.RParen);
            return expr;
        }
        throw new ArgumentException($"Unexpected token in ParseAtom: {token}. Previous: {PreviousToken}");
    }
}
