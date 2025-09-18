using System;

class Solution
{
    static void Main()
    {
        var rn1 = new RomanNumeral(Console.ReadLine().Trim());
        var rn2 = new RomanNumeral(Console.ReadLine().Trim());
        var sum = new RomanNumeral(rn1.Value + rn2.Value);
        Console.WriteLine(sum);
    }
}

class RomanNumeral
{
    public int Value { get; }
    private string _repr;
    
    public RomanNumeral(string roman)
    {
        _repr = roman;
        Value = FromRoman(roman);
    }
    
    public RomanNumeral(int value)
    {
        Value = value;
        _repr = ToRoman(value);
    }
    
    public override string ToString() => _repr;
    
    private static int FromRoman(string s)
    {
        int sum = 0;
        for (int i = 0; i < s.Length; i++)
        {
            int v = CharValue(s[i]);
            if (i + 1 < s.Length && v < CharValue(s[i + 1]))
                sum -= v;
            else
                sum += v;
        }
        return sum;
    }
    
    private static string ToRoman(int n) =>
        n >= 1000 ? "M"  + ToRoman(n - 1000) :
        n >= 900  ? "CM" + ToRoman(n - 900)  :
        n >= 500  ? "D"  + ToRoman(n - 500)  :
        n >= 400  ? "CD" + ToRoman(n - 400)  :
        n >= 100  ? "C"  + ToRoman(n - 100)  :
        n >= 90   ? "XC" + ToRoman(n - 90)   :
        n >= 50   ? "L"  + ToRoman(n - 50)   :
        n >= 40   ? "XL" + ToRoman(n - 40)   :
        n >= 10   ? "X"  + ToRoman(n - 10)   :
        n >= 9    ? "IX" + ToRoman(n - 9)    :
        n >= 5    ? "V"  + ToRoman(n - 5)    :
        n >= 4    ? "IV" + ToRoman(n - 4)    :
        n >= 1    ? "I"  + ToRoman(n - 1)    :
                    "";
    
    private static int CharValue(char c) => c switch
    {
        'I' => 1,   'V' => 5,
        'X' => 10,  'L' => 50,
        'C' => 100, 'D' => 500,
        'M' => 1000, _ => 0
    };
}
