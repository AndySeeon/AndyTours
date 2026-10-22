// Einstieg des Programms „AndyTours.Altdatenuebernahme“ (Spezifikation 002).
//
// Wandelt die BikeXperience-Altdaten einmalig in einen AndyTours-Datenordner um.
// Aufruf:  dotnet run --project tools/AndyTours.Altdatenuebernahme -- [BikeXperience-Ordner] [Zielordner]
// Ohne Angaben: Daten → AppDaten im Projektordner.
//
// Ablauf:
//   1. Aufrufparameter auswerten
//   2. Zielordner prüfen; enthält er schon Daten, nachfragen (j/N)
//   3. Altdaten vollständig lesen – vorher wird nichts geschrieben
//   4. Zwischenordner aufbauen und gegen den Zielordner austauschen
//   5. Zusammenfassung ausgeben
// Rückgabecode: 0 = Erfolg, 1 = Fehler oder abgebrochen (Zielordner dann unverändert).

using System.Text;
using Microsoft.Data.Sqlite;
using AndyTours.Altdatenuebernahme;
using AndyTours.Import.Altdaten;

// Umlaute in der Konsole korrekt ausgeben
Console.OutputEncoding = Encoding.UTF8;

// --- 1. Aufrufparameter ---
Aufrufparameter parameter;
try
{
    parameter = LeseAufrufparameter.Ausfuehren(args, AppContext.BaseDirectory);
}
catch (Exception fehler) when (fehler is ArgumentException or InvalidOperationException)
{
    Console.Error.WriteLine(fehler.Message);
    Console.Error.WriteLine("Aufruf: AndyTours.Altdatenuebernahme [BikeXperience-Ordner] [Zielordner]");
    return 1;
}

Console.WriteLine($"BikeXperience-Ordner: {parameter.Quellordner}");
Console.WriteLine($"Zielordner:           {parameter.Zielordner}");
Console.WriteLine();

// --- 2. Zielordner prüfen ---
switch (PruefeZielordner.Ausfuehren(parameter.Zielordner))
{
    case ZielordnerZustand.FremderInhalt:
        Console.Error.WriteLine("Der Zielordner enthält Dateien, aber keine AndyTours-Daten. " +
            "Damit nichts Fremdes gelöscht wird, bitte einen leeren Ordner angeben. Es wurde nichts geändert.");
        return 1;

    case ZielordnerZustand.MitDaten:
        Console.Write("Der Zielordner enthält bereits AndyTours-Daten. Vorhandene Daten ersetzen? (j/N) ");
        if (!IstJaAntwort.Ausfuehren(Console.ReadLine()))
        {
            Console.WriteLine("Abgebrochen – der Zielordner bleibt unverändert.");
            return 1;
        }

        break;
}

// --- 3. Altdaten lesen (nur lesend) ---
Migrationsergebnis ergebnis;
try
{
    ergebnis = UebernehmeAltdaten.Ausfuehren(parameter.Quellordner);
}
catch (Exception fehler) when (fehler is FileNotFoundException or DirectoryNotFoundException)
{
    Console.Error.WriteLine($"Im Ordner {parameter.Quellordner} liegen keine BikeXperience-Daten: {fehler.Message}");
    Console.Error.WriteLine("Es wurde nichts geschrieben.");
    return 1;
}

// --- 4. Zwischenordner aufbauen und austauschen ---
string? hinweis;
try
{
    Console.WriteLine("Daten werden umgewandelt und Aufzeichnungen kopiert …");
    string zwischenordner = ErstelleZwischenordner.Ausfuehren(ergebnis, parameter.Quellordner, parameter.Zielordner);
    hinweis = TauscheZielordnerAus.Ausfuehren(zwischenordner, parameter.Zielordner);
}
catch (Exception fehler) when (fehler is IOException or UnauthorizedAccessException or SqliteException)
{
    Console.Error.WriteLine($"Die Umwandlung ist abgebrochen: {fehler.Message}");
    Console.Error.WriteLine("Der Zielordner bleibt unverändert. Ist AndyTours oder ein Explorer-Fenster im Zielordner geöffnet?");
    return 1;
}

// --- 5. Zusammenfassung ---
Console.WriteLine();
Console.Write(ErstelleZusammenfassung.Ausfuehren(ergebnis, parameter.Zielordner));
if (hinweis is not null)
{
    Console.WriteLine(hinweis);
}

return 0;
