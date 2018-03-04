using System;
using System.Threading;
using System.Runtime.InteropServices;

namespace WorkBalancer
{
    class Threads
    {
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]

        private delegate int Add([param: MarshalAs(UnmanagedType.I4)] int Zahl1, [param: MarshalAs(UnmanagedType.I4)] int Zahl2);

        public Thread Thread { get; set; }
        public bool IsWorking { get; set; }
        public int OrderID { get; set; }
        public int Ergebnis { get; set; }
        public IntPtr PToMethod { get; set; }

        public int DoExecute(Datensaetze bDatensatz, IntPtr PToMethod)
        {
            // Aufrufen der Sprungadresse der Methode in der DLL
            Add Add = (Add)Marshal.GetDelegateForFunctionPointer(PToMethod, typeof(Add));

            // Aufruf der DLL-Methode
            int Ergebnis = Add(bDatensatz.Zahl1, bDatensatz.Zahl2);

            return Ergebnis;
        }
    }

   
}
