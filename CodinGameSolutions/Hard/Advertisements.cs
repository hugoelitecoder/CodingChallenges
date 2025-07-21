using System;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;

public class Solution
{
    private static void GeneratePermutations<T>(List<T> elements, int k, List<List<T>> allPermutations)
    {
        if (k == 1) { 
            allPermutations.Add(new List<T>(elements)); 
        }
        else
        {
            GeneratePermutations(elements, k - 1, allPermutations);
            for (int i = 0; i < k - 1; i++)
            {
                if (k % 2 == 0) Swap(elements, i, k - 1); else Swap(elements, 0, k - 1);
                GeneratePermutations(elements, k - 1, allPermutations);
            }
        }
    }
    
    private static void Swap<T>(List<T> list, int i, int j) { 
        T temp = list[i]; 
        list[i] = list[j]; 
        list[j] = temp; 
    }

    static void Main(string[] args)
    {
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

        int na = int.Parse(Console.ReadLine());
        List<Advert> allAdverts = new List<Advert>();
        for (int i = 0; i < na; i++)
        {
            string adLine = Console.ReadLine();
            string[] adLineParts = adLine.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);
            string subjectWithColon = adLineParts[1];
            string subject = subjectWithColon.Substring(0, subjectWithColon.Length - 1);
            int X = int.Parse(adLineParts[2]);
            int Y = int.Parse(adLineParts[adLineParts.Length - 1]);
            allAdverts.Add(new Advert(i, subject, X, Y));
        }

        int ni = int.Parse(Console.ReadLine());
        List<Item> allItems = new List<Item>();
        double initialTotalPrice = 0;
        for (int i = 0; i < ni; i++)
        {
            var itemInputParts = Console.ReadLine().Split();
            Item item = new Item(i, itemInputParts[0], itemInputParts[1], itemInputParts[2],
                                   double.Parse(itemInputParts[3], CultureInfo.InvariantCulture));
            initialTotalPrice += item.Price; allItems.Add(item);
        }

        foreach (var item in allItems) {
            foreach (var advert in allAdverts) {
                if (item.Categories.Contains(advert.Subject)) {
                    item.Adverts.Add(advert.Id);
                    advert.Items.Add(item.Id);
                }
            }
        }

        HashSet<int> conflictingAdIdSet = new HashSet<int>();
        foreach (var item in allItems) {
            if (item.Adverts.Count > 1) {
                foreach(int adId in item.Adverts) conflictingAdIdSet.Add(adId);
            }
        }
        List<int> conflictingAdsToPermute = conflictingAdIdSet.ToList();
        
        List<int> nonConflictingAdIds = new List<int>();
        for (int adId = 0; adId < na; ++adId) {
            if (!conflictingAdIdSet.Contains(adId)) nonConflictingAdIds.Add(adId);
        }
        
        double bestOverallDiscount = -100000.0; 

        List<List<int>> permutationsOfConflicting = new List<List<int>>();
        if (conflictingAdsToPermute.Any()) {
            GeneratePermutations(new List<int>(conflictingAdsToPermute), conflictingAdsToPermute.Count, permutationsOfConflicting);
        } else {
            permutationsOfConflicting.Add(new List<int>()); 
        }

        foreach (var permutedConflicting in permutationsOfConflicting) {
            List<int> currentOrder = new List<int>(permutedConflicting);
            currentOrder.AddRange(nonConflictingAdIds);
            
            var advertOrderProcessor = new AdvertOrder(currentOrder, allAdverts, allItems);
            if (advertOrderProcessor.Discount > bestOverallDiscount) {
                bestOverallDiscount = advertOrderProcessor.Discount;
            }
        }
        
        Console.WriteLine((initialTotalPrice - bestOverallDiscount).ToString("F2", CultureInfo.InvariantCulture));
        Console.WriteLine(bestOverallDiscount.ToString("F2", CultureInfo.InvariantCulture));
    }
}

public class Item
{
    public int Id { get; set; }
    public string Product { get; set; }
    public string Group { get; set; }
    public string Brand { get; set; }
    public double Price { get; set; }
    public List<string> Categories { get; private set; }
    public List<int> Adverts { get; set; }
    public int UsedBy { get; set; }

    public Item(int id, string product, string group, string brand, double price)
    {
        Id = id; Product = product; Group = group; Brand = brand; Price = price;
        Categories = new List<string> { product, group, brand };
        Adverts = new List<int>();
        UsedBy = -1;
    }
}

public class Advert
{
    public int Id { get; set; }
    public string Subject { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
    public List<int> Items { get; set; }

    public Advert(int id, string subject, int x, int y)
    {
        Id = id; Subject = subject; X = x; Y = y; Items = new List<int>();
    }
}

public class ItemGroups
{
    public List<int> UsedMain { get; set; }
    public List<int> UsedSecc { get; set; }
    private List<Item> _allItemsRef;
    private List<Advert> _allAdvertsRef;

    public ItemGroups(List<int> usedMain, List<int> usedSecc, List<Item> allItems, List<Advert> allAdverts)
    {
        UsedMain = usedMain;
        UsedSecc = usedSecc;
        _allItemsRef = allItems; 
        _allAdvertsRef = allAdverts;
    }

    public void Exchange(Advert currentApplyingAd)
    {
        Dictionary<int, int> toExchangeMap = currentApplyingAd.X >= currentApplyingAd.Y
            ? ExchangeA(currentApplyingAd)
            : ExchangeB(currentApplyingAd);

        foreach (var pair in toExchangeMap)
        {
            int iiAvailable = pair.Key; int iiUsed = pair.Value;
            UsedSecc.Add(iiAvailable);
            UsedSecc.Remove(iiUsed); 
            _allItemsRef[iiAvailable].UsedBy = _allItemsRef[iiUsed].UsedBy;
            _allItemsRef[iiUsed].UsedBy = -1;
        }
    }

    private Dictionary<int, int> ExchangeA(Advert currentApplyingAd) 
    {
        UsedSecc = UsedSecc.OrderByDescending(id => _allItemsRef[id].Price).ToList();

        Dictionary<int, int> toExchange = new Dictionary<int, int>();
        foreach (int iiUsed in UsedSecc) 
        {
            Item usedItemObject = _allItemsRef[iiUsed];
            if (!usedItemObject.Adverts.Contains(currentApplyingAd.Id)) continue;

            int originalAdId = usedItemObject.UsedBy;
            if (originalAdId == -1) continue; 
            Advert originalAd = _allAdvertsRef[originalAdId]; 

            List<int> availableCandidatesPart1 = originalAd.Items.Where(iia =>
                _allItemsRef[iia].UsedBy == -1 &&
                !toExchange.ContainsKey(iia) && 
                !currentApplyingAd.Items.Contains(iia)
            ).ToList();

            List<int> availableCandidatesPart2 = originalAd.Items.Where(iia =>
                _allItemsRef[iia].UsedBy == -1 &&
                !toExchange.ContainsKey(iia) &&
                currentApplyingAd.Items.Contains(iia)
            ).OrderBy(iia => _allItemsRef[iia].Price).ToList();
            
            List<int> availableCandidates = availableCandidatesPart1.Concat(availableCandidatesPart2).ToList();

            foreach (int iiAvailable in availableCandidates)
            {
                Item availableItemObject = _allItemsRef[iiAvailable];
                if (!currentApplyingAd.Items.Contains(iiAvailable) || 
                    availableItemObject.Price < usedItemObject.Price)
                {
                    toExchange[iiAvailable] = iiUsed;
                    break; 
                }
            }
        }
        return toExchange;
    }

    private Dictionary<int, int> ExchangeB(Advert currentApplyingAd) 
    {
        List<int> reorderedSecListPart1 = UsedSecc.Where(ii => !currentApplyingAd.Items.Contains(ii)).ToList();
        List<int> reorderedSecListPart2 = UsedSecc.Where(ii => currentApplyingAd.Items.Contains(ii))
                                    .OrderBy(id => _allItemsRef[id].Price).ToList();
        UsedSecc = reorderedSecListPart1.Concat(reorderedSecListPart2).ToList();

        Dictionary<int, int> toExchange = new Dictionary<int, int>();
        foreach (int iiUsed in UsedSecc) 
        {
            Item usedItemObject = _allItemsRef[iiUsed];
            int originalAdId = usedItemObject.UsedBy;
            if (originalAdId == -1) continue;
            Advert originalAd = _allAdvertsRef[originalAdId];

            List<int> availableCandidates = originalAd.Items.Where(iia =>
                _allItemsRef[iia].UsedBy == -1 &&
                !toExchange.ContainsKey(iia) &&
                currentApplyingAd.Items.Contains(iia) 
            ).OrderByDescending(id => _allItemsRef[id].Price).ToList();

            foreach (int iiAvailable in availableCandidates)
            {
                Item availableItemObject = _allItemsRef[iiAvailable];
                if (!usedItemObject.Adverts.Contains(currentApplyingAd.Id) ||
                    availableItemObject.Price > usedItemObject.Price) 
                {
                    toExchange[iiAvailable] = iiUsed;
                    break;
                }
            }
        }
        return toExchange;
    }
}

public class AdvertOrder
{
    public List<int> Order { get; set; }
    public double Discount { get; set; }
    public ItemGroups Groups { get; set; }
    private List<Advert> _allAdvertsRef;
    private List<Item> _allItemsRef;

    public AdvertOrder(List<int> order, List<Advert> allAdverts, List<Item> allItems)
    {
        Order = order; 
        Discount = 0;
        _allItemsRef = allItems; 
        _allAdvertsRef = allAdverts;
        Groups = new ItemGroups(new List<int>(), new List<int>(), allItems, allAdverts);
        GetDiscount();
    }

    private void GetDiscount()
    {
        foreach (var item in _allItemsRef) item.UsedBy = -1;
        foreach (var adId in Order) ApplyAdvert(adId);
    }

    private void ApplyAdvert(int adId)
    {
        var currentAd = _allAdvertsRef[adId];
        int X = currentAd.X; int Y = currentAd.Y;

        Groups.Exchange(currentAd);

        List<Item> advertPoolSource = currentAd.Items
            .Select(itemId => _allItemsRef[itemId])
            .Where(item => item.UsedBy == -1) 
            .OrderByDescending(item => item.Price).ToList();

        if (X <= 0) return;
        int adnum = advertPoolSource.Count / X;
        if (adnum == 0) return;

        List<Item> mainUseItems = new List<Item>();
        List<Item> secUseItems = new List<Item>();

        if (X >= Y)
        {
            int diffXY = X - Y;
            int mainUseCountK = adnum * diffXY;
            int count1 = Math.Min(mainUseCountK, advertPoolSource.Count);
            if (count1 > 0) mainUseItems = advertPoolSource.GetRange(0, count1);

            if (Y > 0) {
                int secUseCountK = adnum * Y;
                int start2 = Math.Max(0, advertPoolSource.Count - secUseCountK);
                int count2 = advertPoolSource.Count - start2; 
                if (count2 > 0) secUseItems = advertPoolSource.GetRange(start2, count2);
            }
            foreach (var item in mainUseItems) Discount += item.Price;
        }
        else 
        {
            int diffYX = Y - X;
            int mainUseCountK = adnum; 
            int start1 = Math.Max(0, advertPoolSource.Count - mainUseCountK);
            int count1_else = advertPoolSource.Count - start1;
            if (count1_else > 0) mainUseItems = advertPoolSource.GetRange(start1, count1_else);
            
            if (X - 1 > 0) {
                int secUseCountK = adnum * (X - 1);
                int count2_else = Math.Min(secUseCountK, advertPoolSource.Count);
                if (count2_else > 0) secUseItems = advertPoolSource.GetRange(0, count2_else);
            }
            foreach (var item in mainUseItems) Discount -= item.Price * diffYX;
        }

        foreach (var item in mainUseItems) { 
            Groups.UsedMain.Add(item.Id); 
            item.UsedBy = currentAd.Id; 
        }
        foreach (var item in secUseItems) { 
            Groups.UsedSecc.Add(item.Id); 
            item.UsedBy = currentAd.Id; 
        }
    }
}
