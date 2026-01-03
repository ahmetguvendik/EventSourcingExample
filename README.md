EventSourcingExample
====================

Basit bir EventStoreDB örneği: hesap açma, para yatırma/çekme ve transfer olaylarını oluşturup EventStoreDB’ye yazar, ardından tüm `customer-` stream’lerini dinleyip bakiyeleri konsola loglar.

Önkoşullar
----------
- .NET 9 SDK
- EventStoreDB (varsayılan: `localhost:2113`, TLS kapalı)

Kurulum
-------
```
dotnet restore
```

Çalıştırma
----------
```
dotnet run --project EventSourcingExample/EventSourcingExample.csproj
```

Ne yapar?
---------
- Rastgele `CustomerId`/`AccountId` ile örnek olaylar üretir (`AccountCreatedEvent`, `MoneyDepositedEvent`, `MoneyWithdrawnEvent`, `MoneyTransferedEvent`).
- Olayları `customer-{CustomerId}-stream` adıyla EventStoreDB’ye yazar (`AppendToStreamAsync`).
- `$all` üzerinden `customer-` prefix’li tüm stream’lere abone olur (`SubscribeToAllAsync` + prefix filter) ve her müşteri için `BalanceInfo` sözlüğü tutarak bakiyeyi günceller/ekrana yazar.

Konfigürasyon
-------------
- Bağlantı dizesi: `esdb://localhost:2113?tls=false` (TLS açık ise `?tls=true` ve gerekirse `&tlsVerifyCert=false` ekleyin, kullanıcı/parola sağlamanız gerekebilir).

Örnek çıktı
-----------
```
0@customer-...-stream AccountCreatedEvent: {...}
Güncel bakiye (Customer ...): 0
1@customer-...-stream MoneyDepositedEvent: {...}
Güncel bakiye (Customer ...): 1000
...
Toplam bakiye (Customer ...): ...
```

Notlar
------
- `$all` aboneliği bazı kurulumlarda kimlik doğrulama ister; ihtiyaç olursa uygun kullanıcı/parola ile çalıştırın.
- Uyarıları gidermek isterseniz event modellerine `required` ekleyebilir veya varsayılan değer atayabilirsiniz.

