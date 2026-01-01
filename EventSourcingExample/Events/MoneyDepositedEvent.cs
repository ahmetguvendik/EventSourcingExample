namespace EventSourcingExample.Events;

public class MoneyDepositedEvent
{
    public string AccountId { get; set; }
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
}