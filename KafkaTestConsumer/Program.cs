﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extras.CommonServiceLocator;
using Common.Logging;
using KafkaMessages;
using Microsoft.Practices.ServiceLocation;
using SlimMessageBus;
using SlimMessageBus.Host.Config;
using SlimMessageBus.Host.Kafka;
using SlimMessageBus.Host.Serialization.Json;
using SlimMessageBus.Host.ServiceLocator;
namespace KafkaTestConsumer
{
    class Program
    {
        private static readonly ILog Log = LogManager.GetLogger<Program>();

        static void Main(string[] args)
        {
            Log.Info("Starting worker...");
            using (var container = ContainerSetup.Create())
            {
                var messagBus = container.Resolve<IMessageBus>();
                Log.Info("Worker ready");


                Console.WriteLine("Press enter to stop the application...");
                Console.ReadLine();

                Log.Info("Stopping worker...");
            }
            Log.Info("Worker stopped");
        }
    }


    public class ContainerSetup
    {
        public static IContainer Create()
        {
            var builder = new ContainerBuilder();

            Configure(builder);

            var container = builder.Build();

            // Set the service locator to an AutofacServiceLocator.
            var csl = new AutofacServiceLocator(container);
            ServiceLocator.SetLocatorProvider(() => csl);

            return container;
        }

        private static void Configure(ContainerBuilder builder)
        {

            // SlimMessageBus
            builder.Register(x => BuildMessageBus())
                .As<SlimMessageBus.IMessageBus>()
                .SingleInstance();

            builder.RegisterType<PingHandler>().AsSelf();
            //builder.RegisterType<GenerateThumbnailRequestSubscriber>().AsSelf();
        }

        private static IMessageBus BuildMessageBus()
        {
            // unique id across instances of this application (e.g. 1, 2, 3)
            var instanceId = "1";
            var kafkaBrokers = "172.16.4.241:9092";

            var instanceGroup = $"worker-{instanceId}";
            var sharedGroup = $"workers";

            
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




            var messageBusBuilder = new MessageBusBuilder()
             .Handle<PingRequest, PingResponse>(s =>
             {
                 s.Topic("test-ping", t =>
                 {
                     t.Group("ponggroup")
                         .WithHandler<PingHandler>()
                         .Instances(3);

                      //t.Group(sharedGroup)
                      //    .WithConsumer<GenerateThumbnailRequestSubscriber>()
                      //    .Instances(3);
                  });
             })
             .WithDependencyResolverAsServiceLocator()
             .WithSerializer(new JsonMessageSerializer())
             .WithProviderKafka(kafkaSettings);

            var messageBus = messageBusBuilder.Build();
            return messageBus;
        }
    }

    public class PingHandler : IRequestHandler<PingRequest, PingResponse>
    {
        public Task<PingResponse> OnHandle(PingRequest request, string topic)
        {
            Console.WriteLine("Received Request! {0} {1}", request.Key, DateTime.Now - request.Timestamp);
            return Task.FromResult(new PingResponse() {Key = request.Key, Timestamp = DateTime.Now});
        }
    }

}
