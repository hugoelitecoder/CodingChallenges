using System;
using System.Collections.Generic;
using System.Linq;

class Solution
{
    static void Main()
    {
        int N = int.Parse(Console.ReadLine());
        while (N-- > 0)
        {
            var ops = Console.ReadLine().Split();
            var ds = new DataStructureChecker();
            foreach (var op in ops)
            {
                char type = op[0];
                int  val  = op[1] - '0';
                if (type == 'i') ds.Insert(val);
                else             ds.Remove(val);
            }

            Console.WriteLine(ds.Result());
        }
    }
}

class DataStructureChecker
{
    private readonly Queue<int>               _queue = new();
    private readonly Stack<int>               _stack = new();
    private readonly SortedDictionary<int,int> _pq    = new();

    private bool _isQueue = true, _isStack = true, _isPq = true;

    public void Insert(int x)
    {
        if (_isQueue) _queue.Enqueue(x);
        if (_isStack) _stack.Push(x);
        if (_isPq)    _pq[x] = _pq.GetValueOrDefault(x) + 1;
    }

    public void Remove(int expected)
    {
        if (_isQueue)
        {
            if (_queue.Count == 0 || _queue.Dequeue() != expected)
                _isQueue = false;
        }
        if (_isStack)
        {
            if (_stack.Count == 0 || _stack.Pop() != expected)
                _isStack = false;
        }
        if (_isPq)
        {
            if (_pq.Count == 0)
                _isPq = false;
            else
            {
                int max = _pq.Keys.Max();
                if (max != expected) _isPq = false;
                if (--_pq[max] == 0) _pq.Remove(max);
            }
        }
    }

    public string Result()
    {
        int count = (_isQueue ? 1 : 0)
                  + (_isStack ? 1 : 0)
                  + (_isPq    ? 1 : 0);

        return count switch
        {
            0      => "mystery",
            > 1    => "unsure",
            _      => _isQueue ? "queue"
                     : _isStack ? "stack"
                     : "priority queue"
        };
    }
}
