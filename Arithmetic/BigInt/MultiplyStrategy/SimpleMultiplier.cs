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
                uint digitsARightHalf = digitsA[i] & BetterBigInteger.RightHalfMask;
                uint digitsALeftHalf = digitsA[i] >> BetterBigInteger.DigitHalfBitsCount;
                uint digitsBRightHalf = digitsB[j] & BetterBigInteger.RightHalfMask;
                uint digitsBLeftHalf = digitsB[j] >> BetterBigInteger.DigitHalfBitsCount;

                var rightAStarRightB = new BetterBigInteger([digitsARightHalf * digitsBRightHalf]);
                var rightAStarLeftB = new BetterBigInteger([digitsARightHalf * digitsBLeftHalf]) << BetterBigInteger.DigitHalfBitsCount;
                var leftAStarRightB = new BetterBigInteger([digitsALeftHalf * digitsBRightHalf]) << BetterBigInteger.DigitHalfBitsCount;
                var leftAStarLeftB = new BetterBigInteger([digitsALeftHalf * digitsBLeftHalf]) << BetterBigInteger.DigitBitsCount;

                var multiply = leftAStarLeftB + leftAStarRightB + rightAStarLeftB + rightAStarRightB;
                multiply <<= (i + j) * BetterBigInteger.DigitBitsCount;
                result += multiply;

            }
        }
        return result;
    }
}