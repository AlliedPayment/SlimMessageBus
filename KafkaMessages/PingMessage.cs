using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KafkaMessages
{
    public class PingRequest :SlimMessageBus.IRequestMessage<PingResponse>
    {
        public DateTimeOffset Timestamp { get; set; }
        public string Key { get; set; }
    }
    public class PingResponse
    {
        public DateTimeOffset Timestamp { get; set; }
        public string Key { get; set; }
    }
}
