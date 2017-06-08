using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SlimMessageBus.Host.Pipeline
{
    public class PipelineContext
    {
        public IDictionary<string, object> State { get; set; }
        public Type MessageType { get; set; }
        public object Message { get; set; }
        public byte[] Payload { get; set; }
        public string Topic { get; set; }
        public Intents Intent { get; set; }
        public int? Partition { get; set; }
        public TimeSpan? RequestTimeout { get; set; }
        public Type ResponseMessageType { get; set; }
        public CancellationToken CancellationToken { get; set; }
        public IRequestMessage Request { get; set; }
        public PendingRequestState RequestState { get; set; }
        public string RequestId { get; set; }
        public IPendingRequestStore PendingRequestStore { get; set; }
    }

    public enum Intents
    {
        Publish,
        RequestResponse,
        Response
    }
}
