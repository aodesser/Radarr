﻿using System;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using FluentAssertions;
using NzbDrone.Common.Http;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Download;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.RemotePathMappings;
using NzbDrone.Common.Disk;

namespace NzbDrone.Core.Test.Download.DownloadClientTests
{
    public abstract class DownloadClientFixtureBase<TSubject> : CoreTest<TSubject>
        where TSubject : class, IDownloadClient
    {
        protected readonly string _title = "Droned.1998.1080p.WEB-DL-DRONE";
        protected readonly string _downloadUrl = "http://somewhere.com/Droned.1998.1080p.WEB-DL-DRONE.ext";

        [SetUp]
        public void SetupBase()
        {
            Mocker.GetMock<IConfigService>()
                .SetupGet(s => s.DownloadClientHistoryLimit)
                .Returns(30);

            Mocker.GetMock<IParsingService>()
                .Setup(s => s.Map(It.IsAny<ParsedMovieInfo>(), It.IsAny<string>(), (SearchCriteriaBase)null))
                .Returns(() => new MappingResult{RemoteMovie =  CreateRemoteMovie(), MappingResultType = MappingResultType.Success});

            Mocker.GetMock<IHttpClient>()
                  .Setup(s => s.Get(It.IsAny<HttpRequest>()))
                  .Returns<HttpRequest>(r => new HttpResponse(r, new HttpHeader(), new byte[0]));

            Mocker.GetMock<IRemotePathMappingService>()
                .Setup(v => v.RemapRemoteToLocal(It.IsAny<string>(), It.IsAny<OsPath>()))
                .Returns<string, OsPath>((h, r) => r);
        }

        protected virtual RemoteMovie CreateRemoteMovie()
        {
            var remoteMovie = new RemoteMovie();
            remoteMovie.Release = new ReleaseInfo();
            remoteMovie.Release.Title = _title;
            remoteMovie.Release.DownloadUrl = _downloadUrl;
            remoteMovie.Release.DownloadProtocol = Subject.Protocol;

            remoteMovie.ParsedMovieInfo = new ParsedMovieInfo();

            remoteMovie.Movie = new Movie();

            return remoteMovie;
        }

        protected void VerifyIdentifiable(DownloadClientItem downloadClientItem)
        {
            downloadClientItem.DownloadClient.Should().Be(Subject.Definition.Name);
            downloadClientItem.DownloadId.Should().NotBeNullOrEmpty();
            downloadClientItem.Title.Should().NotBeNullOrEmpty();
        }

        protected void VerifyQueued(DownloadClientItem downloadClientItem)
        {
            VerifyIdentifiable(downloadClientItem);
            downloadClientItem.RemainingSize.Should().NotBe(0);
            //downloadClientItem.RemainingTime.Should().NotBe(TimeSpan.Zero);
            //downloadClientItem.OutputPath.Should().NotBeNullOrEmpty();
            downloadClientItem.Status.Should().Be(DownloadItemStatus.Queued);
        }

        protected void VerifyPaused(DownloadClientItem downloadClientItem)
        {
            VerifyIdentifiable(downloadClientItem);

            downloadClientItem.RemainingSize.Should().NotBe(0);
            //downloadClientItem.RemainingTime.Should().NotBe(TimeSpan.Zero);
            //downloadClientItem.OutputPath.Should().NotBeNullOrEmpty();
            downloadClientItem.Status.Should().Be(DownloadItemStatus.Paused);
        }

        protected void VerifyDownloading(DownloadClientItem downloadClientItem)
        {
            VerifyIdentifiable(downloadClientItem);

            downloadClientItem.RemainingSize.Should().NotBe(0);
            //downloadClientItem.RemainingTime.Should().NotBe(TimeSpan.Zero);
            //downloadClientItem.OutputPath.Should().NotBeNullOrEmpty();
            downloadClientItem.Status.Should().Be(DownloadItemStatus.Downloading);
        }

        protected void VerifyPostprocessing(DownloadClientItem downloadClientItem)
        {
            VerifyIdentifiable(downloadClientItem);

            //downloadClientItem.RemainingTime.Should().NotBe(TimeSpan.Zero);
            //downloadClientItem.OutputPath.Should().NotBeNullOrEmpty();
            downloadClientItem.Status.Should().Be(DownloadItemStatus.Downloading);
        }

        protected void VerifyCompleted(DownloadClientItem downloadClientItem)
        {
            VerifyIdentifiable(downloadClientItem);

            downloadClientItem.Title.Should().NotBeNullOrEmpty();
            downloadClientItem.RemainingSize.Should().Be(0);
            downloadClientItem.RemainingTime.Should().Be(TimeSpan.Zero);
            //downloadClientItem.OutputPath.Should().NotBeNullOrEmpty();
            downloadClientItem.Status.Should().Be(DownloadItemStatus.Completed);
        }

        protected void VerifyWarning(DownloadClientItem downloadClientItem)
        {
            VerifyIdentifiable(downloadClientItem);

            downloadClientItem.Status.Should().Be(DownloadItemStatus.Warning);
        }

        protected void VerifyFailed(DownloadClientItem downloadClientItem)
        {
            VerifyIdentifiable(downloadClientItem);

            downloadClientItem.Status.Should().Be(DownloadItemStatus.Failed);
        }
    }
}
