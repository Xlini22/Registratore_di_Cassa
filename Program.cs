using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;
using System.Globalization;
using System.Xml.Schema;
//salvare il riepilogo su file con [nome cliente] [servizi] [numero incrementale] [data e ora] [metodo di pagamento]

namespace CassaNegozio 
{
    internal class Program
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr GetStdHandle(int nStdHandle);

        const int STD_OUTPUT_HANDLE = -11;
        const uint ENABLE_VIRTUAL_TERMINAL_PROCESSING = 0x0004;


        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;

            if (OperatingSystem.IsWindows())
            {
                EnableWindowsAnsi();
            }

            string comando;
            int variante;
            int count = 0; // Contatore per il riepilogo
            int salvato = 0;
            bool errore = false;
            string[,] riepilogo = new string[20, 2]; // 20 righe, 2 colonne (descrizione e prezzo)
            string nomeCliente = "", metodoPagamento = "";

            // Menu principale
            List<string> Menu =
            [
                "CASSA",
                "",
                "n.  Nome cliente",
                "p.  Metodo di pagamento",
                "",
                "1.  Piega",
                "2.  Taglio",
                "3.  Balsamo",
                "4.  Schiuma-gel",
                "5.  Shampoo",
                "7.  Colore",
                "9.  Meches",
                "44. Lozione",
                "22. Voce personalizzata",
                "99. Cancella ultimo inserimento",
                "999. Cancella tutto",
                "",
                "0. Salva scontrino",
                "",
                "exit.  ESCI",
                "",
            ];

            do
            {
                Console.Clear();
                StampaMenuAffiancato(Menu, riepilogo, count, nomeCliente, metodoPagamento);
                
                // Se c'è un errore, mostra il messaggio e resetta l'errore
                if(errore)
                {
                    Console.WriteLine("\u001b[31m⚠ ERRORE: Inserimento non valido!\u001b[0m");
                    errore = false; // Resetta l'errore per il prossimo ciclo
                }

                if (salvato > 0)
                {
                    // 1 successo
                    // 2 errore metodo di pagamento mancante
                    // 3 errore nome cliente mancante
                    // 4 errore prestazioni mancanti
                    // 5 errore nome cliente e metodo di pagamento mancanti

                    if (salvato == 1)
                    {
                        Console.WriteLine($"\u001b[32m✓ Scontrino salvato con successo in /Documenti/ScontriniConsole\u001b[0m");
                    }
                    else if(salvato == 2)
                    {
                        Console.WriteLine("\u001b[31m⚠ ERRORE: Metodo di pagamento mancante!\u001b[0m");
                    }
                    else if(salvato == 3)
                    {
                        Console.WriteLine("\u001b[31m⚠ ERRORE: Nome cliente mancante!\u001b[0m");
                    }
                    else if(salvato == 4)
                    {
                        Console.WriteLine("\u001b[31m⚠ ERRORE: Devi inserire almento una prestazione!\u001b[0m");
                    }
                    else if (salvato == 5)
                    {
                        Console.WriteLine("\u001b[31m⚠ ERRORE: Devi inserire il metodo di pagamento e il nome cliente!\u001b[0m");
                    }
                    salvato = 0; // Resetta il contatore dei salvataggi per il prossimo ciclo
                }

                comando = InserimentoStringa(false, "SCELTA:");

                if(comando != "exit")
                {
                    switch (comando)
                    {
                        case "n": // Inserisci nome cliente
                            Console.Clear();
                            Console.WriteLine("CLIENTE");
                            Console.WriteLine();
                            Console.Write("Inserisci nome Cliente: ");
                            nomeCliente = Console.ReadLine() ?? string.Empty;
                            break;
                        case "p": // Salva scontrino su file e resetta tutto
                            Console.Clear();
                            Console.WriteLine("METODO DI PAGAMENTO");
                            Console.WriteLine();
                            Console.WriteLine("Default: Contanti");
                            Console.WriteLine("2.       Bancomat");
                            variante = InserimentoControlloN(true, "SCELTA:");
                            switch (variante)
                            {
                                case 1:
                                    metodoPagamento = "Contanti";
                                    break;
                                case 2:
                                    metodoPagamento = "Bancomat";
                                    break;
                            }
                            break;
                        case "1": // Piega
                            Console.Clear();
                            Console.WriteLine("PIEGA");
                            Console.WriteLine();
                            Console.WriteLine("Default: Corta (20€)");
                            Console.WriteLine("2.       Media (22€)");
                            Console.WriteLine("3.       Lunga (24€)");
                            variante = InserimentoControlloN(true, "SCELTA:");
                            switch (variante)
                            {
                                case 1:
                                    riepilogo[count, 0] = "Piega corta";
                                    riepilogo[count, 1] = "20€";
                                    count++;
                                    break;
                                case 2:
                                    riepilogo[count, 0] = "Piega media";
                                    riepilogo[count, 1] = "22€";
                                    count++;
                                    break;
                                case 3:
                                    riepilogo[count, 0] = "Piega lunga";
                                    riepilogo[count, 1] = "24€";
                                    count++;
                                    break;
                                default:
                                    Console.WriteLine("Scelta non valida!");
                                    break;
                            }
                            break;
                        case "2": // Taglio
                            Console.Clear();
                            riepilogo[count, 0] = "Taglio";
                            riepilogo[count, 1] = "24€";
                            count++;
                            break;
                        case "3": // Balsamo
                            Console.Clear();
                            Console.WriteLine("BALSAMO");
                            Console.WriteLine();
                            Console.WriteLine("Default: Normale (2,5€)");
                            Console.WriteLine("2.       System (3,5€)");
                            variante = InserimentoControlloN(true, "SCELTA:");
                            switch (variante)
                            {
                                case 1:
                                    riepilogo[count, 0] = "Balsamo";
                                    riepilogo[count, 1] = "2,5€";
                                    count++;
                                    break;
                                case 2:
                                    riepilogo[count, 0] = "Balsamo System";
                                    riepilogo[count, 1] = "3,5€";
                                    count++;
                                    break;
                                default:
                                    Console.WriteLine("Scelta non valida!");
                                    break;
                            }
                            break;
                        case "4": // Schiuma-gel
                            Console.Clear();
                            riepilogo[count, 0] = "Schiuma-gel";
                            riepilogo[count, 1] = "1€";
                            count++;
                            break;
                        case "5": // Shampoo
                            Console.Clear();
                            Console.WriteLine("SHAMPOO");
                            Console.WriteLine();
                            Console.WriteLine("Default: Normale (1€)");
                            Console.WriteLine("2.       System (3€)");
                            variante = InserimentoControlloN(true, "SCELTA:");
                            switch (variante)
                            {
                                case 1:
                                    riepilogo[count, 0] = "Shampoo Normale";
                                    riepilogo[count, 1] = "1€";
                                    count++;
                                    break;
                                case 2:
                                    riepilogo[count, 0] = "Shampoo System";
                                    riepilogo[count, 1] = "3€";
                                    count++;
                                    break;
                                default:
                                    Console.WriteLine("Scelta non valida!");
                                    break;
                            }
                            break;
                        case "7": // Colore
                            Console.Clear();
                            Console.WriteLine("COLORE");
                            Console.WriteLine();
                            Console.WriteLine("Default: Normale (35€)");
                            Console.WriteLine("2.       Plus (41€)");
                            variante = InserimentoControlloN(true, "SCELTA:");
                            switch (variante)
                            {
                                case 1:
                                    riepilogo[count, 0] = "Colore Normale";
                                    riepilogo[count, 1] = "35€";
                                    count++;
                                    break;
                                case 2:
                                    riepilogo[count, 0] = "Colore Plus";
                                    riepilogo[count, 1] = "41€";
                                    count++;
                                    break;
                                default:
                                    Console.WriteLine("Scelta non valida!");
                                    break;
                            }
                            break;
                        case "9": // Meches
                            Console.Clear();
                            Console.WriteLine("MECHES");
                            Console.WriteLine();
                            Console.WriteLine("Default: Base (60€)");
                            Console.WriteLine("2.       Doppie (70€)");
                            variante = InserimentoControlloN(true, "SCELTA:");
                            switch (variante)
                            {
                                case 1:
                                    riepilogo[count, 0] = "Meches Base";
                                    riepilogo[count, 1] = "60€";
                                    count++;
                                    break;
                                case 2:
                                    riepilogo[count, 0] = "Meches Doppie";
                                    riepilogo[count, 1] = "70€";
                                    count++;
                                    break;
                                default:
                                    Console.WriteLine("Scelta non valida!");
                                    break;
                            }
                            break;
                        case "44": // Lozione anticaduta
                            Console.Clear();
                            riepilogo[count, 0] = "Lozione anticaduta";
                            riepilogo[count, 1] = "6€";
                            count++;
                            break;
                        case "22": // Voce personalizzata
                            Console.Clear();
                            Console.Write("Inserisci descrizione: ");
                            string descrizione = Console.ReadLine() ?? string.Empty;
                            double prezzo;
                            do
                            {
                                Console.Write("Inserisci prezzo: ");
                                string inputPrezzo = Console.ReadLine() ?? string.Empty;
                                if (double.TryParse(inputPrezzo, CultureInfo.InvariantCulture, out prezzo))
                                {
                                    break;
                                }
                                else
                                {
                                    Console.WriteLine("Devi inserire un prezzo valido!");
                                }
                            } while (true);
                            
                            riepilogo[count, 0] = descrizione;
                            riepilogo[count, 1] = $"{prezzo}€";
                            count++;
                            break;
                        case "99": // Cancella ultimo inserimento
                            Console.WriteLine("Hai scelto: Cancella ultimo inserimento");
                            if (count > 0)
                            {
                                count--;
                                riepilogo[count, 0] = "";
                                riepilogo[count, 1] = "";
                            }
                            else
                            {
                                errore = true; // Non ci sono prestazioni da cancellare
                            }
                            break;
                        case "999": // Cancella tutto
                            if(count > 0)
                            {
                                while (count > 0)
                                {
                                    count--;
                                    riepilogo[count, 0] = "";
                                    riepilogo[count, 1] = "";
                                }
                            }else
                            {
                                errore = true;
                            }
                            break;
                        case "0": // Salva scontrino su file e resetta tutto
                            if (count > 0) // casi in cui le prestazioni sono state inserite ma mancano altri fattori
                            {
                                if (!string.IsNullOrWhiteSpace(nomeCliente) && !string.IsNullOrWhiteSpace(metodoPagamento))
                                {
                                    List<string> righe = CreaRiepilogoString(riepilogo, count, nomeCliente, metodoPagamento);
                                    SalvaScontrinoCliente(righe, nomeCliente);

                                    while (count > 0)
                                    {
                                        count--;
                                        riepilogo[count, 0] = "";
                                        riepilogo[count, 1] = "";
                                    }
                                    salvato = 1;
                                }
                                else if (!string.IsNullOrWhiteSpace(nomeCliente) && string.IsNullOrWhiteSpace(metodoPagamento)) // Caso in cui solo il metodo di pagamento non è stato inserito
                                {
                                    salvato = 2; // Codice errore 2
                                }
                                else if (string.IsNullOrWhiteSpace(nomeCliente) && !string.IsNullOrWhiteSpace(metodoPagamento)) // Caso in cui solo il nome cliente non è stato inserito
                                {
                                    salvato = 3; // Codice errore 3
                                }
                                else if (string.IsNullOrWhiteSpace(nomeCliente) && string.IsNullOrWhiteSpace(metodoPagamento)) // Caso in cui non è stato inserito né il nome cliente né il metodo di pagamento
                                {
                                    salvato = 5; // Codice errore 5
                                }
                            }
                            else if (count == 0) // Caso in cui non ci sono prestazioni
                            {
                                salvato = 4; // Codice errore 4
                            }else
                            {
                                errore = true;
                            }
                            break;
                        default:
                            errore = true;
                            break;
                    }
                }
            }while (comando != "exit");
        }


        /// <summary>
        /// Chiede all'utente di inserire un numero intero e lo restituisce.
        /// Continua a chiedere finché l'input non è valido.
        /// </summary>
        /// <param name="defaultAllow">Se true, permette di accettare un input vuoto come valore predefinito (1)</param>
        /// <param name="messaggio">Il messaggio da mostrare all'utente</param>
        /// <returns>Il numero intero inserito dall'utente</returns>
        static int InserimentoControlloN(bool defaultAllow, string messaggio)
        {
            int valore;
            bool r;

            do
            {
                Console.Write(messaggio);
                string input = Console.ReadLine() ?? string.Empty;

                // Se preme solo Invio (vuoto o spazi) → valore = 0
                if (string.IsNullOrWhiteSpace(input) && defaultAllow)
                {
                    valore = 1;
                    return valore;
                }

                r = int.TryParse(input, out valore);

                if (!r)
                {
                    valore = -1; // Valore non valido
                    return valore; // Ritorna -1 per indicare errore e gestire l'avviso di errore dall'esterno
                }

            } while (!r);

            return valore;
        }

        /// <summary>
        /// Funzione simile a InserimentoControlloN ma restituisce una stringa (per poter accettare anche input vuoti come default)
        /// </summary>
        /// <param name="defaultAllow">Se true, permette di accettare un input vuoto come valore predefinito (1)</param>
        /// <param name="messaggio">Il messaggio da mostrare all'utente</param>
        /// <returns>La stringa inserita dall'utente o un valore predefinito</returns>
        static string InserimentoStringa(bool defaultAllow, string messaggio)
        {
            string input;

            Console.Write(messaggio);
            input = Console.ReadLine() ?? string.Empty;

            // Se preme solo Invio (vuoto o spazi) → valore = 1
            if (string.IsNullOrWhiteSpace(input) && defaultAllow)
            {
                input = "1";
                return input;
            }
            else if (string.IsNullOrWhiteSpace(input) && !defaultAllow)
            {
                input = "-1"; // Segnale errore per input vuoto non permesso
                return input;
            }

            return input;
        }

        /// <summary>
        /// stampa il menu principale e il riepilogo affiancati, allineando le descrizioni a sinistra e i prezzi a destra, con una larghezza fissa per la prima colonna.
        /// </summary> <param name="riepilogo">Array con le descrizioni e i prezzi delle prestazioni</param>
        /// <param name="count">Numero di prestazioni inserite finora</param>
        static void StampaMenuAffiancato(List<string> menu, string[,] riepilogo, int count, string nomeCliente, string metodoPagamento)
        {
            // Prendi le righe dei due menu come liste di stringhe
            List<string> menu1 = menu;
            List<string> menu2 = CreaRiepilogoString(riepilogo, count, nomeCliente, metodoPagamento);

            int maxRighe = Math.Max(menu1.Count, menu2.Count);
            int larghezzaColonna = 50; // Larghezza fissa per la prima colonna

            for (int i = 0; i < maxRighe; i++)
            {
                string col1 = i < menu1.Count ? menu1[i] : "";
                string col2 = i < menu2.Count ? menu2[i] : "";

                Console.WriteLine(col1.PadRight(larghezzaColonna) + col2);
            }
        }

        /// <summary>
        /// Crea una lista di stringhe che rappresentano le righe del riepilogo, con descrizioni allineate a sinistra e prezzi a destra, e il totale finale in verde.
        /// </summary> <param name="riepilogo">Array con le descrizioni e i prezzi delle prestazioni</param>
        /// <param name="count">Numero di prestazioni inserite finora</param>
        /// <returns>Lista di stringhe con le righe del riepilogo</returns>
        static List<string> CreaRiepilogoString(string[,] riepilogo, int count, string nomeCliente, string metodoPagamento)
        {
            List<string> righe = [];
            double totale = 0;
            if(count > 0 || !string.IsNullOrWhiteSpace(nomeCliente) || !string.IsNullOrWhiteSpace(metodoPagamento))
            {
                righe.Add("RIEPILOGO:");

                righe.Add("");
                if (!string.IsNullOrWhiteSpace(nomeCliente)) //controllache il nome cliente non sia vuoto o solo spazi prima di aggiungerlo al riepilogo
                {
                    righe.Add("Cliente: " + nomeCliente);
                }

                if (!string.IsNullOrWhiteSpace(metodoPagamento)) //controlla che il metodo di pagamento non sia vuoto o solo spazi prima di aggiungerlo al riepilogo
                {
                    righe.Add("Pagamento: " + metodoPagamento);
                }
                righe.Add("");

                for (int i = 0; i < count; i++)
                {
                    // Allinea la descrizione a sinistra (20 caratteri) e il prezzo a destra (8 caratteri)
                    righe.Add($"{riepilogo[i, 0],-20}{riepilogo[i, 1],8}");
                    string prezzo = riepilogo[i, 1].Replace("€", "").Trim(); // Rimuove il simbolo "€" e spazi
                    prezzo = prezzo.Replace('.', ',');
                    totale += double.Parse(prezzo);
                }
                righe.Add("");
                // "TOTALE:" allineato a sinistra (20), totale a destra (8) in verde
                righe.Add($"\u001b[32m{"TOTALE:",-20}{(totale + "€"),8}\u001b[0m");
            }
            
            return righe;
        }

        /// <summary>
        /// Salva la lista di righe del riepilogo su un file di testo, con un nome che include il nome del cliente e un timestamp per evitare sovrascritture. Sostituisce eventuali caratteri non validi nel nome del cliente con underscore.
        /// </summary>
        /// <param name="righe"></param>
        /// <param name="nomeCliente"></param>
        static void SalvaScontrinoCliente(List<string> righe, string nomeCliente)
        {
            // Pulisce caratteri non validi nel nome file
            foreach (char c in Path.GetInvalidFileNameChars())
                nomeCliente = nomeCliente.Replace(c, '_');
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");

            righe.Add("");
            righe.Add($"Data e ora: {timestamp}");

            // Percorso dinamico Documenti
            string cartellaDocumenti = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string saveFolder = Path.Combine(cartellaDocumenti, "ScontriniConsole");

            // Crea la cartella se non esiste
            Directory.CreateDirectory(saveFolder);

            // Timestamp unico

            string nomeFile = $"{nomeCliente}_{timestamp}.txt";

            string percorsoCompleto = Path.Combine(saveFolder, nomeFile);

            File.WriteAllLines(percorsoCompleto, righe);
        }

        // Abilita il supporto ANSI su Windows per poter usare i colori
        static void EnableWindowsAnsi()
        {
            var handle = GetStdHandle(STD_OUTPUT_HANDLE);

            if (GetConsoleMode(handle, out uint mode))
            {
                SetConsoleMode(handle, mode | ENABLE_VIRTUAL_TERMINAL_PROCESSING);
            }
        }
    }
}