using AndyTours.Core.Modell;

namespace AndyTours.Daten;

/// <summary>
/// Wandelt die Abschnitte eines <see cref="Overlay"/>s in den Binärwert der Spalte <c>Overlay.Punkte</c> und zurück.
/// </summary>
/// <remarks>
/// Aufbau (alle Zahlen im Little-Endian-Format, wie <see cref="BinaryWriter"/> sie schreibt):
/// <list type="number">
/// <item>ein Byte Formatversion (derzeit 1),</item>
/// <item>Anzahl der Abschnitte (32-Bit-Ganzzahl),</item>
/// <item>je Abschnitt die Anzahl seiner Punkte (32-Bit-Ganzzahl) und danach je Punkt Breite und Länge
/// (je 64-Bit-Gleitkommazahl).</item>
/// </list>
/// Gegenüber Text spart das Format etwa die Hälfte des Platzes und liest sich ohne Umwandlung von Zahlen; eine Strecke
/// mit 10 000 Punkten belegt rund 160 KB.
/// </remarks>
public static class OverlayPunkte
{
    /// <summary>Kennung des aktuellen Formats; ein späteres Format bekäme eine neue Nummer.</summary>
    private const byte Formatversion = 1;

    /// <summary>
    /// Verpackt die Abschnitte in den Binärwert.
    /// </summary>
    /// <param name="abschnitte">Die Abschnitte der Strecke.</param>
    /// <returns>Der Binärwert für die Datenbank.</returns>
    public static byte[] Verpacken(IReadOnlyList<IReadOnlyList<Geoposition>> abschnitte)
    {
        using var speicher = new MemoryStream();
        using (var schreiber = new BinaryWriter(speicher))
        {
            schreiber.Write(Formatversion);
            schreiber.Write(abschnitte.Count);
            foreach (IReadOnlyList<Geoposition> abschnitt in abschnitte)
            {
                schreiber.Write(abschnitt.Count);
                foreach (Geoposition punkt in abschnitt)
                {
                    schreiber.Write(punkt.Breite);
                    schreiber.Write(punkt.Laenge);
                }
            }
        }

        return speicher.ToArray();
    }

    /// <summary>
    /// Entpackt die Abschnitte aus dem Binärwert.
    /// </summary>
    /// <param name="daten">Der Binärwert aus der Datenbank.</param>
    /// <returns>Die Abschnitte.</returns>
    /// <exception cref="InvalidDataException">Der Wert hat ein unbekanntes Format oder ist unvollständig.</exception>
    public static IReadOnlyList<IReadOnlyList<Geoposition>> Entpacken(byte[] daten)
    {
        try
        {
            using var leser = new BinaryReader(new MemoryStream(daten));
            if (leser.ReadByte() != Formatversion)
            {
                throw new InvalidDataException("Unbekanntes Format der Overlay-Punkte.");
            }

            int anzahlAbschnitte = leser.ReadInt32();
            var abschnitte = new List<IReadOnlyList<Geoposition>>(Math.Max(anzahlAbschnitte, 0));
            for (int i = 0; i < anzahlAbschnitte; i++)
            {
                int anzahlPunkte = leser.ReadInt32();
                var punkte = new List<Geoposition>(Math.Max(anzahlPunkte, 0));
                for (int j = 0; j < anzahlPunkte; j++)
                {
                    punkte.Add(new Geoposition(leser.ReadDouble(), leser.ReadDouble()));
                }

                abschnitte.Add(punkte);
            }

            return abschnitte;
        }
        catch (EndOfStreamException fehler)
        {
            throw new InvalidDataException("Die Overlay-Punkte sind unvollständig.", fehler);
        }
    }
}
