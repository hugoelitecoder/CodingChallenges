using System;
using System.Collections.Generic;
using System.Globalization;

public class Solution
{
    public static void Main(string[] args)
    {
        if (!int.TryParse(Console.ReadLine(), out int n)) return;

        var engine = new TradeEngine();
        for (int i = 0; i < n; i++)
        {
            var line = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(line)) continue;

            var parts = line.Split();
            if (parts.Length != 4) continue;

            var symbol = parts[0];
            var verb = parts[1];
            if (!int.TryParse(parts[2], out int qty)) continue;
            if (!decimal.TryParse(parts[3], NumberStyles.Number, CultureInfo.InvariantCulture, out decimal price)) continue;

            var order = new Order(symbol, verb, qty, price);
            engine.Process(order);
        }

        var trades = engine.GetTrades();
        if (trades.Count == 0)
        {
            Console.WriteLine("NO TRADE");
            return;
        }

        foreach (var trade in trades)
        {
            Console.WriteLine($"{trade.Symbol} {trade.Qty} {trade.Price.ToString("0.00", CultureInfo.InvariantCulture)}");
        }
    }
}

public class Order
{
    public string Symbol { get; }
    public string Verb { get; }
    public int Qty { get; set; }
    public decimal Price { get; }

    public Order(string symbol, string verb, int qty, decimal price)
    {
        Symbol = symbol;
        Verb = verb;
        Qty = qty;
        Price = price;
    }
}

public class Trade
{
    public string Symbol { get; }
    public int Qty { get; }
    public decimal Price { get; }

    public Trade(string symbol, int qty, decimal price)
    {
        Symbol = symbol;
        Qty = qty;
        Price = price;
    }
}

public class TradeEngine
{
    private readonly Dictionary<string, SortedDictionary<decimal, Queue<Order>>> _buyBook;
    private readonly Dictionary<string, SortedDictionary<decimal, Queue<Order>>> _sellBook;
    private readonly List<Trade> _trades;

    public TradeEngine()
    {
        _buyBook = new Dictionary<string, SortedDictionary<decimal, Queue<Order>>>();
        _sellBook = new Dictionary<string, SortedDictionary<decimal, Queue<Order>>>();
        _trades = new List<Trade>();
    }

    public void Process(Order order)
    {
        if (order.Verb == "BUY")
        {
            Match(order, _sellBook, true);
            if (order.Qty > 0) AddToBook(_buyBook, order, true);
        }
        else
        {
            Match(order, _buyBook, false);
            if (order.Qty > 0) AddToBook(_sellBook, order, false);
        }
    }

    public List<Trade> GetTrades() => _trades;

    private void Match(Order incoming, Dictionary<string, SortedDictionary<decimal, Queue<Order>>> book, bool isBuy)
    {
        if (!book.TryGetValue(incoming.Symbol, out var priceLevels)) return;

        var prices = new List<decimal>(priceLevels.Keys);
        if (isBuy) prices.Sort(); else prices.Sort((a, b) => b.CompareTo(a));

        foreach (var price in prices)
        {
            if ((isBuy && price > incoming.Price) || (!isBuy && price < incoming.Price)) continue;

            var queue = priceLevels[price];
            while (queue.Count > 0 && incoming.Qty > 0)
            {
                var existing = queue.Peek();
                var tradePrice = existing.Price;
                var tradedQty = Math.Min(incoming.Qty, existing.Qty);
                _trades.Add(new Trade(incoming.Symbol, tradedQty, tradePrice));
                incoming.Qty -= tradedQty;
                existing.Qty -= tradedQty;
                if (existing.Qty == 0) queue.Dequeue();
            }

            if (queue.Count == 0) priceLevels.Remove(price);
            if (incoming.Qty == 0) break;
        }
    }

    private void AddToBook(Dictionary<string, SortedDictionary<decimal, Queue<Order>>> book, Order order, bool descending)
    {
        if (!book.TryGetValue(order.Symbol, out var priceLevels))
        {
            priceLevels = descending
                ? new SortedDictionary<decimal, Queue<Order>>(Comparer<decimal>.Create((a, b) => b.CompareTo(a)))
                : new SortedDictionary<decimal, Queue<Order>>();
            book[order.Symbol] = priceLevels;
        }

        if (!priceLevels.TryGetValue(order.Price, out var queue))
        {
            queue = new Queue<Order>();
            priceLevels[order.Price] = queue;
        }

        queue.Enqueue(order);
    }
}