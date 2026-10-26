using AndyTours.Core.Modell;
using AndyTours.Import.Garmin;
using AndyTours.Tests.Datenordner;
using Dynastream.Fit;

namespace AndyTours.Tests.Garmin;

/// <summary>
/// Tests für <see cref="SchreibeFitCourse"/>: Datei schreiben und mit dem SDK zurücklesen.
/// </summary>
[Collection(FitTestdatei.Testgruppe)]
public class SchreibeFitCourseTests
{
    private static readonly System.DateTime StartUtc = new(2026, 10, 26, 8, 0, 0, DateTimeKind.Utc);

    private static readonly Course Beispiel = new("Chiemsee-Runde",
        [
            new Coursepunkt(new Geoposition(47.85, 12.40), 520, 0, TimeSpan.Zero),
            new Coursepunkt(new Geoposition(47.86, 12.40), 525.5, 1.112, TimeSpan.FromSeconds(267)),
            new Coursepunkt(new Geoposition(47.86, 12.42), null, 2.604, TimeSpan.FromSeconds(625)),
        ],
        [new Courseabbiegung(1, Abbiegerichtung.Rechts), new Courseabbiegung(2, Abbiegerichtung.Kreisverkehr, 2)]);

    /// <summary>Die gelesenen Nachrichten einer Datei.</summary>
    private sealed record Inhalt(
        List<FileIdMesg> Kennungen, List<CourseMesg> Courses, List<LapMesg> Runden, List<EventMesg> Ereignisse,
        List<RecordMesg> Punkte, List<CoursePointMesg> Hinweise);

    private static Inhalt Lies(byte[] daten)
    {
        using var datei = new MemoryStream(daten);
        var decoder = new Decode();
        Assert.True(decoder.IsFIT(datei));
        datei.Position = 0;
        Assert.True(decoder.CheckIntegrity(datei));
        datei.Position = 0;

        var inhalt = new Inhalt([], [], [], [], [], []);
        var verteiler = new MesgBroadcaster();
        decoder.MesgEvent += verteiler.OnMesg;
        verteiler.FileIdMesgEvent += (_, e) => inhalt.Kennungen.Add((FileIdMesg)e.mesg);
        verteiler.CourseMesgEvent += (_, e) => inhalt.Courses.Add((CourseMesg)e.mesg);
        verteiler.LapMesgEvent += (_, e) => inhalt.Runden.Add((LapMesg)e.mesg);
        verteiler.EventMesgEvent += (_, e) => inhalt.Ereignisse.Add((EventMesg)e.mesg);
        verteiler.RecordMesgEvent += (_, e) => inhalt.Punkte.Add((RecordMesg)e.mesg);
        verteiler.CoursePointMesgEvent += (_, e) => inhalt.Hinweise.Add((CoursePointMesg)e.mesg);
        decoder.Read(datei);
        return inhalt;
    }

    private static double Grad(int? semicircles) => semicircles!.Value * 180.0 / 2147483648.0;

    [Fact]
    public void Datei_ist_ein_Course_mit_Name_und_Sportart()
    {
        Inhalt inhalt = Lies(SchreibeFitCourse.Ausfuehren(Beispiel, StartUtc));

        Assert.Equal(Dynastream.Fit.File.Course, Assert.Single(inhalt.Kennungen).GetType());
        CourseMesg course = Assert.Single(inhalt.Courses);
        Assert.Equal("Chiemsee-Runde", course.GetNameAsString());
        Assert.Equal(Sport.Cycling, course.GetSport());
    }

    [Fact]
    public void Punkte_mit_Position_Distanz_Hoehe_und_Zeit()
    {
        Inhalt inhalt = Lies(SchreibeFitCourse.Ausfuehren(Beispiel, StartUtc));

        Assert.Equal(3, inhalt.Punkte.Count);
        Assert.Equal(47.86, Grad(inhalt.Punkte[2].GetPositionLat()), 6);
        Assert.Equal(12.42, Grad(inhalt.Punkte[2].GetPositionLong()), 6);
        Assert.Equal(1112, inhalt.Punkte[1].GetDistance()!.Value, 0);
        // Das FIT-Format speichert Höhen auf 0,2 m genau
        Assert.Equal(525.5, inhalt.Punkte[1].GetEnhancedAltitude()!.Value, 0.2);
        Assert.Equal(525.5, inhalt.Punkte[2].GetEnhancedAltitude()!.Value, 0.2);
        uint start = new Dynastream.Fit.DateTime(StartUtc).GetTimeStamp();
        Assert.Equal([0u, 267u, 625u], inhalt.Punkte.Select(p => p.GetTimestamp().GetTimeStamp() - start));
    }

    [Fact]
    public void Runde_und_Zeitmessung_umfassen_die_ganze_Strecke()
    {
        Inhalt inhalt = Lies(SchreibeFitCourse.Ausfuehren(Beispiel, StartUtc));

        LapMesg runde = Assert.Single(inhalt.Runden);
        Assert.Equal(2604, runde.GetTotalDistance()!.Value, 0);
        Assert.Equal(625, runde.GetTotalTimerTime()!.Value, 0);
        Assert.Equal(47.85, Grad(runde.GetStartPositionLat()), 6);
        Assert.Equal(12.42, Grad(runde.GetEndPositionLong()), 6);
        Assert.Equal([EventType.Start, EventType.StopDisableAll], inhalt.Ereignisse.Select(e => e.GetEventType()));
    }

    [Fact]
    public void Abbiegungen_werden_Course_Punkte_mit_Typ_und_Text()
    {
        Inhalt inhalt = Lies(SchreibeFitCourse.Ausfuehren(Beispiel, StartUtc));

        Assert.Equal([CoursePoint.Right, CoursePoint.Generic], inhalt.Hinweise.Select(h => h.GetType()));
        Assert.Equal(["Rechts", "Kreisel 2. Ausf."], inhalt.Hinweise.Select(h => h.GetNameAsString()));
        Assert.Equal(1112, inhalt.Hinweise[0].GetDistance()!.Value, 0);
        Assert.Equal(47.86, Grad(inhalt.Hinweise[0].GetPositionLat()), 6);
    }

    [Fact]
    public void Fehlende_Hoehe_am_Anfang_kommt_vom_ersten_Punkt_mit_Hoehe()
    {
        Course course = Beispiel with { Punkte = [Beispiel.Punkte[0] with { Hoehe = null }, .. Beispiel.Punkte.Skip(1)] };

        Inhalt inhalt = Lies(SchreibeFitCourse.Ausfuehren(course, StartUtc));

        Assert.Equal(525.5, inhalt.Punkte[0].GetEnhancedAltitude()!.Value, 0.2);
    }

    [Fact]
    public void Ganz_ohne_Hoehen_enthaelt_die_Datei_keine()
    {
        Course course = Beispiel with { Punkte = [.. Beispiel.Punkte.Select(p => p with { Hoehe = null })] };

        Inhalt inhalt = Lies(SchreibeFitCourse.Ausfuehren(course, StartUtc));

        Assert.All(inhalt.Punkte, p => Assert.Null(p.GetFieldValue(RecordMesg.FieldDefNum.EnhancedAltitude)));
    }

    [Fact]
    public void Ohne_Abbiegungen_keine_Course_Punkte()
    {
        Inhalt inhalt = Lies(SchreibeFitCourse.Ausfuehren(Beispiel with { Abbiegungen = [] }, StartUtc));

        Assert.Empty(inhalt.Hinweise);
        Assert.Equal(3, inhalt.Punkte.Count);
    }

    [Fact]
    public void Datei_laesst_sich_wie_eine_Aufzeichnung_lesen()
    {
        using var temp = new TempOrdner();
        string pfad = Path.Combine(temp.Pfad, "Chiemsee-Runde.fit");
        System.IO.File.WriteAllBytes(pfad, SchreibeFitCourse.Ausfuehren(Beispiel, StartUtc));

        (List<Verlaufspunkt>? verlauf, string? fehler) = LeseFitVerlauf.Ausfuehren(pfad);

        Assert.Null(fehler);
        Assert.Equal([new Geoposition(47.85, 12.40), new Geoposition(47.86, 12.40), new Geoposition(47.86, 12.42)],
            verlauf!.Select(p => new Geoposition(Math.Round(p.Position!.Breite, 6), Math.Round(p.Position.Laenge, 6))));
    }
}
