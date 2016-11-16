using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace DenizenIRCBot
{
    public class MathContext
    {
        public Stack<double> STK;
        public Dictionary<string, Tuple<int, Action<MathContext>>> Functions;
    }

    /// <summary>
    /// Credits to mcmonkey
    /// </summary>
    class MonkeyMath
    {
        public static int[] Priority = new int[128];
        public static MathOp[] Operations = new MathOp[128];
        public static Action<MathContext>[] Operators = new Action<MathContext>[(int)MathOp.OP_COUNT];

        public static Dictionary<string, Tuple<int, Action<MathContext>>> BaseFunctions = new Dictionary<string, Tuple<int, Action<MathContext>>>();

        static MonkeyMath()
        {
            Priority['('] = 10;
            Priority['+'] = 50;
            Priority['-'] = 50;
            Priority['*'] = 60;
            Priority['/'] = 60;
            Priority['%'] = 60;
            Priority['^'] = 70;
            Priority['F'] = 100;
            Operations['+'] = MathOp.ADD;
            Operations['-'] = MathOp.SUB;
            Operations['*'] = MathOp.MUL;
            Operations['/'] = MathOp.DIV;
            Operations['%'] = MathOp.MOD;
            Operations['^'] = MathOp.EXP;
            Operators[(int)MathOp.ADD] = OPERATOR_ADD;
            Operators[(int)MathOp.SUB] = OPERATOR_SUB;
            Operators[(int)MathOp.MUL] = OPERATOR_MUL;
            Operators[(int)MathOp.DIV] = OPERATOR_DIV;
            Operators[(int)MathOp.MOD] = OPERATOR_MOD;
            Operators[(int)MathOp.EXP] = OPERATOR_EXP;
            BaseFunctions.Add("sin", new Tuple<int, Action<MathContext>>(1, FUNCTION_SIN));
            BaseFunctions.Add("cos", new Tuple<int, Action<MathContext>>(1, FUNCTION_COS));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void FUNCTION_SIN(MathContext m)
        {
            double a = m.STK.Pop();
            m.STK.Push(Math.Sin(a));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void FUNCTION_COS(MathContext m)
        {
            double a = m.STK.Pop();
            m.STK.Push(Math.Cos(a));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void OPERATOR_ADD(MathContext m)
        {
            double b = m.STK.Pop();
            double a = m.STK.Pop();
            m.STK.Push(a + b);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void OPERATOR_SUB(MathContext m)
        {
            double b = m.STK.Pop();
            double a = m.STK.Pop();
            m.STK.Push(a - b);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void OPERATOR_MUL(MathContext m)
        {
            double b = m.STK.Pop();
            double a = m.STK.Pop();
            m.STK.Push(a * b);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void OPERATOR_DIV(MathContext m)
        {
            double b = m.STK.Pop();
            double a = m.STK.Pop();
            m.STK.Push(a / b);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void OPERATOR_MOD(MathContext m)
        {
            double b = m.STK.Pop();
            double a = m.STK.Pop();
            m.STK.Push(a % b);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void OPERATOR_EXP(MathContext m)
        {
            double b = m.STK.Pop();
            double a = m.STK.Pop();
            m.STK.Push(Math.Pow(a, b));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNumberSymbol(char inp)
        {
            return (inp >= '0' && inp <= '9') || inp == '.';
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsOperator(char inp)
        {
            return inp == '+' || inp == '-' || inp == '*' || inp == '/' || inp == '%' || inp == '^';
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsLetter(char inp)
        {
            return (inp >= 'a' && inp <= 'z') || inp == '_';
        }

        static MathContext CalcInternal(List<MathOperation> mops, Dictionary<string, Tuple<int, Action<MathContext>>> functions)
        {
            MathContext math = new MathContext();
            math.STK = new Stack<double>();
            math.Functions = functions;
            for (int i = 0; i < mops.Count; i++)
            {
                if (mops[i].OpValue == MathOp.LDN)
                {
                    math.STK.Push(mops[i].NumericValue);
                }
                else if (mops[i].OpValue == MathOp.FNC)
                {
                    functions[mops[i].FunctionValue].Item2(math);
                }
                else
                {
                    Operators[(int)mops[i].OpValue](math);
                }
            }
            return math;
        }

        public static double Calculate(List<MathOperation> mops, Dictionary<string, Tuple<int, Action<MathContext>>> functions)
        {
            return CalcInternal(mops, functions).STK.Pop();
        }

        public static bool Verify(List<MathOperation> mops, Dictionary<string, Tuple<int, Action<MathContext>>> functions, out string err)
        {
            for (int i = 0; i < mops.Count; i++)
            {
                if (mops[i].OpValue == MathOp.BAD)
                {
                    err = "BAD op call!";
                    return false;
                }
            }
            try
            {
                Dictionary<string, Tuple<int, Action<MathContext>>> tfunc = new Dictionary<string, Tuple<int, Action<MathContext>>>();
                foreach (KeyValuePair<string, Tuple<int, Action<MathContext>>> func in functions)
                {
                    tfunc.Add(func.Key, new Tuple<int, Action<MathContext>>(func.Value.Item1, (m) => {
                        for (int i = 0; i < func.Value.Item1; i++)
                        {
                            m.STK.Pop();
                        }
                        m.STK.Push(0);
                    }));
                }
                if (CalcInternal(mops, tfunc).STK.Count != 1)
                {
                    err = "End stack sized incorrectly!";
                    return false;
                }
                err = null;
                return true;
            }
            catch (FormatException)
            {
                err = "Number formatting invalid!";
                return false;
            }
            catch (OverflowException)
            {
                err = "Number overflow!";
                return false;
            }
            catch (InvalidOperationException)
            {
                err = "Unreasonable stack!";
                return false;
            }
        }

        public static List<MathOperation> Parse(string input, out string err)
        {
            try
            {
                char[] inp = input.ToLowerInvariant().Replace(" ", "").ToCharArray();
                Stack<MathWaiting> waiting = new Stack<MathWaiting>();
                List<MathOperation> result = new List<MathOperation>();
                for (int i = 0; i < inp.Length; i++)
                {
                    if (inp[i] == '(')
                    {
                        waiting.Push(new MathWaiting() { Type = '(' });
                    }
                    else if ((inp[i] == '-' && (i - 1 < 0 || (IsOperator(inp[i - 1]) || inp[i - 1] == '(' || inp[i - 1] == ')'))) || IsNumberSymbol(inp[i]))
                    {
                        StringBuilder snum = new StringBuilder(6);
                        snum.Append(inp[i]);
                        while (i + 1 < inp.Length && IsNumberSymbol(inp[i + 1]))
                        {
                            snum.Append(inp[++i]);
                        }
                        result.Add(new MathOperation() { NumericValue = double.Parse(snum.ToString()), OpValue = MathOp.LDN });
                    }
                    else if (IsLetter(inp[i]))
                    {
                        StringBuilder sfnc = new StringBuilder(6);
                        sfnc.Append(inp[i]);
                        while (inp[i + 1] != '(')
                        {
                            sfnc.Append(inp[++i]);
                        }
                        int p = Priority['F'];
                        while (waiting.Count != 0)
                        {
                            MathWaiting waitnow = waiting.Peek();
                            if (Priority[waitnow.Type] < p)
                            {
                                break;
                            }
                            waiting.Pop();
                            if (waitnow.Type == 'F')
                            {
                                result.Add(new MathOperation() { OpValue = MathOp.FNC, FunctionValue = waitnow.FunctionName });
                            }
                            else
                            {
                                result.Add(new MathOperation() { OpValue = Operations[waitnow.Type] });
                            }
                        }
                        waiting.Push(new MathWaiting() { Type = 'F', FunctionName = sfnc.ToString() });
                    }
                    else if (IsOperator(inp[i]))
                    {
                        int p = Priority[inp[i]];
                        while (waiting.Count != 0)
                        {
                            MathWaiting waitnow = waiting.Peek();
                            if (Priority[waitnow.Type] < p)
                            {
                                break;
                            }
                            waiting.Pop();
                            if (waitnow.Type == 'F')
                            {
                                result.Add(new MathOperation() { OpValue = MathOp.FNC, FunctionValue = waitnow.FunctionName });
                            }
                            else
                            {
                                result.Add(new MathOperation() { OpValue = Operations[waitnow.Type] });
                            }
                        }
                        waiting.Push(new MathWaiting() { Type = inp[i] });
                    }
                    else if (inp[i] == ')')
                    {
                        while (true)
                        {
                            MathWaiting waitnow = waiting.Pop();
                            if (waitnow.Type == '(')
                            {
                                break;
                            }
                            if (waitnow.Type == 'F')
                            {
                                result.Add(new MathOperation() { OpValue = MathOp.FNC, FunctionValue = waitnow.FunctionName });
                            }
                            else
                            {
                                result.Add(new MathOperation() { OpValue = Operations[waitnow.Type] });
                            }
                        }
                    }
                }
                while (waiting.Count != 0)
                {
                    MathWaiting waitnow = waiting.Pop();
                    if (waitnow.Type == '(')
                    {
                        err = "Inconsistent parenthesis!";
                        return null;
                    }
                    if (waitnow.Type == 'F')
                    {
                        result.Add(new MathOperation() { OpValue = MathOp.FNC, FunctionValue = waitnow.FunctionName });
                    }
                    else
                    {
                        result.Add(new MathOperation() { OpValue = Operations[waitnow.Type] });
                    }
                }
                err = null;
                return result;
            }
            catch (FormatException)
            {
                err = "Number formatting invalid!";
                return null;
            }
            catch (OverflowException)
            {
                err = "Number overflow!";
                return null;
            }
            catch (IndexOutOfRangeException)
            {
                err = "Invalid index, potentially a missing symbol!";
                return null;
            }
            catch (InvalidOperationException)
            {
                err = "Unreasonable stack!";
                return null;
            }
        }
    }

    struct MathWaiting
    {
        public char Type;
        public string FunctionName;
    }

    struct MathOperation
    {
        public double NumericValue;
        public string FunctionValue;
        public MathOp OpValue;
    }

    enum MathOp : byte
    {
        /// <summary>
        /// Bad input!
        /// </summary>
        BAD = 0,
        /// <summary>
        /// Addition.
        /// </summary>
        ADD = 1,
        /// <summary>
        /// Subtraction.
        /// </summary>
        SUB = 2,
        /// <summary>
        /// Multiplication.
        /// </summary>
        MUL = 3,
        /// <summary>
        /// Division.
        /// </summary>
        DIV = 4,
        /// <summary>
        /// Modulus.
        /// </summary>
        MOD = 5,
        /// <summary>
        /// Exponent (x to the power of y).
        /// </summary>
        EXP = 6,
        /// <summary>
        /// Load number.
        /// </summary>
        LDN = 7,
        /// <summary>
        /// Function call.
        /// </summary>
        FNC = 8,
        /// <summary>
        /// Operator count.
        /// </summary>
        OP_COUNT = 9
    }
}
