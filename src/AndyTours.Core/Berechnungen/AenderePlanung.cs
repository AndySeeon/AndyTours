using AndyTours.Core.Modell;

namespace AndyTours.Core.Berechnungen;

/// <summary>
/// Ändert die Wegpunkte einer <see cref="Tourenplanung"/>: anhängen, einfügen, verschieben, entfernen, Profil wechseln
/// und ein ermitteltes Teilstück eintragen.
/// </summary>
/// <remarks>
/// Jede Methode liefert eine neue Planung; die übergebene bleibt unverändert. Teilstücke, die eine Änderung berührt,
/// werden <c>null</c> („noch zu ermitteln“); alle übrigen bleiben erhalten, damit der Routingdienst nur das Nötige neu
/// berechnet.
/// </remarks>
public static class AenderePlanung
{
    /// <summary>
    /// Hängt einen Wegpunkt als neues Ziel an.
    /// </summary>
    /// <param name="planung">Die bisherige Planung.</param>
    /// <param name="position">Ort des neuen Wegpunkts.</param>
    /// <returns>Die Planung mit dem neuen letzten Wegpunkt; das neue Teilstück dorthin ist noch zu ermitteln.</returns>
    public static Tourenplanung Anhaengen(Tourenplanung planung, Geoposition position)
    {
        List<Teilstueck?> teilstuecke = [.. planung.Teilstuecke];
        if (planung.Wegpunkte.Count > 0)
        {
            teilstuecke.Add(null);
        }

        return planung with { Wegpunkte = [.. planung.Wegpunkte, position], Teilstuecke = teilstuecke };
    }

    /// <summary>
    /// Fügt einen Wegpunkt zwischen zwei vorhandene ein.
    /// </summary>
    /// <param name="planung">Die bisherige Planung.</param>
    /// <param name="nummer">Nummer (ab 0), die der neue Wegpunkt bekommt; zwischen 1 und der Zahl der Wegpunkte − 1.</param>
    /// <param name="position">Ort des neuen Wegpunkts.</param>
    /// <returns>Die Planung mit dem neuen Wegpunkt; das bisherige Teilstück wird zu zwei noch zu ermittelnden.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Die Nummer liegt nicht zwischen zwei Wegpunkten.</exception>
    public static Tourenplanung Einfuegen(Tourenplanung planung, int nummer, Geoposition position)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(nummer, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(nummer, planung.Wegpunkte.Count - 1);

        List<Geoposition> wegpunkte = [.. planung.Wegpunkte];
        wegpunkte.Insert(nummer, position);
        List<Teilstueck?> teilstuecke = [.. planung.Teilstuecke];
        teilstuecke[nummer - 1] = null;
        teilstuecke.Insert(nummer, null);
        return planung with { Wegpunkte = wegpunkte, Teilstuecke = teilstuecke };
    }

    /// <summary>
    /// Verschiebt einen Wegpunkt an einen neuen Ort.
    /// </summary>
    /// <param name="planung">Die bisherige Planung.</param>
    /// <param name="nummer">Nummer (ab 0) des Wegpunkts.</param>
    /// <param name="position">Neuer Ort.</param>
    /// <returns>Die Planung mit dem verschobenen Wegpunkt; die angrenzenden Teilstücke sind noch zu ermitteln.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Einen Wegpunkt mit dieser Nummer gibt es nicht.</exception>
    public static Tourenplanung Verschieben(Tourenplanung planung, int nummer, Geoposition position)
    {
        PruefeNummer(planung, nummer);

        List<Geoposition> wegpunkte = [.. planung.Wegpunkte];
        wegpunkte[nummer] = position;
        List<Teilstueck?> teilstuecke = [.. planung.Teilstuecke];
        if (nummer > 0)
        {
            teilstuecke[nummer - 1] = null;
        }

        if (nummer < teilstuecke.Count)
        {
            teilstuecke[nummer] = null;
        }

        return planung with { Wegpunkte = wegpunkte, Teilstuecke = teilstuecke };
    }

    /// <summary>
    /// Entfernt einen Wegpunkt.
    /// </summary>
    /// <remarks>
    /// Beim Start oder Ziel fällt das angrenzende Teilstück weg. Bei einem Wegpunkt dazwischen werden seine beiden
    /// Teilstücke zu einem, das die Nachbarn neu verbindet und noch zu ermitteln ist.
    /// </remarks>
    /// <param name="planung">Die bisherige Planung.</param>
    /// <param name="nummer">Nummer (ab 0) des Wegpunkts.</param>
    /// <returns>Die Planung ohne diesen Wegpunkt.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Einen Wegpunkt mit dieser Nummer gibt es nicht.</exception>
    public static Tourenplanung Entfernen(Tourenplanung planung, int nummer)
    {
        PruefeNummer(planung, nummer);

        List<Geoposition> wegpunkte = [.. planung.Wegpunkte];
        wegpunkte.RemoveAt(nummer);
        List<Teilstueck?> teilstuecke = [.. planung.Teilstuecke];
        if (teilstuecke.Count > 0)
        {
            if (nummer == 0)
            {
                teilstuecke.RemoveAt(0);
            }
            else if (nummer == planung.Wegpunkte.Count - 1)
            {
                teilstuecke.RemoveAt(teilstuecke.Count - 1);
            }
            else
            {
                teilstuecke.RemoveAt(nummer);
                teilstuecke[nummer - 1] = null;
            }
        }

        return planung with { Wegpunkte = wegpunkte, Teilstuecke = teilstuecke };
    }

    /// <summary>
    /// Entfernt den zuletzt gesetzten Wegpunkt (das Ziel) samt seinem Teilstück („Letzten Punkt entfernen“).
    /// </summary>
    /// <param name="planung">Die bisherige Planung.</param>
    /// <returns>Die Planung ohne das Ziel; eine leere Planung bleibt leer.</returns>
    public static Tourenplanung EntferneLetzten(Tourenplanung planung) =>
        planung.Wegpunkte.Count == 0 ? planung : Entfernen(planung, planung.Wegpunkte.Count - 1);

    /// <summary>
    /// Wechselt das Fahrprofil; alle Teilstücke sind danach mit dem neuen Profil zu ermitteln.
    /// </summary>
    /// <param name="planung">Die bisherige Planung.</param>
    /// <param name="profil">Das neue Fahrprofil.</param>
    /// <returns>Die Planung mit gleichen Wegpunkten und lauter noch zu ermittelnden Teilstücken.</returns>
    public static Tourenplanung WechsleProfil(Tourenplanung planung, Fahrprofil profil) =>
        planung with { Profil = profil, Teilstuecke = [.. planung.Teilstuecke.Select(_ => (Teilstueck?)null)] };

    /// <summary>
    /// Trägt ein ermitteltes Teilstück ein.
    /// </summary>
    /// <param name="planung">Die bisherige Planung.</param>
    /// <param name="nummer">Nummer (ab 0) des Teilstücks; Teilstück 0 verbindet Wegpunkt 0 und 1.</param>
    /// <param name="teilstueck">Das ermittelte Teilstück.</param>
    /// <returns>Die Planung mit dem eingetragenen Teilstück.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Ein Teilstück mit dieser Nummer gibt es nicht.</exception>
    public static Tourenplanung SetzeTeilstueck(Tourenplanung planung, int nummer, Teilstueck teilstueck)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(nummer);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(nummer, planung.Teilstuecke.Count);

        List<Teilstueck?> teilstuecke = [.. planung.Teilstuecke];
        teilstuecke[nummer] = teilstueck;
        return planung with { Teilstuecke = teilstuecke };
    }

    /// <summary>Wirft eine Ausnahme, wenn es den Wegpunkt mit dieser Nummer nicht gibt.</summary>
    private static void PruefeNummer(Tourenplanung planung, int nummer)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(nummer);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(nummer, planung.Wegpunkte.Count);
    }
}
