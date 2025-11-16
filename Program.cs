using System;
using System.Windows.Forms;
using System.Reflection;
using System.Runtime.InteropServices;

[assembly: AssemblyTitle("VRCChatbox")]
[assembly: AssemblyProduct("VRCChatbox")]
[assembly: AssemblyDescription("A simple input field for VRChat chat box with better support of IME.")]
[assembly: AssemblyCopyright("Copyright 2025 Jeremy Lam aka. Vistanz. Licensed under MIT.")]
[assembly: AssemblyVersion("0.0.2.0")]
[assembly: AssemblyFileVersion("0.0.2.0")]
[assembly: AssemblyInformationalVersion("0.0.2")]

namespace ChatboxApp {
    internal static class Program {
        [STAThread]
        static void Main() {
            ApplicationConfiguration.Initialize();
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
#pragma warning disable CA1416 // Warning still showing regardless of the platform check
                Application.Run(new ChatForm());
#pragma warning restore CA1416
        }
    }
}
