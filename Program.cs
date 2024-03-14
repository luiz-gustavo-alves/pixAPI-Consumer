using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

ConnectionFactory factory = new() 
{
  HostName = "localhost"
};

var connection = factory.CreateConnection();
var channel = connection.CreateModel();

channel.QueueDeclare(
  queue: "payments",
  durable: false,
  exclusive: false,
  autoDelete: false,
  arguments: null  
);

EventingBasicConsumer consumer = new(channel);

Console.WriteLine("[*] Waiting for new messages");
consumer.Received += async(model, ea) => 
{
  var body = ea.Body.ToArray();
  var message = Encoding.UTF8.GetString(body);
  Console.WriteLine($"{message}");
};

channel.BasicConsume(
  queue: "payments",
  autoAck: true,
  consumer: consumer
);

Console.ReadLine();