using AndyTours.Core.Modell;
using AndyTours.Import.Altdaten;

namespace AndyTours.Tests.Altdaten;

/// <summary>
/// Tests für das Zerlegen einzelner Zeilen aus <c>workouts.csv</c> und <c>maintanence.dat</c>.
/// </summary>
public class WandleZeilenTests
{
    /// <summary>Erste Datenzeile aus der echten workouts.csv.</summary>
    private const string Workoutzeile =
        "05.03.2016;2016-03-05-08-40-41;04:33:23;77.83;859;970;53.07;135;Namur;WSA;538;83;5;166; ;08:40;Bike;Training;95.0;91;150.0;0;'';1;'2016/2016-03-05-08-40-41.fit';'2016/2016-03-05-08-40-41.fit';''";

    [Fact]
    public void Workoutzeile_wird_vollstaendig_gelesen()
    {
        AltWorkout w = WandleWorkoutZeile.Ausfuehren(Workoutzeile, 2);
        Trainingseinheit e = w.Einheit;

        Assert.Equal(new DateOnly(2016, 3, 5), e.Datum);
        Assert.Equal("2016-03-05-08-40-41", e.Name);
        Assert.Equal(new TimeSpan(4, 33, 23), e.Fahrzeit);
        Assert.Equal(77.83, e.DistanzKm, precision: 6);
        Assert.Equal(859, e.Hoehenmeter);
        Assert.Equal(970, e.MaxHoehe);
        Assert.Equal(53.07, e.MaxGeschwindigkeit, precision: 6);
        Assert.Equal(135, e.HfDurchschnitt);
        Assert.Equal("WSA", e.Trainingsbereich);
        Assert.Equal(538, e.StartHoehe);
        Assert.Equal(83, e.Trittfrequenz);
        Assert.Equal(5, e.Temperatur);
        Assert.Equal(166, e.HfMax);
        Assert.Equal(new TimeOnly(8, 40), e.Startzeit);
        Assert.Equal(95.0, e.KoerpergewichtKg, precision: 6);
        Assert.Equal(91, e.HfMin);
        Assert.Equal(150.0, e.LeistungWatt, precision: 6);
        Assert.Equal(0, e.Erholungspuls);
        Assert.Equal(string.Empty, e.Kommentar);

        Assert.Equal("Namur", w.EquipmentName);
        Assert.Equal("Bike", w.AktivitaetName);
        Assert.Equal(new List<string> { "Training" }, w.KategorieNamen);

        // Höhenprofil und GPS-Track zeigen auf dieselbe Datei, Dokumentation ist leer
        Assert.Equal(2, w.Anhaenge.Count);
        Assert.Equal((AnhangTyp.Hoehenprofil, "2016/2016-03-05-08-40-41.fit"), w.Anhaenge[0]);
        Assert.Equal((AnhangTyp.GpsTrack, "2016/2016-03-05-08-40-41.fit"), w.Anhaenge[1]);
    }

    [Fact]
    public void Mehrere_Kategorien_und_leere_Kategorie()
    {
        string mehrere = Workoutzeile.Replace(";Bike;Training;", ";Bike;Arbeit,Trainieren;");
        string leer = Workoutzeile.Replace(";Bike;Training;", ";Bike; ;");

        Assert.Equal(new List<string> { "Arbeit", "Trainieren" }, WandleWorkoutZeile.Ausfuehren(mehrere, 2).KategorieNamen);
        Assert.Empty(WandleWorkoutZeile.Ausfuehren(leer, 2).KategorieNamen);
    }

    [Fact]
    public void Falsche_Feldanzahl_wird_gemeldet()
    {
        Assert.Throws<FormatException>(() => WandleWorkoutZeile.Ausfuehren("05.03.2016;nur;drei", 2));
    }

    [Fact]
    public void Wartungszeile_wird_gelesen()
    {
        AltWartung w = WandleWartungszeile.Ausfuehren("10.09.2022;SL Road SL;Kettenwechsel;auf 0,7;0;true;0", 1);

        Assert.Equal("SL Road SL", w.EquipmentName);
        Assert.Equal(new DateOnly(2022, 9, 10), w.Wartung.Datum);
        Assert.Equal("Kettenwechsel", w.Wartung.Taetigkeit);
        Assert.Equal("auf 0,7", w.Wartung.Kommentar);
        Assert.Equal(WartungsTyp.Wartung, w.Wartung.Typ);
        Assert.True(w.Wartung.Aktiv);
    }

    [Fact]
    public void Wartungsalarm_mit_leerem_Kommentar()
    {
        AltWartung w = WandleWartungszeile.Ausfuehren("01.05.2020;-;Kette prüfen; ;1500;true;1", 1);

        Assert.Equal("-", w.EquipmentName);
        Assert.Equal(string.Empty, w.Wartung.Kommentar);
        Assert.Equal(WartungsTyp.Alarm, w.Wartung.Typ);
        Assert.Equal(1500, w.Wartung.AlarmKm);
    }
}
