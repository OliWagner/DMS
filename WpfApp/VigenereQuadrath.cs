using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Vigenere
{
    internal class VigenereQuadrath
    {
        private int[] _zahlen = new int[]
                                    {
                                        1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23,
                                        24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 
                                        45, 46, 47, 48, 49, 50, 51, 52, 53, 54, 55, 56, 57, 58, 59, 60, 61, 62, 63, 64, 65, 
                                        66, 67, 68, 69, 70, 71, 72, 73, 74, 75, 76, 77, 78, 79, 80, 81, 82, 83, 84, 85, 86,
                                        87, 88, 89, 90, 91, 92, 93, 94, 95
                                    };

        private string _codewort;

        private string[] BuchstabenKlein = new string[]
                                               {
                                                   "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n",
                                                   "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z","A", "B", 
                                                   "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", 
                                                   "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z", "ä", "ö", "ü", "ß", 
                                                   "Ä", "Ö", "Ü", "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", 
                                                   ".", ",", ":", ";", "!", "?", "-", "§", "$", "%", "&", "/", "(", ")", "=",
                                                   "[", "]", "+", "*", "'", "@", "<", ">", "{", "}", "\\"
                                               };

        private string buchstabenKlein = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZäöüßÄÖÜ0123456789.,:;!?-§$%&/()=[]+*'@<>{}\\";

        public VigenereQuadrath(string codewort) {
            _codewort = codewort;
        }

        private int[] ErzeugeSchlüsselAusCodewort(string codewort)
        {
            List<string[]> quadrath = this.Quadrath;
            var schlüssel = new int[_zahlen.Length];
            bool fertig = false;
            string wort = codewort;
            //Wort hintereinander schreiben bis es 26 Stellen hat
            while (!fertig)
            {
                wort = wort + wort;

                if (wort.Length > _zahlen.Length)
                {
                    wort = wort.Substring(0, _zahlen.Length);
                }

                if (wort.Length == _zahlen.Length)
                    fertig = true;
            }

            char[] wortArray = wort.ToCharArray();
            for (int i = 0; i < _zahlen.Length; i++)
            {
                schlüssel[i] = buchstabenKlein.IndexOf(wortArray[i]);
                if (schlüssel[i] == 0)
                {
                    schlüssel[i] = _zahlen.Length-1;
                }
            }
            return schlüssel;
        }

        private List<string[]> Quadrath
        {
            get
            {
                var vigQud = new List<string[]>();
                foreach (var i in _zahlen)
                {
                    vigQud.Add(ErzeugeEineZeile(i));
                }
                return vigQud;
            }
        }

        private string[] ErzeugeEineZeile(int startpunkt)
        {
            string[] Zeile = new string[_zahlen.Length];
            int zähler = 0;
            for (int i = startpunkt; i <= _zahlen.Length-1; i++) //Elemente des Arrays AB dem Startpunkt
            {
                Zeile[zähler] = BuchstabenKlein[(_zahlen[i - 1])];
                zähler++;
            }

            for (int i = 0; i < startpunkt; i++) //Elemente des Array BIS zum Startpunkt
            {
                Zeile[zähler] = BuchstabenKlein[_zahlen[i] - 1];
                zähler++;
            }

            return Zeile;
        }

        public string VerschlüssleText(string textRein)
        {
            string text = textRein;
            int[] schlüssel = ErzeugeSchlüsselAusCodewort(_codewort);
            int RestBuchstabenLetzterDurchlauf = 0;
            int anzahlDurchläufe;
            int textLänge = text.Length;
            string verschlüsselterText = "";
            List<string[]> quadrath = this.Quadrath;

            //Anzahl der Durchläufe des int-Schlüssels festlegen           
            if (textLänge > _zahlen.Length)
            {
                anzahlDurchläufe = textLänge / _zahlen.Length;
                if (textLänge % _zahlen.Length != 0)
                    anzahlDurchläufe++;
                RestBuchstabenLetzterDurchlauf = textLänge % _zahlen.Length;
            }
            else
            {
                anzahlDurchläufe = 1;
            }

            for (int i = 0; i < anzahlDurchläufe; i++) //VErtikal, also die Position in der Liste selbst
            {
                int trigger = _zahlen.Length;
                if (text.Length < _zahlen.Length)
                    trigger = text.Length;

                if (RestBuchstabenLetzterDurchlauf != 0 && (i == (anzahlDurchläufe - 1)))
                {
                    trigger = RestBuchstabenLetzterDurchlauf;
                }

                int positionImText = (i * _zahlen.Length);
                    //HIER MUSS FÜR JEDEN DURCHLAUF EINE ANZAHL VON 26 ZEICHEN aus dem Text GENOMMEN WERDEN

                string aktuelleTextStelle = text.Substring(positionImText, trigger);

                for (int a = 0; a < trigger; a++) //Horizontal, den einzelnen Array an der position i auslesen
                {
                    //es muss ein Zeichen aus der zeichenkette genommen werden, das an Pos a
                    char aktuellerBuchstabe = aktuelleTextStelle.ToArray()[a];

                    //Aus dem Schlüssel ist der Int-Wert an Position a zu ermitteln
                    int aktuellerSchlüsselWert = schlüssel[a];

                    //Dieser Wert steht für das zu benutzende Alphabet im Viginerequadrath, also die Position in der Liste, 
                    //AAA  Dieser Array ist für die Verschlüsselung zu verwenden zu verwenden
                    string[] aktuellesAlphabet = quadrath.ElementAt(aktuellerSchlüsselWert - 1);

                    //Das aus der Zeichenkette ermittelte Zeichen muss jetzt in buchstabenKlein gesucht werden
                    int positionImKlartextAlphabet = buchstabenKlein.IndexOf(aktuellerBuchstabe);

                    if (positionImKlartextAlphabet != -1)
                    {
                        //Das Ergebnis der IndexOf-Abfrage ist der Index, an dem im (AAA) der verschlüsselte Buchstabe steht
                        string verschlüsselterBuchstabe = aktuellesAlphabet[positionImKlartextAlphabet];
                        verschlüsselterText += verschlüsselterBuchstabe;
                    }
                    else
                    {
                        verschlüsselterText += " ";
                    }
                }
            }
            //StreamWriter sw = new StreamWriter(@"c:\copy\VIGINERE"+ Guid.NewGuid() +".txt");
            //sw.Write(verschlüsselterText);
            //sw.Close();
            return verschlüsselterText;
        }

        public string EntschlüssleText(string textRein)
        {
            string text = textRein;
            int[] schlüssel = ErzeugeSchlüsselAusCodewort(_codewort);
            int RestBuchstabenLetzterDurchlauf = 0;
            int anzahlDurchläufe;
            int textLänge = text.Length;
            string entschlüsselterText = "";
            List<string[]> quadrath = this.Quadrath;

            //Anzahl der Durchläufe des int-Schlüssels festlegen           
            if (textLänge > _zahlen.Length)
            {
                anzahlDurchläufe = textLänge / _zahlen.Length;
                if (textLänge % _zahlen.Length != 0)
                    anzahlDurchläufe++;
                RestBuchstabenLetzterDurchlauf = textLänge % _zahlen.Length;
            }
            else
            {
                anzahlDurchläufe = 1;
            }

            for (int i = 0; i < anzahlDurchläufe; i++) //VErtikal, also die Position in der Liste selbst
            {
                int trigger = _zahlen.Length;
                if (text.Length < _zahlen.Length)
                    trigger = text.Length;

                if (RestBuchstabenLetzterDurchlauf != 0 && (i == (anzahlDurchläufe - 1)))
                {
                    trigger = RestBuchstabenLetzterDurchlauf;
                }

                int positionImText = (i * _zahlen.Length);
                string aktuelleTextStelle = text.Substring(positionImText, trigger);

                for (int a = 0; a < trigger; a++) //Horizontal, den einzelnen Array an der position i auslesen
                {
                    //es muss ein Zeichen aus der zeichenkette genommen werden, das an Pos a
                    char aktuellerBuchstabe = aktuelleTextStelle.ToArray()[a];

                    //Aus dem Schlüssel ist der Int-Wert an Position a zu ermitteln
                    int aktuellerSchlüsselWert = schlüssel[a];

                    //Dieser Wert steht für das zu benutzende Alphabet im Viginerequadrath, also die Position in der Liste, 
                    string[] aktuellesAlphabet = quadrath.ElementAt(aktuellerSchlüsselWert - 1);
                    string aktuellesAlphabetString = "";
                    foreach (string buchstabe in aktuellesAlphabet)
                    {
                        aktuellesAlphabetString += buchstabe;
                    }
                    //Die Position des verschlüsselten Zeichens in diesem Array ist zu ermitteln
                    int positionImArray = aktuellesAlphabetString.IndexOf(aktuellerBuchstabe);

                    if (positionImArray != -1)
                    {
                        //Der entschlüsselte Buchstabe steht im Array BuchstabenKlein an der ermittelten Stelle
                        string entschlüsselterBuchstabe = BuchstabenKlein[positionImArray];
                        entschlüsselterText += entschlüsselterBuchstabe;
                    }
                    else
                    {
                        entschlüsselterText += " ";
                    }
                }
            }
            return entschlüsselterText;
        }
    }
}