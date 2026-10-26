using AndyTours.Core.Berechnungen;
using AndyTours.Core.Modell;
using Dynastream.Fit;

namespace AndyTours.Import.Garmin;

/// <summary>
/// Schreibt einen <see cref="Course"/> als FIT-Datei, wie sie der Garmin Edge im Ordner <c>Garmin\NewFiles</c>
/// übernimmt (Spec 032).
/// </summary>
/// <remarks>
/// Aufbau nach den Vorgaben von Garmin für Course-Dateien:
/// <list type="number">
/// <item>Dateikennung vom Typ „Course“,</item>
/// <item>Course mit Name und Sportart Radfahren,</item>
/// <item>eine Runde über die ganze Strecke (Start, Ziel, Distanz, Zeit),</item>
/// <item>Start der Zeitmessung, die Punkte mit Position, Distanz, Höhe und Zeit, Ende der Zeitmessung,</item>
/// <item>je Abbiegung ein Course-Punkt mit Typ und kurzem Text (<see cref="BeschreibeAbbiegung.Kurz"/>).</item>
/// </list>
/// Das FIT-Format kennt keinen Kreisverkehr; er wird ein allgemeiner Course-Punkt, dessen Text der Edge beim
/// Ankündigen zeigt, z. B. „Kreisel 2. Ausf.“.
/// <para>
/// Punkte ohne Höhe (z. B. eine Luftlinie in der Planung) erhalten die Höhe des vorigen Punkts, am Anfang die des
/// ersten Punkts mit Höhe; sonst stünde dort ein ungültiger Wert und das Höhenprofil auf dem Edge bräche ein. Hat kein
/// Punkt eine Höhe, enthält die Datei keine.
/// </para>
/// </remarks>
public static class SchreibeFitCourse
{
    /// <summary>Umrechnung von Grad in die Einheit „Semicircles“ des FIT-Formats.</summary>
    private const double SemicirclesJeGrad = 2147483648.0 / 180;

    /// <summary>
    /// Erzeugt die FIT-Datei.
    /// </summary>
    /// <param name="course">Der Course mit mindestens zwei Punkten.</param>
    /// <param name="startUtc">Zeitpunkt des ersten Punkts (die Zeiten der Punkte zählen ab hier), meist jetzt.</param>
    /// <returns>Der Inhalt der Datei.</returns>
    public static byte[] Ausfuehren(Course course, System.DateTime startUtc)
    {
        var start = new Dynastream.Fit.DateTime(startUtc);
        Coursepunkt erster = course.Punkte[0];
        Coursepunkt letzter = course.Punkte[^1];

        using var datei = new MemoryStream();
        var encoder = new Encode(ProtocolVersion.V20);
        encoder.Open(datei);

        var kennung = new FileIdMesg();
        kennung.SetType(Dynastream.Fit.File.Course);
        kennung.SetManufacturer(Manufacturer.Development);
        kennung.SetProduct(0);
        kennung.SetSerialNumber(1);
        kennung.SetTimeCreated(start);
        encoder.Write(kennung);

        var kopf = new CourseMesg();
        kopf.SetName(course.Name);
        kopf.SetSport(Sport.Cycling);
        encoder.Write(kopf);

        var runde = new LapMesg();
        runde.SetMessageIndex(0);
        runde.SetTimestamp(Zeit(start, letzter.Zeit));
        runde.SetStartTime(start);
        runde.SetStartPositionLat(Semicircles(erster.Position.Breite));
        runde.SetStartPositionLong(Semicircles(erster.Position.Laenge));
        runde.SetEndPositionLat(Semicircles(letzter.Position.Breite));
        runde.SetEndPositionLong(Semicircles(letzter.Position.Laenge));
        runde.SetTotalElapsedTime((float)letzter.Zeit.TotalSeconds);
        runde.SetTotalTimerTime((float)letzter.Zeit.TotalSeconds);
        runde.SetTotalDistance((float)(letzter.DistanzKm * 1000));
        encoder.Write(runde);

        encoder.Write(Zeitmessung(start, EventType.Start));
        double? hoehe = course.Punkte.FirstOrDefault(p => p.Hoehe is not null)?.Hoehe;
        foreach (Coursepunkt punkt in course.Punkte)
        {
            hoehe = punkt.Hoehe ?? hoehe;
            var satz = new RecordMesg();
            satz.SetTimestamp(Zeit(start, punkt.Zeit));
            satz.SetPositionLat(Semicircles(punkt.Position.Breite));
            satz.SetPositionLong(Semicircles(punkt.Position.Laenge));
            satz.SetDistance((float)(punkt.DistanzKm * 1000));
            if (hoehe is double meter)
            {
                satz.SetEnhancedAltitude((float)meter);
            }

            encoder.Write(satz);
        }

        Dynastream.Fit.DateTime ende = Zeit(start, letzter.Zeit);
        encoder.Write(Zeitmessung(ende, EventType.StopDisableAll));

        ushort nummer = 0;
        foreach (Courseabbiegung abbiegung in course.Abbiegungen)
        {
            Coursepunkt ort = course.Punkte[abbiegung.Punkt];
            var hinweis = new CoursePointMesg();
            hinweis.SetMessageIndex(nummer++);
            hinweis.SetTimestamp(Zeit(start, ort.Zeit));
            hinweis.SetPositionLat(Semicircles(ort.Position.Breite));
            hinweis.SetPositionLong(Semicircles(ort.Position.Laenge));
            hinweis.SetDistance((float)(ort.DistanzKm * 1000));
            hinweis.SetType(Typ(abbiegung.Richtung));
            hinweis.SetName(BeschreibeAbbiegung.Kurz(abbiegung.Richtung, abbiegung.Ausfahrt));
            encoder.Write(hinweis);
        }

        encoder.Close();
        return datei.ToArray();
    }

    /// <summary>Der Typ des Course-Punkts zu einer Abbiegerichtung.</summary>
    private static CoursePoint Typ(Abbiegerichtung richtung) => richtung switch
    {
        Abbiegerichtung.Links => CoursePoint.Left,
        Abbiegerichtung.Rechts => CoursePoint.Right,
        Abbiegerichtung.LeichtLinks => CoursePoint.SlightLeft,
        Abbiegerichtung.LeichtRechts => CoursePoint.SlightRight,
        Abbiegerichtung.ScharfLinks => CoursePoint.SharpLeft,
        Abbiegerichtung.ScharfRechts => CoursePoint.SharpRight,
        Abbiegerichtung.Wenden => CoursePoint.UTurn,
        _ => CoursePoint.Generic,
    };

    /// <summary>Ereignis der Zeitmessung (Start bzw. Ende).</summary>
    private static EventMesg Zeitmessung(Dynastream.Fit.DateTime zeit, EventType art)
    {
        var ereignis = new EventMesg();
        ereignis.SetTimestamp(zeit);
        ereignis.SetEvent(Event.Timer);
        ereignis.SetEventType(art);
        ereignis.SetEventGroup(0);
        return ereignis;
    }

    /// <summary>Zeitpunkt eines Punkts: Start plus Zeit seit dem Start, auf ganze Sekunden.</summary>
    private static Dynastream.Fit.DateTime Zeit(Dynastream.Fit.DateTime start, TimeSpan seitStart) =>
        new(start.GetTimeStamp() + (uint)Math.Round(seitStart.TotalSeconds));

    /// <summary>Grad in Semicircles.</summary>
    private static int Semicircles(double grad) => (int)Math.Round(grad * SemicirclesJeGrad);
}
