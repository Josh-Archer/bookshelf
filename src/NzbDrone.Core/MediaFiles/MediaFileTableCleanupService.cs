using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common;
using NzbDrone.Common.Extensions;

namespace NzbDrone.Core.MediaFiles
{
    public interface IMediaFileTableCleanupService
    {
        void Clean(string folder, List<string> filesOnDisk);
    }

    public class MediaFileTableCleanupService : IMediaFileTableCleanupService
    {
        private const string CwaUserRoutedIngestSegment = "/cwa-book-ingest/";
        private readonly IMediaFileService _mediaFileService;
        private readonly Logger _logger;

        public MediaFileTableCleanupService(IMediaFileService mediaFileService,
                                            Logger logger)
        {
            _mediaFileService = mediaFileService;
            _logger = logger;
        }

        public void Clean(string folder, List<string> filesOnDisk)
        {
            var dbFiles = _mediaFileService.GetFilesWithBasePath(folder);

            // get files in database that are missing on disk and remove from database
            var missingFiles = dbFiles.ExceptBy(x => x.Path, filesOnDisk, x => x, PathEqualityComparer.Instance).ToList();

            if (IsCwaUserRoutedIngestFolder(folder))
            {
                _logger.Debug("Skipping missing file cleanup for transient CWA ingest folder '{0}':\n{1}",
                              folder,
                              string.Join("\n", missingFiles.Select(x => x.Path)));
                return;
            }

            _logger.Debug("The following files no longer exist on disk, removing from db:\n{0}",
                          string.Join("\n", missingFiles.Select(x => x.Path)));

            _mediaFileService.DeleteMany(missingFiles, DeleteMediaFileReason.MissingFromDisk);
        }

        private static bool IsCwaUserRoutedIngestFolder(string folder)
        {
            if (folder.IsNullOrWhiteSpace())
            {
                return false;
            }

            return folder.ContainsIgnoreCase(CwaUserRoutedIngestSegment) ||
                   folder.Replace('\\', '/').ContainsIgnoreCase(CwaUserRoutedIngestSegment);
        }
    }
}
