using System;
using System.Threading.Tasks;

namespace ChatboxApp {
    public class DynamicThrottler {
        private readonly DateTime[] eventTimestampsDeque;
        private readonly TimeSpan interval;
        private int head = 0, tail = 0;
        private bool isFull;
        private bool hasWaitingTaskScheduled;

        public TimeSpan BufferTime { get; set; }

        public event Action? Callback;

        public DynamicThrottler(TimeSpan interval, int maxEventPerInterval) {
            this.interval = interval;
            eventTimestampsDeque = new DateTime[maxEventPerInterval];
        }

        public bool AcquireNextSafeTime(out TimeSpan nextWaitTime) {
            var now = DateTime.UtcNow;
            if (isFull) {
                var elapsed = now - eventTimestampsDeque[head];
                if (elapsed < interval) {
                    nextWaitTime = interval - elapsed;
                    return false;
                }
                head = (head + 1) % eventTimestampsDeque.Length;
                isFull = false;
            }
            eventTimestampsDeque[tail] = now;
            tail = (tail + 1) % eventTimestampsDeque.Length;
            isFull = tail == head;
            nextWaitTime = TimeSpan.Zero;
            return true;
        }

        public async void Request() {
            if (hasWaitingTaskScheduled) return;
            if (!AcquireNextSafeTime(out var waitTime))
                try {
                    hasWaitingTaskScheduled = true;
                    await Task.Delay(waitTime + BufferTime);
                } finally {
                    hasWaitingTaskScheduled = false;
                }
            Callback?.Invoke();
        }
    }
}