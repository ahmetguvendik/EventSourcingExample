namespace EventSourcingExample.Events;

public class AccountCreatedEvent
{
    public string AccountId { get; set; }
    public string CustomerId { get; set; }
    public int StartBalance { get; set; }
    public DateTime Date { get; set; }
}