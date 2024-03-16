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
HttpClient httpClient = new();

channel.QueueDeclare(
  queue: "payments",
  durable: true,
  exclusive: false,
  autoDelete: false,
  arguments: null
);

EventingBasicConsumer consumer = new(channel);

Console.WriteLine("[*] Waiting for new messages");
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
      await httpClient.PatchAsJsonAsync($"{PSP_URL}/payments/pix", new { Id = message.Id, Status = "SUCCESS" });
      await httpClient.PatchAsync($"{API_URL}/payments/{message.Id}/SUCCESS", null);
      Console.WriteLine("[*] Success Payment - Requests for PSP and API successfully sent!");
      channel.BasicAck(ea.DeliveryTag, false);
    }
    catch
    {
      Console.WriteLine("[#] Success Payment - Failed to send requests for PSP and API...");

      // PSP or API is down - set timeout response
      Thread.Sleep(3000);

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
      await httpClient.PatchAsJsonAsync($"{PSP_URL}/payments/pix", new { Id = message.Id, Status = "FAILED" });
      await httpClient.PatchAsync($"{API_URL}/payments/{message.Id}/FAILED", null);
      Console.WriteLine("[*] Failed Payment - Requests for PSP and API successfully sent!");
      channel.BasicReject(ea.DeliveryTag, false);
    }
    catch
    {
      Console.WriteLine("[#] Failed Payment - Failed to send requests for PSP and API...");

      // PSP or API is down - set timeout response
      Thread.Sleep(3000);

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

Console.ReadLine();