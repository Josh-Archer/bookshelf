using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Books.Calibre;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.RootFolders;
using NzbDrone.Core.Test.Framework;
using VersOne.Epub.Schema;

namespace NzbDrone.Core.Test.MediaFiles.AudioTagServiceFixture
{
    [TestFixture]
    public class EbookTagServiceFixture : CoreTest<EBookTagService>
    {
        [Test]
        public void should_prefer_isbn13()
        {
            var ids = Builder<EpubMetadataIdentifier>
                .CreateListOfSize(2)
                .TheFirst(1)
                .With(x => x.Identifier = "4087738574")
                .TheNext(1)
                .With(x => x.Identifier = "9781455546176")
                .Build()
                .ToList();

            Subject.GetIsbn(ids).Should().Be("9781455546176");
        }

        [Test]
        public void should_skip_writing_tags_when_calibre_id_is_missing()
        {
            var file = Builder<BookFile>.CreateNew()
                .With(x => x.CalibreId = 0)
                .Build();

            Subject.WriteTags(file, true, true);

            Mocker.GetMock<ICalibreProxy>()
                .Verify(v => v.SetFields(It.IsAny<BookFile>(), It.IsAny<CalibreSettings>(), It.IsAny<bool>(), It.IsAny<bool>()), Times.Never());
        }

        [Test]
        public void should_skip_writing_tags_when_root_folder_is_not_calibre()
        {
            var file = Builder<BookFile>.CreateNew()
                .With(x => x.Path = "/media/books/Stephen King/Test.epub")
                .With(x => x.CalibreId = 123)
                .Build();

            Mocker.GetMock<IRootFolderService>()
                .Setup(v => v.GetBestRootFolder(file.Path))
                .Returns(new RootFolder { Path = "/media/books", IsCalibreLibrary = false, CalibreSettings = null });

            Subject.WriteTags(file, true, true);

            Mocker.GetMock<ICalibreProxy>()
                .Verify(v => v.SetFields(It.IsAny<BookFile>(), It.IsAny<CalibreSettings>(), It.IsAny<bool>(), It.IsAny<bool>()), Times.Never());
        }
    }
}
