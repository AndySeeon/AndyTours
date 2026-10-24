using AndyTours.Core.Modell;

namespace AndyTours.Core.Berechnungen;

/// <summary>
/// Wandelt die persönlichen Daten eines Sportlers in die Eingaben des Fensters „Persönliche Daten“ um und zurück.
/// </summary>
/// <remarks>
/// Beide Richtungen stehen zusammen, weil sie dieselbe Regel teilen: Ein unbekannter Wert ist bei Zahlen
/// als 0 gespeichert und erscheint als leeres Feld; ein leeres Feld wird wieder als 0 gespeichert. Ein
/// fehlender Geburtstag ist ebenfalls ein leeres Feld.
/// </remarks>
public static class FahrerEingaben
{
    /// <summary>Kleinste und größte erlaubte Körpergröße in Zentimetern.</summary>
    public const int GroesseMin = 100, GroesseMax = 250;

    /// <summary>Kleinstes und größtes erlaubtes Gewicht in Kilogramm.</summary>
    public const double GewichtMin = 30, GewichtMax = 250;

    /// <summary>Kleinster und größter erlaubter Ruhepuls.</summary>
    public const int RuhepulsMin = 30, RuhepulsMax = 120;

    /// <summary>Kleinster und größter erlaubter Maximalpuls.</summary>
    public const int MaxpulsMin = 100, MaxpulsMax = 230;

    /// <summary>
    /// Liefert die Eingaben, mit denen das Fenster für diesen Sportler gefüllt wird.
    /// </summary>
    /// <param name="fahrer">Der Sportler; ist noch keiner vorhanden, ein leerer <see cref="Fahrer"/>.</param>
    /// <returns>Die Eingaben; unbekannte Werte (0 bzw. kein Datum) als leeres Feld.</returns>
    public static Fahrereingaben AlsEingaben(Fahrer fahrer) => new()
    {
        Vorname = fahrer.Vorname,
        Nachname = fahrer.Nachname,
        Geburtstag = Eingabetext.AlsText(fahrer.Geburtstag),
        Groesse = Eingabetext.AlsText(fahrer.GroesseCm > 0 ? fahrer.GroesseCm : (int?)null),
        Gewicht = Eingabetext.AlsText(fahrer.GewichtKg > 0 ? fahrer.GewichtKg : (double?)null),
        Ruhepuls = Eingabetext.AlsText(fahrer.Ruhepuls > 0 ? fahrer.Ruhepuls : (int?)null),
        Maxpuls = Eingabetext.AlsText(fahrer.Maxpuls > 0 ? fahrer.Maxpuls : (int?)null),
        Heimatort = fahrer.Heimatort,
    };

    /// <summary>
    /// Prüft die Eingaben und erzeugt daraus den Sportler.
    /// </summary>
    /// <remarks>
    /// Jedes Feld darf leer sein (unbekannt). Geprüft wird in dieser Reihenfolge; der Hinweis nennt den
    /// ersten Fehler:
    /// <list type="bullet">
    /// <item>Geburtstag: gültiges Datum, nicht nach <paramref name="heute"/>.</item>
    /// <item>Größe: ganze Zahl von 100 bis 250 cm.</item>
    /// <item>Gewicht: Zahl mit höchstens einer Nachkommastelle von 30 bis 250 kg.</item>
    /// <item>Ruhepuls: ganze Zahl von 30 bis 120; Maximalpuls: ganze Zahl von 100 bis 230.</item>
    /// <item>Sind beide Pulswerte angegeben, muss der Ruhepuls kleiner als der Maximalpuls sein.</item>
    /// </list>
    /// Nummer, Trainingspuls und Koordinaten des Heimatorts werden aus <paramref name="basis"/> übernommen.
    /// </remarks>
    /// <param name="eingaben">Die Eingaben des Fensters.</param>
    /// <param name="basis">Der bisherige Sportler bzw. ein leerer (Nummer 0).</param>
    /// <param name="heute">Das heutige Datum (für den Geburtstag).</param>
    /// <returns>Der neue Sportler und <c>null</c>, oder <c>null</c> und der Hinweis mit dem Grund.</returns>
    public static (Fahrer? Fahrer, string? Hinweis) AlsFahrer(Fahrereingaben eingaben, Fahrer basis, DateOnly heute)
    {
        if (!Eingabetext.LeseDatum(eingaben.Geburtstag, out DateOnly? geburtstag))
        {
            return (null, "Der Geburtstag ist ungültig (Form: 28.05.1963).");
        }

        if (geburtstag > heute)
        {
            return (null, "Der Geburtstag darf nicht in der Zukunft liegen.");
        }

        if (!Eingabetext.LeseGanzzahl(eingaben.Groesse, out int? groesse) || groesse is < GroesseMin or > GroesseMax)
        {
            return (null, $"Die Größe muss eine ganze Zahl von {GroesseMin} bis {GroesseMax} cm sein.");
        }

        // Höchstens eine Nachkommastelle: der Wert ändert sich beim Runden auf eine Stelle nicht
        if (!Eingabetext.LeseZahl(eingaben.Gewicht, out double? gewicht)
            || gewicht is < GewichtMin or > GewichtMax
            || (gewicht is double g && Math.Abs(Math.Round(g, 1) - g) > 1e-9))
        {
            return (null, $"Das Gewicht muss eine Zahl mit höchstens einer Nachkommastelle von {GewichtMin} bis {GewichtMax} kg sein.");
        }

        if (!Eingabetext.LeseGanzzahl(eingaben.Ruhepuls, out int? ruhepuls) || ruhepuls is < RuhepulsMin or > RuhepulsMax)
        {
            return (null, $"Der Ruhepuls muss eine ganze Zahl von {RuhepulsMin} bis {RuhepulsMax} sein.");
        }

        if (!Eingabetext.LeseGanzzahl(eingaben.Maxpuls, out int? maxpuls) || maxpuls is < MaxpulsMin or > MaxpulsMax)
        {
            return (null, $"Der Maximalpuls muss eine ganze Zahl von {MaxpulsMin} bis {MaxpulsMax} sein.");
        }

        if (ruhepuls >= maxpuls)
        {
            return (null, "Der Ruhepuls muss kleiner als der Maximalpuls sein.");
        }

        var fahrer = new Fahrer
        {
            Id = basis.Id,
            Vorname = eingaben.Vorname.Trim(),
            Nachname = eingaben.Nachname.Trim(),
            Geburtstag = geburtstag,
            GroesseCm = groesse ?? 0,
            GewichtKg = gewicht ?? 0,
            Ruhepuls = ruhepuls ?? 0,
            Maxpuls = maxpuls ?? 0,
            TrainingspulsVon = basis.TrainingspulsVon,
            TrainingspulsBis = basis.TrainingspulsBis,
            Heimatort = eingaben.Heimatort.Trim(),
            HeimatBreitengrad = basis.HeimatBreitengrad,
            HeimatLaengengrad = basis.HeimatLaengengrad,
        };
        return (fahrer, null);
    }
}
