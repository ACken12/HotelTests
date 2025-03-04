using System;
using System.Net;
using System.Net.Mail;
using Moq;
using NUnit.Framework;
using Hotel.src.Application;
using Hotel.src.Core.Interfaces;

namespace HotelTests.Application;

[TestFixture]
public class EmailNotificationSenderTests
{
    private const string FromEmail = "sender@example.com";
    private const string EmailPassword = "password123";
    private Mock<ISmtpClient> _smtpClientMock;
    private EmailNotificationSender _emailNotificationSender;

    [SetUp]
    public void Setup()
    {
        // Arrange: Configuramos las variables de entorno necesarias para las pruebas.
        Environment.SetEnvironmentVariable("EMAIL_FROM_ADDRESS", FromEmail);
        Environment.SetEnvironmentVariable("EMAIL_PASSWORD", EmailPassword);

        // Creamos el mock para ISmtpClient y lo inyectamos en el EmailNotificationSender.
        _smtpClientMock = new Mock<ISmtpClient>();
        _emailNotificationSender = new EmailNotificationSender(_smtpClientMock.Object);
    }

    [TearDown]
    public void TearDown()
    {
        Environment.SetEnvironmentVariable("EMAIL_FROM_ADDRESS", null);
        Environment.SetEnvironmentVariable("EMAIL_PASSWORD", null);
    }

    [Test]
    public void Send_ValidEmail_ReturnsTrue()
    {
        // Arrange: Configuramos un correo válido y capturamos el objeto MailMessage enviado.
        var subject = "Test Subject";
        var message = "Test Message";
        var recipient = "recipient@example.com";
        MailMessage capturedMail = null;

        _smtpClientMock
            .Setup(x => x.Send(It.IsAny<MailMessage>()))
            .Callback<MailMessage>(mail => capturedMail = mail);

        // Act: Se invoca el método Send.
        var result = _emailNotificationSender.Send(subject, message, recipient);

        // Assert: Se verifica que el envío fue exitoso y que se configuró correctamente el MailMessage.
        Assert.IsTrue(result, "El envío debería retornar true para un correo válido.");
        _smtpClientMock.Verify(x => x.Send(It.IsAny<MailMessage>()), Times.Once);
        Assert.IsNotNull(capturedMail, "El objeto MailMessage no debería ser nulo.");
        Assert.AreEqual(FromEmail, capturedMail.From.Address, "La dirección de remitente no coincide.");
        Assert.AreEqual(recipient, capturedMail.To[0].Address, "La dirección de destinatario no coincide.");
        Assert.AreEqual(subject, capturedMail.Subject, "El asunto no coincide.");
        Assert.AreEqual(message, capturedMail.Body, "El mensaje no coincide.");
    }


    [Test]
    public void Send_InvalidRecipient_Empty_ReturnsFalse()
    {
        // Arrange
        var subject = "Test Subject";
        var message = "Test Message";
        string invalidRecipient = ""; // Destinatario vacío

        // Act: Se invoca el método con un destinatario vacío.
        var result = _emailNotificationSender.Send(subject, message, invalidRecipient);

        // Assert: Se espera que retorne false y no se invoque el método Send del cliente SMTP.
        Assert.IsFalse(result, "El método debería retornar false para destinatarios vacíos.");
        _smtpClientMock.Verify(x => x.Send(It.IsAny<MailMessage>()), Times.Never, "El método Send no debe ser invocado para destinatarios vacíos.");
    }

    [Test]
    public void Send_InvalidRecipient_Null_ReturnsFalse()
    {
        // Arrange
        var subject = "Test Subject";
        var message = "Test Message";
        string invalidRecipient = null; // Destinatario nulo

        // Act: Se invoca el método con un destinatario nulo.
        var result = _emailNotificationSender.Send(subject, message, invalidRecipient);

        // Assert: Se espera que retorne false y no se invoque el método Send del cliente SMTP.
        Assert.IsFalse(result, "El método debería retornar false para destinatarios nulos.");
        _smtpClientMock.Verify(x => x.Send(It.IsAny<MailMessage>()), Times.Never, "El método Send no debe ser invocado para destinatarios nulos.");
    }


    [Test]
    public void Send_InvalidEmailFormat_EmptyEmail_ReturnsFalse()
    {
        // Arrange
        var subject = "Test Subject";
        var message = "Test Message";
        var invalidEmail = "invalidemail"; // Empty email

        // Act
        var result = _emailNotificationSender.Send(subject, message, invalidEmail);

        // Assert
        Assert.IsFalse(result, $"El método debería retornar false para el email inválido: {invalidEmail}");
        _smtpClientMock.Verify(x => x.Send(It.Is<MailMessage>(m => m.To.ToString() == invalidEmail)), Times.Never, "El método `Send` no debería ser invocado para emails inválidos.");
    }

    [Test]
    public void Send_InvalidEmailFormat_MissingDomain_ReturnsFalse()
    {
        // Arrange
        var subject = "Test Subject";
        var message = "Test Message";
        var invalidEmail = "invalid@"; // Missing domain part

        // Act
        var result = _emailNotificationSender.Send(subject, message, invalidEmail);

        // Assert
        Assert.IsFalse(result, $"El método debería retornar false para el email inválido: {invalidEmail}");
        _smtpClientMock.Verify(x => x.Send(It.Is<MailMessage>(m => m.To.ToString() == invalidEmail)), Times.Never, "El método `Send` no debería ser invocado para emails inválidos.");
    }

    [Test]
    public void Send_InvalidEmailFormat_MissingLocalPart_ReturnsFalse()
    {
        // Arrange
        var subject = "Test Subject";
        var message = "Test Message";
        var invalidEmail = "@invalid.com"; // Missing local part

        // Act
        var result = _emailNotificationSender.Send(subject, message, invalidEmail);

        // Assert
        Assert.IsFalse(result, $"El método debería retornar false para el email inválido: {invalidEmail}");
        _smtpClientMock.Verify(x => x.Send(It.Is<MailMessage>(m => m.To.ToString() == invalidEmail)), Times.Never, "El método `Send` no debería ser invocado para emails inválidos.");
    }




    [Test]
    public void Send_SmtpClientThrowsException_ReturnsFalse()
    {
        // Arrange: Configuramos el mock para que arroje una excepción al intentar enviar.
        var subject = "Test Subject";
        var message = "Test Message";
        var recipient = "recipient@example.com";

        _smtpClientMock
            .Setup(x => x.Send(It.IsAny<MailMessage>()))
            .Throws(new SmtpException("SMTP error"));

        // Act
        var result = _emailNotificationSender.Send(subject, message, recipient);

        // Assert: Se debe retornar false y verificamos que se invocó el método Send exactamente una vez.
        Assert.IsFalse(result, "Si el cliente SMTP arroja excepción, el método debe retornar false.");
        _smtpClientMock.Verify(x => x.Send(It.IsAny<MailMessage>()), Times.Once);
    }

    [Test]
    public void Send_SetsSmtpClientPropertiesCorrectly()
    {
        // Arrange: Variables para capturar las propiedades establecidas en el cliente SMTP.
        var subject = "Test Subject";
        var message = "Test Message";
        var recipient = "recipient@example.com";

        string capturedHost = null;
        int capturedPort = 0;
        bool capturedEnableSsl = false;
        ICredentialsByHost capturedCredentials = null;

        _smtpClientMock
            .SetupSet(x => x.Host = It.IsAny<string>())
            .Callback<string>(value => capturedHost = value);
        _smtpClientMock
            .SetupSet(x => x.Port = It.IsAny<int>())
            .Callback<int>(value => capturedPort = value);
        _smtpClientMock
            .SetupSet(x => x.EnableSsl = It.IsAny<bool>())
            .Callback<bool>(value => capturedEnableSsl = value);
        _smtpClientMock
            .SetupSet(x => x.Credentials = It.IsAny<ICredentialsByHost>())
            .Callback<ICredentialsByHost>(value => capturedCredentials = value);

        // Act
        var result = _emailNotificationSender.Send(subject, message, recipient);

        // Assert: Se verifica que las propiedades del cliente SMTP se asignaron correctamente.
        Assert.IsTrue(result, "El envío debería ser exitoso.");
        Assert.AreEqual("smtp.gmail.com", capturedHost, "El host SMTP no coincide.");
        Assert.AreEqual(587, capturedPort, "El puerto SMTP no coincide.");
        Assert.IsTrue(capturedEnableSsl, "La propiedad EnableSsl debe ser true.");
        Assert.IsInstanceOf<NetworkCredential>(capturedCredentials, "Las credenciales deben ser de tipo NetworkCredential.");

        var networkCredential = (NetworkCredential)capturedCredentials;
        Assert.AreEqual(FromEmail, networkCredential.UserName, "El nombre de usuario de las credenciales no coincide.");
        Assert.AreEqual(EmailPassword, networkCredential.Password, "La contraseña de las credenciales no coincide.");
    }

    [Test]
    public void Send_ValidEmail1_ReturnsTrue()
    {
        // Arrange
        var subject = "Test Subject";
        var message = "Test Message";
        MailMessage capturedMail = null;

        _smtpClientMock
            .Setup(x => x.Send(It.IsAny<MailMessage>()))
            .Callback<MailMessage>(mail => capturedMail = mail);

        // Act
        var validEmail1 = "user@domain.com";
        var result1 = _emailNotificationSender.Send(subject, message, validEmail1);

        // Assert
        Assert.IsTrue(result1);
        _smtpClientMock.Verify(x => x.Send(It.IsAny<MailMessage>()), Times.Once);
        Assert.IsNotNull(capturedMail);
        Assert.AreEqual(validEmail1, capturedMail.To[0].Address);
    }

    [Test]
    public void Send_ValidEmail2_ReturnsTrue()
    {
        // Arrange
        var subject = "Test Subject";
        var message = "Test Message";
        MailMessage capturedMail = null;

        _smtpClientMock
            .Setup(x => x.Send(It.IsAny<MailMessage>()))
            .Callback<MailMessage>(mail => capturedMail = mail);

        // Act
        var validEmail2 = "USER@DOMAIN.COM";
        var result2 = _emailNotificationSender.Send(subject, message, validEmail2);

        // Assert
        Assert.IsTrue(result2);
        _smtpClientMock.Verify(x => x.Send(It.IsAny<MailMessage>()), Times.Once);
        Assert.IsNotNull(capturedMail);
        Assert.AreEqual(validEmail2, capturedMail.To[0].Address);
    }

    [Test]
    public void Send_ValidEmail3_ReturnsTrue()
    {
        // Arrange
        var subject = "Test Subject";
        var message = "Test Message";
        MailMessage capturedMail = null;

        _smtpClientMock
            .Setup(x => x.Send(It.IsAny<MailMessage>()))
            .Callback<MailMessage>(mail => capturedMail = mail);

        // Act
        var validEmail3 = "user.name+alias@sub.domain.co.uk";
        var result3 = _emailNotificationSender.Send(subject, message, validEmail3);

        // Assert
        Assert.IsTrue(result3);
        _smtpClientMock.Verify(x => x.Send(It.IsAny<MailMessage>()), Times.Once);
        Assert.IsNotNull(capturedMail);
        Assert.AreEqual(validEmail3, capturedMail.To[0].Address);
    }




    [Test]
    public void Send_NullSubject_ReturnsTrue()
    {
        // Arrange: Se prueba el caso en el que el asunto es nulo.
        string subject = null;
        var message = "Test Message";
        var recipient = "recipient@example.com";
        MailMessage capturedMail = null;

        _smtpClientMock
            .Setup(x => x.Send(It.IsAny<MailMessage>()))
            .Callback<MailMessage>(mail => capturedMail = mail);

        // Act
        var result = _emailNotificationSender.Send(subject, message, recipient);

        // Assert
        Assert.IsTrue(result, "El método debe retornar true aun cuando el asunto sea nulo.");
        _smtpClientMock.Verify(x => x.Send(It.IsAny<MailMessage>()), Times.Once);
        // Debido al comportamiento de MailMessage, al asignar null se obtiene string.Empty.
        Assert.AreEqual(string.Empty, capturedMail.Subject, "El asunto del MailMessage debe ser string.Empty cuando se asigna null.");
    }

    [Test]
    public void Send_NullMessage_ReturnsTrue()
    {
        // Arrange: Se prueba el caso en el que el mensaje es nulo.
        var subject = "Test Subject";
        string message = null;
        var recipient = "recipient@example.com";
        MailMessage capturedMail = null;

        _smtpClientMock
            .Setup(x => x.Send(It.IsAny<MailMessage>()))
            .Callback<MailMessage>(mail => capturedMail = mail);

        // Act
        var result = _emailNotificationSender.Send(subject, message, recipient);

        // Assert
        Assert.IsTrue(result, "El método debe retornar true aun cuando el mensaje sea nulo.");
        _smtpClientMock.Verify(x => x.Send(It.IsAny<MailMessage>()), Times.Once);
        // Debido al comportamiento de MailMessage, al asignar null se obtiene string.Empty.
        Assert.AreEqual(string.Empty, capturedMail.Body, "El cuerpo del MailMessage debe ser string.Empty cuando se asigna null.");
    }

}
