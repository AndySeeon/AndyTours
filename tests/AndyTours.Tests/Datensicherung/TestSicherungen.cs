using Microsoft.Data.Sqlite;
using AndyTours.Daten;

namespace AndyTours.Tests.Datensicherung;

/// <summary>
/// Erzeugt für die Tests des Wiederherstellens besondere Sicherungsdateien
/// (ältere und neuere Schemaversion, fremde und beschädigte Dateien).
/// </summary>
public static class TestSicherungen
{
    /// <summary>
    /// Schreibt eine gültige Sicherung mit <paramref name="anzahlEinheiten"/> Einheiten des Namens <paramref name="name"/>.
    /// </summary>
    public static void Gueltig(string datei, int anzahlEinheiten, string name)
    {
        using SqliteConnection verbindung = OeffneDatenbank.Ausfuehren(datei);
        DatenbestandTabellen.Speichern(verbindung, TestDatenbestand.Erzeuge(anzahlEinheiten, name));
    }

    /// <summary>
    /// Schreibt eine Sicherung wie von AndyTours mit Schemaversion 1: ohne Tabelle <c>Einstellung</c>.
    /// </summary>
    public static void Version1(string datei, int anzahlEinheiten, string name)
    {
        Gueltig(datei, anzahlEinheiten, name);
        Fuehre(datei, "DROP TABLE Einstellung; PRAGMA user_version = 1;");
    }

    /// <summary>Schreibt eine Sicherung mit der angegebenen Schemaversion (z. B. eine höhere als die aktuelle = neuere Programmversion).</summary>
    public static void MitVersion(string datei, int version)
    {
        Gueltig(datei, 1, "Zukunft");
        Fuehre(datei, $"PRAGMA user_version = {version};");
    }

    /// <summary>Schreibt eine SQLite-Datenbank, die nichts mit AndyTours zu tun hat.</summary>
    public static void Fremd(string datei) => Fuehre(datei, "CREATE TABLE Adresse (Name TEXT);");

    /// <summary>Schreibt eine gültige Sicherung und schneidet sie danach auf die halbe Länge ab.</summary>
    public static void Abgeschnitten(string datei)
    {
        Gueltig(datei, 30, "Abgeschnitten");
        byte[] inhalt = File.ReadAllBytes(datei);
        File.WriteAllBytes(datei, inhalt[..(inhalt.Length / 2)]);
    }

    /// <summary>Führt SQL direkt auf der Datei aus, ohne Schema anzulegen.</summary>
    private static void Fuehre(string datei, string sql)
    {
        using var verbindung = new SqliteConnection(new SqliteConnectionStringBuilder { DataSource = datei, Pooling = false }.ToString());
        verbindung.Open();
        using SqliteCommand befehl = verbindung.CreateCommand();
        befehl.CommandText = sql;
        befehl.ExecuteNonQuery();
    }
}
