using System;
using System.Collections.Generic;
using System.Globalization;

class Solution
{
    static void Main()
    {
        int H = int.Parse(Console.ReadLine());
        var lot = new ParkingLot(7, 2);
        for (int i = 0; i < H; i++)
        {
            var parts = Console.ReadLine().Split();
            int time = Parser.ParseTime(parts[0]);
            bool arrival = parts[1] == ">";
            for (int j = 2; j < parts.Length; j++)
            {
                lot.Process(parts[j], time, arrival);
            }
        }
        lot.CloseDay();
        Console.WriteLine(lot.Summary());
    }
}

static class Parser
{
    public static int ParseTime(string hhmm)
    {
        int hh = int.Parse(hhmm.Substring(0, 2));
        int mm = int.Parse(hhmm.Substring(3, 2));
        return hh * 60 + mm;
    }
}

class ParkingLot
{
    readonly int carCap, motoCap;
    int carCount, motoCount;
    public int CarsRejected { get; private set; }
    public int MotosRejected { get; private set; }
    public decimal TotalFees { get; private set; }
    Dictionary<string, VehicleInfo> parked = new Dictionary<string, VehicleInfo>();

    public ParkingLot(int carSlots, int motoSlots)
    {
        carCap = carSlots;
        motoCap = motoSlots;
    }

    public void Process(string id, int time, bool arrival)
    {
        if (arrival) Arrival(id, time);
        else Departure(id, time);
    }

    void Arrival(string id, int time)
    {
        bool isCar = id[0] == 'C';
        if (isCar)
        {
            if (carCount < carCap)
            {
                parked[id] = new VehicleInfo(id, time, true);
                carCount++;
            }
            else CarsRejected++;
        }
        else
        {
            if (motoCount < motoCap)
            {
                parked[id] = new VehicleInfo(id, time, false);
                motoCount++;
            }
            else MotosRejected++;
        }
    }

    void Departure(string id, int time)
    {
        if (!parked.TryGetValue(id, out var v)) return;
        int duration = time - v.EntryTime;
        if (duration >= 30)
        {
            int intervals = (duration + 14) / 15;
            TotalFees += intervals * (v.IsCar ? 1.2m : 0.7m);
        }
        parked.Remove(id);
        if (v.IsCar) carCount--; else motoCount--;
    }

    public void CloseDay()
    {
        foreach (var v in parked.Values)
            TotalFees += 30m;
        parked.Clear();
    }

    public string Summary()
    {
        return TotalFees.ToString("0.0", CultureInfo.InvariantCulture)
            + " " + CarsRejected
            + " " + MotosRejected;
    }
}

class VehicleInfo
{
    public string Id { get; }
    public int EntryTime { get; }
    public bool IsCar { get; }
    public VehicleInfo(string id, int entry, bool isCar)
    {
        Id = id;
        EntryTime = entry;
        IsCar = isCar;
    }
}
