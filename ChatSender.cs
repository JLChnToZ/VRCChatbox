using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using OscCore;
using OscCore.LowLevel;

namespace ChatboxApp {
    public class ChatSender : IDisposable {
        static readonly object TRUE = true, FALSE = false;
        private readonly DynamicThrottler sendTextThrottler;
        private readonly UdpClient client;
        private readonly MemoryStream stream;
        private readonly OscWriter writer;
        private readonly object[] sendTextArgs;
        private readonly object[] sendTypingArgs;
        private IPEndPoint? endpoint;
        private string text = "";
        private bool textChangedSinceLastSend;
        private bool sendTyping = true;
        private bool disposed;
        private bool hasNonForcedSendPending, hasForcedSendPending;
        private DateTime lastSendRequestTime = DateTime.MinValue;

        public string Destination {
            get => endpoint?.ToString() ?? "";
            set {
                if (!TrySetDestination(value)) throw new ArgumentException("Invalid destination format.", nameof(value));
            }
        }

        public string Text {
            get => text;
            set {
                if (text == value) return;
                text = value ?? "";
                textChangedSinceLastSend = true;
                if (sendTyping) SendTypingNotification();
            }
        }

        public bool ShouldSendTyping {
            get => sendTyping;
            set => sendTyping = value;
        }

        public ChatSender() {
            sendTextArgs = [null!, TRUE, null!];
            sendTypingArgs = new object[1];
            client = new UdpClient();
            stream = new MemoryStream();
            writer = new OscWriter(stream);
            endpoint = new IPEndPoint(IPAddress.Loopback, 9000);
            sendTextThrottler = new DynamicThrottler(TimeSpan.FromSeconds(5), 3) {
                BufferTime = TimeSpan.FromSeconds(0.5),
            };
            sendTextThrottler.Callback += SendTextActual;
        }

        ~ChatSender() => Dispose(false);

        public bool TrySetDestination(string destination) {
            if (IPEndPoint.TryParse(destination, out var newEndpoint)) {
                endpoint = newEndpoint;
                return true;
            }
            return false;
        }

        public async void SendText(bool forced = true) {
            if (!forced) {
                if (!textChangedSinceLastSend || hasNonForcedSendPending) return;
                var threshold = TimeSpan.FromSeconds(1) - (DateTime.UtcNow - lastSendRequestTime);
                if (threshold > TimeSpan.Zero) {
                    hasNonForcedSendPending = true;
                    await Task.Delay(threshold);
                    hasNonForcedSendPending = false;
                    if (!textChangedSinceLastSend) return;
                }
            } else 
                hasForcedSendPending = true;
            lastSendRequestTime = DateTime.UtcNow;
            sendTextThrottler.Request();
        }

        void SendTextActual() {
            sendTextArgs[0] = text;
            sendTextArgs[2] = hasForcedSendPending ? TRUE : FALSE;
            SendMessage("/chatbox/input", sendTextArgs);
            textChangedSinceLastSend = false;
            hasForcedSendPending = false;
        }

        public void SendTypingNotification() {
            sendTypingArgs[0] = sendTyping && textChangedSinceLastSend && !string.IsNullOrEmpty(text) ? TRUE : FALSE;
            SendMessage("/chatbox/typing", sendTypingArgs);
        }

        void SendMessage(string path, params object[] args) {
            if (endpoint is null) throw new InvalidOperationException("Endpoint is not set.");
            lock(stream) {
                var pos = stream.Position;
                new OscMessage(path, args).Write(writer);
                if (!stream.TryGetBuffer(out var buffer)) return;
                client.Send(buffer.Slice((int)pos).AsSpan(), endpoint);
                stream.Seek(0, SeekOrigin.Begin);
                stream.SetLength(0);
            }
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        void Dispose(bool _) {
            if (disposed) return;
            disposed = true;
            client.Dispose();
        }
    }
}