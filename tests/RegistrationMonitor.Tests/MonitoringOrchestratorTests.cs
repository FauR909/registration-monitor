using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using RegistrationMonitor.Core.Entities;
using RegistrationMonitor.Core.Interfaces;
using RegistrationMonitor.Core.Models;
using RegistrationMonitor.Core.Services;

namespace RegistrationMonitor.Tests
{
    [TestFixture]
    public sealed class MonitoringOrchestratorTests
    {
        private Mock<IRegistrationTracker> _trackerMock = null!;
        private Mock<IStatusRepository> _repositoryMock = null!;
        private Mock<INotificationService> _notificationMock = null!;
        private MonitoringOrchestrator _orchestrator = null!;

        [SetUp]
        public void SetUp() 
        {
            _trackerMock = new Mock<IRegistrationTracker>();
            _repositoryMock = new Mock<IStatusRepository>();
            _notificationMock = new Mock<INotificationService>();

            _orchestrator = new MonitoringOrchestrator(
                NullLogger<MonitoringOrchestrator>.Instance,
                _trackerMock.Object,
                _repositoryMock.Object,
                _notificationMock.Object
            );
        }

        /// <summary>
        /// First run of the orchestrator when the status is closed. It should save a record and not send a notification.
        /// </summary>
        [Test]
        public async Task RunCheckAsync_FirstRun_WhenStatusClosed_ShouldSaveRecord_NoNotification()
        {
            //Arrange

            _trackerMock
                .Setup(t => t.GetCurrentStatusAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new RegistrationInfo(RegistrationStatus.Closed, "Closed"));

            _repositoryMock
                .Setup(r => r.GetLastCheckAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync((StatusCheckRecord?)null);

            //Act

            var result = await _orchestrator.RunCheckAsync();

            //Assert

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.WasProcessed, Is.True);
            Assert.That(result.NotificationSent, Is.False);

            _repositoryMock.Verify(
                r => r.AddCheckAsync(
                    It.Is<StatusCheckRecord>(rec =>
                        rec.DetectedStatus == RegistrationStatus.Closed &&
                        rec.NotificationSent == false),
                    It.IsAny<CancellationToken>()),
                Times.Once);

            _notificationMock.Verify(
                n => n.SendRegistrationOpenedAsync(
                    It.IsAny<RegistrationInfo>(),
                    It.IsAny<CancellationToken>()), 
                Times.Never);
        }

        [Test]
        public async Task RunCheckAsync_WhenStatusChangedToOpen_ShouldSaveRecord_SendNotification()
        {
            // Arrange

            _trackerMock
                .Setup(t => t.GetCurrentStatusAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new RegistrationInfo(RegistrationStatus.Open, "Opened"));

            _repositoryMock
                .Setup(r => r.GetLastCheckAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new StatusCheckRecord
                {
                    Id = 1,
                    CheckedAt = DateTimeOffset.UtcNow.AddMinutes(-5),
                    DetectedStatus = RegistrationStatus.Closed,
                    NotificationSent = false
                });

            _notificationMock
                .Setup(n => n.SendRegistrationOpenedAsync(
                    It.IsAny<RegistrationInfo>(),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act

            var result = await _orchestrator.RunCheckAsync();

            // Assert

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.WasProcessed, Is.True);
            Assert.That(result.NotificationSent, Is.True);
            Assert.That(result.DetectedStatus, Is.EqualTo(RegistrationStatus.Open));

            _repositoryMock.Verify(
                r => r.AddCheckAsync(
                    It.Is<StatusCheckRecord>(rec =>
                        rec.DetectedStatus == RegistrationStatus.Open &&
                        rec.NotificationSent == true),
                    It.IsAny<CancellationToken>()),
                Times.Once);

            _notificationMock.Verify(
                n => n.SendRegistrationOpenedAsync(
                    It.IsAny<RegistrationInfo>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Test]
        public async Task RunCheckAsync_WhenStatusUnchanged_SkipsEverything() 
        {
            // Arrange

            _trackerMock
                .Setup(t => t.GetCurrentStatusAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new RegistrationInfo(RegistrationStatus.Closed));

            _repositoryMock
                .Setup(r => r.GetLastCheckAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new StatusCheckRecord
                {
                    DetectedStatus = RegistrationStatus.Closed,
                    CheckedAt = DateTimeOffset.UtcNow.AddMinutes(-1),
                    NotificationSent = false
                });

            // Act

            var result = await _orchestrator.RunCheckAsync();

            // Assert

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.WasProcessed, Is.False);

            _repositoryMock.Verify(
                r => r.AddCheckAsync(
                    It.IsAny<StatusCheckRecord>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);

            _notificationMock.Verify(
                n => n.SendRegistrationOpenedAsync(
                    It.IsAny<RegistrationInfo>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);

        }

        [Test]
        public async Task RunCheckAsync_WhenNotificationPending_RetriesNotification() 
        {
            // Arrange

            _trackerMock
                .Setup(t => t.GetCurrentStatusAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new RegistrationInfo(RegistrationStatus.Open, "Open"));

            _repositoryMock
                .Setup(r => r.GetLastCheckAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new StatusCheckRecord
                {
                    Id = 5,
                    DetectedStatus = RegistrationStatus.Open,
                    NotificationSent = false,
                    CheckedAt = DateTimeOffset.UtcNow.AddMinutes(-30)
                });

            _notificationMock
                .Setup(n => n.SendRegistrationOpenedAsync(
                    It.IsAny<RegistrationInfo>(),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act

            var result = await _orchestrator.RunCheckAsync();

            // Assert

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.WasProcessed, Is.True);

            _notificationMock.Verify(
                n => n.SendRegistrationOpenedAsync(
                    It.IsAny<RegistrationInfo>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Test]
        public async Task RunCheckAsync_WhenTelegramFails_SavesRecord_NoNotification() 
        {
            // Arrange

            _trackerMock
                .Setup(t => t.GetCurrentStatusAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new RegistrationInfo(RegistrationStatus.Open));

            _repositoryMock
                .Setup(r => r.GetLastCheckAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new StatusCheckRecord
                {
                    DetectedStatus = RegistrationStatus.Closed,
                    NotificationSent = false,
                    CheckedAt = DateTimeOffset.UtcNow.AddMinutes(-1)
                });

            _notificationMock
                .Setup(n => n.SendRegistrationOpenedAsync(
                    It.IsAny<RegistrationInfo>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(new HttpRequestException("Telegram anavailable"));

            // Act

            var result = await _orchestrator.RunCheckAsync();

            // Assert

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.NotificationSent, Is.False);

            _repositoryMock.Verify(
                r => r.AddCheckAsync(
                    It.Is<StatusCheckRecord>(rec => rec.NotificationSent == false),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
