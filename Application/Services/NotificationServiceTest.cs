using System;
using NUnit.Framework;
using Moq;
using Hotel.src.Application.Services;
using Hotel.src.Core.Interfaces;

namespace HotelTests.Application.Services
{
    [TestFixture]
    [Author("Julián Vargas", "jose.vaco_24@hotmail.com")]
    public class NotificationServiceTests
    {
        private Mock<INotificationSender> _senderMock;
        private NotificationService _service;

        [SetUp]
        public void Setup()
        {
            // Arrange: Se inicializa el mock y la instancia del servicio.
            _senderMock = new Mock<INotificationSender>();
            _service = new NotificationService(_senderMock.Object);
        }


        [Test]
        public void SendCheckInNotification_ValidData_ReturnsTrue()
        {
            // Arrange
            string recipientEmail = "test@example.com";
            string recipientName = "John Doe";
            DateTime checkInDate = DateTime.Now.AddDays(1);
            string roomDetails = "Room 101";

            _senderMock.Setup(x => x.Send(It.IsAny<string>(), It.IsAny<string>(), recipientEmail))
                       .Returns(true);

            // Act
            bool result = _service.SendCheckInNotification(recipientEmail, recipientName, checkInDate, roomDetails);

            // Assert
            Assert.IsTrue(result);
            _senderMock.Verify(x => x.Send(
                It.Is<string>(s => s == "Recordatorio de su próxima reserva"),
                It.IsAny<string>(),
                recipientEmail), Times.Once);
        }

        [Test]
        public void SendCheckInNotification_SenderReturnsFalse_ReturnsFalse()
        {
            // Arrange
            string recipientEmail = "test@example.com";
            string recipientName = "Jane Doe";
            DateTime checkInDate = DateTime.Now.AddDays(1);
            string roomDetails = "Room 202";

            _senderMock.Setup(x => x.Send(It.IsAny<string>(), It.IsAny<string>(), recipientEmail))
                       .Returns(false);

            // Act
            bool result = _service.SendCheckInNotification(recipientEmail, recipientName, checkInDate, roomDetails);

            // Assert
            Assert.IsFalse(result);
            _senderMock.Verify(x => x.Send(It.IsAny<string>(), It.IsAny<string>(), recipientEmail), Times.Once);
        }

        [Test]
        public void SendCheckInNotification_SenderThrowsException_ReturnsFalse()
        {
            // Arrange
            string recipientEmail = "test@example.com";
            string recipientName = "John";
            DateTime checkInDate = DateTime.Now.AddDays(1);
            string roomDetails = "Room 303";

            _senderMock.Setup(x => x.Send(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                       .Throws(new Exception("Sender error"));

            // Act
            bool result = _service.SendCheckInNotification(recipientEmail, recipientName, checkInDate, roomDetails);

            // Assert
            Assert.IsFalse(result);
            _senderMock.Verify(x => x.Send(It.IsAny<string>(), It.IsAny<string>(), recipientEmail), Times.Once);
        }


        [Test]
        public void SendCheckInNotification_NullRecipientName_ReturnsTrue()
        {
            // Arrange
            string recipientEmail = "nullname@example.com";
            string recipientName = null;
            DateTime checkInDate = DateTime.Now.AddDays(2);
            string roomDetails = "Room 606";

            _senderMock.Setup(x => x.Send(It.IsAny<string>(), It.IsAny<string>(), recipientEmail))
                       .Returns(true);

            // Act
            bool result = _service.SendCheckInNotification(recipientEmail, recipientName, checkInDate, roomDetails);

            // Assert
            Assert.IsTrue(result);
            _senderMock.Verify(x => x.Send(
                It.IsAny<string>(),
                It.Is<string>(m => m.Contains("Estimado/a")),
                recipientEmail), Times.Once);
        }

   
        [Test]
        public void SendCheckInNotification_EmptyRecipientName_ReturnsTrue()
        {
            // Arrange
            string recipientEmail = "emptyname@example.com";
            string recipientName = "";
            DateTime checkInDate = DateTime.Now.AddDays(2);
            string roomDetails = "Room 707";

            _senderMock.Setup(x => x.Send(It.IsAny<string>(), It.IsAny<string>(), recipientEmail))
                       .Returns(true);

            // Act
            bool result = _service.SendCheckInNotification(recipientEmail, recipientName, checkInDate, roomDetails);

            // Assert
            Assert.IsTrue(result);
            _senderMock.Verify(x => x.Send(
                It.IsAny<string>(),
                It.Is<string>(m => m.Contains("Estimado/a")),
                recipientEmail), Times.Once);
        }


      
        [Test]
        public void SendCheckInNotification_FutureCheckInDate_ReturnsTrue()
        {
            // Arrange
            string recipientEmail = "future@example.com";
            string recipientName = "Future Guest";
            DateTime checkInDate = DateTime.Now.AddMonths(1);
            string roomDetails = "Room 808";

            _senderMock.Setup(x => x.Send(It.IsAny<string>(), It.IsAny<string>(), recipientEmail))
                       .Returns(true);

            // Act
            bool result = _service.SendCheckInNotification(recipientEmail, recipientName, checkInDate, roomDetails);

            // Assert
            Assert.IsTrue(result);
            _senderMock.Verify(x => x.Send(It.IsAny<string>(), It.IsAny<string>(), recipientEmail), Times.Once);
        }

     
        [Test]
        public void SendCheckInNotification_PastCheckInDate_ReturnsTrue()
        {
            // Arrange
            string recipientEmail = "past@example.com";
            string recipientName = "Past Guest";
            DateTime checkInDate = DateTime.Now.AddDays(-1);
            string roomDetails = "Room 909";

            _senderMock.Setup(x => x.Send(It.IsAny<string>(), It.IsAny<string>(), recipientEmail))
                       .Returns(true);

            // Act
            bool result = _service.SendCheckInNotification(recipientEmail, recipientName, checkInDate, roomDetails);

            // Assert
            Assert.IsTrue(result);
            _senderMock.Verify(x => x.Send(It.IsAny<string>(), It.IsAny<string>(), recipientEmail), Times.Once);
        }

  
      

        

        [Test]
        public void SendCheckInNotification_VerifySubjectContent()
        {
            // Arrange
            string recipientEmail = "subject@example.com";
            string recipientName = "Subject Tester";
            DateTime checkInDate = new DateTime(2025, 12, 25);
            string roomDetails = "Suite 1";
            string expectedSubject = "Recordatorio de su próxima reserva";

            _senderMock.Setup(x => x.Send(expectedSubject, It.IsAny<string>(), recipientEmail))
                       .Returns(true);

            // Act
            bool result = _service.SendCheckInNotification(recipientEmail, recipientName, checkInDate, roomDetails);

            // Assert
            Assert.IsTrue(result);
            _senderMock.Verify(x => x.Send(
                It.Is<string>(s => s == expectedSubject),
                It.IsAny<string>(),
                recipientEmail), Times.Once);
        }

        [Test]
        public void SendCheckInNotification_MultipleRooms_FormatsDetailsCorrectly()
        {
            // Arrange
            string recipientEmail = "multiroom@example.com";
            string recipientName = "Laura";
            DateTime checkInDate = DateTime.Now.AddDays(2);
            string roomDetails = "Habitación 101 (SUITE)\nHabitación 202 (DOBLE)";

            string capturedMessage = null;
            _senderMock.Setup(x => x.Send(It.IsAny<string>(), It.IsAny<string>(), recipientEmail))
                      .Callback<string, string, string>((_, m, _) => capturedMessage = m)
                      .Returns(true);

            // Act
            _service.SendCheckInNotification(recipientEmail, recipientName, checkInDate, roomDetails);

            // Assert
            Assert.That(capturedMessage, Contains.Substring("Habitación 101 (SUITE)"));
            Assert.That(capturedMessage, Contains.Substring("Habitación 202 (DOBLE)"));
        }

        [Test]
        public void SendCheckInNotification_RoomWithoutNumber_ShowsDefaultText()
        {
            // Arrange
            string recipientEmail = "nonumber@example.com";
            string roomDetails = "Habitación N/A (SUITE)";
            string capturedMessage = null;

            _senderMock.Setup(x => x.Send(It.IsAny<string>(), It.IsAny<string>(), recipientEmail))
                      .Callback<string, string, string>((_, m, _) => capturedMessage = m)
                      .Returns(true);

            // Act
            _service.SendCheckInNotification(recipientEmail, "Juan", DateTime.Now.AddDays(1), roomDetails);

            // Assert
            Assert.That(capturedMessage, Contains.Substring("Habitación N/A (SUITE)"));
        }


        [Test]
        public void SendCheckInNotification_MultipleCalls_IndependentResults()
        {
            // Arrange
            string email1 = "first@example.com";
            string email2 = "second@example.com";

            _senderMock.Setup(x => x.Send(It.IsAny<string>(), It.IsAny<string>(), email1))
                       .Returns(true);
            _senderMock.Setup(x => x.Send(It.IsAny<string>(), It.IsAny<string>(), email2))
                       .Returns(false);

            // Act
            bool result1 = _service.SendCheckInNotification(email1, "First", DateTime.Now.AddDays(1), "Room A");
            bool result2 = _service.SendCheckInNotification(email2, "Second", DateTime.Now.AddDays(2), "Room B");

            // Assert
            Assert.IsTrue(result1);
            Assert.IsFalse(result2);
            _senderMock.Verify(x => x.Send(It.IsAny<string>(), It.IsAny<string>(), email1), Times.Once);
            _senderMock.Verify(x => x.Send(It.IsAny<string>(), It.IsAny<string>(), email2), Times.Once);
        }
    }
}
