namespace Morpeh.BigNumber {
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Numerics;
    using System.Text.RegularExpressions;
    using UnityEngine;
    
    public static class BigNumberSymbol {
        public const string EMPTY = "";
        public const string K = "K";
        public const string M = "M";
        public const string B = "B";
        public const string C = "C";
        public const string D = "D";
        public const string E = "E";
        public const string Aa = "Aa";
        public const string Bb = "Bb";
        public const string Cc = "Cc";
        public const string Dd = "Dd";
        public const string Ee = "Ee";
        public const string AAa = "AAa";
        public const string BBb = "BBb";
        public const string CCc = "CCc";
        public const string DDd = "DDd";
        public const string EEe = "EEe";
    }
    
    [Serializable]
    public struct BigNumber : IComparable<BigNumber>, IEquatable<BigNumber>  {
        private const int DIGIT_VALUE = 1000;
        private const string REGEXP_PATTERN = @"([.\d]+)([a-zA-Z]+)";
        private const string FLOAT_FORMAT = "0.##";
        private static readonly List<string> symbols = new List<string>
        {
            BigNumberSymbol.EMPTY,
            BigNumberSymbol.K,
            BigNumberSymbol.M,
            BigNumberSymbol.B,
            BigNumberSymbol.C,
            BigNumberSymbol.D,
            BigNumberSymbol.E,
            BigNumberSymbol.Aa,
            BigNumberSymbol.Bb,
            BigNumberSymbol.Cc,
            BigNumberSymbol.Dd,
            BigNumberSymbol.Ee,
            BigNumberSymbol.AAa,
            BigNumberSymbol.BBb,
            BigNumberSymbol.CCc,
            BigNumberSymbol.DDd,
            BigNumberSymbol.EEe
        };
        
        [SerializeField]
        private BigInteger value;
        
        public BigNumber(decimal number, string digit = "") {
            var digitIndex = symbols.IndexOf(digit);
            this.value = digitIndex > 0 ? new BigInteger(number * DIGIT_VALUE) : new BigInteger(number);
            for (var i = 0; i < digitIndex - 1; i++)
            {
                this.value *= DIGIT_VALUE;
            }
        }
        public BigNumber(float value) => this.value = new BigInteger(value);
        public BigNumber(double value) => this.value = new BigInteger(value);
        public BigNumber(int value) => this.value = new BigInteger(value);
        public BigNumber(long value) => this.value = new BigInteger(value);
        public BigNumber(BigInteger value) => this.value = value;
        
        public static BigNumber Parse(string value) {
            var re = new Regex(REGEXP_PATTERN);
            var result = re.Match(value);

            if (result.Groups.Count > 1)
            {
                var dcml = Convert.ToDecimal(float.Parse(result.Groups[1].Value, CultureInfo.InvariantCulture));
                var chr = result.Groups[2].Value;

                return new BigNumber(dcml, chr);
            }
             
            return new BigNumber(BigInteger.Parse(value));
        }

        public void SetBigInteger(BigInteger value) {
            this.value = value;
        }
        
        public void SetBigInteger(BigNumber value) {
            this.value = value.value;
        }

        public override string ToString() {
            var digit_index = 0;
            var tmp = this.value;
            while (tmp >= DIGIT_VALUE * DIGIT_VALUE)
            {
                tmp /= DIGIT_VALUE;
                digit_index++;
            }
            if(tmp >= DIGIT_VALUE)
                digit_index++;

            var dcml = (float) tmp / (digit_index == 0 ? 1 : (float)DIGIT_VALUE);
            var int_decimal = (int) dcml;
            if(dcml == int_decimal)
                return $"{int_decimal}{symbols[digit_index]}";
            else
                return $"{dcml.ToString(FLOAT_FORMAT, CultureInfo.InvariantCulture)}{symbols[digit_index]}";
        }
        public string ToBigIntegerString() => this.value.ToString();

        public static BigNumber operator +(BigNumber left, int right) => new BigNumber(left.value + right);
        public static BigNumber operator +(BigNumber left, float right) => new BigNumber(left.value + new BigInteger(right));
        public static BigNumber operator +(BigNumber left, double right) => new BigNumber(left.value + new BigInteger(right));
        public static BigNumber operator +(BigNumber left, BigInteger right) => new BigNumber(left.value + right);
        public static BigNumber operator +(BigNumber left, BigNumber right) => new BigNumber(left.value + right.value);
        
        public static BigNumber operator -(BigNumber left, int right) => new BigNumber(left.value - right);
        public static BigNumber operator -(BigNumber left, float right) => new BigNumber(left.value - new BigInteger(right));
        public static BigNumber operator -(BigNumber left, double right) => new BigNumber(left.value - new BigInteger(right));
        public static BigNumber operator -(BigNumber left, BigInteger right) => new BigNumber(left.value - right);
        public static BigNumber operator -(BigNumber left, BigNumber right) => new BigNumber(left.value - right.value);
        
        public static BigNumber operator *(BigNumber left, int right) => new BigNumber(left.value * right);
        public static BigNumber operator *(BigNumber left, float right) {
            var result = (left.value * new BigInteger(right * 100)) / 100;
            return new BigNumber(result);
        }
        public static BigNumber operator *(BigNumber left, double right) {
            var result = (left.value * (int)(right * 100)) / 100;
            return new BigNumber(result);
        }
        public static BigNumber operator *(BigNumber left, BigNumber right) => new BigNumber(left.value * right.value);
        
        public static BigNumber operator /(BigNumber left, int right) => new BigNumber(left.value / right);
        public static BigNumber operator /(BigNumber left, float right) {
            var result = (left.value * 100 / new BigInteger(right * 100));
            return new BigNumber(result);
        }
        public static BigNumber operator /(BigNumber left, double right) {
            var result = (left.value * 100 / (int)(right * 100)) ;
            return new BigNumber(result);
        }
        public static BigNumber operator /(BigNumber left, BigNumber right) => new BigNumber(left.value / right.value);

        public static BigNumber Pow(BigNumber value, int pow) {
            return new BigNumber(BigInteger.Pow(value.value, pow));
        }
        
        public int CompareTo(BigNumber other)  => this.value.CompareTo(other.value);
        public bool Equals(BigNumber other) => this.value.Equals(other.value);
        
        public static bool operator <(BigNumber left, BigNumber right) => left.CompareTo(right) < 0;
        public static bool operator <=(BigNumber left, BigNumber right) => left.CompareTo(right) <= 0;
        public static bool operator >(BigNumber left, BigNumber right) => left.CompareTo(right) > 0;
        public static bool operator >=(BigNumber left, BigNumber right) => left.CompareTo(right) >= 0;
        public static bool operator ==(BigNumber left, BigNumber right) => left.Equals(right);
        public static bool operator !=(BigNumber left, BigNumber right) => !left.Equals(right);
        
        public static implicit operator BigNumber(double value) => new BigNumber(value);
        public static implicit operator BigNumber(float value) => new BigNumber(value);
        public static implicit operator BigNumber(int value) => new BigNumber(value);
        public static implicit operator BigNumber(long value) => new BigNumber(value);
        public static implicit operator BigNumber(BigInteger value) => new BigNumber(value);
        
        public static explicit operator float(BigNumber bnValue) => (float)((double)bnValue.value);
        public static explicit operator double(BigNumber bnValue) => (double)bnValue.value;
        public static explicit operator BigInteger(BigNumber bnValue) => bnValue.value;

        public override int GetHashCode() =>  this.value.GetHashCode();
        public override bool Equals(object obj) =>  obj is BigNumber other && this.Equals(other);
    }
}
