using System.IO.Abstractions.TestingHelpers;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.MediaFiles
{
    [TestFixture]
    public class LibraryPathSanitizerFixture : FileSystemTest<DiskScanService>
    {
        [Test]
        public void should_sanitize_file_name_on_disk()
        {
            var dirtyPath = @"C:\Test\Books\File\rName.epub".Replace(@"\r", "\r");
            var cleanPath = @"C:\Test\Books\FileName.epub";

            FileSystem.AddFile(dirtyPath, new MockFileData("content"));

            var result = LibraryPathSanitizer.SanitizeFileNameOnDisk(DiskProvider, dirtyPath, null);

            result.Should().Be(cleanPath);
            DiskProvider.FileExists(cleanPath).Should().BeTrue();
            DiskProvider.FileExists(dirtyPath).Should().BeFalse();
        }

        [Test]
        public void should_sanitize_directory_name_on_disk()
        {
            var dirtyDirectory = @"C:\Test\Books\Bad\rFolder".Replace(@"\r", "\r");
            var cleanDirectory = @"C:\Test\Books\BadFolder";

            FileSystem.AddDirectory(dirtyDirectory);

            var result = LibraryPathSanitizer.SanitizeDirectoryNameOnDisk(DiskProvider, dirtyDirectory, null);

            result.Should().Be(cleanDirectory);
            DiskProvider.FolderExists(cleanDirectory).Should().BeTrue();
            DiskProvider.FolderExists(dirtyDirectory).Should().BeFalse();
        }
    }
}
