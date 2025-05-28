// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

namespace NBB.Messaging.Kafka
{
    public class KafkaOptions
    {
        public string BootstrapServers { get; set; }
        public string GroupId { get; set; }
        public string ClientId { get; set; }
        public int? AckWait { get; set; }
    }
}
