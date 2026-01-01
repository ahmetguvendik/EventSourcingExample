namespace EventSourcingExample.Events;

public class MoneyWithdrawnEvent
{
    public string AccountId { get; set; }
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
}