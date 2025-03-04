using Hotel.src.Application.Services.Jobs;
using Hotel.src.Core.Entities;
using Hotel.src.Core.Enums;
using Hotel.src.Core.Interfaces.IRepository;
using Hotel.src.Core.Interfaces.IServices;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HotelTests.Application.Services.Jobs
{
    [TestFixture]
    [Author("Julián Vargas", "jose.vaco_24@hotmail.com")]
    public class CheckInNotificationJobTests
    {
        private Mock<IReservationRepository> _reservationRepoMock;
        private Mock<IUserRepository> _userRepoMock;
        private Mock<INotificationService> _notificationServiceMock;
        private CheckInNotificationJob _job;

        [SetUp]
        public void Setup()
        {
            _reservationRepoMock = new Mock<IReservationRepository>();
            _userRepoMock = new Mock<IUserRepository>();
            _notificationServiceMock = new Mock<INotificationService>();
            _job = new CheckInNotificationJob(
                _reservationRepoMock.Object,
                _userRepoMock.Object,
                _notificationServiceMock.Object,
                2);
        }

        [Test]
        public void Execute_WithValidReservation_ShouldSendNotification()
        {
            var reservation = new Reservation
            {
                ID = 1,
                STARTDATE = DateTime.Now.AddDays(2),
                User = new User { EMAIL = "test@example.com", NAME = "John" },
                ReservationRooms = new List<ReservationRoom>()
            };

            _reservationRepoMock.Setup(x => x.GetUpcomingReservations(2))
                .Returns(new List<Reservation> { reservation });
            _notificationServiceMock.Setup(x => x.SendCheckInNotification(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<string>()))
                .Returns(true);

            _job.Execute();

            _notificationServiceMock.Verify(x => x.SendCheckInNotification(
                "test@example.com", "John", reservation.STARTDATE, It.IsAny<string>()), Times.Once);
        }

        [Test]
        public void Execute_UserWithoutEmail_ShouldNotSendNotification()
        {
            var reservation = new Reservation
            {
                ID = 2,
                User = new User { EMAIL = "", NAME = "Alice" }
            };

            _reservationRepoMock.Setup(x => x.GetUpcomingReservations(2))
                .Returns(new List<Reservation> { reservation });

            _job.Execute();

            _notificationServiceMock.Verify(x => x.SendCheckInNotification(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<string>()), Times.Never);
        }

        [Test]
        public void Execute_ReservationWithoutUser_ShouldNotSendNotification()
        {
            var reservation = new Reservation
            {
                ID = 3,
                User = null
            };

            _reservationRepoMock.Setup(x => x.GetUpcomingReservations(2))
                .Returns(new List<Reservation> { reservation });

            _job.Execute();

            _notificationServiceMock.Verify(x => x.SendCheckInNotification(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<string>()), Times.Never);
        }


        [Test]
        public void Execute_NotificationFails_ShouldLogError()
        {
            var reservation = new Reservation
            {
                ID = 4,
                STARTDATE = DateTime.Now.AddDays(2),
                User = new User { EMAIL = "fail@example.com", NAME = "Error User" }
            };

            _reservationRepoMock.Setup(x => x.GetUpcomingReservations(2))
                .Returns(new List<Reservation> { reservation });
            _notificationServiceMock.Setup(x => x.SendCheckInNotification(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<string>()))
                .Returns(false);

            _job.Execute();

            _notificationServiceMock.Verify(x => x.SendCheckInNotification(
                "fail@example.com", "Error User", reservation.STARTDATE, It.IsAny<string>()), Times.Once);
        }

        [Test]
        public void Execute_NoReservations_ShouldNotSendAnyNotifications()
        {
            // Arrange
            _reservationRepoMock.Setup(x => x.GetUpcomingReservations(2))
                .Returns(new List<Reservation>());

            // Act
            _job.Execute();

            // Assert
            _notificationServiceMock.Verify(x => x.SendCheckInNotification(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<string>()), Times.Never);
        }


    }
}