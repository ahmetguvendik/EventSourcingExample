namespace EventSourcingExample.Events;

public class MoneyTransferedEvent
{
    public string AccountId { get; set; }
    public string TargetAccountId { get; set; }
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
}