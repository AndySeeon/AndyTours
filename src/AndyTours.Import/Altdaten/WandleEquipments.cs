using AndyTours.Core.Modell;

namespace AndyTours.Import.Altdaten;

/// <summary>
/// Erzeugt die Equipments aus den INI-Abschnitten <c>[Equipment.&lt;Name&gt;]</c>.
/// </summary>
public static class WandleEquipments
{
    /// <summary>Vorsilbe der Equipment-Abschnitte; der Rest des Abschnittsnamens ist der Equipment-Name.</summary>
    private const string Vorsilbe = "Equipment.";

    /// <summary>
    /// Liest alle Equipments.
    /// </summary>
    /// <param name="ini">Die gelesene <c>bikexperience.ini</c>.</param>
    /// <param name="aktivitaeten">Bereits gelesene Aktivitäten, um den Aktivitätsnamen in eine Nummer umzusetzen.</param>
    /// <returns>Die Equipments mit fortlaufenden Nummern ab 1, in der Reihenfolge der Datei.</returns>
    public static List<Equipment> Ausfuehren(IniDatei ini, IReadOnlyList<Aktivitaet> aktivitaeten)
    {
        var equipments = new List<Equipment>();

        foreach (string abschnitt in ini.Abschnitte.Keys.Where(name => name.StartsWith(Vorsilbe, StringComparison.OrdinalIgnoreCase)))
        {
            var werte = HoleIniSchluesselwerte.Ausfuehren(ini, abschnitt);
            string Wert(string schluessel) => werte.TryGetValue(schluessel, out string? wert) ? wert : string.Empty;
            bool Ja(string schluessel) => Wert(schluessel).Equals("true", StringComparison.OrdinalIgnoreCase);

            // Aktivität über den Namen suchen; unbekannte Namen bleiben ohne Zuordnung.
            Aktivitaet? aktivitaet = aktivitaeten.FirstOrDefault(
                a => a.Name.Equals(Wert("activity"), StringComparison.OrdinalIgnoreCase));

            equipments.Add(new Equipment
            {
                Id = equipments.Count + 1,
                Name = abschnitt[Vorsilbe.Length..],
                AktivitaetId = aktivitaet?.Id,
                GewichtKg = ParseZahl.Ausfuehren(Wert("weight")),
                RadumfangMm = ParseGanzzahl.Ausfuehren(Wert("wheeldiameter")),
                CrCoeff = ParseZahl.Ausfuehren(Wert("crCoeff")),
                CmCoeff = ParseZahl.Ausfuehren(Wert("cmCoeff")),
                CwCoeff = ParseZahl.Ausfuehren(Wert("cwCoeff")),
                CwaCoeff = ParseZahl.Ausfuehren(Wert("cwaCoeff")),
                SinCoeff = ParseZahl.Ausfuehren(Wert("sinCoeff")),
                Kettenblaetter = ParseZahlenliste.Ausfuehren(Wert("chainwheels")),
                Ritzel = ParseZahlenliste.Ausfuehren(Wert("sprockets")),
                Geraetezuordnung = Wert("assignment"),
                StandardKategorie = Wert("category"),
                IstStandard = Ja("default"),
                Ausgemustert = Ja("rejected"),
                Anschaffungsdatum = ParseAltesDatum.Ausfuehren(Wert("date")),
                Bildpfad = Wert("picture"),
                AnfangsdistanzKm = ParseZahl.Ausfuehren(Wert("initDst")),
            });
        }

        return equipments;
    }
}
