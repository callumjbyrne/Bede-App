Configuration configuration = new(
    ticketCost: 1.00m,
    minPlayerCount: 10,
    maxPlayerCount: 15,
    minTicketPurchases: 1,
    maxTicketPurchases: 10,
    cpuStartingBalance: 10.0m
);
Lottery theLottery = new(configuration);
decimal startingBalance = 10.0m;
Player thePlayer = new(1, startingBalance);
theLottery.Players.Insert(0, thePlayer);
Game theGame = new(theLottery, thePlayer);
theGame.Play();

// Classes
public class Player(int id, decimal startingBalance)
{
    public int ID = id;
    public string Name {
        get {
            return $"Player {ID}";
        }
    }
    public int TicketsPurchased = 0;
    public decimal Balance = startingBalance;
}

public class Configuration(decimal ticketCost, int minPlayerCount, int maxPlayerCount, int minTicketPurchases, int maxTicketPurchases, decimal cpuStartingBalance)
{
    public decimal TicketCost = ticketCost;
    public int MinPlayerCount = minPlayerCount;
    public int MaxPlayerCount = maxPlayerCount;
    public int MinTicketPurchases = minTicketPurchases;
    public int MaxTicketPurchases = maxTicketPurchases;
    public decimal CPUStartingBalance = cpuStartingBalance;
}

public class Ticket(Player recipient)
{
    public Player Recipient = recipient;
}

public class Lottery(Configuration configuration)
{
    public Configuration Configuration = configuration;
    public List<Ticket> TicketsSold = [];
    public decimal TotalPool {
        get { return TicketsSold.Count * Configuration.TicketCost; }
    }
    public List<Player> Players = [];
    private List<Ticket> WinningTickets = [];
    private decimal TotalPrizeMoney = 0;

    public int PurchaseTickets(Player player, int ticketsToPurchase) {
        decimal purchaseCost = ticketsToPurchase * Configuration.TicketCost;
        if (ticketsToPurchase > Configuration.MaxTicketPurchases || ticketsToPurchase < Configuration.MinTicketPurchases) {
            throw new Exception($"Cannot purchase {ticketsToPurchase} tickets. Please choose a value in the permitted range ({Configuration.MinTicketPurchases} - {Configuration.MaxTicketPurchases})");
        }
        else if (purchaseCost > player.Balance) {
            int maxAffordableTickets = (int)Math.Floor(player.Balance / Configuration.TicketCost);
            ticketsToPurchase = maxAffordableTickets;
        }
        player.TicketsPurchased = ticketsToPurchase;
        player.Balance -= purchaseCost;
        for (int i = 0; i < ticketsToPurchase; i++) {
            TicketsSold.Add(new Ticket(player));
        }
        return player.TicketsPurchased;
    }

    // Prize determination
    /*
        Grand Prize: A single ticket will win 50% of the total ticket revenue.
        Second Tier: 10% of the total number of tickets (rounded to the nearest whole number) will share 30% of the total ticket revenue equally.
        Third Tier: 20% of the total number of tickets (rounded to the nearest whole number) will share 10% of the total ticket revenue equally.
    */    
    public void DeterminePrizes()
    {
        Thread.Sleep(2000);
        Console.WriteLine("Results are now coming in...");

        // Grand Prize
        Thread.Sleep(3000);
        int grandPrizeTicketIndex = new Random().Next(0, TicketsSold.Count-1);
        Ticket grandPrizeTicket = TicketsSold[grandPrizeTicketIndex];
        decimal grandPrizeTicketWinnings = TotalPool * 0.5m;
        Console.WriteLine($"**GRAND PRIZE** - Congratulations, {grandPrizeTicket.Recipient.Name}! You have won the GRAND PRIZE of ${grandPrizeTicketWinnings:0.00}!");
        grandPrizeTicket.Recipient.Balance += grandPrizeTicketWinnings;
        WinningTickets.Add(grandPrizeTicket);
        TotalPrizeMoney += grandPrizeTicketWinnings;

        // Second Tier
        Thread.Sleep(1000);
        int secondTierWinningPct = 10;
        int secondTierWinningShare = 30;
        TotalPrizeMoney += CalculateTieredWinnings(tierName: "SECOND", secondTierWinningPct, secondTierWinningShare);

        // Third Tier
        Thread.Sleep(1000);
        int thirdTierWinningPct = 20;
        int thirdTierWinningShare = 10;
        TotalPrizeMoney += CalculateTieredWinnings(tierName: "THIRD", thirdTierWinningPct, thirdTierWinningShare);

        Console.WriteLine($"House profit: ${TotalPool - TotalPrizeMoney:0.00}");
    }

    public decimal CalculateTieredWinnings(string tierName, int winningPercentage, int distributionPercentage) {
        int tieredTicketCount = (int)(TicketsSold.Count * (winningPercentage/100m));
        List<Ticket> tieredTickets = TicketsSold.Except(WinningTickets).OrderBy(t => new Random().Next()).Take(tieredTicketCount).ToList();
        decimal tieredTicketWinnings = TotalPool * (distributionPercentage/100m);
        decimal tieredDistributedWinnings = tieredTicketWinnings / tieredTicketCount;
        string tieredRecipients = string.Join(tieredTicketCount > 2 ? ", " : " & ", tieredTickets.DistinctBy(t => t.Recipient).Select(t => {
            Player recipient = t.Recipient;
            int recipientWinningTickets = tieredTickets.Count(x => x.Recipient == recipient);

            return $"{t.Recipient.ID}{(recipientWinningTickets > 1 ? $"(x{recipientWinningTickets})" : "")}";
        }));

        foreach (var ticket in tieredTickets) {
            ticket.Recipient.Balance += tieredDistributedWinnings;
        }
        Console.WriteLine($"{tierName} TIER - Congratulations Player{(tieredTicketCount > 1 ? "s" : "")} {tieredRecipients}! You have each won ${tieredDistributedWinnings:0.00}!");
        WinningTickets.AddRange(tieredTickets);
        return tieredTicketWinnings;
    }

    public void Reset()
    {
        TicketsSold = [];
        Players = [];
        WinningTickets = [];
        TotalPrizeMoney = 0;
    }
}

public class Game(Lottery lottery, Player thePlayer)
{
    public void Play()
    {
        string logoText = @"
****************************
*       BEDE LOTTERY       *
****************************       
        ";
        Console.WriteLine(logoText);
        Console.WriteLine($"{thePlayer.Name}, welcome to the Bede Lottery!");

        string introText = $@"
 * Your current balance is ${thePlayer.Balance:0.00}
 * Each ticket costs ${lottery.Configuration.TicketCost:0.00}
 * You may purchase between {lottery.Configuration.MinTicketPurchases} and {lottery.Configuration.MaxTicketPurchases} tickets.
        ";
        Console.WriteLine(introText);

        TakePlayerInput();
        CreateCPUPlayers();

        string totalsText = $@"
 * In total, {lottery.TicketsSold.Count} tickets have been purchased
 * The total prize pool stands at ${lottery.TotalPool:0.00}
        ";
        Console.WriteLine(totalsText);
        
        lottery.DeterminePrizes();
        Thread.Sleep(4000);

        Console.WriteLine("\nPlay again? Y/N");
        string? response = Console.ReadLine(); 
        if (response != null && response.ToLower() == "y") {
            lottery.Reset();
            Play();
        }
    }

    void TakePlayerInput()
    {
        bool validTicketInput = false;
        while (!validTicketInput) {
            Console.WriteLine($"OK {thePlayer.Name}, How many tickets would you like to purchase?");
            if (!int.TryParse(Console.ReadLine(), out int ticketsToPurchase)) {
                Console.WriteLine("Invalid input. Please try again.");
            }
            else {
                try {
                    int purchased = lottery.PurchaseTickets(thePlayer, ticketsToPurchase);
                    validTicketInput = true;
                    if (purchased < ticketsToPurchase) {
                        Console.WriteLine($"Cost to purchase {ticketsToPurchase} tickets (${ticketsToPurchase * lottery.Configuration.TicketCost:0.00}) exceeds current balance. Purchasing maximum affordable instead.");          
                    }
                    Console.WriteLine($"Purchased {purchased} tickets.");
                } catch (Exception e) {
                    Console.WriteLine(e.Message);
                }
            }
        }
    }

    void CreateCPUPlayers()
    {
        // Subtract 1 to account for the human player.
        int cpuCount = new Random().Next(lottery.Configuration.MinPlayerCount-1, lottery.Configuration.MaxPlayerCount-1);

        for (int i = 0; i < cpuCount; i++) {
            int cpuID = i+2;
            int cpuTicketsToPurchase = new Random().Next(lottery.Configuration.MinTicketPurchases, lottery.Configuration.MaxTicketPurchases);
            Player cpuPlayer = new(cpuID, lottery.Configuration.CPUStartingBalance);
            lottery.PurchaseTickets(cpuPlayer, cpuTicketsToPurchase);
            lottery.Players.Add(cpuPlayer);
        }

        Console.WriteLine($"{lottery.Players.Count} players have joined the game:");
        foreach (var player in lottery.Players) {
            Console.WriteLine($" > {player.Name} has purchased {player.TicketsPurchased} tickets.");
        }        
    }
}