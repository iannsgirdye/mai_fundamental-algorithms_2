using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Formats.Asn1;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using Arithmetic.BigInt.Interfaces;
using Arithmetic.BigInt.MultiplyStrategy;

namespace Arithmetic.BigInt;

public sealed class BetterBigInteger : IBigInteger
{
    private int _signBit;

    private uint _smallValue; // Если число маленькое, храним его прямо в этом поле, а _data == null.
    private uint[]? _data;

    public bool IsNegative => _signBit == 1;

    internal bool IsZero => (_data == null && _smallValue == 0) ||
                            (_data != null && _data.Length == 1 && _data[0] == 0);

    public const int SystemBase = sizeof(uint) * 8;

    private const int KaratsubaStart = 32;

    #region Constructors

    /// От массива цифр (little endian)
    public BetterBigInteger(uint[] digits, bool isNegative = false)
    {
        if (digits == null)
            throw new ArgumentNullException(nameof(digits));

        var length = TrimmedLength(digits);
        if (length == 0)
        {
            this._signBit = 0;
            this._smallValue = 0;
            this._data = null;
            return;
        }
        if (length == 1)
        {
            this._signBit = isNegative ? 1 : 0;
            this._smallValue = digits[0];
            this._data = null;
            return;
        }
        this._signBit = isNegative ? 1 : 0;
        this._smallValue = 0;
        this._data = new uint[length];
        Array.Copy(digits, this._data, length);
    }

    public BetterBigInteger(IEnumerable<uint> digits, bool isNegative = false)
        : this(digits?.ToArray() ?? throw new ArgumentNullException(nameof(digits)), isNegative)
    {
    }

    public BetterBigInteger(string value, int radix)
    {
        if (string.IsNullOrEmpty(value))
            throw new ArgumentException("Value cannot be null or empty", nameof(value));
        if (radix < 2 || radix > 36)
            throw new ArgumentException("Radix must be between 2 and 36", nameof(radix));

        bool negative = value[0] == '-' ? true : false;
        int start = value[0] == '-' || value[0] == '+' ? 1 : 0;

        if (start == value.Length)
            throw new FormatException("Invalid number string");

        var resultDigits = new List<uint> { 0 };
        for (int i = start; i < value.Length; i++)
        {
            int digit = CharToDigit(value[i], radix);
            if (digit < 0)
                throw new FormatException($"Invalid character '{value[i]}' for radix {radix}");
            MultiplyByInt(resultDigits, radix);
            AddInt(resultDigits, digit);
        }

        var arr = resultDigits.ToArray();
        var temp = new BetterBigInteger(arr, negative);
        this._signBit = temp._signBit;
        this._smallValue = temp._smallValue;
        this._data = temp._data;
    }

    internal static int TrimmedLength(uint[] arr)
    {
        int length = arr.Length;
        while (length > 0 && arr[length - 1] == 0)
            length--;
        return length;
    }

    internal static uint[] TrimLeadingZeros(uint[] arr)
    {
        var length = TrimmedLength(arr);
        if (length == 0)
            return new uint[] { 0 };
        if (length == arr.Length)
            return arr;
        var result = new uint[length];
        Array.Copy(arr, result, length);
        return result;
    }

    #endregion

    #region Constructors` utilities

    private static int CharToDigit(char c, int radix)
    {
        if (c >= '0' && c <= '9')
            return c - '0';
        if (c >= 'A' && c <= 'Z')
            return c - 'A' + 10;
        if (c >= 'a' && c <= 'z')
            return c - 'a' + 10;
        return -1;
    }

    private static void MultiplyByInt(List<uint> digits, int multiplier)
    {
        CalculatebyInt(digits, multiplier, 0);
    }

    private static void AddInt(List<uint> digits, int added)
    {
        CalculatebyInt(digits, 1, added);
    }

    private static void CalculatebyInt(List<uint> digits, int multiplier, int startCarry)
    {
        ulong carry = (ulong)startCarry;
        for (int i = 0; i < digits.Count; i++)
        {
            ulong result = (ulong)digits[i] * (ulong)multiplier + carry;
            digits[i] = (uint)result;
            carry = result >> SystemBase;
        }
        while (carry > 0)
        {
            digits.Add((uint)carry);
            carry >>= SystemBase;
        }
    }

    #endregion

    public ReadOnlySpan<uint> GetDigits() => this._data ?? [this._smallValue];

    #region Comparison functions 

    public int CompareTo(IBigInteger? other)
    {
        if (other is null) return 1;
        if (ReferenceEquals(this, other)) return 0;

        if (this.IsNegative && !other.IsNegative) return -1;
        if (!this.IsNegative && other.IsNegative) return 1;

        int sign = this.IsNegative ? -1 : 1;
        return sign * CompareMagnitude(this, other);
    }

    public bool Equals(IBigInteger? other) => this.CompareTo(other) == 0;

    public override bool Equals(object? obj) => obj is IBigInteger other && Equals(other);

    private static int CompareMagnitude(IBigInteger a, IBigInteger b)
    {
        var digitsA = a.GetDigits();
        var digitsB = b.GetDigits();

        if (digitsA.Length != digitsB.Length)
            return digitsA.Length.CompareTo(digitsB.Length);

        for (int i = digitsA.Length - 1; i >= 0; i--)
        {
            if (digitsA[i] != digitsB[i])
                return digitsA[i].CompareTo(digitsB[i]);
        }
        return 0;
    }

    #endregion

    public override int GetHashCode()
    {
        HashCode hash = new();
        hash.Add(this._signBit);
        if (this._data is null)
            hash.Add(this._smallValue);
        else
        {
            foreach (var digit in this._data)
                hash.Add(digit);
        }
        return hash.ToHashCode();
    }

    #region Comparison operations

    public static bool operator ==(BetterBigInteger a, BetterBigInteger b) => Equals(a, b);
    public static bool operator !=(BetterBigInteger a, BetterBigInteger b) => !Equals(a, b);
    public static bool operator <(BetterBigInteger a, BetterBigInteger b) => a.CompareTo(b) < 0;
    public static bool operator >(BetterBigInteger a, BetterBigInteger b) => a.CompareTo(b) > 0;
    public static bool operator <=(BetterBigInteger a, BetterBigInteger b) => a.CompareTo(b) <= 0;
    public static bool operator >=(BetterBigInteger a, BetterBigInteger b) => a.CompareTo(b) >= 0;

    #endregion

    #region Arithmetic operations
    public static BetterBigInteger operator +(BetterBigInteger a, BetterBigInteger b)
    {
        if (a.IsNegative == b.IsNegative)
        {
            var sum = AddMagnitude(a, b);
            return new BetterBigInteger(sum, a.IsNegative);
        }
        else
        {
            if (a == b) 
                return new BetterBigInteger("0", 10);
            if (a > b)
            {
                var diff = SubtractMagnitude(a, b);
                return new BetterBigInteger(diff, a.IsNegative);
            }
            else
            {
                var diff = SubtractMagnitude(b, a);
                return new BetterBigInteger(diff, b.IsNegative);
            }
        }
    }

    public static BetterBigInteger operator -(BetterBigInteger a, BetterBigInteger b) => a + (-b);

    public static BetterBigInteger operator -(BetterBigInteger a) => new(a.GetDigits().ToArray(), !a.IsNegative);

    public static BetterBigInteger operator /(BetterBigInteger a, BetterBigInteger b)
    {
        if (b.IsZero) throw new DivideByZeroException();
        var (quotient, _) = DivideMagnitude(a, b);
        bool neg = a.IsNegative ^ b.IsNegative;
        return new BetterBigInteger(quotient, neg);
    }

    public static BetterBigInteger operator %(BetterBigInteger a, BetterBigInteger b)
    {
        if (b.IsZero) throw new DivideByZeroException();
        var (_, remainder) = DivideMagnitude(a, b);
        bool neg = a.IsNegative;
        return new BetterBigInteger(remainder, neg);
    }

    public static BetterBigInteger operator *(BetterBigInteger a, BetterBigInteger b)
    {
        if (a.IsZero || b.IsZero)
            return new ("0", 10);
        
        var maxLength = Math.Max(a.GetDigits().Length, b.GetDigits().Length);
        if (maxLength > KaratsubaStart)
            return new KaratsubaMultiplier().Multiply(a, b);
        return new SimpleMultiplier().Multiply(a, b);
    }

    #endregion

    #region Magnitude calculations

    private static uint[] AddMagnitude(BetterBigInteger a, BetterBigInteger b)
    {
        var digitsA = a.GetDigits();
        var digitsB = b.GetDigits();
        int maxLen = Math.Max(digitsA.Length, digitsB.Length);
        var result = new uint[maxLen + 1];
        ulong carry = 0;
        for (int i = 0; i < maxLen; i++)
        {
            ulong digitA = i < digitsA.Length ? digitsA[i] : 0;
            ulong digitB = i < digitsB.Length ? digitsB[i] : 0;
            ulong digitSum = digitA + digitB + carry;
            result[i] = (uint)digitSum;
            carry = digitSum >> SystemBase;
        }
        result[maxLen] = (uint)carry;

        return TrimLeadingZeros(result);
    }

    private static uint[] SubtractMagnitude(BetterBigInteger bigger, BetterBigInteger smaller)
    {
        var digitsBigger = bigger.GetDigits();
        var digitsSmaller = smaller.GetDigits();
        int maxLen = Math.Max(digitsBigger.Length, digitsSmaller.Length);
        var result = new uint[maxLen];
        long borrow = 0;
        for (int i = 0; i < maxLen; i++)
        {
            long digitBigger = i < digitsBigger.Length ? digitsBigger[i] : 0;
            long digitSmaller = i < digitsSmaller.Length ? digitsSmaller[i] : 0;
            long digitDiff = digitBigger - digitSmaller - borrow;
            if (digitDiff < 0)
            {
                digitDiff += 1L << SystemBase;
                borrow = 1;
            }
            else
                borrow = 0;
            result[i] = (uint)digitDiff;
        }
        return TrimLeadingZeros(result);
    }

    private static (uint[] quotient, uint[] remainder) DivideMagnitude(BetterBigInteger dividend, BetterBigInteger divisor)
    {
        if (CompareMagnitude(dividend, divisor) < 0)
            return (new uint[] { 0 }, dividend.GetDigits().ToArray());
        if (CompareMagnitude(dividend, divisor) == 0)
            return (new uint[] { 1 }, new uint[] { 0 });

        var dividendDigits = dividend.GetDigits().ToArray();
        var divisorDigits = divisor.GetDigits().ToArray();

        var quotientDigits = new uint[dividendDigits.Length - divisorDigits.Length + 1];

        var remainderDigits = dividendDigits;

        int shift = remainderDigits.Length - divisorDigits.Length;

        for (int i = shift; i >= 0; i--)
        {
            ulong remainderHigh = (ulong)(i < remainderDigits.Length ? remainderDigits[i] : 0);
            if (i + 1 < remainderDigits.Length)
                remainderHigh |= (ulong)remainderDigits[i + 1] << SystemBase;

            ulong divisorHigh = divisorDigits[divisorDigits.Length - 1];
            ulong guess = remainderHigh / divisorHigh;
            if (guess >= 1UL << SystemBase)
                guess = 0xFFFFFFFF;

            uint[] temp = new uint[divisorDigits.Length + 1];
            ulong carry = 0;
            for (int j = 0; j < divisorDigits.Length; j++)
            {
                ulong product = (ulong)divisorDigits[j] * guess + carry;
                temp[j] = (uint)product;
                carry = product >> SystemBase;
            }
            temp[divisorDigits.Length] = (uint)carry;

            long borrow = 0;
            for (int j = 0; j < temp.Length; j++)
            {
                long diff = (long)(j + i < remainderDigits.Length ? remainderDigits[j + i] : 0)
                            - temp[j] - borrow;
                if (diff < 0)
                {
                    diff += 1L << SystemBase;
                    borrow = 1;
                }
                else
                {
                    borrow = 0;
                }
                if (j + i < remainderDigits.Length)
                    remainderDigits[j + i] = (uint)diff;
            }

            if (borrow != 0)
            {
                guess--;
                carry = 0;
                for (int j = 0; j < divisorDigits.Length; j++)
                {
                    ulong sum = (ulong)(j + i < remainderDigits.Length ? remainderDigits[j + i] : 0)
                                + divisorDigits[j] + carry;
                    if (j + i < remainderDigits.Length)
                        remainderDigits[j + i] = (uint)sum;
                    carry = sum >> SystemBase;
                }
            }

            quotientDigits[i] = (uint)guess;
        }

        var quotientTrimmed = TrimLeadingZeros(quotientDigits);
        var remainderTrimmed = TrimLeadingZeros(remainderDigits);

        return (quotientTrimmed, remainderTrimmed);
    }

    #endregion

    #region Bitwise operations
    public static BetterBigInteger operator ~(BetterBigInteger a)
    {
        int len = a.GetDigits().Length + 2;
        uint[] twos = ToTwosComplement(a, len);
        uint[] result = new uint[len];
        for (int i = 0; i < len; i++)
            result[i] = ~twos[i];
        var (digits, neg) = FromTwosComplement(result);
        return new BetterBigInteger(digits, neg);
    }

    public static BetterBigInteger operator &(BetterBigInteger a, BetterBigInteger b)
    {
        int len = Math.Max(a.GetDigits().Length + 2, b.GetDigits().Length + 2);
        uint[] twosA = ToTwosComplement(a, len);
        uint[] twosB = ToTwosComplement(b, len);
        uint[] result = new uint[len];
        for (int i = 0; i < len; i++)
            result[i] = twosA[i] & twosB[i];
        var (digits, neg) = FromTwosComplement(result);
        return new BetterBigInteger(digits, neg);
    }

    public static BetterBigInteger operator |(BetterBigInteger a, BetterBigInteger b)
    {
        int len = Math.Max(a.GetDigits().Length + 2, b.GetDigits().Length + 2);
        uint[] twosA = ToTwosComplement(a, len);
        uint[] twosB = ToTwosComplement(b, len);
        uint[] result = new uint[len];
        for (int i = 0; i < len; i++)
            result[i] = twosA[i] | twosB[i];
        var (digits, neg) = FromTwosComplement(result);
        return new BetterBigInteger(digits, neg);
    }

    public static BetterBigInteger operator ^(BetterBigInteger a, BetterBigInteger b)
    {
        int len = Math.Max(a.GetDigits().Length + 2, b.GetDigits().Length + 2);
        uint[] twosA = ToTwosComplement(a, len);
        uint[] twosB = ToTwosComplement(b, len);
        uint[] result = new uint[len];
        for (int i = 0; i < len; i++)
            result[i] = twosA[i] ^ twosB[i];
        var (digits, neg) = FromTwosComplement(result);
        return new BetterBigInteger(digits, neg);
    }

    public static BetterBigInteger operator <<(BetterBigInteger a, int shift)
    {
        if (shift < 0)
            throw new ArgumentOutOfRangeException(nameof(shift), "Shift must be non-negative");
        if (shift == 0)
            return new BetterBigInteger(a.GetDigits().ToArray(), a.IsNegative);
        if (a.IsZero)
            return new BetterBigInteger("0", 10);

        var digits = a.GetDigits().ToArray();
        var shifted = ShiftLeftUnsigned(digits, shift);
        bool negative = a.IsNegative && !AllZero(shifted);
        return new BetterBigInteger(shifted, negative);
    }

    public static BetterBigInteger operator >>(BetterBigInteger a, int shift)
    {
        if (shift < 0)
            throw new ArgumentOutOfRangeException(nameof(shift), "Shift must be non-negative");
        if (shift == 0) return new BetterBigInteger(a.GetDigits().ToArray(), a.IsNegative);
        if (a.IsZero) return new BetterBigInteger("0", 10);

        int len = a.GetDigits().Length + 2;
        uint[] twos = ToTwosComplement(a, len);
        uint[] shifted = ArithmeticShiftRight(twos, shift);
        var (digits, neg) = FromTwosComplement(shifted);
        return new BetterBigInteger(digits, neg);
    }

    private static uint[] ToTwosComplement(BetterBigInteger x, int uintLength)
    {
        if (uintLength <= 0)
            throw new ArgumentException("Length must be positive", nameof(uintLength));

        var digits = x.GetDigits();
        uint[] result = new uint[uintLength];

        if (x.IsNegative)
        {
            int copyLen = Math.Min(digits.Length, uintLength);
            for (int i = 0; i < copyLen; i++)
                result[i] = ~digits[i];
            for (int i = digits.Length; i < uintLength; i++)
                result[i] = 0xFFFFFFFF;

            ulong carry = 1;
            for (int i = 0; i < uintLength && carry > 0; i++)
            {
                ulong sum = (ulong)result[i] + carry;
                result[i] = (uint)sum;
                carry = sum >> SystemBase;
            }
        }
        else
        {
            int copyLen = Math.Min(digits.Length, uintLength);
            for (int i = 0; i < copyLen; i++)
                result[i] = digits[i];
        }
        return result;
    }

    private static (uint[] digits, bool isNegative) FromTwosComplement(uint[] twos)
    {
        if (twos.Length == 0)
            return (new uint[] { 0 }, false);

        bool negative = (twos[twos.Length - 1] & 0x80000000) != 0;
        if (!negative)
        {
            var length = TrimmedLength(twos);
            if (length == 0)
                return (new uint[] { 0 }, false);
            uint[] digits = new uint[length];
            Array.Copy(twos, digits, length);
            return (digits, false);
        }
        else
        {
            uint[] inverted = new uint[twos.Length];
            for (int i = 0; i < twos.Length; i++)
                inverted[i] = ~twos[i];

            ulong carry = 1;
            for (int i = 0; i < inverted.Length && carry > 0; i++)
            {
                ulong sum = (ulong)inverted[i] + carry;
                inverted[i] = (uint)sum;
                carry = sum >> SystemBase;
            }

            var length = TrimmedLength(inverted);
            if (length == 0)
                return (new uint[] { 0 }, false);

            uint[] digits = new uint[length];
            Array.Copy(inverted, digits, length);
            return (digits, true);
        }
    }

    private static uint[] ArithmeticShiftRight(uint[] twos, int shift)
    {
        if (shift == 0)
            return (uint[])twos.Clone();

        int wordShift = shift / SystemBase;
        int bitShift = shift % SystemBase;
        bool negative = (twos[twos.Length - 1] & 0x80000000) != 0;
        uint signFill = negative ? 0xFFFFFFFF : 0;

        int newLen = twos.Length - wordShift;
        if (newLen <= 0)
        {
            if (negative)
                return new uint[] { 0xFFFFFFFF };
            else
                return new uint[] { 0 };
        }
        uint[] result = new uint[newLen];
        if (bitShift == 0)
        {
            for (int i = 0; i < newLen; i++)
                result[i] = twos[i + wordShift];
        }
        else
        {
            for (int i = 0; i < newLen; i++)
            {
                uint low = twos[i + wordShift];
                uint high = (i + wordShift + 1 < twos.Length) ? twos[i + wordShift + 1] : signFill;
                result[i] = (low >> bitShift) | (high << (SystemBase - bitShift));
            }
        }
        return result;
    }

    private static uint[] ShiftLeftUnsigned(uint[] digits, int shift)
    {
        if (shift == 0)
            return (uint[])digits.Clone();
        int wordShift = shift / SystemBase;
        int bitShift = shift % SystemBase;
        int newLen = digits.Length + wordShift + (bitShift > 0 ? 1 : 0);
        uint[] result = new uint[newLen];
        if (bitShift == 0)
        {
            for (int i = 0; i < digits.Length; i++)
                result[i + wordShift] = digits[i];
        }
        else
        {
            uint carry = 0;
            for (int i = 0; i < digits.Length; i++)
            {
                ulong val = ((ulong)digits[i] << bitShift) | carry;
                result[i + wordShift] = (uint)val;
                carry = (uint)(val >> SystemBase);
            }
            if (carry != 0)
                result[digits.Length + wordShift] = carry;
        }
        return TrimLeadingZeros(result);
    }

    #endregion

    #region String operations

    public override string ToString() => ToString(10);

    public string ToString(int radix)
    {
        if (radix < 2 || radix > 36)
            throw new ArgumentException("Radix must be between 2 and 36", nameof(radix));

        if (IsZero)
            return "0";

        var digits = GetDigits().ToArray();
        bool negative = IsNegative;

        var chars = new List<char>();

        while (true)
        {
            if (digits.Length == 0)
                break;

            if (digits.Length == 1 && digits[0] == 0)
                break;

            int remainder = 0;
            // ulong carry = 0;

            for (int i = digits.Length - 1; i >= 0; i--)
            {
                ulong value = ((ulong)remainder << SystemBase) | digits[i];
                uint quotient = (uint)(value / (ulong)radix);
                remainder = (int)(value % (ulong)radix);
                digits[i] = quotient;
            }

            var length = TrimmedLength(digits);
            if (length == 0)
            {
                char digitChar = remainder < 10 ? (char)('0' + remainder) : (char)('a' + remainder - 10);
                chars.Add(digitChar);
                break;
            }
            else
            {
                char digitChar = remainder < 10 ? (char)('0' + remainder) : (char)('a' + remainder - 10);
                chars.Add(digitChar);
                Array.Resize(ref digits, length);
            }
        }

        chars.Reverse();
        string result = new string(chars.ToArray());
        return negative ? "-" + result : result;
    }

    #endregion

    private static bool AllZero(uint[] arr)
    {
        foreach (var v in arr)
            if (v != 0) return false;
        return true;
    }
}