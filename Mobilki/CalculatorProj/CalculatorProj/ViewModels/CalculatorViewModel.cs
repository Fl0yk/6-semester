using CalculatorProj.Constans;
using CalculatorProj.Enums;
using CalculatorProj.Exceptions;
using CalculatorProj.Models.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Globalization;

namespace CalculatorProj.ViewModels
{
    public partial class CalculatorViewModel : ObservableObject
    {
        [ObservableProperty]
        private string display;

        [ObservableProperty]
        private string expression ="";

        private const int _inputSize = 15;

        private IEngineeringCalculator<double> _calculator;

        private CalculatorState _curState = new();

        private Stack<CalculatorState> _states = new();

        public CalculatorViewModel(IEngineeringCalculator<double> baseCalculator)
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
            Display = _curState.First;
            _calculator = baseCalculator;
        }

        #region Event handlers

        [RelayCommand]
        public void InputDigitHandler(string input)
        {
            switch (_curState.State)
            {
                case State.Equal:
                case State.FirstInit:
                case State.Error:
                    //if (_curState.First.Length >= _inputSize)
                        //return;
                    if (_states.Count == 0)
                        Expression = "";

                    _curState.First = IsBaseStateOfDigit(_curState.First) ? 
                        input : _curState.First + input;

                    Display = _curState.First.ToString();
                    break;
                case State.SecondInit:
                    if (_curState.Second.Length >= _inputSize)
                        return;

                    _curState.Second = IsBaseStateOfDigit(_curState.Second) ?
                        input : _curState.Second + input;

                    Display = _curState.Second.ToString();
                    break;
                case State.ThirdInit:
                    if (_curState.Third.Length >= _inputSize)
                        return;

                    _curState.Third = IsBaseStateOfDigit(_curState.Third) ?
                        input : _curState.Third + input;

                    Display = _curState.Third.ToString();
                    break;
                default:
                    break;

            }
        }

        [RelayCommand]
        public void InputCommaHandler()
        {
            switch (_curState.State)
            {
                case State.FirstInit:
                case State.Equal:
                    if (!IsContainsComma(_curState.First))
                    {
                        _curState.First += ',';
                        Display = _curState.First;
                    }
                    break;
                case State.SecondInit:
                    if (!IsContainsComma(_curState.Second))
                    {
                        _curState.Second += ',';
                        Display = _curState.Second;
                    }
                    break;
                case State.ThirdInit:
                    if (!IsContainsComma(_curState.Third))
                    {
                        _curState.Third += ',';
                        Display = _curState.Third;
                    }
                    break;
                default:
                    break;

            }

            bool IsContainsComma(String digit)
            {
                return digit.Contains(",");
            }
        }

        [RelayCommand]
        public void BinaryOperationHandler(string operation)
        {
            if (!Constants.BinaryOpDict.ContainsKey(operation))
            {
                SetErrorState("Данная операция не существует");
                return;
            }

            BinaryOpEnum op = Constants.BinaryOpDict[operation];

            string resF1S, resS2T, resF1S2T;
            try
            {
                resF1S = GetBinaryOpResult(_curState.First, _curState.FirstOp, _curState.Second);
                resS2T = GetBinaryOpResult(_curState.Second, _curState.SecondOp, _curState.Third);
                resF1S2T = GetBinaryOpResult(_curState.First, _curState.FirstOp, resS2T);
            }
            catch (CalculationException)
            {
                SetErrorState();
                return;
            }

            string operand = "";
            // Выбрали число, выбрали операцию, на экране остается первое число, пока не начнут вводить второе
            // 
            switch (_curState.State)
            {
                case State.Error:
                case State.Equal:
                    Expression = "";
                    goto case State.FirstInit;
                case State.FirstInit:
                    _curState.FirstOp = op;
                    _curState.State = State.SecondInit;

                    operand = _curState.First;
                    break;
                case State.SecondInit:
                    operand = _curState.Second;

                    //Например, сначала +, а затем *
                    if (IsComplexOp(op) && IsSimpleOp(_curState.FirstOp))
                    {
                        _curState.SecondOp = op;
                        _curState.State = State.ThirdInit;
                    }
                    else
                    {
                        _curState.First = resF1S;
                        _curState.FirstOp = op;
                        _curState.Second = "0";
                    }
                    break;
                case State.ThirdInit:
                    operand = _curState.Third;
                    if (IsSimpleOp(op))
                    {
                        _curState.First = resF1S2T;
                        _curState.FirstOp = op;
                        _curState.Second = "0";
                        _curState.Third = "0";
                        _curState.State = State.SecondInit;

                        //Display = _curState.First;
                    }
                    else
                    {
                        _curState.Second = resS2T;
                        _curState.SecondOp = op;
                        _curState.Third = "0";

                        //Display += _curState.Second;
                    }
                    break;
            }

            if (Expression.Length == 0 || Expression.Last() != ')')
                Expression += operand;

            Expression += operation;
            Display = "0";
        }

        [RelayCommand]
        public void UnaryOperatorHandler(string operation)
        {
            if (!Constants.UnaryOpDict.ContainsKey(operation))
            {
                SetErrorState("Такой операции нет");
                return;
            }

            UnaryOpEnum op = Constants.UnaryOpDict[operation];

            try
            {
                switch (_curState.State)
                {
                    case State.Error:
                    case State.Equal:
                        Expression = "";
                        goto case State.FirstInit;
                    case State.FirstInit:
                        _curState.First = GetUnaryOpResult(_curState.First, op);
                        Display = _curState.First;
                        break;
                    case State.SecondInit:
                        _curState.Second = GetUnaryOpResult(_curState.Second, op);
                        Display = _curState.Second;
                        break;
                    case State.ThirdInit:
                        _curState.Third = GetUnaryOpResult(_curState.Third, op);
                        Display = _curState.Third;
                        break;
                }
            }
            catch (CalculationException)
            {
                SetErrorState();
            }

        }


        [RelayCommand]
        public void EqualHandler()
        {
            while(_states.Count > 0)
            {
                CloseBracket();
            }

            string resF1S, resS2T, resF1S2T;

            try
            {
                resF1S = GetBinaryOpResult(_curState.First, _curState.FirstOp, _curState.Second);
                resS2T = GetBinaryOpResult(_curState.Second, _curState.SecondOp, _curState.Third);
                resF1S2T = GetBinaryOpResult(_curState.First, _curState.FirstOp, resS2T);
            }
            catch (CalculationException)
            {
                SetErrorState();
                return;
            }
            string lastOperand = "";

            switch (_curState.State)
            {
                case State.SecondInit:
                    lastOperand = _curState.Second;
                    _curState.First = resF1S;
                    _curState.Second = "0";
                    break;
                case State.ThirdInit:
                    lastOperand = _curState.Third;
                    ResetState();
                    _curState.First = resF1S2T;
                    break;
            }

            _curState.State = State.Equal;
            Display = _curState.First;

            if (Expression.Length == 0)
                return;
            
            // Могут сломать унарные операторы
            if (Expression.Last() != ')')
                Expression += lastOperand;
            Expression += "=";
        }

        [RelayCommand]
        public void ClearCurrentDigit()
        {
            switch (_curState.State)
            {
                case State.FirstInit:
                case State.Equal:
                    _curState.First = "0";
                    Display = _curState.First;
                    break;
                case State.SecondInit:
                    _curState.Second = "0";
                    Display = _curState.Second;
                    break;
                case State.ThirdInit:
                    _curState.Third = "0";
                    Display = _curState.Third;
                    break;
            }
        }

        [RelayCommand]
        public void ClearOneSymbol()
        {
            switch (_curState.State)
            {
                case State.FirstInit:
                case State.Equal:
                    _curState.First = Clear(_curState.First);
                    Display = _curState.First;
                    break;
                case State.SecondInit:
                    _curState.Second = Clear(_curState.Second);
                    Display = _curState.Second;
                    break;
                case State.ThirdInit:
                    _curState.Third= Clear(_curState.Third);
                    Display = _curState.Third;
                    break;
            }

            string Clear(string digit)
            {
                string res = digit.Remove(digit.Length - 1);

                if (res.Length == 0 || res == "-")
                {
                    return "0";
                }
                else
                {
                    return res;
                }
            }
        }

        [RelayCommand]
        public void ClearAll()
        {
            ResetState(clearStates: true);
            Display = _curState.First;
            Expression = string.Empty;
        }

        [RelayCommand]
        public void OpenBracket()
        {
            _states.Push(_curState);
            ResetState();
            Display = _curState.First;
            Expression += "(";
        }

        [RelayCommand]
        public void CloseBracket()
        {
            if (_states.Count == 0)
                return;

            CalculatorState st = _states.Pop();

            string resState = EvalStateResult();

            if (_curState.State == State.Error)
                return;

            switch (_curState.State)
            {
                case State.FirstInit:
                    Expression += _curState.First;
                    break;
                case State.SecondInit:
                    Expression += _curState.Second;
                    break;
                case State.ThirdInit:
                    Expression += _curState.Third;
                    break;
            }

            Expression += ")";

            _curState = st;

            switch (_curState.State)
            {
                case State.FirstInit:
                case State.Equal:
                    _curState.First = resState;
                    break;
                case State.SecondInit:
                    _curState.Second = resState;
                    break;
                case State.ThirdInit:
                    _curState.Third = resState;
                    break;
            }
        }

        [RelayCommand]
        public void ConstantPi()
        {
            switch (_curState.State)
            {
                case State.FirstInit:
                case State.Equal:
                case State.Error:
                    _curState.First = _calculator.Pi.ToString();
                    Display = _curState.First;
                    break;
                case State.SecondInit:
                    _curState.Second = _calculator.Pi.ToString();
                    Display = _curState.Second;
                    break;
                case State.ThirdInit:
                    _curState.Third = _calculator.Pi.ToString();
                    Display = _curState.Third;
                    break;
            }
        }

        [RelayCommand]
        public void ConstantE()
        {
            switch (_curState.State)
            {
                case State.FirstInit:
                case State.Equal:
                case State.Error:
                    _curState.First = _calculator.E.ToString();
                    Display = _curState.First;
                    break;
                case State.SecondInit:
                    _curState.Second = _calculator.E.ToString();
                    Display = _curState.Second;
                    break;
                case State.ThirdInit:
                    _curState.Third = _calculator.E.ToString();
                    Display = _curState.Third;
                    break;
            }
        }

        #endregion

        #region Helper functions

        private string GetBinaryOpResult(string first, BinaryOpEnum op, string second)
        {
            double f = double.Parse(first);
            double s = double.Parse(second);

            switch (op)
            {
                case BinaryOpEnum.Sum:
                    return _calculator.Sum(f, s).ToString();
                case BinaryOpEnum.Diff:
                    return _calculator.Diff(f, s).ToString();
                case BinaryOpEnum.Mult:
                    return _calculator.Mult(f, s).ToString();
                case BinaryOpEnum.Div:  
                    return _calculator.Div(f, s).ToString();
                case BinaryOpEnum.Yqrt:
                    return _calculator.Yqrt(s, f).ToString();
                case BinaryOpEnum.PowY:
                    return _calculator.PowY(f, s).ToString();
                default:
                    throw new Exception("???");
            }
        }

        private string GetUnaryOpResult(string digit, UnaryOpEnum op)
        {
            double d = double.Parse(digit);
            switch (op)
            {
                case UnaryOpEnum.Sqrt:
                    return _calculator.Sqrt(d).ToString();
                case UnaryOpEnum.Minus:
                    return _calculator.Minus(d).ToString();
                case UnaryOpEnum.Reverse:
                    return _calculator.Reverse(d).ToString();
                case UnaryOpEnum.Ln:
                    return _calculator.Ln(d).ToString();
                case UnaryOpEnum.LogTen:
                    return _calculator.Log10(d).ToString();
                case UnaryOpEnum.ePow:
                    return _calculator.ePow(d).ToString();
                case UnaryOpEnum.TenPow:
                    return _calculator.TenPow(d).ToString();
                case UnaryOpEnum.Sin:
                    return _calculator.Sin(d).ToString();
                case UnaryOpEnum.Cos:
                    return _calculator.Cos(d).ToString();
                case UnaryOpEnum.Tan:
                    return _calculator.Tan(d).ToString();
                case UnaryOpEnum.Tanh:
                    return _calculator.Tanh(d).ToString();
                case UnaryOpEnum.Sinh:
                    return _calculator.Sinh(d).ToString();
                case UnaryOpEnum.Cosh:
                    return _calculator.Cosh(d).ToString();
                case UnaryOpEnum.Pow2:
                    return _calculator.Square(d).ToString();
                case UnaryOpEnum.Pow3:
                    return _calculator.Cube(d).ToString();
                default:
                    throw new Exception("???");
            }
        }

        private bool IsSimpleOp(BinaryOpEnum op)
        {
            return op == BinaryOpEnum.Sum || op == BinaryOpEnum.Diff;
        }

        private bool IsComplexOp(BinaryOpEnum op) => !IsSimpleOp(op);

        private void ResetState(bool clearStates = false)
        {
            _curState = new();

            if (clearStates)
                _states.Clear();
        }

        private void SetErrorState(string? message = null)
        {
            ResetState(clearStates: true);
            _curState.State = State.Error;
            Display = message ?? "Error";
        }

        private bool IsBaseStateOfDigit(string digit)
        {
            return digit == "0" || digit == "-0";
        }

        private string EvalStateResult()
        {
            string resF1S, resS2T, resF1S2T;

            try
            {
                resF1S = GetBinaryOpResult(_curState.First, _curState.FirstOp, _curState.Second);
                resS2T = GetBinaryOpResult(_curState.Second, _curState.SecondOp, _curState.Third);
                resF1S2T = GetBinaryOpResult(_curState.First, _curState.FirstOp, resS2T);
            }
            catch (CalculationException)
            {
                SetErrorState();
                return "0";
            }

            switch (_curState.State)
            {
                case State.ThirdInit:
                    return resF1S2T;
                case State.Error:
                    throw new Exception("Invalid state");
                default:
                    return resF1S;
            }
        }

        private bool LastIsBinaryOp()
        {
            return Constants.BinaryOpDict.Keys.Contains(Expression.Last().ToString());
        }

        #endregion

        #region Helper classes
        private class CalculatorState
        {
            public String First { get; set; } = new("0");
            public String Second { get; set; } = new("0");
            public String Third { get; set; } = new("0");

            public BinaryOpEnum FirstOp { get; set; } = BinaryOpEnum.Sum;
            public BinaryOpEnum SecondOp { get; set; } = BinaryOpEnum.Sum;

            public State State { get; set; } = State.FirstInit;
        }

        private enum State
        {
            FirstInit,
            SecondInit,
            ThirdInit,
            Equal,
            Error
        }

        #endregion
    }
}
