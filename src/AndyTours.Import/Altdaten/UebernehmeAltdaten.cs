using AndyTours.Core.Modell;

namespace AndyTours.Import.Altdaten;

/// <summary>
/// Übernimmt den gesamten Datenbestand von BikeXperience aus dessen Datenordner.
/// </summary>
/// <remarks>
/// Ablauf:
/// <list type="number">
/// <item>Stammdaten aus <c>bikexperience.ini</c> lesen (Sportler, Aktivitäten, Kategorien, Equipments, Schwellwerte).</item>
/// <item>Trainingseinheiten aus <c>workouts.csv</c> lesen und deren Namensverweise in Nummern umsetzen;
/// fehlende Stammdaten werden dabei ergänzt.</item>
/// <item>Wartungen aus <c>maintanence.dat</c> lesen und den Equipments zuordnen.</item>
/// <item>In allen Texten beschädigte Umlaute reparieren und Anhangpfade vereinheitlichen.</item>
/// </list>
/// Die Altdateien werden dabei nur gelesen, nie verändert. Gespeichert wird das Ergebnis erst
/// durch den Aufrufer (siehe <c>AndyTours.Daten.SpeichereDatenbestand</c>).
/// Die nicht genutzten Dateien <c>planning.dat</c> und <c>train.ini</c> sind leer und werden nicht übernommen.
/// </remarks>
public static class UebernehmeAltdaten
{
    /// <summary>
    /// Führt die Übernahme aus.
    /// </summary>
    /// <param name="datenordner">Ordner mit <c>bikexperience.ini</c>, <c>workouts.csv</c>, <c>maintanence.dat</c> und den Jahresordnern.</param>
    /// <returns>Datenbestand und Migrationsbericht.</returns>
    /// <exception cref="FileNotFoundException"><c>bikexperience.ini</c> oder <c>workouts.csv</c> fehlt.</exception>
    public static Migrationsergebnis Ausfuehren(string datenordner)
    {
        var bericht = new Migrationsbericht();
        var bestand = new Datenbestand();

        string iniPfad = Path.Combine(datenordner, "bikexperience.ini");
        string workoutsPfad = Path.Combine(datenordner, "workouts.csv");
        string wartungPfad = Path.Combine(datenordner, "maintanence.dat");

        // Ohne Einstellungen und Einheiten ist eine Übernahme sinnlos – dann sofort abbrechen.
        foreach (string pflichtdatei in new[] { iniPfad, workoutsPfad })
        {
            if (!File.Exists(pflichtdatei))
            {
                throw new FileNotFoundException($"Die Datei {Path.GetFileName(pflichtdatei)} fehlt im Datenordner.", pflichtdatei);
            }
        }

        // Repariert Umlaute in einem Text und protokolliert das Ergebnis mit Fundstelle.
        string Repariere(string text, string fundstelle)
        {
            string repariert = RepariereUmlaute.Ausfuehren(text);
            if (repariert != text)
            {
                bericht.AnzahlReparierteTexte++;
            }

            if (repariert.Contains(RepariereUmlaute.Ersatzzeichen))
            {
                bericht.NichtReparierteTexte.Add($"{fundstelle}: {repariert}");
            }

            return repariert;
        }

        // --- 1. Stammdaten ---
        AltEinstellungen einstellungen = LeseBikexperienceIni.Ausfuehren(iniPfad);
        bestand.Fahrer.Add(einstellungen.Fahrer);
        bestand.Aktivitaeten.AddRange(einstellungen.Aktivitaeten);
        bestand.Kategorien.AddRange(einstellungen.Kategorien);
        bestand.Equipments.AddRange(einstellungen.Equipments);
        bestand.Schwellwerte.AddRange(einstellungen.Schwellwerte);

        foreach (Schwellwert schwellwert in bestand.Schwellwerte)
        {
            schwellwert.Name = Repariere(schwellwert.Name, $"bikexperience.ini, Schwellwert {schwellwert.Art}");
        }

        foreach (Equipment equipment in bestand.Equipments)
        {
            equipment.Name = Repariere(equipment.Name, "bikexperience.ini, Equipment");
        }

        // --- 2. Trainingseinheiten ---
        long fahrerId = einstellungen.Fahrer.Id;
        long naechsteAnhangId = 1;

        foreach (AltWorkout workout in LeseWorkoutsCsv.Ausfuehren(workoutsPfad, bericht))
        {
            Trainingseinheit einheit = workout.Einheit;
            string fundstelle = $"workouts.csv Zeile {workout.Zeilennummer}";

            // Nummern fortlaufend in der Reihenfolge der Datei vergeben
            einheit.Id = bestand.Trainingseinheiten.Count + 1;
            einheit.FahrerId = fahrerId;
            einheit.Name = Repariere(einheit.Name, $"{fundstelle}, Name");
            einheit.Kommentar = Repariere(einheit.Kommentar, $"{fundstelle}, Kommentar");

            einheit.AktivitaetId = FindeOderErgaenzeAktivitaet.Ausfuehren(bestand, workout.AktivitaetName, bericht).Id;

            if (workout.EquipmentName.Length > 0)
            {
                einheit.EquipmentId = FindeOderErgaenzeEquipment.Ausfuehren(bestand, workout.EquipmentName, einheit.AktivitaetId, bericht).Id;
            }

            // Distinct: Eine Kategorie soll nicht doppelt zugeordnet werden, falls sie zweimal dasteht.
            einheit.KategorieIds = workout.KategorieNamen
                .Select(name => FindeOderErgaenzeKategorie.Ausfuehren(bestand, name, bericht).Id)
                .Distinct()
                .ToList();

            foreach (var (typ, originalpfad) in workout.Anhaenge)
            {
                string relativ = NormalisiereAnhangPfad.Ausfuehren(originalpfad);

                // Absolute Pfade wurden umgestellt – das soll im Bericht nachvollziehbar sein.
                // Meldungen nur einmal je Pfad, weil Höhenprofil und GPS-Track meist dieselbe Datei sind.
                if (relativ != originalpfad.Replace('\\', '/'))
                {
                    bericht.HinweisEinmalig($"{fundstelle}: Anhangpfad „{originalpfad}“ → „{relativ}“.");
                }

                string fehlt = $"{fundstelle}: Anhang „{relativ}“ nicht gefunden.";
                if (!File.Exists(Path.Combine(datenordner, relativ)) && !bericht.Warnungen.Contains(fehlt))
                {
                    bericht.Warnungen.Add(fehlt);
                }

                einheit.Anhaenge.Add(new Anhang { Id = naechsteAnhangId++, Typ = typ, RelativerPfad = relativ });
            }

            bestand.Trainingseinheiten.Add(einheit);
        }

        // --- 3. Wartungen ---
        if (File.Exists(wartungPfad))
        {
            foreach (AltWartung alt in LeseWartungsdatei.Ausfuehren(wartungPfad, bericht))
            {
                Wartung wartung = alt.Wartung;
                string fundstelle = $"maintanence.dat Zeile {alt.Zeilennummer}";

                wartung.Id = bestand.Wartungen.Count + 1;
                wartung.Taetigkeit = Repariere(wartung.Taetigkeit, $"{fundstelle}, Tätigkeit");
                wartung.Kommentar = Repariere(wartung.Kommentar, $"{fundstelle}, Kommentar");

                // „-" steht im Original für „gilt für kein bestimmtes Equipment".
                if (alt.EquipmentName.Length > 0 && alt.EquipmentName != "-")
                {
                    wartung.EquipmentId = FindeOderErgaenzeEquipment.Ausfuehren(bestand, alt.EquipmentName, null, bericht).Id;
                }

                bestand.Wartungen.Add(wartung);
            }
        }
        else
        {
            bericht.Warnungen.Add("maintanence.dat nicht gefunden – es wurden keine Wartungen übernommen.");
        }

        return new Migrationsergebnis { Bestand = bestand, Bericht = bericht };
    }
}
