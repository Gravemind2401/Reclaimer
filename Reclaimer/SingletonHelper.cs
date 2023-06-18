using Newtonsoft.Json;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;

namespace Reclaimer
{
    internal static class SingletonHelper
    {
        public static bool EnsureSingleInstance(string name, Action<string[]> callback)
        {
            try
            {
                ServerLoop(name, callback);
                return true;
            }
            catch (IOException) //server limit exceeded
            {
                ClientPassThrough(name);
                return false;
            }
        }

        private static void ServerLoop(string pipeName, Action<string[]> callback)
        {
            var server = new NamedPipeServerStream(pipeName, PipeDirection.In, 1);
            Process.GetCurrentProcess().Exited += delegate
            {
                server.Close();
                server.Dispose();
            };

            Task.Run(() =>
            {
                while (true)
                {
                    server.WaitForConnection();
                    using (var reader = new StreamReader(server, leaveOpen: true))
                    {
                        var content = reader.ReadToEnd();
                        var args = JsonConvert.DeserializeObject<string[]>(content);
                        callback(args);
                    }
                    server.Disconnect();
                }
            });
        }

        private static void ClientPassThrough(string pipeName)
        {
            using (var client = new NamedPipeClientStream(".", pipeName, PipeDirection.Out))
            {
                client.Connect();
                using (var writer = new StreamWriter(client))
                    writer.Write(JsonConvert.SerializeObject(Environment.GetCommandLineArgs()));
            }

            Environment.Exit(0);
        }
    }
}