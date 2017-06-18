using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GS_PatEditor.Editor.Exporters.CodeFormat
{
    public abstract class Expression : IWritable
    {
        public abstract void Write(TextWriter writer, int indent);
    }

    public class CustomCodeExpr : Expression
    {
        private readonly string _Value;

        public CustomCodeExpr(string val)
        {
            _Value = val;
        }

        public override void Write(TextWriter writer, int indent)
        {
            writer.Write(_Value);
        }
    }

    public class ConstStringExpr : Expression
    {
        private readonly string _Value;

        public ConstStringExpr(string val)
        {
            _Value = val;
        }

        public override void Write(TextWriter writer, int indent)
        {
            writer.Write('"');
            writer.Write(_Value.Replace("\\", "\\\\").Replace("\"", "\\\""));
            writer.Write('"');
        }
    }

    public class ConstNumberExpr : Expression
    {
        private readonly string _Value;

        public ConstNumberExpr(int val)
        {
            _Value = val.ToString();
        }

        public ConstNumberExpr(double val)
        {
            _Value = val.ToString();
        }

        public override void Write(TextWriter writer, int indent)
        {
            writer.Write(_Value);
        }
    }

    public class FunctionCallExpr : Expression
    {
        private readonly Expression _Func;
        private readonly IEnumerable<Expression> _Args;

        public FunctionCallExpr(Expression func, IEnumerable<Expression> args)
        {
            _Func = func;
            _Args = args;
        }

        public override void Write(TextWriter writer, int indent)
        {
            _Func.Write(writer, indent);
            writer.Write('(');
            Expression lastArg = null;
            foreach (var arg in _Args)
            {
                if (lastArg != null)
                {
                    lastArg.Write(writer, indent);
                    writer.Write(", ");
                }
                lastArg = arg;
            }
            if (lastArg != null)
            {
                lastArg.Write(writer, indent);
            }
            writer.Write(")");
        }
    }

    public class IndexExpr : Expression
    {
        private readonly Expression _Table;
        private readonly string _Index;
        private readonly Expression _IndexExpr;

        public IndexExpr(Expression table, string index)
        {
            _Table = table;
            if (index.Length > 0 &&
                Char.IsLetter(index, 0) &&
                index.All(c => Char.IsLetterOrDigit(c) || c == '_') &&
                !_Keywords.Contains(index))
            {
                _Index = index;
                _IndexExpr = null;
            }
            else
            {
                _Index = null;
                _IndexExpr = new ConstStringExpr(index);
            }
        }

        public IndexExpr(Expression table, int index)
        {
            _Table = table;
            _Index = null;
            _IndexExpr = new ConstNumberExpr(index);
        }

        public IndexExpr(Expression table, Expression index)
        {
            _Table = table;
            _Index = null;
            _IndexExpr = index;
        }

        private static string[] _Keywords = new[] {
            #region Keyword List
            "base",
            "break",
            "case",
            "catch",
            "class",
            "clone",
            "continue",
            "const",
            "default",
            "delete",
            "else",
            "enum",
            "extends",
            "for",
            "foreach",
            "function",
            "if",
            "in",
            "local",
            "null",
            "resume",
            "return",
            "switch",
            "this",
            "throw",
            "try",
            "typeof",
            "while",
            "yield",
            "constructor",
            "instanceof",
            "true",
            "false",
            "static",
            #endregion
        };

        public override void Write(TextWriter writer, int indent)
        {
            _Table.Write(writer, indent);
            if (_Index != null)
            {
                writer.Write('.');
                writer.Write(_Index);
            }
            else
            {
                writer.Write('[');
                _IndexExpr.Write(writer, indent);
                writer.Write(']');
            }
        }
    }

    public class BiOpExpr : Expression
    {
        public enum Op
        {
            Add,
            Minus,
            Multiply,
            Divide,
            Mod,
            Assign,
            And,
            Or,
            Greater,
            GreaterOrEqual,
            Less,
            NotEqual,
            Equal,
        }

        private readonly Expression _Left, _Right;
        private readonly Op _Op;

        public BiOpExpr(Expression left, Expression right, Op op)
        {
            _Left = left;
            _Right = right;
            _Op = op;
        }

        public override void Write(TextWriter writer, int indent)
        {
            if (_Op != Op.Assign)
            {
                writer.Write('(');
            }
            _Left.Write(writer, indent);
            if (_Op != Op.Assign)
            {
                writer.Write(')');
            }
            writer.Write(GetOpStr(_Op));
            if (_Op != Op.Assign)
            {
                writer.Write('(');
            }
            _Right.Write(writer, indent);
            if (_Op != Op.Assign)
            {
                writer.Write(')');
            }
        }

        private static string GetOpStr(Op op)
        {
            switch (op)
            {
                case Op.Add: return " + ";
                case Op.Minus: return " - ";
                case Op.Multiply: return " * ";
                case Op.Divide: return " / ";
                case Op.Mod: return " % ";
                case Op.Assign: return " = ";
                case Op.Or: return " || ";
                case Op.And: return " && ";
                case Op.Greater: return " > ";
                case Op.GreaterOrEqual: return " >= ";
                case Op.Less: return " < ";
                case Op.NotEqual: return " != ";
                case Op.Equal: return " == ";
                default: return " ";
            }
        }
    }

    public class UnOpExpr : Expression
    {
        public enum Op
        {
            Sub,
            Not,
            BitNot,
        }

        private readonly Expression _Expr;
        private readonly Op _Op;

        public UnOpExpr(Expression expr, Op op)
        {
            _Expr = expr;
            _Op = op;
        }

        public override void Write(TextWriter writer, int indent)
        {
            writer.Write(GetOpLeftStr(_Op));
            writer.Write('(');
            _Expr.Write(writer, indent);
            writer.Write(')');
            writer.Write(GetOpRightStr(_Op));
        }

        private static string GetOpLeftStr(Op op)
        {
            switch (op)
            {
                case Op.Sub: return "-";
                case Op.Not: return "!";
                case Op.BitNot: return "~";
                default: return "";
            }
        }

        private static string GetOpRightStr(Op op)
        {
            switch (op)
            {
                default: return "";
            }
        }
    }

    public class ThisExpr : Expression
    {
        public static readonly ThisExpr Instance = new ThisExpr();

        public override void Write(TextWriter writer, int indent)
        {
            writer.Write("this");
        }
    }

    public class IdentifierExpr : Expression
    {
        private readonly string _Str;

        public IdentifierExpr(string str)
        {
            _Str = str;
        }

        public override void Write(TextWriter writer, int indent)
        {
            writer.Write(_Str);
        }
    }

    public class ArrayExpr : Expression
    {
        private readonly Expression[] _Expr;

        public ArrayExpr(params Expression[] expr)
        {
            _Expr = expr;
        }

        public override void Write(TextWriter writer, int indent)
        {
            writer.Write('[');
            Expression lastExpr = null;
            foreach(var e in _Expr)
            {
                if (lastExpr != null)
                {
                    lastExpr.Write(writer, indent);
                    writer.Write(", ");
                }
                lastExpr = e;
            }
            if (lastExpr != null)
            {
                lastExpr.Write(writer, indent);
            }
            writer.Write(']');
        }
    }
}
