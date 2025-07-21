using System;
using System.Collections.Generic;

class Solution
{
    public static void Main(string[] args)
    {
        var store = new KeyValueStore();
        var n = int.Parse(Console.ReadLine());
        for (var i = 0; i < n; i++)
        {
            var line = Console.ReadLine();
            store.ProcessCommand(line);
        }
    }
}

class KeyValueStore
{
    private Dictionary<string, string> _kv;
    private List<string> _order;

    public KeyValueStore()
    {
        _kv = new Dictionary<string, string>();
        _order = new List<string>();
    }

    public void ProcessCommand(string line)
    {
        var firstSpace = line.IndexOf(' ');
        var cmd = firstSpace == -1 ? line : line.Substring(0, firstSpace);
        var rest = firstSpace == -1 ? "" : line.Substring(firstSpace + 1);

        if (cmd == "SET")
        {
            SetKeys(rest);
        }
        else if (cmd == "GET")
        {
            GetValues(rest);
        }
        else if (cmd == "EXISTS")
        {
            ExistsKeys(rest);
        }
        else if (cmd == "KEYS")
        {
            PrintKeys();
        }
    }

    private void SetKeys(string data)
    {
        var pairs = data.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        for (var i = 0; i < pairs.Length; i++)
        {
            var eq = pairs[i].IndexOf('=');
            var key = pairs[i].Substring(0, eq);
            var val = pairs[i].Substring(eq + 1);
            if (!_kv.ContainsKey(key))
            {
                _order.Add(key);
            }
            _kv[key] = val;
        }
    }

    private void GetValues(string data)
    {
        var keys = data.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var res = new List<string>();
        for (var i = 0; i < keys.Length; i++)
        {
            if (_kv.ContainsKey(keys[i]))
            {
                res.Add(_kv[keys[i]]);
            }
            else
            {
                res.Add("null");
            }
        }
        Console.WriteLine(string.Join(' ', res));
    }

    private void ExistsKeys(string data)
    {
        var keys = data.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var res = new List<string>();
        for (var i = 0; i < keys.Length; i++)
        {
            res.Add(_kv.ContainsKey(keys[i]) ? "true" : "false");
        }
        Console.WriteLine(string.Join(' ', res));
    }

    private void PrintKeys()
    {
        if (_order.Count == 0)
        {
            Console.WriteLine("EMPTY");
            return;
        }
        Console.WriteLine(string.Join(' ', _order));
    }
}
