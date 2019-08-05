using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Security.Cryptography;

namespace Rimuovi_duplicati
{
    class Program
    {
        public static List<Item> ListaFiles = new List<Item>();

        static void Main(string[] args)
        {
            Console.Write("Selezionare il disco: ");
            var disco = Console.ReadLine();

            var directoryRoot = new List<string>();

            try
            {
                directoryRoot = Directory.GetDirectories(disco + "\\").ToList();
            }
            catch
            {
                Console.WriteLine("Impossibile trovare il disco " + disco);
                return;
            }

            var completeList = new List<string>();
            foreach(var dir in directoryRoot)
            {
                var filesList = new List<string>();
                Console.WriteLine("Analizzo " + dir + "...");

                try
                {
                    filesList = Directory.GetFiles(dir, "*.*", SearchOption.AllDirectories).ToList();
                    completeList.AddRange(filesList);
                    Console.WriteLine("Inserito " + dir + " in database!");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Impossibile accedere a: " + dir);
                    Console.WriteLine("Errore: " + ex.Message);
                }
            }

            Console.WriteLine("Trovati " + completeList.Count + " files!");
            Console.WriteLine("Inizio la scansione...");

            double percentuale = (double)100 / (double)completeList.Count;
            double percentoSvolto = 0;
            foreach(var file in completeList)
            {
                Console.WriteLine("Analizzo il file " + Path.GetFileName(file));
                try
                {
                    using (var fs = new FileStream(file, FileMode.Open, FileAccess.Read))
                    {
                        var item = new Item();
                        item.Sha1 = BitConverter.ToString(SHA1.Create().ComputeHash(fs));
                        item.Nome = file;
                        Console.WriteLine("sha1: " + item.Sha1);

                        ListaFiles.Add(item);
                        percentoSvolto = percentoSvolto + percentuale;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Il file " + file + " non è accessibile :/");
                    Console.WriteLine("Errore: " + ex.Message);
                    percentoSvolto = percentoSvolto + percentuale;
                }
                Console.WriteLine("Completamento al " + percentoSvolto.ToString("0.000") + "%");
            }

            Console.Clear();
            Console.WriteLine("Analizzo i duplicati trovati...");
            var duplicati = ListaFiles.GroupBy(s => s.Sha1)
                             .Where(g => g.Count() > 1)
                             .Select(g => g.Key).ToList(); // or .SelectMany(g => g)

            var fileTxt = new List<string>();
            fileTxt.Add("Trovati " + duplicati.Count + " files duplicati su " + ListaFiles.Count + " files scansionati.");

            foreach(var dupsha1 in duplicati)
            {
                var duplicato = ListaFiles.Where(dup => dup.Sha1.Equals(dupsha1)).ToList();
                var tmpPrecedente = string.Empty;
                foreach(var beccato in duplicato)
                {
                    Console.WriteLine("Trovato: " + beccato.Nome);

                    if (tmpPrecedente.Equals(beccato.Sha1))
                    {
                        fileTxt.Add(beccato.Nome);
                        tmpPrecedente = beccato.Sha1;
                    }
                    else
                    {
                        fileTxt.Add("-----------------------------------------------------------------------");
                        fileTxt.Add("SHA1: " + beccato.Sha1);
                        fileTxt.Add(beccato.Nome);
                        tmpPrecedente = beccato.Sha1;
                    }
                    
                }
            }

            Console.WriteLine("Trovati " + duplicati.Count + " files duplicati su " + completeList.Count);
            File.WriteAllLines(Path.Combine(Directory.GetCurrentDirectory(), "duplicati.txt"), fileTxt);
            Console.ReadLine();
        }

        public class Item
        {
            public string Nome { get; set; }
            public string Sha1 { get; set; }
        }

    }
}
