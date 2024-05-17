# Pix API Payments Consumer

Consumer aplication to estabilish communication with PSPs and process payments status from API requests.
- Update payment status to `SUCCESS`.
- Update payment status to `FAILED` (expiration time exceed).

## How to Install and Run the Project
Clone this repository: `git clone https://github.com/luiz-gustavo-alves/pixAPI-Payments-Consumer.git`
<br>
Access root folder and run consumer environment:
```bash
dotnet run
```

## Message DTO
```c#
public class PaymentMessageServiceDTO
{
  public required long Id { get; set; }
  public required string Status { get; set; }
  public required string Token { get; set; }
  public required MakePaymentDTO DTO { get; set; }
}
```

## Error Handler
Invalid Message DTO or without "time-to-live" timestamp on header:
  - Reject message.
 
<br>

API or/and PSP unavaible:
  - Reject and publish same message to queue.

## Links

| Description | URL |
| --- | --- |
| Pix API | https://github.com/luiz-gustavo-alves/pixAPI
| PSP Mock | https://github.com/luiz-gustavo-alves/pixAPI-PSP-Mock
| Concilliation Consumer | https://github.com/luiz-gustavo-alves/pixAPI-Concilliation-Consumer
