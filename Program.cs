using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Newtonsoft.Json;
using consumer.DTOs;
using System.Net.Http.Json;
using System.Net.Http.Headers;

ConnectionFactory factory = new()
{
  HostName = "localhost"
};

var connection = factory.CreateConnection();
var channel = connection.CreateModel();

string API_URL = "http://localhost:5180";
string PSP_URL = "http://localhost:5280";
HttpClient httpClient = new() 
{
  Timeout = TimeSpan.FromSeconds(120)
};

channel.QueueDeclare(
  queue: "payments",
  durable: true,
  exclusive: false,
  autoDelete: false,
  arguments: null
);

EventingBasicConsumer consumer = new(channel);
consumer.Received += async (model, ea) =>
{
  var body = ea.Body.ToArray();
  PaymentMessageServiceDTO? message = JsonConvert.DeserializeObject<PaymentMessageServiceDTO>(Encoding.UTF8.GetString(body));
  if (message is null)
  {
    channel.BasicReject(ea.DeliveryTag, false);
  }

  long timeToLive = (long)ea.BasicProperties.Headers["time-to-live"];
  if (timeToLive > new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds())
  {
    try
    {
      Console.WriteLine("[*] Success Payment - Sending requests for PSP and API...");
      httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", message.Token);
      await httpClient.PostAsJsonAsync($"{PSP_URL}/payments/pix", message.DTO);

      UpdatePaymentStatusDTO dto = new() { Status = "SUCCESS" };
      var serializedDTO = JsonConvert.SerializeObject(dto);
      var content = new StringContent(serializedDTO, Encoding.UTF8, "application/json-patch+json");
      await httpClient.PatchAsync($"{API_URL}/payments/{message.Id}", content);

      await httpClient.PatchAsJsonAsync($"{PSP_URL}/payments/pix", new { Id = message.Id, Status = dto.Status });
      Console.WriteLine("[*] Success Payment - Requests for PSP and API successfully sent!");
      channel.BasicAck(ea.DeliveryTag, false);
    }
    catch
    {
      Console.WriteLine("[#] Success Payment - Failed to send requests for PSP and API...");
      var newHeaders = ea.BasicProperties.Headers;
      channel.BasicReject(ea.DeliveryTag, false);
      channel.BasicPublish(exchange: "",
        routingKey: "payments",
        basicProperties: ea.BasicProperties,
        body: body
      );
    }
  }
  else
  {
    try
    {
      Console.WriteLine("[*] Failed Payment - Sending requests for PSP and API...");
      httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", message.Token);
      await httpClient.PostAsJsonAsync($"{PSP_URL}/payments/pix", message.DTO);

      UpdatePaymentStatusDTO dto = new() { Status = "FAILED" };
      var serializedDTO = JsonConvert.SerializeObject(dto);
      var content = new StringContent(serializedDTO, Encoding.UTF8, "application/json-patch+json");
      await httpClient.PatchAsync($"{API_URL}/payments/{message.Id}", content);

      await httpClient.PatchAsJsonAsync($"{PSP_URL}/payments/pix", new { Id = message.Id, Status = dto.Status });
      Console.WriteLine("[*] Failed Payment - Requests for PSP and API successfully sent!");
      channel.BasicReject(ea.DeliveryTag, false);
    }
    catch
    {
      Console.WriteLine("[#] Failed Payment - Failed to send requests for PSP and API...");

      // PSP or/and API is down - set 5s timeout
      Thread.Sleep(5000);

      var newHeaders = ea.BasicProperties.Headers;
      channel.BasicReject(ea.DeliveryTag, false);
      channel.BasicPublish(exchange: "",
        routingKey: "payments",
        basicProperties: ea.BasicProperties,
        body: body
      );
    }
  }
};

channel.BasicConsume(
  queue: "payments",
  autoAck: false,
  consumer: consumer
);

Console.WriteLine("[*] Waiting for new messages");
Console.ReadLine();