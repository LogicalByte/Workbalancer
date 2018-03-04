using System;

namespace WorkBalancer
{
    class Datensaetze
    {
        public int Zahl1 { get; set; }
        public int Zahl2 { get; set; }
        public string Operator { get; set; }
        public int Ergebnis { get; set; } = 0;
        public int OrderID { get; set; }

        public Datensaetze(string[] input, int Row)
        {
            Zahl1 = Convert.ToInt32(input[0]);
            Operator = input[1];
            Zahl2 = Convert.ToInt32(input[2]);
        }
    }
}
