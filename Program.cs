using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Xml.Schema;
//salvare il riepilogo su file con [nome cliente] [servizi] [numero incrementale] [data e ora] [metodo di pagamento]

namespace CassaNegozio 
{
    internal class Program
    {
        #region Import per abilitare ANSI su Windows
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr GetStdHandle(int nStdHandle);

        const int STD_OUTPUT_HANDLE = -11;
        const uint ENABLE_VIRTUAL_TERMINAL_PROCESSING = 0x0004;
        #endregion

        static void Main(string[] args)
        {
            Console.Title = "Registratore di Cassa";
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
                "     CASSA",
                "",
                "n.   Nome cliente",
                "p.   Metodo di pagamento",
                "",
                "\u001b[90m1.   Piega\u001b[0m",
                "\u001b[32m2.   Taglio\u001b[0m",
                "\u001b[94m3.   Balsamo\u001b[0m",
                "\u001b[95m4.   Schiuma-gel\u001b[0m",
                "\u001b[33m5.   Shampoo\u001b[0m",
                "\u001b[31m7.   Colore\u001b[0m",
                "\u001b[96m9.   Meches\u001b[0m",
                "\u001b[95m44.  Lozione\u001b[0m",
                "",
                "22.  Voce personalizzata",
                "99.  Cancella ultimo inserimento",
                "999. Cancella tutto",
                "",
                "0.   Salva scontrino",
                "",
                "exit.  ESCI",
                ""
            ];

            do
            {
                Console.Clear();
                StampaMenuAffiancato(Menu, riepilogo, count, nomeCliente, metodoPagamento);
                
                // Se c'è un errore, mostra il messaggio e resetta l'errore
                if(errore)
                {
                    Console.WriteLine("\u001b[31m[!] ERRORE: Inserimento non valido!\u001b[0m");
                    errore = false; // Resetta l'errore per il prossimo ciclo
                }

                // Se è stato salvato, mostra il messaggio corrispondente e resetta il contatore dei salvataggi
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
                        Console.WriteLine("\u001b[31m[!] ERRORE: Metodo di pagamento mancante!\u001b[0m");
                    }
                    else if(salvato == 3)
                    {
                        Console.WriteLine("\u001b[31m[!] ERRORE: Nome cliente mancante!\u001b[0m");
                    }
                    else if(salvato == 4)
                    {
                        Console.WriteLine("\u001b[31m[!] ERRORE: Devi inserire almeno una prestazione!\u001b[0m");
                    }
                    else if (salvato == 5)
                    {
                        Console.WriteLine("\u001b[31m[!] ERRORE: Devi inserire il metodo di pagamento e il nome cliente!\u001b[0m");
                    }
                    salvato = 0; // Resetta il contatore dei salvataggi per il prossimo ciclo
                }

                // Chiede all'utente di inserire un comando finché non è valido (gestisce l'errore di input vuoto o non valido)
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

                            // dichiarazione lista di tuple per il menu variante della piega, in modo da poterla passare alla funzione MostraMenuVariante
                            var piegaMenu = new List<(string, double)>
                            {
                                ("Piega corta", 20),
                                ("Piega media", 22),
                                ("Piega lunga", 24)
                            };

                            MostraMenuVariante("PIEGA", piegaMenu, riepilogo, ref count);

                            break;
                        case "2": // Taglio
                            Console.Clear();
                            riepilogo[count, 0] = "Taglio";
                            riepilogo[count, 1] = "24€";
                            count++;
                            break;
                        case "3": // Balsamo

                            var balsamoMenu = new List<(string, double)>
                            {
                                ("Balsamo normale", 2.5),
                                ("Balsamo system", 3.5)
                            };

                            MostraMenuVariante("BALSAMO", balsamoMenu, riepilogo, ref count);
                            
                            break;
                        case "4": // Schiuma-gel
                            Console.Clear();
                            riepilogo[count, 0] = "Schiuma-gel";
                            riepilogo[count, 1] = "1€";
                            count++;
                            break;
                        case "5": // Shampoo

                            var shampooMenu = new List<(string, double)>
                            {
                                ("Shampoo normale", 1),
                                ("Shampoo system", 3)
                            };

                            MostraMenuVariante("SHAMPOO", shampooMenu, riepilogo, ref count);

                            break;
                        case "7": // Colore

                            var coloreMenu = new List<(string, double)>
                            {
                                ("Colore normale", 35),
                                ("Colore plus", 41)
                            };

                            MostraMenuVariante("COLORE", coloreMenu, riepilogo, ref count);

                            break;
                        case "9": // Meches

                            var mechesMenu = new List<(string, double)>
                            {
                                ("Meches Base", 60),
                                ("Meches Doppie", 70)
                            };

                            MostraMenuVariante("MECHES", mechesMenu, riepilogo, ref count);

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
                            if (count > 0) // casi in cui le prestazioni sono state inserite
                            {
                                // tutto corretto, salva scontrino
                                if (!string.IsNullOrWhiteSpace(nomeCliente) && !string.IsNullOrWhiteSpace(metodoPagamento))
                                {
                                    // crea riepilogo e salva lo scontrino su file
                                    List<string> righe = CreaRiepilogoString(riepilogo, count, nomeCliente, metodoPagamento);
                                    SalvaScontrinoCliente(righe, nomeCliente);

                                    // resetta tutte le prestazioni
                                    while (count > 0)
                                    {
                                        count--;
                                        riepilogo[count, 0] = "";
                                        riepilogo[count, 1] = "";
                                    }
                                    
                                    // resetta nome cliente e metodo di pagamento
                                    nomeCliente = "";
                                    metodoPagamento = "";
                                    
                                    // segnala che è stato salvato correttamente
                                    salvato = 1;
                                }
                                // Caso in cui solo il metodo di pagamento non è stato inserito
                                else if (!string.IsNullOrWhiteSpace(nomeCliente) && string.IsNullOrWhiteSpace(metodoPagamento)) 
                                {
                                    salvato = 2; // metodo di pagamento mancante, codice errore 2
                                }
                                // Caso in cui solo il nome cliente non è stato inserito
                                else if (string.IsNullOrWhiteSpace(nomeCliente) && !string.IsNullOrWhiteSpace(metodoPagamento)) 
                                {
                                    salvato = 3; // nome cliente mancante, codice errore 3
                                }
                                // Caso in cui non è stato inserito né il nome cliente né il metodo di pagamento
                                else if (string.IsNullOrWhiteSpace(nomeCliente) && string.IsNullOrWhiteSpace(metodoPagamento)) 
                                {
                                    salvato = 5; // mancano entrambi: nome cliente e metodo di pagamento, codice errore 5
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

        #region Funzioni
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
        /// Stampa un menu di varianti (es. per la piega: corta, media, lunga) e permette all'utente di scegliere una variante. Aggiorna il riepilogo con la descrizione e il prezzo della variante scelta. Gestisce l'errore di input non valido e permette di annullare la scelta tornando al menu principale. La lista di opzioni è passata come parametro come lista di tuple (Nome, Prezzo) per semplificare la gestione del menu variante e rendere il codice più modulare.
        /// </summary>
        /// <param name="titolo">Quello che viene mostrato come titolo del menu</param>
        /// <param name="opzioni">Le opzioni disponibili nel menu</param>
        /// <param name="riepilogo">L'array dove verrà salvata la scelta dell'utente</param>
        /// <param name="count">L'indice del riepilogo da aggiornare</param>
        /// <returns>Ritorna l'indice della variante scelta (1-based) o 0 se annullata.</returns>
        static int MostraMenuVariante(string titolo, List<(string Nome, double Prezzo)> opzioni, string[,] riepilogo, ref int count)
        {
            bool errore = false;
            int scelta;

            do
            {
                Console.Clear();
                Console.WriteLine(titolo);
                Console.WriteLine();

                for (int i = 0; i < opzioni.Count; i++)
                {
                    string prefisso = i == 0 ? "Default:" : $"{i + 1}.";
                    Console.WriteLine($"{prefisso,-10} {opzioni[i].Nome} ({opzioni[i].Prezzo}€)");
                }

                Console.WriteLine("");
                Console.WriteLine("0.         ANNULLA");
                Console.WriteLine("");

                if (errore)
                {
                    Console.WriteLine("\u001b[31m[!] ERRORE: Inserimento non valido!\u001b[0m");
                    errore = false;
                }

                scelta = InserimentoControlloN(true, "SCELTA:");

                if (scelta >= 1 && scelta <= opzioni.Count)
                {
                    riepilogo[count, 0] = opzioni[scelta - 1].Nome;
                    riepilogo[count, 1] = opzioni[scelta - 1].Prezzo + "€";
                    count++;
                    return scelta;
                }
                else if (scelta == 0)
                {
                    return 0;
                }
                else
                {
                    errore = true;
                }

            } while (true);
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
            int larghezzaColonna = 50;

            for (int i = 0; i < maxRighe; i++)
            {
                string col1 = i < menu1.Count ? menu1[i] : "";
                string col2 = i < menu2.Count ? menu2[i] : "";

                int lunghezzaVisiva = RimuoviAnsi(col1).Length;
                int spaziDaAggiungere = larghezzaColonna - lunghezzaVisiva;

                if (spaziDaAggiungere < 0)
                    spaziDaAggiungere = 0;

                Console.WriteLine(col1 + new string(' ', spaziDaAggiungere) + col2);
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
                if (!string.IsNullOrWhiteSpace(nomeCliente)) //controlla che il nome cliente non sia vuoto o solo spazi prima di aggiungerlo al riepilogo
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
            {
                nomeCliente = nomeCliente.Replace(c, '_');
            }

            string data = DateTime.Now.ToString("dd-MM-yyyy");
            string ora = DateTime.Now.ToString("HH-mm-ss");
            string oraDisplay = ora.Replace('-', ':');

            righe.Add("");
            righe.Add($"Data: {data}");
            righe.Add($"Ora: {oraDisplay}");

            // Percorso dinamico Documenti
            string cartellaDocumenti = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string saveFolder = Path.Combine(cartellaDocumenti, "ScontriniConsole");

            // Crea la cartella se non esiste
            Directory.CreateDirectory(saveFolder);

            // Nome file unico basato su nome, data e ora
            string nomeFile = $"{nomeCliente}_{data}_{ora}.txt";

            string percorsoCompleto = Path.Combine(saveFolder, nomeFile);

            // rimuove eventuali codici di colore ANSI dalle righe prima di salvarle su file, per evitare che appaiano come caratteri strani nel file di testo
            for (int i = 0; i < righe.Count; i++)
            {
                if (righe[i] == null) continue;
                righe[i] = Regex.Replace(righe[i], "\\x1B\\[[0-9;]*m", "");
                righe[i] = righe[i].Replace("b[32m", "").Replace("b[0m", "").Replace("[0m", "");
            }

            // Salva le righe su file
            File.WriteAllLines(percorsoCompleto, righe);
        }

        /// <summary>
        /// Rimuove le sequenze ANSI (es. ESC[32m) da una stringa, in modo da poter calcolare correttamente la lunghezza visiva della stringa senza i codici di formattazione. Utile per allineare correttamente le colonne quando si usano colori o altri stili ANSI.
        /// </summary>
        /// <param name="input"></param>
        /// <returns> La stringa di input senza le sequenze ANSI/// </returns>
        static string RimuoviAnsi(string input)
        {
            return Regex.Replace(input, @"\x1B\[[0-9;]*m", "");
        }

        /// <summary>
        /// Abilita il supporto per le sequenze ANSI su Windows, in modo da poter usare colori e stili nel menu e nel riepilogo. Su altri sistemi operativi (Linux, macOS) il supporto ANSI è già abilitato di default.
        /// </summary>
        static void EnableWindowsAnsi()
        {
            var handle = GetStdHandle(STD_OUTPUT_HANDLE);

            if (GetConsoleMode(handle, out uint mode))
            {
                SetConsoleMode(handle, mode | ENABLE_VIRTUAL_TERMINAL_PROCESSING);
            }
        }
        #endregion
    }
}