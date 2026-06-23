using System.Numerics;
using Arithmetic.BigInt.Interfaces;

namespace Arithmetic.BigInt.MultiplyStrategy;

internal class FftMultiplier : IMultiplier
{
    private static readonly int[] Mods = { 998244353, 1004535809, 469762049 };
    private const int PrimitiveRoot = 3;

    public BetterBigInteger Multiply(BetterBigInteger a, BetterBigInteger b)
    {
        if (a.IsZero || b.IsZero)
            return new BetterBigInteger([0], false);

        var aDigits = a.GetDigits();
        var bDigits = b.GetDigits();

        int totalLen = aDigits.Length + bDigits.Length - 1;
        int n = 1;
        while (n < totalLen) n <<= 1;

        int[,] residues = new int[Mods.Length, n];
        for (int idx = 0; idx < Mods.Length; idx++)
        {
            int mod = Mods[idx];
            int[] fa = new int[n];
            int[] fb = new int[n];

            for (int i = 0; i < aDigits.Length; i++) fa[i] = (int)(aDigits[i] % mod);
            for (int i = 0; i < bDigits.Length; i++) fb[i] = (int)(bDigits[i] % mod);

            Ntt(fa, false, mod);
            Ntt(fb, false, mod);

            for (int i = 0; i < n; i++)
                fa[i] = (int)((long)fa[i] * fb[i] % mod);

            Ntt(fa, true, mod);

            for (int i = 0; i < n; i++)
                residues[idx, i] = fa[i];
        }

        BigInteger[] coeffs = new BigInteger[totalLen];
        BigInteger m1 = Mods[0], m2 = Mods[1], m3 = Mods[2];
        BigInteger m1m2 = m1 * m2;

        for (int i = 0; i < totalLen; i++)
        {
            long r1 = residues[0, i];
            long r2 = residues[1, i];
            long r3 = residues[2, i];

            long diff12 = (r2 - r1) % (long)m2;
            if (diff12 < 0) diff12 += (long)m2;
            long inv_m1_mod_m2 = ModInverse((long)(m1 % m2), (int)m2);
            long t1 = diff12 * inv_m1_mod_m2 % (long)m2;

            BigInteger x12 = r1 + m1 * t1;

            long diff23 = (r3 - (long)(x12 % m3)) % (long)m3;
            if (diff23 < 0) diff23 += (long)m3;
            long m1m2_mod_m3 = (long)((m1 % m3) * (m2 % m3) % m3);
            long inv_m1m2_mod_m3 = ModInverse(m1m2_mod_m3, (int)m3);
            long t2 = diff23 * inv_m1m2_mod_m3 % (long)m3;

            coeffs[i] = x12 + m1m2 * t2;
        }

        var resultDigits = new List<uint>();
        BigInteger carry = 0;
        for (int i = 0; i < coeffs.Length; i++)
        {
            BigInteger cur = coeffs[i] + carry;
            resultDigits.Add((uint)(cur & 0xFFFFFFFF));
            carry = cur >> 32;
        }
        while (carry > 0)
        {
            resultDigits.Add((uint)(carry & 0xFFFFFFFF));
            carry >>= 32;
        }

        int trim = resultDigits.Count;
        while (trim > 0 && resultDigits[trim - 1] == 0) trim--;
        if (trim == 0) return new BetterBigInteger([0], false);
        resultDigits.RemoveRange(trim, resultDigits.Count - trim);

        bool negative = a.IsNegative ^ b.IsNegative;
        return new BetterBigInteger(resultDigits.ToArray(), negative);
    }

    private void Ntt(int[] a, bool invert, int mod)
    {
        int n = a.Length;
        for (int i = 1, j = 0; i < n; i++)
        {
            int bit = n >> 1;
            for (; (j & bit) != 0; bit >>= 1)
                j ^= bit;
            j ^= bit;
            if (i < j)
                (a[i], a[j]) = (a[j], a[i]);
        }

        for (int len = 2; len <= n; len <<= 1)
        {
            int wlen = PowMod(PrimitiveRoot, (mod - 1) / len, mod);
            if (invert) wlen = ModInverse(wlen, mod);

            for (int i = 0; i < n; i += len)
            {
                long w = 1;
                int half = len >> 1;
                for (int j = 0; j < half; j++)
                {
                    int u = a[i + j];
                    int v = (int)(a[i + j + half] * w % mod);
                    int x = u + v;
                    if (x >= mod) x -= mod;
                    int y = u - v;
                    if (y < 0) y += mod;
                    a[i + j] = x;
                    a[i + j + half] = y;
                    w = w * wlen % mod;
                }
            }
        }

        if (invert)
        {
            int invN = ModInverse(n, mod);
            for (int i = 0; i < n; i++)
                a[i] = (int)((long)a[i] * invN % mod);
        }
    }

    private int PowMod(long a, long e, int mod)
    {
        long result = 1;
        while (e > 0)
        {
            if ((e & 1) == 1) result = result * a % mod;
            a = a * a % mod;
            e >>= 1;
        }
        return (int)result;
    }

    private int ModInverse(long a, int mod)
    {
        return PowMod(a, mod - 2, mod);
    }
}