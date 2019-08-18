using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Publisher
{
    class Program
    {
        private static MqttFactory factory = new MqttFactory();
        private static IMqttClient mqttClient;

        static async Task Main(string[] args)
        {
            Console.WriteLine("MQTT Publisher");
            mqttClient = factory.CreateMqttClient();
            var options = new MqttClientOptionsBuilder()
                .WithClientId("Client1")
                .WithTcpServer("localhost", 1883)
                .WithCleanSession()
                .Build();

            mqttClient.UseDisconnectedHandler(async e =>
            {
                Console.WriteLine("### DISCONNECTED FROM SERVER ###");
                await Task.Delay(TimeSpan.FromSeconds(5));

                try
                {
                    await mqttClient.ConnectAsync(options, CancellationToken.None);
                }
                catch
                {
                    Console.WriteLine("### RECONNECTING FAILED ###");
                }
            });

            var t = await mqttClient.ConnectAsync(options, CancellationToken.None);
            var message = string.Empty;

            while(message != "exit")
            {
                Console.WriteLine("Write your message or exit for close client");
                message = Console.ReadLine();

                if(!string.IsNullOrEmpty(message) && message != "exit")
                {
                    var mqttMessage = new MqttApplicationMessageBuilder()
                        .WithTopic("test")
                        .WithPayload(message)
                        .WithExactlyOnceQoS()
                        .WithRetainFlag()
                        .Build();

                    var r = await mqttClient.PublishAsync(mqttMessage, CancellationToken.None);
                }
            }
        }
    }
}
