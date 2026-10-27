// WPF-Projekte binden System.IO nicht automatisch ein (Namenskonflikt Path ↔ System.Windows.Shapes.Path)
using System.IO;
using AndyTours.Import.Garmin;
using MediaDevices;

namespace AndyTours.App.Funktionen;

/// <summary>
/// Findet Garmin-Geräte, die nur als Mediengerät (MTP) angeschlossen sind (Edge 1040), als Ziel für Courses
/// (Spec 032).
/// </summary>
/// <remarks>
/// Gegenstück zu <see cref="FindeLaufwerksziele"/> und wie <see cref="FindeMtpGeraete"/> über das Paket
/// <c>MediaDevices</c> (siehe <c>docs/adr/0002-zugriff-auf-mediengeraete.md</c>). Ziel ist der Ordner
/// <c>Garmin\NewFiles</c> des ersten Speichers mit einem Ordner <c>Garmin</c>. Die Verbindung wird für jede Aktion
/// hergestellt und danach wieder getrennt. Ein Mediengerät kann eine Datei nicht überschreiben; eine vorhandene wird
/// deshalb zuerst gelöscht. Einen automatischen Test gibt es nicht, weil dafür ein echtes Gerät nötig ist.
/// </remarks>
public static class FindeMtpZiele
{
    /// <summary>Ordner, aus dem der Edge neue Dateien übernimmt, relativ zum Speicher.</summary>
    private const string NeueDateien = @"Garmin\NewFiles";

    /// <summary>
    /// Durchsucht alle angeschlossenen Mediengeräte von Garmin.
    /// </summary>
    /// <remarks>
    /// Speicher, die ein Laufwerk sind (z. B. <c>\E:</c>), werden übergangen, weil <see cref="FindeLaufwerksziele"/> sie
    /// schon liefert. Ein Gerät, das sich nicht lesen lässt, wird ebenfalls übergangen.
    /// </remarks>
    /// <returns>Die gefundenen Geräte.</returns>
    public static List<Courseziel> Ausfuehren()
    {
        List<Courseziel> ziele = [];
        foreach (MediaDevice geraet in MediaDeviceManager.Instance?.GetDevices() ?? [])
        {
            if (!FindeMtpGeraete.IstGarmin(geraet))
            {
                continue;
            }

            try
            {
                geraet.Connect(MediaDeviceAccess.GenericRead);
                string? speicher = geraet.GetDirectories(@"\")
                    .Where(s => !FindeMtpGeraete.LaufwerksSpeicher().IsMatch(s))
                    .FirstOrDefault(s => geraet.DirectoryExists(s + @"\Garmin"));
                if (speicher is not null)
                {
                    string ordner = speicher + @"\" + NeueDateien;
                    ziele.Add(new Courseziel(
                        FindeMtpGeraete.Geraetename(geraet),
                        datei => Verbunden(geraet, MediaDeviceAccess.GenericRead, () => geraet.FileExists(ordner + @"\" + datei)),
                        (datei, inhalt) => Verbunden(geraet, MediaDeviceAccess.GenericRead | MediaDeviceAccess.GenericWrite,
                            () => Schreibe(geraet, ordner, datei, inhalt))));
                }
            }
            catch (Exception fehler) when (FindeMtpGeraete.IstGeraetefehler(fehler))
            {
                // Gerät nicht lesbar: übergehen, die übrigen Geräte werden trotzdem angeboten
            }
            finally
            {
                if (geraet.IsConnected)
                {
                    geraet.Disconnect();
                }
            }
        }

        return ziele;
    }

    /// <summary>Legt bei Bedarf den Ordner an, löscht eine vorhandene Datei gleichen Namens und lädt die neue hoch.</summary>
    private static bool Schreibe(MediaDevice geraet, string ordner, string datei, byte[] inhalt)
    {
        if (!geraet.DirectoryExists(ordner))
        {
            geraet.CreateDirectory(ordner);
        }

        string pfad = ordner + @"\" + datei;
        if (geraet.FileExists(pfad))
        {
            geraet.DeleteFile(pfad);
        }

        using var daten = new MemoryStream(inhalt);
        geraet.UploadFile(daten, pfad);
        return true;
    }

    /// <summary>
    /// Führt eine Aktion mit hergestellter Verbindung aus und trennt danach wieder; Gerätefehler werden als
    /// <see cref="IOException"/> weitergegeben.
    /// </summary>
    private static T Verbunden<T>(MediaDevice geraet, MediaDeviceAccess zugriff, Func<T> aktion)
    {
        try
        {
            geraet.Connect(zugriff);
            return aktion();
        }
        catch (Exception fehler) when (FindeMtpGeraete.IstGeraetefehler(fehler) && fehler is not IOException)
        {
            throw new IOException($"Das Gerät „{FindeMtpGeraete.Geraetename(geraet)}“ ist nicht erreichbar: {fehler.Message}", fehler);
        }
        finally
        {
            if (geraet.IsConnected)
            {
                geraet.Disconnect();
            }
        }
    }
}
