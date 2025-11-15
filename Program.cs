using System;
using System.Windows.Forms;
using System.Reflection;

[assembly: AssemblyTitle("VRCChatbox")]
[assembly: AssemblyProduct("VRCChatbox")]
[assembly: AssemblyDescription("A simple input field for VRChat chat box with better support of IME.")]
[assembly: AssemblyCopyright("Copyright 2025 Jeremy Lam aka. Vistanz. Licensed under MIT.")]
[assembly: AssemblyVersion("0.0.1.0")]
[assembly: AssemblyFileVersion("0.0.1.0")]
[assembly: AssemblyInformationalVersion("0.0.1")]

namespace ChatboxApp {
    internal static class Program {
        [STAThread]
        static void Main() {
            ApplicationConfiguration.Initialize();
            Application.Run(new ChatForm());
        }
    }
}
