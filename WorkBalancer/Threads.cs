using System;
using System.Threading;

namespace WorkBalancer
{
    class Threads
    {
        public Thread Thread { get; set; }
        public bool IsWorking { get; set; }
        public int OrderID { get; set; }
        public int Ergebnis { get; set; }


        /// <summary>
        /// Summiert zwei Zahlen.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <remarks>
        /// Die Berechnung wartet zufällig zwischen 300 und 5000ms, bis das Ergebnis
        /// zurückgegeben wird. Dadurch wird die Arbeit des Workbalancers hervorgehoben.
        /// Einen anderen Zweck erfüllt die Verzögerung nicht.
        /// </remarks>
        public void Add(int a, int b)
        {
            Random rnd = new Random();

            Thread.Sleep(rnd.Next(300, 5001));
            Ergebnis = a + b;
        }
    }  
}
