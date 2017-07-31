using Microsoft.AspNetCore.Http;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using System.IO;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using UserServices.Middlewares;
using UserServices.Models;

namespace UserServicesTests.UnitTests.Middlewares {
    [TestFixture]
    public class PasswordEncryptorTest {

        private PasswordEncryptor _passwordEncryptor;
        private Mock<RequestDelegate> _nextMock;
        private Mock<HttpContext> _httpContextMock;

        [SetUp]
        public void SetUp() {
            _httpContextMock = new Mock<HttpContext>()
                .SetupAllProperties();
            _nextMock = new Mock<RequestDelegate>();
            _passwordEncryptor = new PasswordEncryptor(next: _nextMock.Object);
        }

        [Test]
        public async Task Should_encrypt_the_password() {
            var user = new User {
                FirstName = "Test",
                LastName = "Test",
                Email = "test@test.com",
                Password = "test123"
            };
            var body = new MemoryStream(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(user)));
            _httpContextMock.Setup(c => c.Request.Body).Returns(body);

            await _passwordEncryptor.Invoke(_httpContextMock.Object);

            _nextMock.Verify(next => next(_httpContextMock.Object), Times.Once);
            _httpContextMock.VerifySet(c => c.Request.Body = It.IsAny<Stream>(), Times.Once);
        }
    }
}
