using System;

namespace WorkBalancer
{
    class Datensaetze
    {
        public int Zahl1 { get; set; }
        public int Zahl2 { get; set; }
        public int Ergebnis { get; set; } = -1;
        public int OrderID { get; set; }

        public Datensaetze(string[] input, int bOrderID)
        {
            Zahl1 = Convert.ToInt32(input[0]);
            Zahl2 = Convert.ToInt32(input[1]);
            OrderID = bOrderID;
        }
    }
}
