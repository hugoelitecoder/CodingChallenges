using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

public class Solution
{
    public static void Main(string[] args)
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();

        var n = int.Parse(Console.ReadLine());
        var c = int.Parse(Console.ReadLine());
        
        var customers = new Customer[n];
        for (var i = 0; i < n; i++)
        {
            var inputs = Console.ReadLine().Split(' ');
            var index = int.Parse(inputs[0]);
            var x = int.Parse(inputs[1]);
            var y = int.Parse(inputs[2]);
            var demand = int.Parse(inputs[3]);
            customers[index] = new Customer(index, x, y, demand);
        }
        
        var solver = new CVRPSolver(customers, c, stopwatch);
        var finalRoutes = solver.Solve();
        
        var sb = new StringBuilder();
        for (var i = 0; i < finalRoutes.Count; i++)
        {
            sb.Append(string.Join(" ", finalRoutes[i]));
            if (i < finalRoutes.Count - 1)
            {
                sb.Append(';');
            }
        }

        Console.WriteLine(sb.ToString());
        
        stopwatch.Stop();
        Console.Error.WriteLine($"[DEBUG] Final Score: {solver.GetBestScore()}. Total execution time: {stopwatch.ElapsedMilliseconds} ms");
    }
}

public class Customer
{
    public int Id { get; }
    public int X { get; }
    public int Y { get; }
    public int Demand { get; }
    public Customer(int id, int x, int y, int demand) { Id = id; X = x; Y = y; Demand = demand; }
}

public class CVRPSolver
{
    private const int TIME_LIMIT_MS = 9800;
    
    private readonly Customer[] _customers;
    private readonly int _capacity;
    private readonly int[,] _distances;
    private readonly int _locationCount;
    private readonly Stopwatch _stopwatch;
    private readonly Random _random;
    private VrpSolution _bestSolution;

    private const double SHAW_DISTANCE_WEIGHT = 1.5;
    private const double SHAW_DEMAND_WEIGHT = 0.5;
    private const double SHAW_TIME_WEIGHT = 0.5;

    public CVRPSolver(Customer[] customers, int capacity, Stopwatch stopwatch)
    {
        _customers = customers;
        _capacity = capacity;
        _locationCount = customers.Length;
        _distances = new int[_locationCount, _locationCount];
        _stopwatch = stopwatch;
        _random = new Random();
    }
    
    public int GetBestScore() => _bestSolution?.TotalDistance ?? -1;

    public List<List<int>> Solve()
    {
        PrecomputeDistances();
        
        var currentSolution = CreateInitialSolutionClarkeWright();
        LocalSearch(currentSolution);
        _bestSolution = currentSolution.Clone();
        
        Console.Error.WriteLine($"[DEBUG] Initial solution: {_bestSolution.TotalDistance}");
        
        double temperature = _bestSolution.TotalDistance * 0.05;
        double coolingRate = 0.99975; 

        int iteration = 0;
        while (_stopwatch.ElapsedMilliseconds < TIME_LIMIT_MS)
        {
            var memento = currentSolution.CreateMemento();
            
            var choice = _random.NextDouble();
            if (choice < 0.4)
            {
                int destructionSize = _random.Next(2, Math.Max(3, (int)(_locationCount * 0.2)));
                var removedCustomers = ShawDestroy(currentSolution, destructionSize);
                GreedyRepair(currentSolution, removedCustomers);
            }
            else
            {
                LocalSearch(currentSolution);
            }

            var delta = currentSolution.TotalDistance - memento.TotalDistance;
            if (delta < 0)
            {
                if (currentSolution.TotalDistance < _bestSolution.TotalDistance)
                {
                    _bestSolution = currentSolution.Clone();
                }
            }
            else if (_random.NextDouble() >= Math.Exp(-delta / temperature))
            {
                currentSolution.RestoreFromMemento(memento);
            }
            
            if (temperature > 0.1) temperature *= coolingRate;
            iteration++;
        }
        
        return _bestSolution.Routes;
    }

    private void PrecomputeDistances()
    {
        for (var i = 0; i < _locationCount; i++)
        for (var j = i; j < _locationCount; j++)
        {
            var dist = (int)Math.Round(Math.Sqrt(Math.Pow(_customers[i].X - _customers[j].X, 2) + Math.Pow(_customers[i].Y - _customers[j].Y, 2)));
            _distances[i, j] = _distances[j, i] = dist;
        }
    }
    
    private VrpSolution CreateInitialSolutionClarkeWright()
    {
        var savings = new List<(int i, int j, int val)>();
        for (var i = 1; i < _locationCount; i++)
        for (var j = i + 1; j < _locationCount; j++)
        {
            savings.Add((i, j, _distances[0, i] + _distances[0, j] - _distances[i, j]));
        }
        savings.Sort((a, b) => b.val.CompareTo(a.val));

        var routes = new Dictionary<int, (List<int> nodes, int load)>();
        var customerToRouteKey = new int[_locationCount];
        for (var i = 1; i < _locationCount; i++)
        {
            routes[i] = (new List<int> { i }, _customers[i].Demand);
            customerToRouteKey[i] = i;
        }

        foreach (var (i, j, _) in savings)
        {
            if (_customers[i].Demand > _capacity || _customers[j].Demand > _capacity) continue;
            
            var keyI = customerToRouteKey[i];
            var keyJ = customerToRouteKey[j];
            if (keyI == 0 || keyJ == 0 || keyI == keyJ) continue;

            var routeI = routes[keyI];
            var routeJ = routes[keyJ];
            if (routeI.load + routeJ.load > _capacity) continue;
            
            bool iIsEnd = routeI.nodes[^1] == i;
            bool jIsStart = routeJ.nodes[0] == j;
            bool iIsStart = routeI.nodes[0] == i;
            bool jIsEnd = routeJ.nodes[^1] == j;

            if (iIsEnd && jIsStart)
            {
                routeI.nodes.AddRange(routeJ.nodes);
                routeI.load += routeJ.load;
                routes[keyI] = routeI;
                routes.Remove(keyJ);
                foreach (var node in routeJ.nodes) customerToRouteKey[node] = keyI;
            }
            else if (iIsStart && jIsEnd)
            {
                routeJ.nodes.AddRange(routeI.nodes);
                routeJ.load += routeI.load;
                routes[keyJ] = routeJ;
                routes.Remove(keyI);
                foreach (var node in routeI.nodes) customerToRouteKey[node] = keyJ;
            }
        }
        return new VrpSolution(routes.Values.Select(r => r.nodes).ToList(), _customers, _distances, _capacity);
    }
    
    private List<int> ShawDestroy(VrpSolution solution, int destructionSize)
    {
        var removedCustomers = new List<int>(destructionSize);
        var allCustomers = solution.GetAllCustomers().ToList();
        if (allCustomers.Count == 0) return removedCustomers;

        destructionSize = Math.Min(destructionSize, allCustomers.Count);
        
        int firstCust = allCustomers[_random.Next(allCustomers.Count)];
        solution.RemoveCustomer(firstCust);
        removedCustomers.Add(firstCust);
        
        for (int i = 1; i < destructionSize; i++)
        {
            int lastRemoved = removedCustomers[_random.Next(removedCustomers.Count)];
            var candidates = solution.GetAllCustomers().ToList();
            if(candidates.Count == 0) break;
            
            int bestCandidate = -1;
            double bestRelatedness = double.MinValue;
            
            foreach(var c in candidates)
            {
                double relatedness = 1.0 / (SHAW_DISTANCE_WEIGHT * _distances[lastRemoved, c] + SHAW_DEMAND_WEIGHT * Math.Abs(_customers[lastRemoved].Demand - _customers[c].Demand) + 1);
                if (relatedness > bestRelatedness)
                {
                    bestRelatedness = relatedness;
                    bestCandidate = c;
                }
            }
            solution.RemoveCustomer(bestCandidate);
            removedCustomers.Add(bestCandidate);
        }

        return removedCustomers;
    }
    
    private void GreedyRepair(VrpSolution solution, List<int> removedCustomers)
    {
        removedCustomers = removedCustomers.OrderBy(c => _random.Next()).ToList();
        foreach (var customerId in removedCustomers)
        {
            var bestInsertion = FindBestInsertion(solution, customerId);
            solution.ApplyInsertion(customerId, bestInsertion);
        }
    }

    private InsertionInfo FindBestInsertion(VrpSolution solution, int customerId)
    {
        var bestInsertion = new InsertionInfo(-1, 0, _distances[0, customerId] + _distances[customerId, 0]);
        var customerDemand = _customers[customerId].Demand;

        for (var r = 0; r < solution.Routes.Count; r++)
        {
            if (solution.RouteLoads[r] + customerDemand > _capacity) continue;
            var route = solution.Routes[r];
            for (var i = 0; i <= route.Count; i++)
            {
                var prevNode = i == 0 ? 0 : route[i - 1];
                var nextNode = i == route.Count ? 0 : route[i];
                var cost = _distances[prevNode, customerId] + _distances[customerId, nextNode] - _distances[prevNode, nextNode];
                if (cost < bestInsertion.Cost)
                {
                    bestInsertion = new InsertionInfo(r, i, cost);
                }
            }
        }
        return bestInsertion;
    }
    
    private void LocalSearch(VrpSolution solution)
    {
        bool improved = true;
        while(improved)
        {
            improved = false;
            if (RelocateOperator(solution)) { improved = true; continue; }
            if (SwapOperator(solution)) { improved = true; continue; }
            if (TwoOptOperator(solution)) { improved = true; continue; }
        }
    }

    private bool TwoOptOperator(VrpSolution solution)
    {
        for (int r = 0; r < solution.Routes.Count; r++)
        {
            if (solution.Routes[r].Count < 2) continue;
            for (int i = 0; i < solution.Routes[r].Count - 1; i++)
            {
                for (int j = i + 1; j < solution.Routes[r].Count; j++)
                {
                    if (solution.TryReverseSegment(r, i, j)) return true;
                }
            }
        }
        return false;
    }

    private bool RelocateOperator(VrpSolution solution)
    {
        for (int r1 = 0; r1 < solution.Routes.Count; r1++)
        {
            for (int i = 0; i < solution.Routes[r1].Count; i++)
            {
                for (int r2 = 0; r2 < solution.Routes.Count; r2++)
                {
                    if (r1 == r2) continue;
                    for (int j = 0; j <= solution.Routes[r2].Count; j++)
                    {
                        if (solution.TryRelocate(r1, i, r2, j)) return true;
                    }
                }
                for (int j = 0; j <= solution.Routes[r1].Count; j++)
                {
                    if (i == j || i + 1 == j) continue;
                    if (solution.TryRelocate(r1, i, r1, j)) return true;
                }
            }
        }
        return false;
    }

    private bool SwapOperator(VrpSolution solution)
    {
        for (int r1 = 0; r1 < solution.Routes.Count; r1++)
        {
            for (int r2 = r1 + 1; r2 < solution.Routes.Count; r2++)
            {
                for (int i = 0; i < solution.Routes[r1].Count; i++)
                {
                    for (int j = 0; j < solution.Routes[r2].Count; j++)
                    {
                        if (solution.TrySwap(r1, i, r2, j)) return true;
                    }
                }
            }
        }
        return false;
    }
}

class VrpSolution
{
    public List<List<int>> Routes { get; private set; }
    public List<int> RouteLoads { get; private set; }
    public int TotalDistance { get; private set; }
    private readonly Customer[] _customers;
    private readonly int[,] _distances;
    private readonly int _capacity;

    public VrpSolution(List<List<int>> routes, Customer[] customers, int[,] distances, int capacity)
    {
        Routes = routes.Where(r => r.Any()).ToList();
        _customers = customers;
        _distances = distances;
        _capacity = capacity;
        RouteLoads = new List<int>();
        Recalculate();
    }
    
    public VrpSolution Clone() => new VrpSolution(this.Routes.Select(r => new List<int>(r)).ToList(), _customers, _distances, _capacity);
    public IEnumerable<int> GetAllCustomers() => Routes.SelectMany(r => r);
    
    public void Recalculate()
    {
        TotalDistance = 0;
        RouteLoads.Clear();
        foreach (var route in Routes)
        {
            if (route.Count == 0) continue;
            int routeDist = 0;
            int routeLoad = 0;
            int prevNode = 0;
            foreach (var node in route)
            {
                routeDist += _distances[prevNode, node];
                routeLoad += _customers[node].Demand;
                prevNode = node;
            }
            routeDist += _distances[prevNode, 0];
            TotalDistance += routeDist;
            RouteLoads.Add(routeLoad);
        }
    }

    public void RemoveCustomer(int customerId)
    {
        for (int r = 0; r < Routes.Count; ++r)
        {
            int p = Routes[r].IndexOf(customerId);
            if (p != -1)
            {
                var prev = p == 0 ? 0 : Routes[r][p-1];
                var next = p == Routes[r].Count - 1 ? 0 : Routes[r][p+1];
                TotalDistance -= (_distances[prev, customerId] + _distances[customerId, next] - _distances[prev, next]);
                RouteLoads[r] -= _customers[customerId].Demand;
                Routes[r].RemoveAt(p);
                if(Routes[r].Count == 0) { Routes.RemoveAt(r); RouteLoads.RemoveAt(r); }
                return;
            }
        }
    }

    public void ApplyInsertion(int customerId, InsertionInfo info)
    {
        if (info.RouteIndex == -1)
        {
            Routes.Add(new List<int> { customerId });
            RouteLoads.Add(_customers[customerId].Demand);
        }
        else
        {
            Routes[info.RouteIndex].Insert(info.Position, customerId);
            RouteLoads[info.RouteIndex] += _customers[customerId].Demand;
        }
        TotalDistance += info.Cost;
    }

    public bool TryReverseSegment(int routeIndex, int i, int j)
    {
        var route = Routes[routeIndex];
        var p1 = i == 0 ? 0 : route[i-1];
        var c1 = route[i];
        var p2 = j == route.Count - 1 ? 0 : route[j+1];
        var c2 = route[j];

        var currentCost = _distances[p1, c1] + _distances[c2, p2];
        var newCost = _distances[p1, c2] + _distances[c1, p2];

        if (newCost < currentCost)
        {
            TotalDistance -= (currentCost - newCost);
            route.Reverse(i, j - i + 1);
            return true;
        }
        return false;
    }
    
    public bool TryRelocate(int r1, int i, int r2, int j)
    {
        var route1 = Routes[r1];
        var customerId = route1[i];
        int customerDemand = _customers[customerId].Demand;

        if (r1 != r2 && RouteLoads[r2] + customerDemand > _capacity) return false;

        int p1 = i == 0 ? 0 : route1[i - 1];
        int n1 = i == route1.Count - 1 ? 0 : route1[i + 1];
        int removalCost = _distances[p1, customerId] + _distances[customerId, n1] - _distances[p1, n1];

        int insertionCost;
        if(r1 == r2)
        {
            if (j == i || j == i + 1) return false;
            var tempRoute = new List<int>(route1);
            tempRoute.RemoveAt(i);
            int insertionPosInTemp = (j > i) ? j - 1 : j;
            int p2 = (insertionPosInTemp == 0) ? 0 : tempRoute[insertionPosInTemp - 1];
            int n2 = (insertionPosInTemp == tempRoute.Count) ? 0 : tempRoute[insertionPosInTemp];
            insertionCost = _distances[p2, customerId] + _distances[customerId, n2] - _distances[p2, n2];
        } else {
            var route2 = Routes[r2];
            int p2 = j == 0 ? 0 : route2[j - 1];
            int n2 = j == route2.Count ? 0 : route2[j];
            insertionCost = _distances[p2, customerId] + _distances[customerId, n2] - _distances[p2, n2];
        }

        if (insertionCost - removalCost < 0)
        {
            TotalDistance += (insertionCost - removalCost);
            route1.RemoveAt(i);
            if (r1 == r2)
            {
                Routes[r1].Insert(j > i ? j-1 : j, customerId);
            }
            else
            {
                Routes[r2].Insert(j, customerId);
                RouteLoads[r1] -= customerDemand;
                RouteLoads[r2] += customerDemand;
                if (route1.Count == 0) 
                { 
                    int r1Original = Routes.IndexOf(route1) + 1;
                    Routes.RemoveAt(r1); 
                    RouteLoads.RemoveAt(r1);
                }
            }
            return true;
        }
        return false;
    }
    
    public bool TrySwap(int r1, int i, int r2, int j)
    {
        var route1 = Routes[r1];
        var route2 = Routes[r2];
        var cust1 = route1[i];
        var cust2 = route2[j];
        var demand1 = _customers[cust1].Demand;
        var demand2 = _customers[cust2].Demand;

        if (RouteLoads[r1] - demand1 + demand2 > _capacity || RouteLoads[r2] - demand2 + demand1 > _capacity) return false;

        var p1 = i == 0 ? 0 : route1[i-1];
        var n1 = i == route1.Count - 1 ? 0 : route1[i+1];
        var p2 = j == 0 ? 0 : route2[j-1];
        var n2 = j == route2.Count - 1 ? 0 : route2[j+1];

        var currentCost = _distances[p1, cust1] + _distances[cust1, n1] + _distances[p2, cust2] + _distances[cust2, n2];
        var newCost = _distances[p1, cust2] + _distances[cust2, n1] + _distances[p2, cust1] + _distances[cust1, n2];
        
        if (newCost < currentCost)
        {
            TotalDistance -= (currentCost - newCost);
            route1[i] = cust2;
            route2[j] = cust1;
            RouteLoads[r1] = RouteLoads[r1] - demand1 + demand2;
            RouteLoads[r2] = RouteLoads[r2] - demand2 + demand1;
            return true;
        }
        return false;
    }

    public VrpMemento CreateMemento() => new VrpMemento(TotalDistance, Routes.Select(r => new List<int>(r)).ToList(), new List<int>(RouteLoads));
    public void RestoreFromMemento(VrpMemento memento)
    {
        this.TotalDistance = memento.TotalDistance;
        this.Routes = memento.Routes;
        this.RouteLoads = memento.RouteLoads;
    }
}

readonly struct VrpMemento
{
    public int TotalDistance { get; }
    public List<List<int>> Routes { get; }
    public List<int> RouteLoads { get; }
    public VrpMemento(int totalDistance, List<List<int>> routes, List<int> routeLoads)
    {
        TotalDistance = totalDistance;
        Routes = routes;
        RouteLoads = routeLoads;
    }
}

struct InsertionInfo
{
    public int RouteIndex { get; }
    public int Position { get; }
    public int Cost { get; }
    public InsertionInfo(int routeIndex, int position, int cost)
    {
        RouteIndex = routeIndex;
        Position = position;
        Cost = cost;
    }
}