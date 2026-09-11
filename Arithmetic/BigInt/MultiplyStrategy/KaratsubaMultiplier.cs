using Arithmetic.BigInt.Interfaces;

namespace Arithmetic.BigInt.MultiplyStrategy;

internal class KaratsubaMultiplier : IMultiplier
{
    public BetterBigInteger Multiply(BetterBigInteger a, BetterBigInteger b)
    {
        bool negative = a.IsNegative ^ b.IsNegative;
        var result = Karatsuba(a, b);
        return new BetterBigInteger(result.GetDigits().ToArray(), negative);
    }

    private BetterBigInteger Karatsuba(BetterBigInteger a, BetterBigInteger b)
    {
        var digitsA = a.GetDigits();
        var digitsB = b.GetDigits();

        if (digitsA.Length == 1 || digitsB.Length == 1)
            return new SimpleMultiplier().Multiply(a, b);

        int maxLen = Math.Max(digitsA.Length, digitsB.Length);
        if (maxLen % 2 != 0) maxLen++;

        if (digitsA.Length < maxLen)
        {
            var newA = new uint[maxLen];
            digitsA.CopyTo(newA);
            a = new BetterBigInteger(newA);
            digitsA = a.GetDigits();
        }
        if (digitsB.Length < maxLen)
        {
            var newB = new uint[maxLen];
            digitsB.CopyTo(newB);
            b = new BetterBigInteger(newB);
            digitsB = b.GetDigits();
        }

        int lengthHalf = maxLen / 2;

        // [Left][Right], where Left > Right
        var digitsARight = digitsA.Slice(0, lengthHalf).ToArray();
        var digitsALeft = digitsA.Slice(lengthHalf, lengthHalf).ToArray();
        var ARight = new BetterBigInteger(digitsARight);
        var ALeft = new BetterBigInteger(digitsALeft);

        var digitsBRight = digitsB.Slice(0, lengthHalf).ToArray();
        var digitsBLeft = digitsB.Slice(lengthHalf, lengthHalf).ToArray();
        var BRight = new BetterBigInteger(digitsBRight);
        var BLeft = new BetterBigInteger(digitsBLeft);

        var ARightStarBRight = Karatsuba(ARight, BRight);
        var ALeftStarBLeft = Karatsuba(ALeft, BLeft);

        var sumA = ARight + ALeft;
        var sumB = BRight + BLeft;
        var ARightStarBLeftPlusALeftStarBRight = Karatsuba(sumA, sumB) - ARightStarBRight - ALeftStarBLeft;

        var result = ShiftWords(ALeftStarBLeft, 2 * lengthHalf) +
                     ShiftWords(ARightStarBLeftPlusALeftStarBRight, lengthHalf) +
                     ARightStarBRight;
        return result;
    }

    private BetterBigInteger ShiftWords(BetterBigInteger value, int words)
    {
        if (words == 0 || value.IsZero)
            return value;
        var digits = value.GetDigits().ToArray();
        var result = new uint[digits.Length + words];
        Array.Copy(digits, 0, result, words, digits.Length);
        return new BetterBigInteger(result, false);
    }
}