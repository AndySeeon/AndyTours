namespace AndyTours.Tests.Altdatenuebernahme;

/// <summary>
/// Erzeugt einen kleinen, künstlichen BikeXperience-Ordner für die Tests der Umwandlung.
/// </summary>
/// <remarks>
/// Enthält bewusst alle Sonderfälle der Spezifikation 002:
/// <list type="bullet">
/// <item>Zugangsdaten in der INI (<c>[WebAccess]</c>-Passwort, Dropbox-Schlüssel), die nicht übernommen werden dürfen.</item>
/// <item>Eine Einheit mit vorhandener und eine mit fehlender Aufzeichnung.</item>
/// <item>Eine fehlerhafte Zeile in <c>workouts.csv</c> (Zeile 4).</item>
/// <item>Eine Wartung und einen Jahresordner mit Aufzeichnung.</item>
/// </list>
/// </remarks>
public static class TestAltdaten
{
    /// <summary>Passwort aus <c>[WebAccess]</c>; darf im Zielordner nirgends auftauchen.</summary>
    public const string Passwort = "Geheim-4711";

    /// <summary>Dropbox-Schlüssel aus <c>[Settings]</c>; darf im Zielordner nirgends auftauchen.</summary>
    public const string DropboxSchluessel = "Dropbox-Schluessel-0815";

    /// <summary>Relativer Pfad der vorhandenen Aufzeichnung.</summary>
    public const string VorhandeneAufzeichnung = "2016/2016-03-05-08-40-41.fit";

    /// <summary>Relativer Pfad der fehlenden Aufzeichnung.</summary>
    public const string FehlendeAufzeichnung = "2017/2017-04-01-09-00-00.fit";

    /// <summary>
    /// Baut eine gültige Datenzeile nach dem Muster der echten workouts.csv.
    /// </summary>
    /// <param name="datum">Datum der Einheit, z. B. „05.03.2016“.</param>
    /// <param name="name">Name der Einheit.</param>
    /// <param name="aufzeichnung">Relativer Pfad der Aufzeichnung.</param>
    private static string Workoutzeile(string datum, string name, string aufzeichnung) =>
        $"{datum};{name};04:33:23;77.83;859;970;53.07;135;Namur;WSA;538;83;5;166; ;08:40;Bike;Training;95.0;91;150.0;0;'';1;'{aufzeichnung}';'{aufzeichnung}';''";

    /// <summary>
    /// Legt die Dateien im angegebenen (vorhandenen) Ordner an.
    /// </summary>
    /// <param name="ordner">Leerer Ordner, der zum BikeXperience-Ordner wird.</param>
    public static void Erzeuge(string ordner)
    {
        File.WriteAllLines(Path.Combine(ordner, "bikexperience.ini"),
        [
            "[Settings]",
            $"dropboxKey={DropboxSchluessel}",
            "[Activity]",
            "Bike=true;km/h;8000;80000;100",
            "[Category]",
            "Training=false",
            "[Biker]",
            "firstname=Testfahrer",
            "birthday=28.05.1963",
            "pulsemin=50",
            "pulsemax=168",
            "[LimitsGrad]",
            "Flach;0,3;170,0,0",
            "[Equipment.Namur]",
            "activity=Bike",
            "rejected=false",
            "[WebAccess]",
            "user=andy",
            $"password={Passwort}",
        ]);

        File.WriteAllLines(Path.Combine(ordner, "workouts.csv"),
        [
            "3",
            Workoutzeile("05.03.2016", "Stoisser B�ckeralm", VorhandeneAufzeichnung),
            Workoutzeile("01.04.2017", "Ohne Aufzeichnung", FehlendeAufzeichnung),
            "06.04.2017;kaputte Zeile;nur drei Felder",
        ]);

        File.WriteAllLines(Path.Combine(ordner, "maintanence.dat"), ["10.09.2022;Namur;Kettenwechsel;auf 0,7;0;true;0"]);

        Directory.CreateDirectory(Path.Combine(ordner, "2016"));
        File.WriteAllBytes(Path.Combine(ordner, VorhandeneAufzeichnung), [1, 2, 3, 4, 5]);
    }
}
