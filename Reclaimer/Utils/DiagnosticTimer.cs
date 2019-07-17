using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Reclaimer.Utils
{
    internal class DiagnosticTimer : IDisposable
    {
        private readonly string caller;
        private readonly DateTime start;

        public DiagnosticTimer([CallerMemberName] string caller = null)
        {
            this.caller = caller;
            start = DateTime.Now;
        }

        public void Dispose()
        {
            var end = DateTime.Now;
            System.Diagnostics.Debug.WriteLine($"{caller} took {(end - start).TotalMilliseconds}ms");
        }
    }
}
