using AndyTours.Core.Modell;

namespace AndyTours.Core.Berechnungen;

/// <summary>
/// Ermittelt das Saisonziel, das für eine Aktivität in einem Jahr gilt.
/// </summary>
/// <remarks>
/// Ein Ziel gilt ab seinem Jahr weiter, bis es für ein späteres Jahr geändert wird:
/// <list type="number">
/// <item>Gibt es eigene Ziele der Aktivität bis einschließlich des Jahres, gilt das jüngste davon – für
/// genau dieses Jahr als eigenes Ziel, sonst als „übernommen aus &lt;Jahr&gt;“.</item>
/// <item>Sonst gilt das Grundziel der Aktivität (aus BikeXperience übernommen), sofern es mindestens
/// einen Wert enthält.</item>
/// <item>Sonst gibt es kein Ziel (alle Werte 0, keine Kennzeichnung).</item>
/// </list>
/// Wird auch von der Ansicht „Aktuell“ für den Jahresfortschritt verwendet.
/// </remarks>
public static class ErmittleSaisonziel
{
    /// <summary>
    /// Ermittelt das wirksame Ziel.
    /// </summary>
    /// <param name="aktivitaet">Die Aktivität (mit ihrem Grundziel).</param>
    /// <param name="ziele">Alle eigenen Saisonziele (auch anderer Aktivitäten).</param>
    /// <param name="jahr">Das Jahr.</param>
    /// <returns>Das wirksame Ziel mit Herkunft.</returns>
    public static WirksamesSaisonziel Ausfuehren(Aktivitaet aktivitaet, IEnumerable<Saisonziel> ziele, int jahr)
    {
        // 1. Jüngstes eigenes Ziel bis einschließlich des Jahres
        Saisonziel? eigenes = ziele
            .Where(z => z.AktivitaetId == aktivitaet.Id && z.AbJahr <= jahr)
            .MaxBy(z => z.AbJahr);
        if (eigenes is not null)
        {
            bool fuerDiesesJahr = eigenes.AbJahr == jahr;
            return new WirksamesSaisonziel(eigenes.ZielKm, eigenes.ZielHm, eigenes.ZielAnzahl,
                fuerDiesesJahr, fuerDiesesJahr ? null : eigenes.AbJahr, false);
        }

        // 2. Grundziel aus BikeXperience (dort als Kommazahl gespeichert, hier auf ganze Zahlen gerundet)
        int km = (int)Math.Round(aktivitaet.ZielKm);
        int hm = (int)Math.Round(aktivitaet.ZielHm);
        int anzahl = aktivitaet.ZielAnzahl;
        bool hatGrundziel = km > 0 || hm > 0 || anzahl > 0;

        // 3. Ohne Grundziel bleibt alles 0
        return new WirksamesSaisonziel(km, hm, anzahl, false, null, hatGrundziel);
    }
}
