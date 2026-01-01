using System.Text;
using System.Text.Json;
using EventSourcingExample.Events;
using EventSourcingExample.Models;
using EventStore.Client;

var settings = EventStoreClientSettings.Create("esdb://localhost:2113?tls=false");
var eventStoreClient = new EventStoreClient(settings);
Func<object, EventData> wrap = e => new(
    eventId: Uuid.NewUuid(),
    type: e.GetType().Name,
    data: JsonSerializer.SerializeToUtf8Bytes(e)
);

AccountCreatedEvent accountCreatedEvent = new AccountCreatedEvent()
{
    AccountId = Guid.NewGuid().ToString(),
    CustomerId =  Guid.NewGuid().ToString(),
    StartBalance = 0,
    Date =  DateTime.Now
};

MoneyDepositedEvent moneyDepositedEvent = new MoneyDepositedEvent()
{
    AccountId = accountCreatedEvent.AccountId,
    Amount = 2000,
    Date =  DateTime.Now
};

MoneyDepositedEvent moneyDepositedEvent2 = new MoneyDepositedEvent()
{
    AccountId = accountCreatedEvent.AccountId,
    Amount = 500,
    Date =  DateTime.Now
};

MoneyWithdrawnEvent moneyWithdrawnEvent = new MoneyWithdrawnEvent()
{
    AccountId = accountCreatedEvent.AccountId,
    Amount = 1000,
    Date = DateTime.Now
};

MoneyTransferedEvent moneyTransferedEvent = new MoneyTransferedEvent()
{
    AccountId = accountCreatedEvent.AccountId,
    TargetAccountId = Guid.NewGuid().ToString(),
    Amount = 500,
    Date = DateTime.Now
};

MoneyTransferedEvent moneyTransferedEvent2 = new MoneyTransferedEvent()
{
    AccountId = accountCreatedEvent.AccountId,
    TargetAccountId = Guid.NewGuid().ToString(),
    Amount = 100,
    Date = DateTime.Now
};

var streamName = $"customer-{accountCreatedEvent.CustomerId}-stream";
var balanceInfo = new BalanceInfo { AccountId = accountCreatedEvent.AccountId, Balance = accountCreatedEvent.StartBalance };

await eventStoreClient.AppendToStreamAsync(
    streamName: streamName,
    eventData: new []
    {
        wrap(accountCreatedEvent),
        wrap(moneyDepositedEvent),
        wrap(moneyDepositedEvent2),
        wrap(moneyTransferedEvent),
        wrap(moneyTransferedEvent2),
        wrap(moneyWithdrawnEvent),
    },
    expectedState: StreamState.Any
);

using var subscription = await eventStoreClient.SubscribeToStreamAsync(
    streamName,
    FromStream.Start,
    subscriptionDropped: (x,y,z) => Console.WriteLine("Disconnected"),
    eventAppeared: (_, resolvedEvent, _) =>
    {
        var data = Encoding.UTF8.GetString(resolvedEvent.Event.Data.Span);
        var streamId = resolvedEvent.OriginalStreamId;
        Console.WriteLine($"{resolvedEvent.Event.EventNumber}@{streamId} {resolvedEvent.Event.EventType}: {data}");

        switch (resolvedEvent.Event.EventType)
        {
            case nameof(AccountCreatedEvent):
                var created = JsonSerializer.Deserialize<AccountCreatedEvent>(data);
                balanceInfo.Balance = created?.StartBalance ?? balanceInfo.Balance;
                break;
            case nameof(MoneyDepositedEvent):
                var deposit = JsonSerializer.Deserialize<MoneyDepositedEvent>(data);
                if (deposit?.Amount is { } dep) balanceInfo.Balance += dep;
                break;
            case nameof(MoneyWithdrawnEvent):
                var withdraw = JsonSerializer.Deserialize<MoneyWithdrawnEvent>(data);
                if (withdraw?.Amount is { } w) balanceInfo.Balance -= w;
                break;
            case nameof(MoneyTransferedEvent):
                var transfer = JsonSerializer.Deserialize<MoneyTransferedEvent>(data);
                if (transfer?.Amount is { } t) balanceInfo.Balance -= t;
                break;
        }

        Console.WriteLine($"Güncel bakiye (Customer {accountCreatedEvent.CustomerId}): {balanceInfo.Balance}");
        return Task.CompletedTask;
    });

// Küçük bir bekleme: abonelikten mevcut olayların yazılmasını bekleyelim.
await Task.Delay(1000);

Console.WriteLine($"Toplam bakiye (Customer {accountCreatedEvent.CustomerId}): {balanceInfo.Balance}");

Console.Read();

