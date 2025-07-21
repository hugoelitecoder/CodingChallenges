using System;
using System.Text;
using System.Globalization;

class Solution
{
    static void Main(string[] args)
    {
        var text = Console.ReadLine() ?? "";

        var trigraphs = new (string tri, char repl)[]
        {
            ("??=", '#'),
            ("??/", '\\'),
            ("??'", '^'),
            ("??(", '['),
            ("??)", ']'),
            ("??!", '|'),
            ("??-", '~')
        };
        foreach (var (tri, repl) in trigraphs)
            text = text.Replace(tri, repl.ToString());

        var sb = new StringBuilder();
        for (int i = 0; i < text.Length; i++)
        {
            if (text[i] == '\\' && i + 1 < text.Length)
            {
                char c = text[i + 1];
                if ((c == 'x' || c == 'X') && i + 3 < text.Length
                    && IsHex(text[i + 2]) && IsHex(text[i + 3]))
                {
                    // \xhh
                    var hex = text.Substring(i + 2, 2);
                    sb.Append((char)int.Parse(hex, NumberStyles.HexNumber));
                    i += 3;
                }
                else if ((c == 'u' || c == 'U') && i + (c == 'u' ? 5 : 9) < text.Length)
                {
                    int count = c == 'u' ? 4 : 8;
                    bool ok = true;
                    for (int j = 0; j < count; j++)
                        if (!IsHex(text[i + 2 + j])) { ok = false; break; }
                    if (ok)
                    {
                        var hex = text.Substring(i + 2, count);
                        sb.Append((char)int.Parse(hex, NumberStyles.HexNumber));
                        i += 1 + count;
                        continue;
                    }
                    sb.Append(text[i + 1]);
                    i++;
                }
                else
                {
                    sb.Append(text[i + 1]);
                    i++;
                }
            }
            else
            {
                sb.Append(text[i]);
            }
        }
        text = sb.ToString();

        sb.Clear();
        for (int i = 0; i < text.Length; i++)
        {
            if (text[i] == '&')
            {
                int semi = text.IndexOf(';', i + 1);
                if (semi > i)
                {
                    var entity = text.Substring(i + 1, semi - i - 1);
                    string repl = null;
                    switch (entity)
                    {
                        case "amp": repl = "&"; break;
                        case "lt":  repl = "<"; break;
                        case "gt":  repl = ">"; break;
                        case "bsol": repl = "\\"; break;
                    }
                    if (repl != null)
                    {
                        sb.Append(repl);
                        i = semi;
                        continue;
                    }
                    if (entity.Length > 1 && entity[0] == '#')
                    {
                        var numstr = entity.Substring(1);
                        if (int.TryParse(numstr, out int code) && code >= 1 && code <= 127)
                        {
                            sb.Append((char)code);
                            i = semi;
                            continue;
                        }
                    }
                    sb.Append('&').Append(entity).Append(';');
                    i = semi;
                }
                else
                {
                    sb.Append(text[i]);
                }
            }
            else
            {
                sb.Append(text[i]);
            }
        }

        Console.WriteLine(sb.ToString());
    }

    static bool IsHex(char c) =>
        ('0' <= c && c <= '9') ||
        ('a' <= c && c <= 'f') ||
        ('A' <= c && c <= 'F');
}
