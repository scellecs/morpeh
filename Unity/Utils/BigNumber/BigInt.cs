using System;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;
using System.Text.RegularExpressions;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Assertions;

namespace Morpeh.BigInt {
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
    public struct BigInt : IComparable<BigInt>, IEquatable<BigInt>  {
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

        [HideLabel]
        [SerializeField]
        private BigInteger value;
        
        public BigInt(decimal number, string digit = "") {
            var digitIndex = symbols.IndexOf(digit);
            this.value = new BigInteger(number);
            for (var i = 0; i < digitIndex; i++)
            {
                this.value *= DIGIT_VALUE;
            }
        }
        public BigInt(float value) => this.value = new BigInteger(value);
        public BigInt(int value) => this.value = new BigInteger(value);
        public BigInt(BigInteger value) => this.value = value;
        
        public static BigInt Parse(string value) {
            var re = new Regex(REGEXP_PATTERN);
            var result = re.Match(value);

            if (result.Groups.Count > 1)
            {
                var dcml = Convert.ToDecimal(float.Parse(result.Groups[1].Value, CultureInfo.InvariantCulture));
                var chr = result.Groups[2].Value;

                return new BigInt(dcml, chr);
            }
             
            return new BigInt(BigInteger.Parse(value));
        }

        public void SetBitInteger(BigInteger value) {
            this.value = value;
        }

        public int CompareTo(BigInt other) {
            return this.value.CompareTo(other.value);
        }

        public bool Equals(BigInt other) {
            return this.value.Equals(other.value);
        }

        public override string ToString() {
            var digit_index = 0;
            var tmp = this.value;
            BigInteger divider = 1;
            BigInteger decimalDivider = DIGIT_VALUE;
            while (tmp >= DIGIT_VALUE)
            {
                tmp /= DIGIT_VALUE;
                digit_index++;
                divider *= DIGIT_VALUE;
                decimalDivider *= DIGIT_VALUE;
            }

            var dcml = (int)(this.value % decimalDivider) / (float)divider;
            var int_decimal = (int) dcml;
            if(dcml == int_decimal)
                return $"{int_decimal}{symbols[digit_index]}";
            else
                return $"{dcml.ToString(FLOAT_FORMAT, CultureInfo.InvariantCulture)}{symbols[digit_index]}";
        }
        public string ToBigIntegerString() => this.value.ToString();

        public static BigInt operator +(BigInt left, int right) => new BigInt(left.value + right);
        public static BigInt operator +(BigInt left, float right) => new BigInt(left.value + new BigInteger(right));
        public static BigInt operator +(BigInt left, BigInt right) => new BigInt(left.value + right.value);
        
        public static BigInt operator -(BigInt left, int right) => new BigInt(left.value - right);
        public static BigInt operator -(BigInt left, float right) => new BigInt(left.value - new BigInteger(right));
        public static BigInt operator -(BigInt left, BigInt right) => new BigInt(left.value - right.value);
        
        public static BigInt operator *(BigInt left, int right) => new BigInt(left.value * right);

        public static BigInt operator *(BigInt left, float right) {
            var result = (left.value * (int)(right * 100)) / 100;
            return new BigInt(result);
        }
        public static BigInt operator *(BigInt left, BigInt right) => new BigInt(left.value * right.value);
        
        public static BigInt operator /(BigInt left, int right) => new BigInt(left.value / right);

        public static BigInt operator /(BigInt left, float right) {
            var result = (left.value / (int)(right * 100)) * 100;
            return new BigInt(result);
        }
        public static BigInt operator /(BigInt left, BigInt right) => new BigInt(left.value / right.value);

        public static BigInt Pow(BigInt value, int pow) {
            return new BigInt(BigInteger.Pow(value.value, pow));
        }
    }
}