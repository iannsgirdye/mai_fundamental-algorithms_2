using System.Drawing;
using Arithmetic.BigInt.Interfaces;

namespace Arithmetic.BigInt.MultiplyStrategy;

internal class SimpleMultiplier : IMultiplier
{
    public BetterBigInteger Multiply(BetterBigInteger a, BetterBigInteger b)
    {
        var digitsA = a.GetDigits();
        var digitsB = b.GetDigits();
        var result = new uint[digitsA.Length + digitsB.Length];
        for (int i = 0; i < digitsB.Length; i++)
        {
            ulong carry = 0;
            for (int j = 0; j < digitsA.Length; j++)
            {
                ulong product = (ulong)digitsA[i] * (ulong)digitsB[j] + (ulong)result[i + j] + carry;
                result[i + j] = (uint)product;
                carry = product >> BetterBigInteger.SystemBase;
            }
            for (int k = i + digitsA.Length; carry > 0; k++)
            {
                ulong sum = (ulong)result[k] + carry;
                result[k] = (uint)sum;
                carry = sum >> BetterBigInteger.SystemBase;
            }
        }
        var trimmedResult = BetterBigInteger.TrimLeadingZeros(result);
        bool negative = a.IsNegative ^ b.IsNegative;
        return new BetterBigInteger(trimmedResult, negative);
    }
}