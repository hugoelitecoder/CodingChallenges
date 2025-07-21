using System;
using System.Linq;
using System.Globalization;
using System.Collections.Generic;

class Solution
{
    static void Main(string[] args)
    {
        string queryText = Console.ReadLine();
        int rows = int.Parse(Console.ReadLine());
        string[] headers = Console.ReadLine().Split(' ');
        var rawData = new List<string[]>();
        for (int i = 0; i < rows; i++)
            rawData.Add(Console.ReadLine().Split(' '));

        var query = Query.Parse(queryText);
        var table = new Table(headers, rawData);
        var indexMap = table.GetColumnIndexMap();

        var filtered = query.ApplyWhere(table, indexMap);
        var sorted = query.ApplyOrderBy(filtered, indexMap);
        var selectCols = query.SelectColumns(table.GetColumnNames());

        Console.WriteLine(string.Join(" ", selectCols));
        foreach (var row in sorted)
        {
            var output = selectCols.Select(c => row.Values[c]);
            Console.WriteLine(string.Join(" ", output));
        }
    }
}

public class Query
{
    public List<string> SelectColumnsList { get; private set; }
    public string WhereColumn { get; private set; }
    public string WhereValue { get; private set; }
    public string OrderByColumn { get; private set; }
    public bool OrderDescending { get; private set; }

    public static Query Parse(string query)
    {
        var q = new Query();
        string upper = query.ToUpperInvariant();

        int selStart = upper.IndexOf("SELECT ") + 7;
        int fromPos = upper.IndexOf(" FROM ");
        string rawCols = query.Substring(selStart, fromPos - selStart).Trim();
        q.SelectColumnsList = rawCols == "*"
            ? null
            : rawCols.Split(',').Select(c => c.Trim()).ToList();

        int wherePos = upper.IndexOf(" WHERE ");
        if (wherePos != -1)
        {
            int orderPos = upper.IndexOf(" ORDER BY ");
            string cond = orderPos == -1
                ? query.Substring(wherePos + 7)
                : query.Substring(wherePos + 7, orderPos - (wherePos + 7));
            var parts = cond.Split('=');
            q.WhereColumn = parts[0].Trim();
            q.WhereValue = parts[1].Trim();
        }

        int orderByPos = upper.IndexOf(" ORDER BY ");
        if (orderByPos != -1)
        {
            string clause = query.Substring(orderByPos + 9).Trim();
            var parts = clause.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            q.OrderByColumn = parts[0];
            q.OrderDescending = parts.Length > 1 && parts[1].ToUpperInvariant() == "DESC";
        }

        return q;
    }

    public List<Row> ApplyWhere(Table table, Dictionary<string,int> indexMap)
    {
        var rows = table.Rows.AsEnumerable();
        if (WhereColumn != null)
            rows = rows.Where(r => r.Values[WhereColumn] == WhereValue);
        return rows.ToList();
    }

    public List<Row> ApplyOrderBy(IEnumerable<Row> rows, Dictionary<string,int> indexMap)
    {
        if (OrderByColumn == null)
            return rows.ToList();

        Func<Row, double> keySelector = r => double.Parse(r.Values[OrderByColumn], CultureInfo.InvariantCulture);
        return OrderDescending
            ? rows.OrderByDescending(keySelector).ToList()
            : rows.OrderBy(keySelector).ToList();
    }

    public List<string> SelectColumns(IEnumerable<string> headers)
    {
        return SelectColumnsList == null
            ? headers.ToList()
            : SelectColumnsList;
    }
}

public class Column
{
    public string Name { get; }
    public bool IsNumeric { get; }
    public Column(string name, bool isNumeric)
    {
        Name = name;
        IsNumeric = isNumeric;
    }
}

public class Row
{
    private readonly Dictionary<string, string> _values;
    public Row(Dictionary<string, string> values)
    {
        _values = values;
    }
    public IReadOnlyDictionary<string, string> Values => _values;
}

public class Table
{
    public List<Column> Columns { get; }
    public List<Row> Rows { get; }

    public Table(IEnumerable<string> headers, IEnumerable<string[]> data)
    {
        var headerList = headers.ToList();
        Columns = headerList
            .Select((h, idx) => new Column(
                h,
                data.Any(r => double.TryParse(r[idx], NumberStyles.Any, CultureInfo.InvariantCulture, out _))
            ))
            .ToList();

        Rows = data
            .Select(r =>
            {
                var dict = new Dictionary<string, string>();
                for (int i = 0; i < r.Length; i++)
                    dict[headerList[i]] = r[i];
                return new Row(dict);
            })
            .ToList();
    }

    public Dictionary<string,int> GetColumnIndexMap()
    {
        return Columns.ToDictionary(c => c.Name, c => Columns.IndexOf(c));
    }

    public IEnumerable<string> GetColumnNames()
    {
        return Columns.Select(c => c.Name);
    }
}
