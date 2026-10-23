using Microsoft.Data.Sqlite;

namespace AndyTours.Daten;

/// <summary>
/// Legt die Tabellen der AndyTours-Datenbank an, sofern sie noch nicht existieren.
/// </summary>
/// <remarks>
/// Konventionen der Datenbank:
/// <list type="bullet">
/// <item>Datumswerte als Text „JJJJ-MM-TT", Uhrzeiten als „hh:mm" – so sortieren sie auch als Text richtig.</item>
/// <item>Zeitdauern als ganze Sekunden.</item>
/// <item>Wahrheitswerte als 0/1 (SQLite kennt keinen eigenen Typ dafür).</item>
/// <item>Aufzählungen (Anhangtyp, Wartungstyp, Schwellwertart) als Name im Klartext, damit die
/// Datenbank auch mit einem SQLite-Werkzeug lesbar bleibt.</item>
/// </list>
/// Die Schemaversion steht in <c>PRAGMA user_version</c>; spätere Änderungen am Schema können
/// daran erkennen, ob eine ältere Datenbank angepasst werden muss.
/// </remarks>
public static class ErstelleSchema
{
    /// <summary>Aktuelle Version des Datenbankschemas.</summary>
    public const int Schemaversion = 2;

    /// <summary>
    /// Führt die CREATE-Anweisungen aus. Vorhandene Tabellen und Daten bleiben unverändert.
    /// </summary>
    /// <param name="verbindung">Geöffnete Datenbankverbindung.</param>
    public static void Ausfuehren(SqliteConnection verbindung)
    {
        using SqliteCommand befehl = verbindung.CreateCommand();
        befehl.CommandText = $"""
            -- Sportler (Nutzerverwaltung)
            CREATE TABLE IF NOT EXISTS Fahrer (
                Id                  INTEGER PRIMARY KEY,
                Vorname             TEXT    NOT NULL,
                Nachname            TEXT    NOT NULL,
                Geburtstag          TEXT,
                GroesseCm           INTEGER NOT NULL,
                GewichtKg           REAL    NOT NULL,
                Ruhepuls            INTEGER NOT NULL,
                Maxpuls             INTEGER NOT NULL,
                TrainingspulsVon    INTEGER NOT NULL,
                TrainingspulsBis    INTEGER NOT NULL,
                Heimatort           TEXT    NOT NULL,
                HeimatBreitengrad   REAL,
                HeimatLaengengrad   REAL
            );

            CREATE TABLE IF NOT EXISTS Aktivitaet (
                Id                       INTEGER PRIMARY KEY,
                Name                     TEXT    NOT NULL UNIQUE COLLATE NOCASE,
                IstHaupt                 INTEGER NOT NULL,
                Geschwindigkeitseinheit  TEXT    NOT NULL,
                ZielKm                   REAL    NOT NULL,
                ZielHm                   REAL    NOT NULL,
                ZielAnzahl               INTEGER NOT NULL
            );

            CREATE TABLE IF NOT EXISTS Kategorie (
                Id           INTEGER PRIMARY KEY,
                Name         TEXT    NOT NULL UNIQUE COLLATE NOCASE,
                IstStandard  INTEGER NOT NULL
            );

            -- Ausrüstung; Kettenblätter und Ritzel als kommagetrennte Zähnezahlen („39,53")
            CREATE TABLE IF NOT EXISTS Equipment (
                Id                 INTEGER PRIMARY KEY,
                Name               TEXT    NOT NULL UNIQUE COLLATE NOCASE,
                AktivitaetId       INTEGER REFERENCES Aktivitaet(Id),
                GewichtKg          REAL    NOT NULL,
                RadumfangMm        INTEGER NOT NULL,
                CrCoeff            REAL    NOT NULL,
                CmCoeff            REAL    NOT NULL,
                CwCoeff            REAL    NOT NULL,
                CwaCoeff           REAL    NOT NULL,
                SinCoeff           REAL    NOT NULL,
                Kettenblaetter     TEXT    NOT NULL,
                Ritzel             TEXT    NOT NULL,
                Geraetezuordnung   TEXT    NOT NULL,
                StandardKategorie  TEXT    NOT NULL,
                IstStandard        INTEGER NOT NULL,
                Ausgemustert       INTEGER NOT NULL,
                Anschaffungsdatum  TEXT,
                Bildpfad           TEXT    NOT NULL,
                AnfangsdistanzKm   REAL    NOT NULL
            );

            CREATE TABLE IF NOT EXISTS Trainingseinheit (
                Id                  INTEGER PRIMARY KEY,
                FahrerId            INTEGER NOT NULL REFERENCES Fahrer(Id),
                Datum               TEXT    NOT NULL,
                Startzeit           TEXT,
                Name                TEXT    NOT NULL,
                FahrzeitSekunden    INTEGER NOT NULL,
                DistanzKm           REAL    NOT NULL,
                Hoehenmeter         INTEGER NOT NULL,
                MaxHoehe            INTEGER NOT NULL,
                StartHoehe          INTEGER NOT NULL,
                MaxGeschwindigkeit  REAL    NOT NULL,
                HfDurchschnitt      INTEGER NOT NULL,
                HfMax               INTEGER NOT NULL,
                HfMin               INTEGER NOT NULL,
                Trittfrequenz       INTEGER NOT NULL,
                Temperatur          INTEGER NOT NULL,
                LeistungWatt        REAL    NOT NULL,
                KoerpergewichtKg    REAL    NOT NULL,
                Erholungspuls       INTEGER NOT NULL,
                Trainingsbereich    TEXT    NOT NULL,
                Kommentar           TEXT    NOT NULL,
                AktivitaetId        INTEGER NOT NULL REFERENCES Aktivitaet(Id),
                EquipmentId         INTEGER REFERENCES Equipment(Id)
            );

            -- Die Tabelle wird fast immer nach Datum gefiltert und sortiert
            CREATE INDEX IF NOT EXISTS IX_Trainingseinheit_Datum ON Trainingseinheit(Datum);

            -- n:m-Zuordnung Einheit ↔ Kategorie; wird eine Einheit gelöscht, verschwinden ihre Zuordnungen mit
            CREATE TABLE IF NOT EXISTS EinheitKategorie (
                EinheitId    INTEGER NOT NULL REFERENCES Trainingseinheit(Id) ON DELETE CASCADE,
                KategorieId  INTEGER NOT NULL REFERENCES Kategorie(Id),
                PRIMARY KEY (EinheitId, KategorieId)
            );

            CREATE TABLE IF NOT EXISTS Anhang (
                Id             INTEGER PRIMARY KEY,
                EinheitId      INTEGER NOT NULL REFERENCES Trainingseinheit(Id) ON DELETE CASCADE,
                Typ            TEXT    NOT NULL,
                RelativerPfad  TEXT    NOT NULL
            );

            CREATE INDEX IF NOT EXISTS IX_Anhang_EinheitId ON Anhang(EinheitId);

            CREATE TABLE IF NOT EXISTS Wartung (
                Id           INTEGER PRIMARY KEY,
                Datum        TEXT    NOT NULL,
                EquipmentId  INTEGER REFERENCES Equipment(Id),
                Taetigkeit   TEXT    NOT NULL,
                Kommentar    TEXT    NOT NULL,
                Typ          TEXT    NOT NULL,
                AlarmKm      REAL    NOT NULL,
                Aktiv        INTEGER NOT NULL
            );

            CREATE TABLE IF NOT EXISTS Schwellwert (
                Id           INTEGER PRIMARY KEY,
                Art          TEXT    NOT NULL,
                Reihenfolge  INTEGER NOT NULL,
                Name         TEXT    NOT NULL,
                Von          REAL    NOT NULL,
                Bis          REAL    NOT NULL,
                Farbe        TEXT    NOT NULL
            );

            -- Einstellungen, die zum Datenbestand gehören (z. B. Datensicherung), als Schlüssel/Wert-Paare;
            -- seit Schemaversion 2. Ältere Datenbanken erhalten die Tabelle beim nächsten Öffnen automatisch.
            CREATE TABLE IF NOT EXISTS Einstellung (
                Schluessel  TEXT    PRIMARY KEY,
                Wert        TEXT    NOT NULL
            );

            PRAGMA user_version = {Schemaversion};
            """;
        befehl.ExecuteNonQuery();
    }
}
