using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FindLocks {
    class Program {
        static void Main(string[] args) {
            var config = new ConfigurationBuilder()
                .AddJsonFile("appSettings.json", false, false)
                .Build();
            var appSettings = new AppSettings();
            config.Bind(appSettings);
            // --
            if (args.Length == 0) {
                Console.WriteLine("You must specify a (partial) file path to search.");
                return;
            }
            if (string.IsNullOrWhiteSpace(appSettings.HandlePath)) {
                Console.WriteLine($"The Handle.exe path wasn't specified in appSettings.");
                return;
            }
            if (!System.IO.File.Exists(appSettings.HandlePath)) {
                Console.WriteLine($"The Handle.exe wasn't found: {appSettings.HandlePath}");
                return;
            }
            var proc = new Process {
                StartInfo = new ProcessStartInfo {
                    FileName = appSettings.HandlePath,
                    Arguments = $"-accepteula -u {args[0]}",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true,
                }
            };
            proc.Start();
            var handles = new List<Handle>();
            var pattern = @"^(?<PNAME>[^ ]*)\s*pid: (?<PID>[0-9]*)\s*type: (?<TYPE>[^ ]*)\s*(?<USER>[^ ]*)\s*(?<HID>.*?): (?<HNAME>.*)";
            var regex = new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);
            while (!proc.StandardOutput.EndOfStream) {
                var line = proc.StandardOutput.ReadLine();
                if (string.IsNullOrWhiteSpace(line)) continue;
                var matches = regex.Matches(line);
                if (matches.Count == 0) continue;
                var groups = matches[0].Groups;
                var handle = new Handle();
                handles.Add(handle);
                handle.ProcessName = groups.GetValueOrDefault("PNAME").Value;
                handle.ProcessId = groups.GetValueOrDefault("PID").Value;
                handle.Type = groups.GetValueOrDefault("TYPE").Value;
                handle.User = groups.GetValueOrDefault("USER").Value;
                handle.HandleId = groups.GetValueOrDefault("HID").Value;
                handle.Name = groups.GetValueOrDefault("HNAME").Value;
            }
            var results = handles.Select(x => new { x.ProcessName, x.ProcessId, x.User }).Distinct();
            foreach (var x in results)
                Console.WriteLine($"{x.User}: {x.ProcessName} ({x.ProcessId})");
        }
    }

    class AppSettings { 
        public string HandlePath { get; set; }
    }

    class Handle {
        public string ProcessName { get; set; }
        public string ProcessId { get; set; }
        public string Type { get; set; }
        public string User { get; set; }
        public string HandleId { get; set; }
        public string Name { get; set; }
    }
}
