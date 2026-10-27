// Einstieg des Programms „AndyTours.Altdatenuebernahme“ (Spezifikationen 002 und 033).
//
// Wandelt die BikeXperience-Altdaten einmalig in einen AndyTours-Datenordner um. Für das Installationsprogramm
// kann es außerdem erkennen, was in einem Ordner liegt, und Aufzeichnungen im FIT-Format als Touren übernehmen.
// Aufrufe:
//   AndyTours.Altdatenuebernahme [BikeXperience-Ordner] [Zielordner] [--ohne-rueckfrage]
//       Ohne Angaben: Daten → AppDaten im Projektordner.
//   AndyTours.Altdatenuebernahme --erkennen <Ordner>
//       Rückgabecode 10/11/12 (BikeXperience / nur Aufzeichnungen / nichts); Ausgabe: Anzahl der Aufzeichnungen.
//   AndyTours.Altdatenuebernahme --aufzeichnungen <Ordner> <Zielordner> [--ohne-rueckfrage]
//
// Ablauf beim Umwandeln und Übernehmen:
//   1. Aufrufparameter auswerten
//   2. Zielordner prüfen; enthält er schon Daten, nachfragen (j/N) – ohne Rückfrage bleibt er unverändert (Code 2)
//   3. Bisherige Daten vollständig lesen – vorher wird nichts geschrieben
//   4. Zwischenordner aufbauen und gegen den Zielordner austauschen
//   5. Zusammenfassung ausgeben
// Rückgabecodes: siehe Rueckgabecode.cs (0 = Erfolg, 1 = Fehler oder abgebrochen; Zielordner dann unverändert).

using System.Globalization;
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
    Console.Error.WriteLine("Aufruf: AndyTours.Altdatenuebernahme [BikeXperience-Ordner] [Zielordner] [--ohne-rueckfrage]");
    Console.Error.WriteLine("        AndyTours.Altdatenuebernahme --erkennen <Ordner>");
    Console.Error.WriteLine("        AndyTours.Altdatenuebernahme --aufzeichnungen <Ordner> <Zielordner> [--ohne-rueckfrage]");
    return Rueckgabecode.Fehler;
}

if (parameter.Modus == Programmmodus.Erkennen)
{
    return Erkenne(parameter.Quellordner);
}

bool aufzeichnungen = parameter.Modus == Programmmodus.Aufzeichnungen;
Console.WriteLine(aufzeichnungen
    ? $"Ordner mit Aufzeichnungen: {parameter.Quellordner}"
    : $"BikeXperience-Ordner: {parameter.Quellordner}");
Console.WriteLine($"Zielordner:           {parameter.Zielordner}");
Console.WriteLine();

// --- 2. Zielordner prüfen ---
switch (PruefeZielordner.Ausfuehren(parameter.Zielordner))
{
    case ZielordnerZustand.FremderInhalt:
        Console.Error.WriteLine("Der Zielordner enthält Dateien, aber keine AndyTours-Daten. " +
            "Damit nichts Fremdes gelöscht wird, bitte einen leeren Ordner angeben. Es wurde nichts geändert.");
        return Rueckgabecode.Fehler;

    case ZielordnerZustand.MitDaten when parameter.OhneRueckfrage:
        Console.WriteLine("Im Zielordner liegen bereits AndyTours-Daten. Sie bleiben unverändert; es wurde nichts übernommen.");
        return Rueckgabecode.ZielHatDaten;

    case ZielordnerZustand.MitDaten:
        Console.Write("Der Zielordner enthält bereits AndyTours-Daten. Vorhandene Daten ersetzen? (j/N) ");
        if (!IstJaAntwort.Ausfuehren(Console.ReadLine()))
        {
            Console.WriteLine("Abgebrochen – der Zielordner bleibt unverändert.");
            return Rueckgabecode.Fehler;
        }

        break;
}

return aufzeichnungen
    ? UebernimmAufzeichnungen(parameter.Quellordner, parameter.Zielordner)
    : WandleUm(parameter.Quellordner, parameter.Zielordner);

// Meldet über den Rückgabecode, was im Ordner liegt, und gibt die Anzahl der Aufzeichnungen aus.
// Die Ausgabe ist nur die Zahl, damit das Installationsprogramm sie einfach lesen kann.
static int Erkenne(string ordner)
{
    Altdatenfund fund = ErkenneAltdaten.Ausfuehren(ordner);
    Console.WriteLine(fund.Aufzeichnungen.Count.ToString(CultureInfo.InvariantCulture));
    return fund.Art switch
    {
        Altdatenart.BikeXperience => Rueckgabecode.BikeXperience,
        Altdatenart.NurAufzeichnungen => Rueckgabecode.NurAufzeichnungen,
        _ => Rueckgabecode.Nichts,
    };
}

// Schritte 3 bis 5 für BikeXperience-Daten
static int WandleUm(string quellordner, string zielordner)
{
    // --- 3. Altdaten lesen (nur lesend) ---
    Migrationsergebnis ergebnis;
    try
    {
        ergebnis = UebernehmeAltdaten.Ausfuehren(quellordner);
    }
    catch (Exception fehler) when (fehler is FileNotFoundException or DirectoryNotFoundException)
    {
        Console.Error.WriteLine($"Im Ordner {quellordner} liegen keine BikeXperience-Daten: {fehler.Message}");
        Console.Error.WriteLine("Es wurde nichts geschrieben.");
        return Rueckgabecode.Fehler;
    }

    // --- 4. Zwischenordner aufbauen und austauschen ---
    string? hinweis;
    try
    {
        Console.WriteLine("Daten werden umgewandelt und Aufzeichnungen kopiert …");
        string zwischenordner = ErstelleZwischenordner.Ausfuehren(ergebnis, quellordner, zielordner);
        hinweis = TauscheZielordnerAus.Ausfuehren(zwischenordner, zielordner);
    }
    catch (Exception fehler) when (fehler is IOException or UnauthorizedAccessException or SqliteException)
    {
        MeldeAbbruch("Die Umwandlung", fehler);
        return Rueckgabecode.Fehler;
    }

    // --- 5. Zusammenfassung ---
    Console.WriteLine();
    Console.Write(ErstelleZusammenfassung.Ausfuehren(ergebnis, zielordner));
    if (hinweis is not null)
    {
        Console.WriteLine(hinweis);
    }

    return Rueckgabecode.Erfolg;
}

// Schritte 3 bis 5 für einen Ordner, in dem nur Aufzeichnungen liegen
static int UebernimmAufzeichnungen(string quellordner, string zielordner)
{
    // --- 3. Aufzeichnungen suchen (nur lesend) ---
    Altdatenfund fund = ErkenneAltdaten.Ausfuehren(quellordner);
    if (fund.Art != Altdatenart.NurAufzeichnungen)
    {
        Console.Error.WriteLine(fund.Art == Altdatenart.BikeXperience
            ? $"Im Ordner {quellordner} liegen BikeXperience-Daten; bitte ohne {LeseAufrufparameter.SchalterAufzeichnungen} umwandeln."
            : $"Im Ordner {quellordner} liegen keine Aufzeichnungen.");
        Console.Error.WriteLine("Es wurde nichts geschrieben.");
        return Rueckgabecode.Fehler;
    }

    // --- 4. Zwischenordner aufbauen und austauschen ---
    Aufzeichnungsergebnis ergebnis;
    string? hinweis;
    try
    {
        Console.WriteLine($"{fund.Aufzeichnungen.Count} Aufzeichnungen werden als Touren übernommen …");
        (string zwischenordner, ergebnis) = ErstelleAufzeichnungsordner.Ausfuehren(fund.Aufzeichnungen, quellordner, zielordner, DateTime.Now);
        hinweis = TauscheZielordnerAus.Ausfuehren(zwischenordner, zielordner);
    }
    catch (Exception fehler) when (fehler is IOException or UnauthorizedAccessException or SqliteException)
    {
        MeldeAbbruch("Die Übernahme der Aufzeichnungen", fehler);
        return Rueckgabecode.Fehler;
    }

    // --- 5. Zusammenfassung ---
    Console.WriteLine();
    Console.Write(ErstelleAufzeichnungsbericht.Zusammenfassung(ergebnis, zielordner));
    if (hinweis is not null)
    {
        Console.WriteLine(hinweis);
    }

    return Rueckgabecode.Erfolg;
}

// Meldet einen Abbruch beim Schreiben; der Zielordner ist dann unverändert
static void MeldeAbbruch(string was, Exception fehler)
{
    Console.Error.WriteLine($"{was} ist abgebrochen: {fehler.Message}");
    Console.Error.WriteLine("Der Zielordner bleibt unverändert. Ist AndyTours oder ein Explorer-Fenster im Zielordner geöffnet?");
}
