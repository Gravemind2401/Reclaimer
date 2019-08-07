using System;
using System.Diagnostics;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Ipc;
using System.Threading;

// https://stackoverflow.com/questions/12000591/open-file-with-a-running-process

namespace Reclaimer
{
    internal static class InstanceManager
    {
        public static bool CreateSingleInstance(string name, EventHandler<InstanceCallbackEventArgs> callback)
        {
            EventWaitHandle eventWaitHandle = null;
            string eventName = string.Format("{0}-{1}", Environment.MachineName, name);

            InstanceProxy.IsFirstInstance = false;
            InstanceProxy.CommandLineArgs = Environment.GetCommandLineArgs();

            try
            {
                // try opening existing wait handle
                eventWaitHandle = EventWaitHandle.OpenExisting(eventName);
            }
            catch
            {
                // got exception = handle wasn't created yet
                InstanceProxy.IsFirstInstance = true;
            }

            if (InstanceProxy.IsFirstInstance)
            {
                eventWaitHandle = new EventWaitHandle(false, EventResetMode.AutoReset, eventName);

                // register wait handle for this instance (process)
                ThreadPool.RegisterWaitForSingleObject(eventWaitHandle, WaitOrTimerCallback, callback, Timeout.Infinite, false);
                eventWaitHandle.Close();

                // register shared type (used to pass data between processes)
                RegisterRemoteType(name);
            }
            else
            {
                // pass console arguments to shared object
                UpdateRemoteObject(name);

                // invoke (signal) wait handle on other process
                eventWaitHandle?.Set();
                Environment.Exit(0);
            }

            return InstanceProxy.IsFirstInstance;
        }

        private static void UpdateRemoteObject(string uri)
        {
            // register net-pipe channel
            var clientChannel = new IpcClientChannel();
            ChannelServices.RegisterChannel(clientChannel, true);

            // get shared object from other process
            var proxy = Activator.GetObject(typeof(InstanceProxy), $"ipc://{Environment.MachineName}{uri}/{uri}") as InstanceProxy;

            // pass current command line args to proxy
            proxy?.SetCommandLineArgs(InstanceProxy.IsFirstInstance, InstanceProxy.CommandLineArgs);

            // close current client channel
            ChannelServices.UnregisterChannel(clientChannel);
        }

        private static void RegisterRemoteType(string uri)
        {
            // register remote channel (net-pipes)
            var serverChannel = new IpcServerChannel(Environment.MachineName + uri);
            ChannelServices.RegisterChannel(serverChannel, true);

            // register shared type
            RemotingConfiguration.RegisterWellKnownServiceType(typeof(InstanceProxy), uri, WellKnownObjectMode.Singleton);

            // close channel, on process exit
            Process process = Process.GetCurrentProcess();
            process.Exited += delegate { ChannelServices.UnregisterChannel(serverChannel); };
        }

        private static void WaitOrTimerCallback(object state, bool timedOut)
        {
            // invoke event handler on other process
            var callback = state as EventHandler<InstanceCallbackEventArgs>;
            callback?.Invoke(state, new InstanceCallbackEventArgs(InstanceProxy.IsFirstInstance, InstanceProxy.CommandLineArgs));
        }
    }

    internal class InstanceCallbackEventArgs : EventArgs
    {
        public bool IsFirstInstance { get; }
        public string[] Arguments { get; }

        public InstanceCallbackEventArgs(bool firstInstance, params string[] args)
        {
            IsFirstInstance = firstInstance;
            Arguments = args;
        }
    }

    [Serializable]
    [System.Security.Permissions.PermissionSet(System.Security.Permissions.SecurityAction.Demand, Name = "FullTrust")]
    internal class InstanceProxy : MarshalByRefObject
    {
        private static bool firstInstance;
        private static string[] arrCommandLineArgs;

        public static bool IsFirstInstance
        {
            get { return firstInstance; }
            set { firstInstance = value; }
        }

        public static string[] CommandLineArgs
        {
            get { return arrCommandLineArgs; }
            set { arrCommandLineArgs = value; }
        }

        public void SetCommandLineArgs(bool isFirstInstance, string[] commandLineArgs)
        {
            firstInstance = isFirstInstance;
            arrCommandLineArgs = commandLineArgs;
        }
    }
}