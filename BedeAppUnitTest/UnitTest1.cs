namespace BedeAppUnitTest;

public class Tests
{
    Configuration Configuration;

    [SetUp]
    public void Setup()
    {
        Configuration = new(
            ticketCost: 1.00m,
            minPlayerCount: 10,
            maxPlayerCount: 15,
            minTicketPurchases: 1,
            maxTicketPurchases: 10,
            cpuStartingBalance: 10.0m
        );
    }

    [Test]
    public void CannotPurchaseMoreThanMaximum()
    {
        Player player = new(1, 10m);
        Lottery lottery = new(Configuration);
        Assert.Throws<Exception>(() => lottery.PurchaseTickets(player, Configuration.MaxTicketPurchases + 1));
    }

    [Test]
    public void CannotPurchaseFewerThanMinimum()
    {
        Player player = new(2, 10m);
        Lottery lottery = new(Configuration);
        Assert.Throws<Exception>(() => lottery.PurchaseTickets(player, Configuration.MinTicketPurchases - 1));
    }

    [Test]
    public void PurchaseUpdatesPlayerBalance()
    {
        decimal playerStartingBalance = 10m;
        int ticketsToPurchase = 6;
        decimal expectedBalance = playerStartingBalance - ticketsToPurchase;
        Player player = new(3, playerStartingBalance);

        Lottery lottery = new(Configuration);
        lottery.PurchaseTickets(player, ticketsToPurchase);

        Assert.That(expectedBalance, Is.EqualTo(player.Balance));
    }

    [Test]
    public void PurchaseUpdatesTicketsSold()
    {   
        decimal playerStartingBalance = 10m;
        int ticketsToPurchase = 6;
        int playerID = 4;
        Player player = new(playerID, playerStartingBalance);
    
        Lottery lottery = new(Configuration);
        lottery.PurchaseTickets(player, ticketsToPurchase);

        Assert.That(lottery.TicketsSold.Count, Is.EqualTo(ticketsToPurchase));
        Assert.That(lottery.TicketsSold.All(t => t.Recipient.ID == playerID));
    }
}