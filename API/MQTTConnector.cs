using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using MQTTnet.Server;
using Newtonsoft.Json;

namespace TrabalhoSistemas.API
{
    public static class MQTTConnector
    {
        private static readonly (string, int) Info = ("broker.hivemq.com", 1883);
        public static NodeMestre NodeMestre;
        public static string ID = "provaexpprotcdl";
        public static NodeSecundario[] NodeSecundarios;
        public static IMqttClient Client;
        
        public static async Task<IMqttClient> Start()
        {
            var options = new MqttClientOptionsBuilder()
                .WithTcpServer(Info.Item1, Info.Item2)
                .Build();

            var client = new MqttFactory().CreateMqttClient();
            await client.ConnectAsync(options);
            
            Client = client;
            Console.WriteLine("Connected.");

            NodeMestre = new NodeMestre();
            NodeSecundarios = new[] {new NodeSecundario(), new NodeSecundario() };

//            Console.WriteLine("Conectado");
//            for (int i = 1; i <= NumeroDeVagas; i++)
//            {
//                await Client.SubscribeAsync(new TopicFilterBuilder().WithTopic($"sistemas_ciberfisicos_20192/vaga/{i}").Build());
//                Console.WriteLine($"Inscrito a vaga {i}.");
//            }

            await Client.SubscribeAsync(new TopicFilterBuilder().WithTopic($"{ID}/termometro").Build());
            await Client.SubscribeAsync(new TopicFilterBuilder().WithTopic($"{ID}/amareloe").Build());
            
            Client.UseApplicationMessageReceivedHandler(async e =>
            {
                var parsedPayload = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
                var topic = e.ApplicationMessage.Topic;
                Console.WriteLine($"Recebido:\n  Tópico: {topic}\n    Payload: {parsedPayload}");
                await ParseOperation(topic, parsedPayload);
            });

            return client;
        }

        public static async Task Stop()
        {
            await Client.DisconnectAsync();
        }

        public async static Task ParseOperation(string topic, string payload)
        {
            var splitStr = topic.Split("/");
            var cleanedStr = splitStr[1];

            Console.WriteLine(cleanedStr);

            if (cleanedStr == "amareloe")
            {
                var status = payload == "1";
                NodeMestre.Status = status;
            }

            if (cleanedStr == "termometro")
            {
                NodeMestre.Temperatura = Int32.Parse(payload);
            }
        }
    }

    public class NodeMestre
    {
        public bool Status { get; set; }
        public int Slider { get; set; }
        public int Temperatura { get; set; }
    }
    
    public class NodeSecundario
    {
        public bool Status { get; set; }
        public int Slider { get; set; }
        public string Text { get; set; }

        public NodeSecundario()
        {
            Status = false;
            Slider = 0;
            Text = "";
        }
    }
}