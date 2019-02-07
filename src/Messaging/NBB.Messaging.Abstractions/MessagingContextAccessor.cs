using System.Threading;

namespace NBB.Messaging.Abstractions
{
    public class MessagingContextAccessor
    {
        private static readonly AsyncLocal<MessagingContext> AsyncLocal = new AsyncLocal<MessagingContext>();

        public MessagingContext MessagingContext
        {
            get => AsyncLocal.Value;
            set => AsyncLocal.Value = value;
        }
    }
}
