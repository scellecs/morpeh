namespace Morpeh.BigNumber {
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Text.RegularExpressions;
    using UnityEngine;
    using UnityEngine.Assertions;
    
    public static class BigNumberChar {
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
    public struct BigNumber : IComparable<BigNumber>, IEquatable<BigNumber> {
        public override bool Equals(object obj) => obj is BigNumber other && Equals(other);

        public override int GetHashCode() => (this.digits != null ? this.digits.GetHashCode() : 0);

        private const int DIGIT_VALUE = 1000;
        private static readonly List<string> chars = new List<string>
        {
            BigNumberChar.EMPTY,
            BigNumberChar.K,
            BigNumberChar.M,
            BigNumberChar.B,
            BigNumberChar.C,
            BigNumberChar.D,
            BigNumberChar.E,
            BigNumberChar.Aa,
            BigNumberChar.Bb,
            BigNumberChar.Cc,
            BigNumberChar.Dd,
            BigNumberChar.Ee,
            BigNumberChar.AAa,
            BigNumberChar.BBb,
            BigNumberChar.CCc,
            BigNumberChar.DDd,
            BigNumberChar.EEe
        };

        [SerializeField] private List<int> digits;
        public static BigNumber Parse(string value) {
            var re = new Regex(@"([.\d]+)([a-zA-Z]+)");
            var result = re.Match(value);

            if (result.Groups.Count > 1)
            {
                var dcml = Convert.ToDecimal(float.Parse(result.Groups[1].Value, CultureInfo.InvariantCulture));
                var chr = result.Groups[2].Value;

                return new BigNumber(dcml, chr);
            }
            else
            {
                return new BigNumber(Convert.ToDecimal(value, CultureInfo.InvariantCulture));
            }
        }
        public BigNumber(decimal number, string digit = "") {
            var digitIndex = chars.IndexOf(digit);
            Assert.AreNotEqual(digitIndex, -1, $"Unknown character: {number} {digit}");
            this.digits = new List<int>();
            for (var i = 0; i <= digitIndex; ++i)
            {
                this.digits.Add(0);
            }

            var integerPart = (int) number;
            var decimalPart = number - integerPart;
            var decimalValue = (int) (decimalPart * DIGIT_VALUE);

            var integerIndex = digitIndex;
            var decimalIndex = digitIndex - 1;

            this.digits[integerIndex] = integerPart;
            if(decimalIndex >= 0)
                this.digits[decimalIndex] = decimalValue;
        }

        public BigNumber(int value) {
            var digitIndex = 0;
            var tmp = value;
            this.digits = new List<int> {0};
            while (tmp >= DIGIT_VALUE)
            {
                tmp /= DIGIT_VALUE;
                digitIndex++;
                this.digits.Add(0);
            }

            for (var i = digitIndex; i >= 0; --i)
            {
                var divider = IntPow(DIGIT_VALUE, i);
                var decimalDivider = IntPow(DIGIT_VALUE, i + 1);
                this.digits[i] = (value % decimalDivider) / divider;
            }
        }


        public override string ToString() {
            var integerIndex = this.digits.Count - 1;
            var decimalIndex = this.digits.Count - 2;
            if (decimalIndex < 0 || this.digits[decimalIndex] <= 0)
                return $"{this.digits[integerIndex]}{chars[integerIndex]}";
            var decimalPart = this.digits[decimalIndex] / 100;
            return decimalPart > 0 ? $"{this.digits[integerIndex]}.{decimalPart}{chars[integerIndex]}" 
                : $"{this.digits[integerIndex]}{chars[integerIndex]}";
        }
        private BigNumber NormalizeOverflow() {
            var newDigitValue = 0;
            for (var i = 0; i < this.digits.Count; ++i)
            {
                var value = this.digits[i];
                if (value < DIGIT_VALUE) continue;
                
                var nextDigitPart = value / DIGIT_VALUE;
                var thisDigitPart = value % DIGIT_VALUE;
                var nextDigitIndex = i + 1;
                this.digits[i] = thisDigitPart;
                if (nextDigitIndex >= this.digits.Count)
                {
                    newDigitValue += nextDigitPart;
                }
                else
                {
                    this.digits[nextDigitIndex] += nextDigitPart;
                }
            }
            
            if(newDigitValue == 0) return this;
            this.digits.Add(newDigitValue);
            return this;
        }

        private BigNumber NormalizeNegative() {
            for (var i = 0; i < this.digits.Count; ++i)
            {
                var value = this.digits[i];
                if(value >= 0) continue;

                this.digits[i] = 0;
                var nextIndex = i + 1;
                if (nextIndex >= this.digits.Count)
                    continue;
                else
                {
                    var digitValue = DIGIT_VALUE - Math.Abs(value);
                    this.digits[i] = digitValue;
                    this.digits[nextIndex]--;
                }
            }
            
            for (var i = this.digits.Count - 1; i >=0; --i)
            {
                if(this.digits[i] > 0) break;
                this.digits.RemoveAt(i);
            }

            return this;
        }

        public static BigNumber operator +(BigNumber left, int right) => left + new BigNumber(right);
        public static BigNumber operator -(BigNumber left, int right) => left - new BigNumber(right);

        public static BigNumber operator +(BigNumber left, BigNumber right) {
            var (maxNumber, minNumber) = left.digits.Count >= right.digits.Count ? (left, right) : (right, left);
            var result = new BigNumber {digits = new List<int>()};
            result.digits.AddRange(maxNumber.digits);
            for (var i = 0; i < minNumber.digits.Count; ++i)
            {
                result.digits[i] += minNumber.digits[i];
            }
            return result.NormalizeOverflow();
        }

        public static BigNumber operator -(BigNumber left, BigNumber right) {
            if(left <= right) return new BigNumber(0);
            
            var (maxNumber, minNumber) = (left, right);
            var result = new BigNumber {digits = new List<int>()};
            result.digits.AddRange(maxNumber.digits);
            for (var i = 0; i < minNumber.digits.Count; ++i)
            {
                result.digits[i] -= minNumber.digits[i];
            }
            return result.NormalizeNegative();
        }

        public static BigNumber operator *(BigNumber value, int factor) {
            var result = new BigNumber {digits = new List<int>()};
            for (var i = 0; i < value.digits.Count; ++i)
            {
                result.digits.Add(value.digits[i] * factor);
            }
            return result.NormalizeOverflow();
        }

        private static BigNumber OperationHelper(BigNumber value, Func<int, float, float> func, float factor) {
            var result = new BigNumber {digits = new List<int>()};
            var floatResults = new List<float>();
            for (var i = 0; i < value.digits.Count; ++i)
            {
                floatResults.Add(func(value.digits[i], factor));
            }
            for (var i = value.digits.Count - 1; i >= 0; --i)
            {
                var floatValue = floatResults[i];
                var integerValue = (int)floatValue;
                var decimalPart = floatValue - integerValue;
                var decimalValue = (int) (decimalPart * DIGIT_VALUE);

                var prevDigitIndex = i - 1;
                if (prevDigitIndex >= 0)
                {
                    floatResults[prevDigitIndex] += decimalValue;
                }

                floatResults[i] = integerValue;
            }
            for (var i = 0; i < value.digits.Count; ++i)
            {
                result.digits.Add((int)floatResults[i]);
            }

            result.NormalizeNegative();
            return result.NormalizeOverflow();
        }

        public static BigNumber operator *(BigNumber value, float factor) {
            return OperationHelper(value, (i, f) => i * f, factor);
        }
        
        public static BigNumber operator /(BigNumber value, float factor) {
            return OperationHelper(value, (i, f) => i / f, factor);
        }
        
        public static BigNumber operator /(BigNumber value, int factor) {
            return OperationHelper(value, (i, f) => i / f, factor);
        }

        public int CompareTo(BigNumber value) {
            if (this.digits.Count != value.digits.Count) return this.digits.Count.CompareTo(value.digits.Count);
            for (var i = this.digits.Count - 1; i >= 0; i--)
            {
                if (this.digits[i] != value.digits[i]) return this.digits[i].CompareTo(value.digits[i]);
            }
            return 0;
        }
        
        public bool Equals(BigNumber value)
        {
            if (this.digits.Count != value.digits.Count) return false;
            for (var i = this.digits.Count - 1; i >= 0; i--)
            {
                if (this.digits[i] != value.digits[i]) return false;
            }
            return true;
        }
        
        public static bool operator <(BigNumber left, BigNumber right) => left.CompareTo(right) < 0;
        public static bool operator <=(BigNumber left, BigNumber right) => left.CompareTo(right) <= 0;
        public static bool operator >(BigNumber left, BigNumber right) => left.CompareTo(right) > 0;
        public static bool operator >=(BigNumber left, BigNumber right) => left.CompareTo(right) >= 0;
        public static bool operator ==(BigNumber left, BigNumber right) => left.Equals(right);
        public static bool operator !=(BigNumber left, BigNumber right) => !left.Equals(right);
        
        private static int IntPow(int x, int pow)
        {
            int ret = 1;
            while ( pow != 0 )
            {
                if ( (pow & 1) == 1 )
                    ret *= x;
                x *= x;
                pow >>= 1;
            }
            return ret;
        }
    }
}