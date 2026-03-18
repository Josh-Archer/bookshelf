using System.IO;
using System.IO.Abstractions;
using System.Linq;
using NLog;
using NzbDrone.Common.Disk;

namespace NzbDrone.Core.MediaFiles
{
    public static class LibraryPathSanitizer
    {
        public static string SanitizeComponent(string value)
        {
            if (value == null)
            {
                return null;
            }

            return new string(value.Where(c => !char.IsControl(c)).ToArray()).Trim(' ', '.');
        }

        public static string SanitizeFileNameOnDisk(IDiskProvider diskProvider, string path, Logger logger)
        {
            var fileName = Path.GetFileName(path);
            var sanitized = SanitizeComponent(fileName);

            if (sanitized == fileName || string.IsNullOrWhiteSpace(sanitized))
            {
                return path;
            }

            var parent = Path.GetDirectoryName(path);
            var destination = Path.Combine(parent, sanitized);

            if (diskProvider.FileExists(destination))
            {
                logger?.Warn("Unable to sanitize file name because destination exists: {0}", destination);
                return destination;
            }

            logger?.Info("Sanitizing file name from {0} to {1}", path, destination);
            diskProvider.MoveFile(path, destination);

            return destination;
        }

        public static string SanitizeDirectoryNameOnDisk(IDiskProvider diskProvider, string path, Logger logger)
        {
            var normalizedPath = path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            var directoryName = Path.GetFileName(normalizedPath);
            var sanitized = SanitizeComponent(directoryName);

            if (sanitized == directoryName || string.IsNullOrWhiteSpace(sanitized))
            {
                return path;
            }

            var parent = Path.GetDirectoryName(normalizedPath);
            var destination = Path.Combine(parent, sanitized);

            if (diskProvider.FolderExists(destination))
            {
                logger?.Warn("Unable to sanitize directory name because destination exists: {0}", destination);
                return destination;
            }

            logger?.Info("Sanitizing directory name from {0} to {1}", path, destination);
            diskProvider.MoveFolder(path, destination);

            return destination;
        }

        public static IFileInfo[] SanitizeBookFilesOnDisk(IDiskProvider diskProvider, IFileInfo[] files, Logger logger)
        {
            return files.Select(file =>
                {
                    var sanitizedPath = SanitizeFileNameOnDisk(diskProvider, file.FullName, logger);
                    return diskProvider.GetFileInfo(sanitizedPath);
                })
                .ToArray();
        }
    }
}
