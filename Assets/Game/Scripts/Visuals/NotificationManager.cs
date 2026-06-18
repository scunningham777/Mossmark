using System;

namespace Mossmark.Visuals
{
    public static class NotificationManager
    {
        public static event Action<string> MessagePosted;

        public static void Post(string message) => MessagePosted?.Invoke(message);
    }
}
