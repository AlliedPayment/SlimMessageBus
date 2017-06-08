using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Logging;
using KafkaMessages;
using SlimMessageBus;
using SlimMessageBus.Host;
using SlimMessageBus.Host.Config;
using SlimMessageBus.Host.Kafka;
using SlimMessageBus.Host.Serialization.Json;

namespace KafkaTest
{
    class Program
    {


        private static readonly ILog Log = LogManager.GetLogger<Program>();

        static void Main(string[] args)
        {
            SlimMessageBus.IMessageBus _bus;

            //var testTopic = $"test-ping-{DateTime.Now.Ticks}";
            var topic = $"test-ping";

            // some unique string across all application instances
            var instanceId = "1";
            // address to your Kafka broker
            var kafkaBrokers = "172.16.4.241:9092";
            var kafkaSettings = new KafkaMessageBusSettings(kafkaBrokers);

            kafkaSettings.ProducerConfigFactory = () => new Dictionary<string, object>
            {
                {"batch.num.messages", 1},
                {"message.max.bytes", 1000},
                {"api.version.request", true},
                {"queue.buffering.max.ms", 1},
                {"socket.blocking.max.ms", 1},
                {"fetch.wait.max.ms", 10},
                {"fetch.error.backoff.ms", 10},
                {"fetch.min.bytes", 10},
                {"socket.nagle.disable", true}

            };
            kafkaSettings.ConsumerConfigFactory = (group) => new Dictionary<string, object>
            {
                {"fetch.wait.max.ms", 10},
                {"fetch.error.backoff.ms", 10},
                {"queued.min.messages", 1},
                {"api.version.request", true},
                {"queue.buffering.max.ms", 1},
                {"socket.blocking.max.ms", 1},
                {"fetch.min.bytes", 10},
                {"statistics.interval.ms", 500000},
                {"socket.nagle.disable", true}
            };
            //  conf->set("fetch.min.bytes", "1", errstr);
            //  conf->set("queued.min.messages", "1", errstr);
            var messageBusBuilder = new MessageBusBuilder()
                .Publish<PingRequest>(x =>
                {
                    x.DefaultTopic(topic);
                })
                .ExpectRequestResponses(x =>
                {
                    x.ReplyToTopic($"worker-{instanceId}-response");
                    x.Group($"worker-{instanceId}");
                    x.DefaultTimeout(TimeSpan.FromSeconds(30));
                })
                .WithSerializer(new JsonMessageSerializer())
                .WithProviderKafka(kafkaSettings);

            _bus = (KafkaMessageBus)messageBusBuilder.Build();
            var b = new Program();
            bool cancel = false;
            Console.CancelKeyPress += delegate {
                cancel = true;
            };
            Log.Info("Starting worker...");
         //   using (var container = ContainerSetup.Create())
            {
         //       var messagBus = container.Resolve<IMessageBus>();
                Log.Info("Worker ready");
                
                while (!cancel)
                {
                    Log.Info("Sending message");
                    //var tasks = new List<Task<PingResponse>>();
                    //for (int i = 0; i < 1; i++)
                    //{
                        var t = Send(_bus);
               //         tasks.Add(t);
             //       }
              //      Task.WaitAll(tasks.OfType<Task>().ToArray());
             //       foreach (var t in tasks)
                    {
                        var r = t.Result;
                        Console.WriteLine("{0} {1} {2}", r.Key, r.Timestamp, (DateTime.Now - r.Timestamp).ToString());
                    }

                 //   System.Threading.Thread.Sleep(100);
                }

            }
            Log.Info("Worker stopped");
        
      

        }

        private static async Task<PingResponse> Send(IMessageBus _bus)
        {
            var req = new PingRequest() {Key = "Brian", Timestamp = DateTime.Now};
            return await _bus.Send<PingResponse>(req, TimeSpan.FromMinutes(15));
        }

    }
}
