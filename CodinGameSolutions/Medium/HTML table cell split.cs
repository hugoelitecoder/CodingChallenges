using System;
using System.Collections.Generic;
using System.Linq;

namespace SpanTableEditor
{
    class Cell
    {
        public int Cs { get; set; }
        public int Rs { get; set; }
        public Cell(int cs, int rs) { Cs = cs; Rs = rs; }
    }

    class Row
    {
        public List<Cell> Cells { get; } = new List<Cell>();
        public Dictionary<int, (int row, int cellIndex)> Map { get; } = new Dictionary<int, (int, int)>();
    }

    class TableEditor
    {
        private List<Row> rows;
        private int targetIndex;
        private string direction;
        private int targetRow, targetCell, targetColumn;

        public TableEditor(List<string> spanLines, int targetIndex, string direction)
        {
            this.targetIndex = targetIndex;
            this.direction = direction;
            rows = ReadRows(spanLines);
            BuildMapAndFindTarget();
        }

        public void Execute()
        {
            if (direction == "C") ExpandColumns();
            else ExpandRows();
            PrintRows();
        }

        private static List<Row> ReadRows(List<string> lines)
        {
            var list = new List<Row>(lines.Count);
            foreach (var line in lines)
            {
                var row = new Row();
                var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                foreach (var part in parts)
                {
                    var p = part.Split(',');
                    row.Cells.Add(new Cell(int.Parse(p[0]), int.Parse(p[1])));
                }
                list.Add(row);
            }
            return list;
        }

        private void BuildMapAndFindTarget()
        {
            int linearIndex = 0;
            for (int r = 0; r < rows.Count; r++)
            {
                var row = rows[r];
                int colCursor = 0;
                for (int c = 0; c < row.Cells.Count; c++, linearIndex++)
                {
                    while (row.Map.ContainsKey(colCursor)) colCursor++;
                    row.Map[colCursor] = (r, c);
                    if (linearIndex == targetIndex)
                    {
                        targetRow = r;
                        targetCell = c;
                        targetColumn = colCursor;
                    }
                    var cell = row.Cells[c];
                    for (int dr = 0; dr < cell.Rs; dr++)
                        for (int dc = 0; dc < cell.Cs; dc++)
                            rows[r + dr].Map[colCursor + dc] = (r, c);
                    colCursor += cell.Cs;
                }
            }
        }

        private void ExpandColumns()
        {
            var toExpand = new Dictionary<int, HashSet<int>>();
            foreach (var row in rows)
            {
                var (r, c) = row.Map[targetColumn];
                if (!toExpand.TryGetValue(r, out var set))
                {
                    set = new HashSet<int>();
                    toExpand[r] = set;
                }
                set.Add(c);
            }
            foreach (var kv in toExpand)
            {
                foreach (var c in kv.Value)
                {
                    if (kv.Key == targetRow && c == targetCell) continue;
                    rows[kv.Key].Cells[c].Cs++;
                }
            }
            var original = rows[targetRow].Cells[targetCell];
            rows[targetRow].Cells.Insert(targetCell, new Cell(1, original.Rs));
        }

        private void ExpandRows()
        {
            var toExpand = new Dictionary<int, HashSet<int>>();
            foreach (var kv in rows[targetRow].Map)
            {
                var (r, c) = kv.Value;
                if (!toExpand.TryGetValue(r, out var set))
                {
                    set = new HashSet<int>();
                    toExpand[r] = set;
                }
                set.Add(c);
            }
            foreach (var kv in toExpand)
            {
                foreach (var c in kv.Value)
                {
                    if (kv.Key == targetRow && c == targetCell) continue;
                    rows[kv.Key].Cells[c].Rs++;
                }
            }
            var orig = rows[targetRow].Cells[targetCell];
            var newRow = new Row();
            newRow.Cells.Add(new Cell(orig.Cs, 1));
            rows.Insert(targetRow + 1, newRow);
        }

        private void PrintRows()
        {
            foreach (var row in rows)
            {
                Console.WriteLine(string.Join(" ",
                    row.Cells.Select(cell => $"{cell.Cs},{cell.Rs}")));
            }
        }
    }

    class Program
    {
        static void Main()
        {
            int nr = int.Parse(Console.ReadLine());
            var spanLines = new List<string>(nr);
            for (int i = 0; i < nr; i++)
                spanLines.Add(Console.ReadLine());
            var inputs = Console.ReadLine().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            int isIndex = int.Parse(inputs[0]);
            string ds = inputs[1];
            var editor = new TableEditor(spanLines, isIndex, ds);
            editor.Execute();
        }
    }
}
