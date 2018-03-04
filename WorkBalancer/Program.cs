using System;
using System.Collections.Generic;
using System.Threading;
using System.Runtime.InteropServices;

namespace WorkBalancer
{
    class Program
    {
        static class NativeMethods
        {
            [DllImport("kernel32.dll")]
            public static extern IntPtr LoadLibrary(string dllToLoad);

            [DllImport("kernel32.dll")]
            public static extern IntPtr GetProcAddress(IntPtr hModule, string procedureName);
        }

        static void Main(string[] args)
        {
            int AnzahlThreads = Environment.ProcessorCount;
            Console.WriteLine(AnzahlThreads + " logische Prozessoren gefunden.");
            Console.Write("Lese Datensaetze ein...");
            List<Datensaetze> Datensaetze = DatensaetzeEinlesen();
            Console.WriteLine("fertig!");

            Console.Write("Berechnen der Datensaetze...");
            Berechnungen(AnzahlThreads, Datensaetze);
            Console.WriteLine("fertig!");
        }

        static List<Datensaetze> DatensaetzeEinlesen()
        {
            string line = "";
            int Count = 0;
            List < Datensaetze > Datensaetze = new List<Datensaetze>();

            System.IO.StreamReader file = new System.IO.StreamReader("source.txt");
            while((line = file.ReadLine()) != null) 
            {
                Count++;
                string[] input = line.Split(' ');
                Datensaetze.Add(new Datensaetze(input, Count));
            }

            file.Close();
            return Datensaetze;
        }

        static void Berechnungen(int bAnzahlThreads, List<Datensaetze> bDatensaetze)
        {
            // Berechnungsschleifenvariable, ob alle Datensätze berechnet und alle Threads fertig sind
            bool bcontinue = false;
            // Welcher Datensatz wird als nächstes berechnet?
            int dataCount = 0;

            Threads[] Threads = new Threads[bAnzahlThreads];
           
            for (int i = 1; i <= bAnzahlThreads; i++)
            {
                Threads[i - 1] = new Threads
                {
                    Thread = null,
                    IsWorking = false,
                    OrderID = 0,
                    Ergebnis = -1,
                    PToMethod = NativeMethods.GetProcAddress(NativeMethods.LoadLibrary("sampledll.dll"), "Add")
                };
            }

            while (bcontinue == false)
            {
                bcontinue = true;
                // Durchlauf aller Threads
                for (int i = 0; i <= bAnzahlThreads - 1; i++)
                {
                    // Abfrage, ob dem Thread eine Berechnung zugewiesen wurde und Datensätze noch nicht berechnet wurden
                    if ((Threads[i].IsWorking == false) && (dataCount < bDatensaetze.Count))
                    {
                        // While-Schleife wird auf jeden Fall noch 1 Mal wiederholt.
                        bcontinue = false;
                        Threads[i].IsWorking = true;
                        // Wichtig den Wert hier abzuspeichern und dann zu verwenden, da es sonst zu Zählfehler wegen Asynchronität kommen kann (wegen dataCount++)
                        Threads[i].OrderID = dataCount;
                        // Zuweisen der im thread verwendeten Methode
                        Threads[i].Thread = new Thread(delegate () { Threads[i].Ergebnis = Threads[i].DoExecute(bDatensaetze[Threads[i].OrderID], Threads[i].PToMethod); });

                        Threads[i].Thread.Start();
                        dataCount++;
                        // Fortschrittsanzeige
                        //drawProgressInfo(startzeit, dataCount, datensätze.Count, countHCMFehler, stopWatch, true);
                        // break ist wichtig, da sonst i zu weit hochgezählt werden könnte und dadurch ein OutofRange entsteht
                        break;
                    }
                    // Abfrage, ob dem Thread eine Berechnung zugewiesen wurde und der Thread noch daran arbeitet
                    // Die Schleife wird nochmal durchlaufen, wenn dem thread eine Arbeit zugewiesen wurde bzw. worden war. Nur zur Sicherheit. ;)
                    if (Threads[i].IsWorking == true)
                    {
                        bcontinue = false;
                        if (!Threads[i].Thread.IsAlive)
                        {
                            // Wenn nicht, wird das Ergebnis in die Ergebnisliste geschrieben (inkl. Datensatznummer für die spätere Sortierung)
                            Threads[i].IsWorking = false;
                            for (int j = 0; j <= bDatensaetze.Count - 1; j++)
                            {
                                if (bDatensaetze[j].OrderID == Threads[i].OrderID) { bDatensaetze[j].Ergebnis = Threads[i].Ergebnis; break; }
                            }
                        }
                    }
                    // Sind noch Datensätze vorhanden? Wenn ja, dann durchlaufe nochmal die Schleife
                    if (dataCount < bDatensaetze.Count) { bcontinue = false; }
                }
            }
        }
    }
}
