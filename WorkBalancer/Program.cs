using System;
using System.Collections.Generic;
using System.Threading;
using System.IO;

namespace WorkBalancer
{
    class Program
    {
        static void Main(string[] args)
        {
            // Herausfinden, wie viele Threads maximal parallel bearbeitet werden können
            int AnzahlThreads = Environment.ProcessorCount;
            Console.WriteLine(AnzahlThreads + " logische Prozessoren gefunden.");

            Console.Write("Lese Datensaetze ein...");
            List<Datensaetze> Datensaetze = DatensaetzeEinlesen();
            Console.WriteLine("fertig!");

            Console.WriteLine("Berechnen der Datensaetze...");
            Berechnungen(AnzahlThreads, Datensaetze);
            Console.WriteLine("...fertig!");

            Console.Write("Schreibe Ergebnisse...");
            DatensaetzeSchreiben(Datensaetze);
            Console.WriteLine("fertig!");
        }

        /// <summary>
        /// Einlesen der Datensätze
        /// </summary>
        /// <returns>Alle eingelesenen Datensätze</returns>
        static List<Datensaetze> DatensaetzeEinlesen()
        {
            string line = "";
            int Count = 0;
            List < Datensaetze > Datensaetze = new List<Datensaetze>();

            try
            {
                StreamReader file = new StreamReader("source.txt");
                while ((line = file.ReadLine()) != null)
                {
                    string[] input = line.Split(' ');
                    Datensaetze.Add(new Datensaetze(input, Count));
                    Count++;
                }
                file.Close();
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                Environment.Exit(0);
            }

            return Datensaetze;
        }

        /// <summary>
        /// Schreiben der Datensätze
        /// </summary>
        /// <param name="bDatensaetze">Datensatz-Liste</param>
        static void DatensaetzeSchreiben(List<Datensaetze> bDatensaetze)
        {
            try
            {
                StreamWriter file = File.CreateText("result.txt");
                for (int i = 0; i <= bDatensaetze.Count - 1; i++)
                {
                    file.WriteLine(bDatensaetze[i].Zahl1 + " " + bDatensaetze[i].Zahl2 + " " + bDatensaetze[i].Ergebnis);
                }
                file.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Environment.Exit(0);
            }
        }

        /// <summary>
        /// Berechnen der Datensätze.
        /// </summary>
        /// <param name="bAnzahlThreads">Maximale Threadanzahl</param>
        /// <param name="bDatensaetze">Datensätze</param>
        /// <remarks>
        /// Berechnung der Datensätze. Zunächst werden die Threads initialisiert. Dies kann wichtig sein, wenn zum Beispiel
        /// aus Klassenbibliotheken Methoden per Referenz übergeben werden. Anstatt die Refrenz später immer wieder neu zu setzen,
        /// geschieht das für jeden Thread nur ein einziges Mal -> Geschwindigkeitsgewinn. In diesem einfachen Beispiel hat das
        /// Vorbereiten keinen Mehrwert.
        /// Der Workbalancer funktioniert wie folgt:
        /// Die Schleife wird solange durchlaufen, bis alle Datensätze berechnet wurden. Dafür sorgt das bcontinue.
        /// Wenn ein Thread untätig ist, wird ihm eine neue Aufgabe zugewiesen. Dies geschieht, indem das Thread-Array 
        /// durchlaufen und deren Status abgefragt wird.
        /// Wenn der Thread nicht mehr "am Leben" ist (IsWorking aber true ist), ist dies ein Zeichen, dass das Ergebnis der
        /// Berechnung vorliegt. Dieses wird an den Datensatz übergeben.
        /// Optimierungsmöglichkeiten:
        /// Eine Pipeline-Struktur könnte die Verarbeitung noch beschleunigen. Zumindest erspart es die dataCount-Variable und
        /// somit letztendlich den Break -> dieser startet die For-Schleife neu und somit wird die Auswertung verlangsamt.
        /// </remarks>
        static void Berechnungen(int bAnzahlThreads, List<Datensaetze> bDatensaetze)
        {
            // Berechnungsschleifenvariable, ob alle Datensätze berechnet und alle Threads fertig sind
            bool bcontinue = false;
            // Welcher Datensatz wird als nächstes berechnet?
            int dataCount = 0;

            // Vorbereiten der Threads, um sie später zu verwenden
            Threads[] Threads = new Threads[bAnzahlThreads];           
            for (int i = 1; i <= bAnzahlThreads; i++)
            {
                Threads[i - 1] = new Threads
                {
                    Thread = null,
                    IsWorking = false,
                    OrderID = 0,
                    Ergebnis = -1,
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
                        Console.WriteLine("Thread " + (i + 1) + " wird gestartet (Datensatz: " + (dataCount + 1) + "/" + bDatensaetze.Count + ").");
                        // While-Schleife wird auf jeden Fall noch 1 Mal wiederholt.
                        bcontinue = false;
                        Threads[i].IsWorking = true;
                        // Wichtig den Wert hier abzuspeichern und dann zu verwenden, da es sonst zu Zählfehlern wegen Asynchronität kommen kann (wegen dataCount++)
                        Threads[i].OrderID = dataCount;
                        // Zuweisen der im Thread verwendeten Methode
                        Threads[i].Thread = new Thread(delegate() { Threads[i].Add(bDatensaetze[Threads[i].OrderID].Zahl1, bDatensaetze[Threads[i].OrderID].Zahl2); });
                        Threads[i].Thread.Start();
                        dataCount++;
                        // break ist wichtig, da sonst i zu weit hochgezählt werden könnte und dadurch ein OutofRange entsteht
                        break;
                    }
                    // Abfrage, ob dem Thread eine Berechnung zugewiesen wurde und der Thread noch daran arbeitet
                    // Die Schleife wird nochmal durchlaufen, wenn dem Thread eine Arbeit zugewiesen wurde bzw. worden war. Nur zur Sicherheit.
                    if (Threads[i].IsWorking == true)
                    {
                        bcontinue = false;
                        if (!Threads[i].Thread.IsAlive)
                        {
                            Console.WriteLine("Thread " + (i + 1) + " hat ein Ergebnis.");
                            // Wenn nicht, wird das Ergebnis an den Datensatz übergeben
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
