using Antlr4.Runtime.Misc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PJP_projekt
{
    public class GrammarVisitor : GrammarBaseVisitor<object?>
    {
        
        Dictionary<string, object?> Variables { get; } = new();
        public StringBuilder sb = new StringBuilder();
        public GrammarVisitor()
        {
            Variables["write"] = new Func<object?[], object?>(Write);
            Variables["read"] = new Func<object?[], object?>(Read);
        }
        public object? Write (object?[] args)
        {
            var newArgs = ((GrammarParser.ExpressionContext[])args).Select(Visit);
            int howMany = 0;
            foreach (var arg in newArgs)
            {
                //if (arg is string s)
                //{
                //    sb.AppendLine("push S \"" + s + "\"");
                //}
                //else if (arg is int i)
                //{
                //    sb.AppendLine("push I " + i);
                //}

                //else if (arg is float f)
                //{
                //    sb.AppendLine("push f " + f);
                //}

                //else if (arg is bool b)
                //{
                //    sb.AppendLine("push b " + b);
                //}

                Console.Write(arg + " ");
                howMany++;
            }
            sb.AppendLine("print " + howMany);
            Console.Write("\n");
            return null;
            
        }
        public object? Read(object?[] args)
        {

            foreach (var arg in args)
            {

                var neco = (GrammarParser.ExpressionContext)arg;
                var key = neco.children[0].ToString();
                if (Variables.ContainsKey(key) == false)
                {
                    sb.AppendLine("You cannot read input into undeclared variable");
                    throw new Exception("You cannot read input into undeclared variable");
                }
                object? input = Console.ReadLine();
                //check typy
                var castedInput = Cast(input, Variables[key].GetType());
                if (!IsAcceptableType(key, castedInput))
                {
                    sb.AppendLine("You cannot read input into different datatype variable");
                    throw new Exception("You cannot read input into different datatype variable");
                }
                Variables[key] = castedInput;



            }
            
            return null;

        }
        public static dynamic Cast(dynamic obj, Type castTo)
        {
            if (castTo == typeof(float))
            {
                return float.Parse(obj, System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
            }
            return Convert.ChangeType(obj, castTo);
        }
        public override object VisitFunctionCall([NotNull] GrammarParser.FunctionCallContext context)
        {
            var name = context.IDENTIFIER().GetText();



            var args = context.expression().ToArray();


            if (!Variables.ContainsKey(name))
            {
                throw new Exception($"Function {name} is not defined");
            }
            if ((Variables[name] is not Func<object?[], object> func))
            {
                throw new Exception($"Variable {name} is not defined.");
            }

            return func(args);
        }

        public override object? VisitDeclare([NotNull] GrammarParser.DeclareContext context)
        {
            var variables = context.IDENTIFIER();
            foreach (var singleVar in variables)
            {
                var varName = singleVar.GetText();
                
                if (Variables.ContainsKey(varName))
                {
                    Console.WriteLine($"Variable {varName} is already defined.");
                    return null;
                    //throw new Exception($"Variable {varName} is already defined.");
                }
            }
            


            var declarationType = context.datatype().GetText();

            // dam defaultní hodnotu do proměne když není nic přiřazeno
            var notDeclaredWithValue = new object();
            switch (declarationType)
            {
                case "int":
                    notDeclaredWithValue = 0;
                    break;
                case "float":
                    notDeclaredWithValue = 0.0f;
                    break;
                case "string":
                    notDeclaredWithValue = "";
                    break;
                case "bool":
                    notDeclaredWithValue = false;
                    break;
                default:
                    break;
            }
            
            context.expression();
            var value = context.expression() == null ? notDeclaredWithValue : Visit(context.expression());
            var actualType = value.GetType().ToString();

            if (actualType.Contains("Int"))
            {
                actualType = "int";

            }
            else if (actualType.Contains("String"))
            {
                actualType = "string";
            }
            else if (actualType.Contains("Bool"))
            {
                actualType = "bool";
            }
            else if (actualType == "System.Single")
            {
                actualType = "float";
            }
            if (declarationType != actualType)
            {
                throw new Exception($"Declaration type {declarationType} does not match assigned data type {actualType}");
            }

            foreach (var singleVar in variables)
            {
                var varName = singleVar.GetText();
                if (value is string s)
                {
                    if (s == "")
                    {
                        sb.AppendLine("push S \"\"" );
                    }
                    else
                        sb.AppendLine("push S " + s);
                }
                else if (value is int i)
                {
                    sb.AppendLine("push I " + i);
                }
                else if (value is float f)
                {
                    sb.AppendLine("push F " + f);
                }
                else if (value is bool b)
                {
                    sb.AppendLine("push B " + b);
                }
                sb.AppendLine("save " + singleVar.GetText());
                Variables[varName] = value;
            }

            // Console.WriteLine(varName + ": "+value.ToString());
            return null;
        }
        private bool IsAcceptableType ( string varName , object? assignedValue)
        {
            if (!Variables.ContainsKey(varName))
            {
                //Console.WriteLine("Undefined variable " + varName);
                return false ;
                //throw new Exception("Undefined variable "+ varName);
            }
            var type1 = Variables[varName].GetType();
            var type2 = assignedValue.GetType();
            if (type1 != type2)
            {
                if ( (type2 == typeof(int) && type1 == typeof(float)))
                {
                    return true;
                }
                return false;
            }
            return true;
        }
        public override object? VisitAssignment([NotNull] GrammarParser.AssignmentContext context)
        {
            var variables = context.IDENTIFIER();


            var value = Visit(context.expression());
            foreach (var singleVar in variables)
            {
                var varName = singleVar.GetText();
                if (IsAcceptableType(varName, value) == false)
                {
                    Console.WriteLine("You can not assing different data type to this variable");
                    return null;
                    //throw new Exception("You can not assing different data type to this variable");
                }
                Variables[varName] = value;

                sb.AppendLine("save " + varName);
            }
            // Console.WriteLine(varName + ": "+value.ToString());
            return value;
        }
        public override object VisitIdentifierExpression([NotNull] GrammarParser.IdentifierExpressionContext context)
        {
            var varName = context.IDENTIFIER().GetText();
            sb.AppendLine("load " + varName);
            if (Variables.ContainsKey(varName) == false)
            {
                throw new Exception($"Variable {varName} is not defined.");
            }

            return Variables[varName];
        }
        public override object? VisitConstant([NotNull] GrammarParser.ConstantContext context)
        {
            if (context.INTEGER() is { } i)
            {
                var str = i.ToString();
                if (str.StartsWith("0x"))
                {
                    string hexanum = str.Split('x')[1];
                    int val = Int32.Parse(hexanum, System.Globalization.NumberStyles.HexNumber);
                    sb.AppendLine("push I " + val);
                    return val;
                }
                else if (str.StartsWith("0"))
                {
                    var value = Convert.ToInt32(str, 8);
                    sb.AppendLine("push I " + value);
                    return value;
                }
                else
                {
                    sb.AppendLine("push I " + i);
                    return int.Parse(i.GetText());
                }

            }
            if (context.FLOAT() is { } f)
            {
                var text = f.GetText();
                float fff = float.Parse(text, System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
                sb.AppendLine("push F " + fff);
                return fff;
            }
            if (context.STRING() is { } s)
            {
                sb.AppendLine("push S " + s);
                return s.GetText()[1..^1];
            }
            if (context.BOOL() is { } b)
            {
                sb.AppendLine("push B " + b);
                return b.GetText() == "true";
            }
            
            return null;
        }
        public override object VisitMultiplicativeExpression([NotNull] GrammarParser.MultiplicativeExpressionContext context)
        {
            var left = Visit(context.expression()[0]);
            var right = Visit(context.expression()[1]);

            var op = context.multOp().GetText();

            return op switch
            {
                "*" => Mult(left, right),
                "/" => Divide(left, right),
                "%" => Modulus(left, right),
                _ => throw new NotImplementedException()
            };
        }
        private object? Modulus(object? left, object? right)
        {
            sb.AppendLine("mod");
            if (left is int l && right is int r)
            {
                return l % r;
            }



            //throw new Exception("You can only use mod with integers");
            Console.WriteLine("You can only use mod with integers");
            return null;
        }
        private object? Divide(object? left, object? right)
        {
            sb.AppendLine("div");
            if (left is int l && right is int r)
            {
                return l / r;
            }

            if (left is float lf && right is float rf)
            {
                return lf / rf;
            }
            if (left is float lff && right is int ri)
            {
                return lff / ri;
            }

            if (left is float li && right is int rff)
            {
                return li / rff;
            }
            throw new NotImplementedException();
        }
        private object? Mult(object? left, object? right)
        {
            sb.AppendLine("mul");
            if (left is int l && right is int r)
            {
                return l * r;
            }

            if (left is float lf && right is float rf)
            {
                return lf * rf;
            }
            if (left is float lff && right is int ri)
            {
                return lff * ri;
            }

            if (left is float li && right is int rff)
            {
                return li * rff;
            }
            throw new NotImplementedException();
        }
        public override object? VisitAdditiveExpression([NotNull] GrammarParser.AdditiveExpressionContext context)
        {

            var left = Visit(context.expression()[0]);
            var right = Visit(context.expression()[1]);

            var op = context.addOp().GetText();
            
            return op switch
            {
                "+" => Add(left , right),
                "." => Concat(left , right),
                "-" => Subtract(left, right),
                _ => throw new NotImplementedException()
            };


        }
        
        public override object VisitIfBlock([NotNull] GrammarParser.IfBlockContext context)
        {
            var visit = Visit(context.expression());

            var elseifblock = context.elseIfBlock();
            //var justText = Visit(context.block());
            //var count = context.block().line();
            if (visit is bool b && b )
            {
                if (context.block() != null)
                    Visit(context.block());
                else if (context.line() != null)
                    Visit(context.line());

            }
            else
            {
                if (elseifblock != null)
                    Visit(elseifblock);              

            }
            return null;
        }
        public override object VisitElseIfBlock([NotNull] GrammarParser.ElseIfBlockContext context)
        {
            if (context.block() != null)
                Visit(context.block());
            else if (context.line() != null)
                Visit(context.line());
            return null;
        }
        private object? Subtract(object? left, object? right)
        {
            sb.AppendLine("minus");
            if (left is int l && right is int r)
            {
                return l - r;
            }

            if (left is float lf && right is float rf)
            {
                return lf - rf;
            }
            if (left is float lff && right is int ri)
            {
                return lff - ri;
            }

            if (left is float li && right is int rff)
            {
                return li - rff;
            }
            throw new NotImplementedException();
        }
        private object? Concat(object? left, object? right)
        {
            sb.AppendLine("concat");
            if (left is string ls && right is string rs)
            {
                return $"{ls}{rs}";
            }
            Console.WriteLine($"Cannot concat these values: {left?.GetType()} and {right?.GetType()}.");
            return null;
           // throw new Exception($"Cannot add these values: {left?.GetType()} and {right?.GetType()}.");
        }
        private object? Add(object? left, object? right)
        {
            sb.AppendLine("add");
            if (left is int l && right is int r)
            {
                return l + r;
            }

            if (left is float lf && right is float rf)
            {
                return lf + rf;
            }

            if (left is float lff && right is int ri)
            {
                return lff + ri;
            }

            if (left is float li && right is int rff)
            {
                return li + rff;
            }
            ;
            Console.WriteLine($"Cannot add these values: {left?.GetType()} and {right?.GetType()}.");
            return null;
            throw new Exception($"Cannot add these values: {left?.GetType()} and {right?.GetType()}.");
        }

        public override object? VisitWhileBlock([NotNull] GrammarParser.WhileBlockContext context)
        {
            var visit = Visit(context.expression());


            while (visit is bool b && b)
            {               
                
                Visit(context.block());
                visit = Visit(context.expression());

            }

            return null;

        }


        public override object VisitParenthesizedExpression([NotNull] GrammarParser.ParenthesizedExpressionContext context)
        {
            var input = Visit(context.expression());

            return input;
        }
        public override object? VisitComparisonExpression([NotNull] GrammarParser.ComparisonExpressionContext context)
        {

            var left = Visit(context.expression()[0]);
            var right = Visit(context.expression()[1]);

            var op = context.compareOp().GetText();
            if ((left is int && right is float) || (left is float && right is int))
            {
                sb.AppendLine("itof");
            }
            return op switch
            {
                "==" => IsEquals(left, right),
                "!=" =>  NotEquals(left, right),
                ">" => GreaterThan(left, right),
                "<" => LessThan(left, right),
                "=>" => GreaterThenOrEqual(left, right),
                "<=" => LesserThenOrEqual(left, right),
                _ => throw new NotImplementedException()
            };
        }

        private bool IsEquals(object? left, object? right)
        {
            sb.AppendLine("eq");
            if (left is int l && right is int r)
            {
                return l == r;
            }

            if (left is float lf && right is float rf)
            {
                return lf == rf;
            }

            if (left is float lff && right is int ri)
            {
                return lff == ri;
            }

            if (left is float li && right is int rff)
            {
                return li == rff;
            }
            if (left is string sl && right is string sr)
            {
                return sl == sr;
            }
            if (left is bool lb && right is bool rb)
            {
                return lb == rb;
            }
            throw new Exception($"Cannot compare these types {left.GetType()} and {right.GetType()}");
        }
        private bool NotEquals(object? left, object? right)
        {
            sb.AppendLine("neq");
            if (left is int l && right is int r)
            {
                return l != r;
            }

            if (left is float lf && right is float rf)
            {
                return lf != rf;
            }

            if (left is float lff && right is int ri)
            {
                return lff != ri;
            }

            if (left is float li && right is int rff)
            {
                return li != rff;
            }
            if (left is string sl && right is string sr)
            {
                return sl != sr;
            }
            if (left is bool lb && right is bool rb)
            {
                return lb != rb;
            }
            throw new Exception($"Cannot compare these types {left.GetType()} and {right.GetType()}");
        }
        private bool GreaterThan(object? left, object? right)
        {
            sb.AppendLine("gt");
            if (left is int l && right is int r)
            {
                return l > r;
            }

            if (left is float lf && right is float rf)
            {
                return lf > rf;
            }

            if (left is float lff && right is int ri)
            {
                return lff > ri;
            }

            if (left is int li && right is float rff)
            {
                return li > rff;
            }
            throw new Exception($"Cannot compare these types {left.GetType()} and {right.GetType()}");
        }
        private bool LessThan(object? left, object? right)
        {
            sb.AppendLine("lt");
            if (left is int l && right is int r)
            {
                return l < r;
            }

            if (left is float lf && right is float rf)
            {
                return lf < rf;
            }

            if (left is float lff && right is int ri)
            {
                return lff < ri;
            }

            if (left is int li && right is float rff)
            {
                return li < rff;
            }
            throw new Exception($"Cannot compare these types {left.GetType()} and {right.GetType()}");
        }
        private bool GreaterThenOrEqual(object? left, object? right)
        {
            sb.AppendLine("gteq");
            if (left is int l && right is int r)
            {
                return l >= r;
            }

            if (left is float lf && right is float rf)
            {
                return lf >= rf;
            }

            if (left is float lff && right is int ri)
            {
                return lff >= ri;
            }

            if (left is float li && right is int rff)
            {
                return li >= rff;
            }
            throw new Exception($"Cannot compare these types {left.GetType()} and {right.GetType()}");
        }
        private bool LesserThenOrEqual(object? left, object? right)
        {
            sb.AppendLine("lteq");
            if (left is int l && right is int r)
            {
                return l <= r;
            }

            if (left is float lf && right is float rf)
            {
                return lf <= rf;
            }

            if (left is float lff && right is int ri)
            {
                return lff <= ri;
            }

            if (left is float li && right is int rff)
            {
                return li <= rff;
            }
            throw new Exception($"Cannot compare these types {left.GetType()} and {right.GetType()}");
        }

        public override object? VisitCompareOp([NotNull] GrammarParser.CompareOpContext context)
        {
            return base.VisitCompareOp(context);
        }

        public override object VisitBooleanExpression([NotNull] GrammarParser.BooleanExpressionContext context)
        {
            var left = Visit(context.expression()[0]);
            var right = Visit(context.expression()[1]);

            var op = context.boolOp().GetText();

            return op switch
            {
                "&&" => BoolAnd(left, right),
                "||" => BoolOr(left, right),

                _ => throw new NotImplementedException()
            };
            
        }
        private bool BoolAnd(object? left, object? right)
        {
            sb.AppendLine("and");
            if (left is bool l && right is bool r)
            {
                return l && r;
            }
            throw new Exception($"Cannot compare these types {left.GetType()} and {right.GetType()}");
        }
        private bool BoolOr(object? left, object? right)
        {
            sb.AppendLine("or");
            if (left is bool l && right is bool r)
            {
                return l || r;
            }
            throw new Exception($"Cannot compare these types {left.GetType()} and {right.GetType()}");
        }

        public override object? VisitUnaryMinusExpression([NotNull] GrammarParser.UnaryMinusExpressionContext context)
        {
            sb.AppendLine("uminus");
            var input = Visit(context.expression());

            if (input is int myInt )
            {
                return -myInt;
            }

            if (input is int myFloat)
            {
                return -myFloat;
            }
            throw new Exception("You can not unary minus to this datatype");
        }
        public override object VisitNotExpression([NotNull] GrammarParser.NotExpressionContext context)
        {
            
            var input = Visit(context.expression());
            sb.AppendLine("not");
            if (input is bool myBool)
            {
                return !myBool;
            }
            

            throw new Exception("You can not unary negate to this datatype");
        }
    }
}
