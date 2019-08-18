using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using MQTTnet.Client.Subscribing;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Subscriber
{
    class Program
    {
        private static MqttFactory factory = new MqttFactory();
        private static IMqttClient mqttClient;

        static async Task Main(string[] args)
        {
            Console.WriteLine("MQTT Subscriber");
            mqttClient = factory.CreateMqttClient();

            var options = new MqttClientOptionsBuilder()
                .WithClientId("Client2")
                .WithTcpServer("localhost", 1883)
                .WithCleanSession()
                .Build();

            mqttClient.UseApplicationMessageReceivedHandler(e =>
            {
                Console.WriteLine("### RECEIVED APPLICATION MESSAGE ###");
                Console.WriteLine($"+ Topic = {e.ApplicationMessage.Topic}");
                Console.WriteLine($"+ Payload = {Encoding.UTF8.GetString(e.ApplicationMessage.Payload)}");
                Console.WriteLine($"+ QoS = {e.ApplicationMessage.QualityOfServiceLevel}");
                Console.WriteLine($"+ Retain = {e.ApplicationMessage.Retain}");
                Console.WriteLine();
            });

            mqttClient.UseDisconnectedHandler(async e =>
            {
                Console.WriteLine("### DISCONNECTED FROM SERVER ###");
                await Task.Delay(TimeSpan.FromSeconds(5));

                try
                {
                    await mqttClient.ConnectAsync(options, CancellationToken.None); // Since 3.0.5 with CancellationToken
                }
                catch
                {
                    Console.WriteLine("### RECONNECTING FAILED ###");
                }
            });

            var subscribeOption = new MqttClientSubscribeOptions();

            subscribeOption.TopicFilters.Add(new TopicFilter { Topic = "test" });

            var t = await mqttClient.ConnectAsync(options, CancellationToken.None);
            var k = await mqttClient.SubscribeAsync(subscribeOption, CancellationToken.None);

            Console.ReadKey();
        }
    }
}
