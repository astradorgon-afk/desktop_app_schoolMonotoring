namespace AttendanceUI.Services;

/// <summary>Hands an uploaded file to the OS so it opens in the default viewer (photos, PDF reader).</summary>
public static class AttachmentOpener
{
    public static async Task OpenAsync(byte[] data, string fileName)
    {
        var safeName = string.Concat(Path.GetFileName(fileName).Select(c => Path.GetInvalidFileNameChars().Contains(c) ? '_' : c));
        if (string.IsNullOrWhiteSpace(safeName)) safeName = "attachment";

        var folder = Path.Combine(FileSystem.CacheDirectory, "attachments", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        var path = Path.Combine(folder, safeName);
        await File.WriteAllBytesAsync(path, data);

        await Launcher.Default.OpenAsync(new OpenFileRequest(safeName, new ReadOnlyFile(path)));
    }
}
