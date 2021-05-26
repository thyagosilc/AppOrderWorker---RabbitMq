using AppOrderWorker.Domain;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;

namespace AppOrderWorker
{
    class Program
    {
        static void Main(string[] args)
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: "PedidoQueue",
                                     durable: false,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) =>
                {
                    try
                    {
                        var body = ea.Body.ToArray();
                        var message = Encoding.UTF8.GetString(body);
                        Console.WriteLine(" [x] Received {0}", message);

                        var order = System.Text.Json.JsonSerializer.Deserialize<Order>(message);

                        Console.WriteLine($"Order: {order.OrderNumber} | {order.ItemName} | {order.Price:N2}", message);
                        channel.BasicAck(ea.DeliveryTag, false);

                    }
                    catch (Exception)
                    {
                        //Logger
                        channel.BasicNack(ea.DeliveryTag, false,true);
                    }


                };

                channel.BasicConsume(queue: "PedidoQueue",
                                     autoAck: false,
                                     consumer: consumer);

                Console.WriteLine(" Press [enter] to exit.");
                Console.ReadLine();
            }
        }
    }
}
