using System.Drawing;
using System.Runtime.InteropServices.Marshalling;
using Arithmetic.BigInt.Interfaces;

namespace Arithmetic.BigInt.MultiplyStrategy;

internal class SimpleMultiplier : IMultiplier
{
    private const int DigitBitsCount = sizeof(uint) * 8;
    private const int DigitHalfBitsCount = DigitBitsCount / 2;
    private const uint RightHalfMask = (1 << DigitHalfBitsCount) - 1;

    public BetterBigInteger Multiply(BetterBigInteger a, BetterBigInteger b)
    {
        var digitsA = a.GetDigits();
        var digitsB = b.GetDigits();
        var result = new BetterBigInteger([0], a.IsNegative ^ b.IsNegative);
        for (int i = 0; i < digitsA.Length; i++)
        {
            for (int j = 0; j < digitsB.Length; j++)
            {
                uint digitsARightHalf = digitsA[i] & RightHalfMask;
                uint digitsALeftHalf = digitsA[i] >> DigitHalfBitsCount;
                uint digitsBRightHalf = digitsB[j] & RightHalfMask;
                uint digitsBLeftHalf = digitsB[j] >> DigitHalfBitsCount;

                var rightAStarRightB = new BetterBigInteger([digitsARightHalf * digitsBRightHalf]);
                var rightAStarLeftB = new BetterBigInteger([digitsARightHalf * digitsBLeftHalf]) << DigitHalfBitsCount;
                var leftAStarRightB = new BetterBigInteger([digitsALeftHalf * digitsBRightHalf]) << DigitHalfBitsCount;
                var leftAStarLeftB = new BetterBigInteger([digitsALeftHalf * digitsBLeftHalf]) << DigitBitsCount;

                var multiply = leftAStarLeftB + leftAStarRightB + rightAStarLeftB + rightAStarRightB;
                multiply <<= (i + j) * DigitBitsCount;
                result += multiply;

            }
        }
        return result;
    }
}