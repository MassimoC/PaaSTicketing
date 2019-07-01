# PaaS Ticketing
Extremely simple PaaS ticketing system used as playground project.
Powered by [Arcus](https://arcus-azure.net) and lot of copying and pasting from Stack Overflow.

![Scenario](./media/scenario.png)

## Sources and Types
app://ticketing.api
- command://order.pay

app://ticketing.services.payment
- command://order.deliver
- command://order.update

app://ticketing.services.shipping
- command://order.update

app://ticketing.services.labeling
- event://order.completed

## CloudEvent
Messages and commands are propagated as CloudEvent
```json
{
  "specversion": "0.2",
  "type": "command://order.pay",
  "source": "app://ticketing.api",
  "id": "b273a33d-7e03-43e8-9055-d5724240db50",
  "time": "2019-04-15T13:41:45.2478895Z",
  "contenttype": "application/json",
  "data": "{\"attendee\":\"Test user\",\"orderId\":\"29f9a5c5-0d22-4f07-93c3-1e769337755c\",\"token\":\"b9249d\"}"
}
```

## EventGrid
Example of the EventGrid sample

```json
[
  {
    "id": "a367a3a5-270d-4ae3-be30-2118f7d130d8",
    "subject": "/ticketing/orders/12345678",
    "data": {
      "OrderId": "12345678",
      "Recipient": "massimo.crippa@something.com"
    },
    "eventType": "event://order.completed",
    "eventTime": "2019-04-01T20:47:21.3876192Z",
    "dataVersion": "1.0",
    "metadataVersion": "1",
    "topic": "/subscriptions/{sid}/resourceGroups/{rg}/providers/Microsoft.EventGrid/topics/{topicname}"
  }
]
```

## JsonPatch
Content-Type: json-patch+json
Path: /core/v1/orders/{oid}
```json
[
	{ 
	"op": "replace", 
	"path": "/status", 
	"value": "Paid" 
	}
]
```

## JWT token (client credentials)
```json
{
  "alg": "RS256",
  "kid": "2d44575830a2a4b16d8e68e7c305ad15",
  "typ": "JWT"
}.{
  "nbf": 1556446005,
  "exp": 1556449605,
  "iss": "http://localhost",
  "aud": [
    "http://localhost/resources",
    "api://ticketing-core"
  ],
  "client_id": "ticketingtestapp",
  "scope": [
    "api://ticketing-core"
  ]
}.[Signature]
```


## NuGet Packages
- Microsoft.AspNetCore.TestHost
- Microsoft.Azure.ServiceBus
- Microsoft.Azure.WebJobs.Extensions.ServiceBus
- Microsoft.Azure.WebJobs.Extensions.EventGrid
- Microsoft.Azure.WebJobs.Extensions.SendGrid
- Microsoft.Azure.WebJobs.Extensions.DurableTask
- Microsoft.AspNetCore.JsonPatch
- IdentityServer4.AccessTokenValidation
- CloudNative.CloudEvents
- Microsoft.Azure.Functions.Extensions

## E2E correlation
![Application Insights end-to-end transaction](./media/e2e-correlation.png)
