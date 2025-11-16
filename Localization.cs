using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace ChatboxApp {
    public partial class Localizer {
        private readonly Dictionary<string, string> active;
        private readonly Dictionary<string, string> fallback;

        public string Culture { get; private set; }

        public Localizer() {
            var locales = LoadEmbeddedLocales();
            fallback = locales.TryGetValue("en", out var en) ? en : new Dictionary<string, string>();
            for (var cult = CultureInfo.CurrentUICulture; cult != CultureInfo.InvariantCulture; cult = cult.Parent)
                if (locales.TryGetValue(cult.Name, out var dict)) {
                    active = dict;
                    Culture = cult.Name;
                    return;
                }
            active = fallback;
            Culture = "en";
        }

        private static Dictionary<string, Dictionary<string, string>> LoadEmbeddedLocales() {
            var locales = new Dictionary<string, Dictionary<string, string>>();
            var matcherRegex = LocalesMatcher();
            var asm = Assembly.GetExecutingAssembly();
            foreach (var name in asm.GetManifestResourceNames()) {
                var match = matcherRegex.Match(name);
                if (!match.Success) continue;
                var culture = match.Groups[1].Value;
                using var s = asm.GetManifestResourceStream(name);
                if (s == null) continue;
                using var sr = new StreamReader(s);
                var json = sr.ReadToEnd();
                try {
                    var dict = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
                    if (dict != null) locales[culture] = dict;
                } catch { }
            }
            return locales;
        }

        public string T(string key) {
            if (string.IsNullOrEmpty(key))
                return "";
            if ((active != null && active.TryGetValue(key, out var value)) ||
                (fallback != null && fallback.TryGetValue(key, out value))) return value;
            return key;
        }

        [GeneratedRegex(@"\.Resources\.locales\.([a-zA-Z\-]+)\.json$", RegexOptions.Compiled)]
        internal static partial Regex LocalesMatcher();
    }
}
