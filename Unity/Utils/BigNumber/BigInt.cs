using System;
using System.Numerics;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Morpeh.BigNumber {
    [Serializable]
    public struct BigInt {
        
#if UNITY_EDITOR && ODIN_INSPECTOR
       // [ReadOnly]
        //public string stringValue;
#endif
        
        [HideLabel]
        public BigInteger value;

        public BigInt(int value) {
            this.value = value;
#if UNITY_EDITOR && ODIN_INSPECTOR
            //this.stringValue = value.ToString();
#endif
        }
        
        public BigInt(BigInteger value) {
            this.value = value;
#if UNITY_EDITOR && ODIN_INSPECTOR
            //this.stringValue = value.ToString();
#endif
        }

        public override string ToString() => this.value.ToString();

        public string ToBigIntegerString() => this.value.ToString();

        public static BigInt Parse(string value) {
            return new BigInt(BigInteger.Parse(value));
        }

        public static BigInt operator +(BigInt left, int right) => new BigInt(left.value + right);
        public static BigInt operator +(BigInt left, BigInt right) => new BigInt(left.value + right.value);
    }
}