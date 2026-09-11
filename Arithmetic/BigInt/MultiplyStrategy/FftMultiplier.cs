using Arithmetic.BigInt.Interfaces;
using System.Numerics;

namespace Arithmetic.BigInt.MultiplyStrategy;

internal class FftMultiplier : IMultiplier
{
    private static readonly int[] Mods = { 998244353, 1004535809, 469762049 };
    private const int PrimitiveRoot = 3;

    public BetterBigInteger Multiply(BetterBigInteger left, BetterBigInteger right)
    {
        var leftDigits = left.GetDigits();
        var rightDigits = right.GetDigits();

        int convolutionLength = leftDigits.Length + rightDigits.Length - 1;

        int transformSize = 1;
        while (transformSize < convolutionLength)
            transformSize <<= 1;

        int[,] remainders = new int[Mods.Length, transformSize];

        for (int modulusIndex = 0; modulusIndex < Mods.Length; modulusIndex++)
        {
            int modulus = Mods[modulusIndex];

            int[] transformedLeft = new int[transformSize];
            int[] transformedRight = new int[transformSize];

            for (int i = 0; i < leftDigits.Length; i++)
                transformedLeft[i] = (int)(leftDigits[i] % modulus);
            for (int i = 0; i < rightDigits.Length; i++)
                transformedRight[i] = (int)(rightDigits[i] % modulus);

            NumberTheoreticTransform(transformedLeft, inverse: false, modulus);
            NumberTheoreticTransform(transformedRight, inverse: false, modulus);

            for (int i = 0; i < transformSize; i++)
                transformedLeft[i] = (int)((long)transformedLeft[i] * transformedRight[i] % modulus);

            NumberTheoreticTransform(transformedLeft, inverse: true, modulus);

            for (int i = 0; i < transformSize; i++)
                remainders[modulusIndex, i] = transformedLeft[i];
        }

        BigInteger[] convolutionCoefficients = new BigInteger[convolutionLength];
        BigInteger modulus1 = Mods[0];
        BigInteger modulus2 = Mods[1];
        BigInteger modulus3 = Mods[2];
        BigInteger productMods12 = modulus1 * modulus2;

        for (int i = 0; i < convolutionLength; i++)
        {
            long remainder1 = remainders[0, i];
            long remainder2 = remainders[1, i];
            long remainder3 = remainders[2, i];

            long difference12 = (remainder2 - remainder1) % (long)modulus2;
            if (difference12 < 0) difference12 += (long)modulus2;
            long inverseModulus1Mod2 = ModInverse((long)(modulus1 % modulus2), (int)modulus2);
            long t1 = difference12 * inverseModulus1Mod2 % (long)modulus2;

            BigInteger x12 = remainder1 + modulus1 * t1;

            long difference23 = (remainder3 - (long)(x12 % modulus3)) % (long)modulus3;
            if (difference23 < 0) difference23 += (long)modulus3;
            long productMods12Mod3 = (long)((modulus1 % modulus3) * (modulus2 % modulus3) % modulus3);
            long inverseProductMods12Mod3 = ModInverse(productMods12Mod3, (int)modulus3);
            long t2 = difference23 * inverseProductMods12Mod3 % (long)modulus3;

            convolutionCoefficients[i] = x12 + productMods12 * t2;
        }

        var resultDigits = new List<uint>();
        BigInteger carry = 0;
        for (int i = 0; i < convolutionCoefficients.Length; i++)
        {
            BigInteger currentValue = convolutionCoefficients[i] + carry;
            resultDigits.Add((uint)(currentValue & 0xFFFFFFFF));
            carry = currentValue >> 32;
        }
        while (carry > 0)
        {
            resultDigits.Add((uint)(carry & 0xFFFFFFFF));
            carry >>= 32;
        }

        int trim = resultDigits.Count;
        while (trim > 0 && resultDigits[trim - 1] == 0)
            trim--;
        if (trim == 0)
            return new BetterBigInteger([0], false);
        resultDigits.RemoveRange(trim, resultDigits.Count - trim);

        bool negative = left.IsNegative ^ right.IsNegative;
        return new BetterBigInteger(resultDigits.ToArray(), negative);
    }

    private void NumberTheoreticTransform(int[] array, bool inverse, int modulus)
    {
        int length = array.Length;

        for (int i = 1, j = 0; i < length; i++)
        {
            int bit = length >> 1;
            for (; (j & bit) != 0; bit >>= 1)
                j ^= bit;
            j ^= bit;
            if (i < j)
                (array[i], array[j]) = (array[j], array[i]);
        }

        for (int blockLength = 2; blockLength <= length; blockLength <<= 1)
        {
            int rootStep = PowMod(PrimitiveRoot, (modulus - 1) / blockLength, modulus);
            if (inverse)
                rootStep = ModInverse(rootStep, modulus);

            for (int start = 0; start < length; start += blockLength)
            {
                long currentRoot = 1;
                int halfBlock = blockLength >> 1;

                for (int offset = 0; offset < halfBlock; offset++)
                {
                    int leftValue = array[start + offset];
                    int rightValue = (int)(array[start + offset + halfBlock] * currentRoot % modulus);

                    int sum = leftValue + rightValue;
                    if (sum >= modulus) sum -= modulus;

                    int difference = leftValue - rightValue;
                    if (difference < 0) difference += modulus;

                    array[start + offset] = sum;
                    array[start + offset + halfBlock] = difference;

                    currentRoot = currentRoot * rootStep % modulus;
                }
            }
        }

        if (inverse)
        {
            int inverseOfLength = ModInverse(length, modulus);
            for (int i = 0; i < length; i++)
                array[i] = (int)((long)array[i] * inverseOfLength % modulus);
        }
    }

    private int PowMod(long baseValue, long exponent, int modulus)
    {
        long result = 1;
        while (exponent > 0)
        {
            if ((exponent & 1) == 1)
                result = result * baseValue % modulus;
            baseValue = baseValue * baseValue % modulus;
            exponent >>= 1;
        }
        return (int)result;
    }

    private int ModInverse(long value, int modulus)
    {
        return PowMod(value, modulus - 2, modulus);
    }
}