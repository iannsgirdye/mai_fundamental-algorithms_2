using System.Drawing;
using System.Runtime.InteropServices.Marshalling;
using Arithmetic.BigInt.Interfaces;

namespace Arithmetic.BigInt.MultiplyStrategy;

internal class SimpleMultiplier : IMultiplier
{
    public BetterBigInteger Multiply(BetterBigInteger a, BetterBigInteger b)
    {
        var digitsA = a.GetDigits();
        var digitsB = b.GetDigits();
        var result = new BetterBigInteger([0], a.IsNegative ^ b.IsNegative);
        for (int i = 0; i < digitsA.Length; i++)
        {
            for (int j = 0; j < digitsB.Length; j++)
            {
                uint digitARightHalf = digitsA[i] & BetterBigInteger.RightHalfMask;
                uint digitALeftHalf = digitsA[i] >> BetterBigInteger.DigitHalfBitsCount;
                uint digitBRightHalf = digitsB[j] & BetterBigInteger.RightHalfMask;
                uint digitBLeftHalf = digitsB[j] >> BetterBigInteger.DigitHalfBitsCount;

                var rightAStarRightB = new BetterBigInteger([digitARightHalf * digitBRightHalf]);
                var rightAStarLeftB = new BetterBigInteger([digitARightHalf * digitBLeftHalf]) << BetterBigInteger.DigitHalfBitsCount;
                var leftAStarRightB = new BetterBigInteger([digitALeftHalf * digitBRightHalf]) << BetterBigInteger.DigitHalfBitsCount;
                var leftAStarLeftB = new BetterBigInteger([digitALeftHalf * digitBLeftHalf]) << BetterBigInteger.DigitBitsCount;

                var multiply = leftAStarLeftB + leftAStarRightB + rightAStarLeftB + rightAStarRightB;
                multiply <<= (i + j) * BetterBigInteger.DigitBitsCount;
                result += multiply;

            }
        }
        return result;
    }
}