using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using UserServices.Controllers;
using UserServices.Models;
using UserServices.Repositories;
using UserServicesDotNetCore.Entities;
using UserServicesDotNetCore.Helpers;

namespace UserServicesTests.UnitTests.Controllers {

    [TestFixture]
    public class UserControllerTest {

        private UsersController _controller;
        private Mock<IUserServiceRepository> _userServiceRepositoryMock;
        private Mock<IMapper> _autoMapperMock;

        [SetUp]
        public void SetUp() {
            _autoMapperMock = new Mock<IMapper>();

            //_autoMapperMock.Setup(map => map.Map(It.IsAny<UserEntity>(), It.IsAny<User>())).Returns(It.IsAny<User>());
            //_autoMapperMock.Setup(map => map.Map(It.IsAny<User>(), It.IsAny<UserEntity>())).Returns(It.IsAny<UserEntity>());

            _userServiceRepositoryMock = new Mock<IUserServiceRepository>();
            _controller = new UsersController(_autoMapperMock.Object, _userServiceRepositoryMock.Object);
        }

        [Test]
        public void Get_Should_list_all_users() {
            var expected = GetExpectedUsers();
            var setup = GetUsersEntityMock();
            _userServiceRepositoryMock.Setup(s => s.GetUsers()).ReturnsAsync(setup);
            _autoMapperMock.Setup(map => map.Map<List<User>>(setup)).Returns(expected);

            var actual = (List<User>)((ObjectResult)_controller.Get().Result).Value;

            for (int i = 0; i < 2; i++) {
                Assert.AreEqual(expected[i].FirstName, actual[i].FirstName);
                Assert.AreEqual(expected[i].LastName, actual[i].LastName);
                Assert.AreEqual(expected[i].Email, actual[i].Email);
                Assert.AreEqual(expected[i].Role, actual[i].Role);
            }
        }

        [Test]
        public void Get_Should_return_an_empty_array_if_there_is_no_user() {
            _userServiceRepositoryMock.Setup(s => s.GetUsers()).ReturnsAsync(new List<UserEntity>());
            _autoMapperMock.Setup(map => map.Map<List<User>>(It.IsAny<List<UserEntity>>())).Returns(new List<User>());

            var actual = (List<User>)((ObjectResult)_controller.Get().Result).Value;

            Assert.IsEmpty(actual);
        }

        [Test]
        public void Get_Should_return_status_code_500_if_error() {
            _userServiceRepositoryMock.Setup(user => user.GetUsers()).Throws(new Exception());
            var expected = (int)HttpStatusCode.InternalServerError;

            var actual = ((ObjectResult)_controller.Get().Result).StatusCode;

            Assert.AreEqual(expected, actual);
        }

        [TestCase("00000000-1111-1234-2222-333333334444")]
        [TestCase("00000000-1111-1234-2222-888888887777")]
        public void Get_Should_return_one_user_by_id(string id) {
            var guidId = new Guid(id);
            var users = GetUsersEntityMock();
            var expected = users.FirstOrDefault(user => user.Id == guidId);
            var userMapper = GetExpectedUsers().FirstOrDefault(e => e.Email == expected.Email);
            _userServiceRepositoryMock.Setup(user => user.GetUserById(It.IsAny<Guid>())).ReturnsAsync(expected);
            _autoMapperMock.Setup(map => map.Map<User>(expected)).Returns(userMapper);

            var actual = (User)((ObjectResult)_controller.Get(guidId).Result).Value;

            Assert.AreEqual(expected.FirstName, actual.FirstName);
            Assert.AreEqual(expected.LastName, actual.LastName);
            Assert.AreEqual(expected.Email, actual.Email);
            Assert.AreEqual(expected.Role, actual.Role);
        }

        [Test]
        public void Get_Should_return_status_code_404_if_user_dont_exist() {
            var guidId = Guid.NewGuid();
            _userServiceRepositoryMock.Setup(user => user.GetUserById(It.IsAny<Guid>())).ReturnsAsync((UserEntity)null);
            var expected = (int)HttpStatusCode.NotFound;

            var actual = ((ObjectResult)_controller.Get(guidId).Result).StatusCode;

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Get_Should_return_status_code_500_if_error_to_get_user() {
            var guidId = Guid.NewGuid();
            _userServiceRepositoryMock.Setup(user => user.GetUserById(It.IsAny<Guid>())).Throws(new Exception());
            var expected = (int)HttpStatusCode.InternalServerError;

            var actual = ((ObjectResult)_controller.Get(guidId).Result).StatusCode;

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Register_Should_return_status_code_201_if_register_with_success() {
            var user = GetUser();
            var statusCodeExpected = (int)HttpStatusCode.Created;
            _userServiceRepositoryMock.Setup(u => u.Insert(It.IsAny<UserEntity>())).Returns(Task.FromResult(0));

            var actual = ((ObjectResult)_controller.Register(user).Result);

            Assert.AreEqual(statusCodeExpected, actual.StatusCode);
        }

        [Test]
        public void Register_Should_return_status_code_400_if_model_state_is_invalid() {
            _controller.ModelState.AddModelError("Error", "Error");
            var statusCodeExpected = (int)HttpStatusCode.BadRequest;

            var actual = ((ObjectResult)_controller.Register(new User()).Result);

            Assert.AreEqual(statusCodeExpected, actual.StatusCode);
        }

        [TestCase("")]
        [TestCase(" ")]
        [TestCase(null)]
        public void Register_Should_return_message_field_is_required_if_mail_is_null_or_empy(string email) {
            var expected = "The Email field is required.";
            _controller.ModelState.AddModelError("Email", expected);
            var user = GetUser(email: email);

            var result = ((SerializableError)((ObjectResult)_controller.Register(user).Result).Value).Values;
            var actual = result.Select(value => (string[])value).FirstOrDefault()[0];

            Assert.AreEqual(expected, actual);
        }

        [TestCase("email@")]
        [TestCase("@email")]
        [TestCase("email.com")]
        public void Register_Should_return_error_message_if_mail_is_invalid(string email) {
            var expected = "The Email field is not a valid e-mail address.";
            _controller.ModelState.AddModelError("Email", expected);
            var user = GetUser(email: email);

            var result = ((SerializableError)((ObjectResult)_controller.Register(user).Result).Value).Values;
            var actual = result.Select(value => (string[])value).FirstOrDefault()[0];

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Register_Should_return_error_message_if_mail_is_invalid() {
            var expected = "Email must be a maximum 100 characters.";
            _controller.ModelState.AddModelError("Email", expected);
            var user = GetUser(email: "thisisabigemailmostbigofworld@thisisabigemailmostbigofworldthisisabigemailmostbigofworldthisisabigemail");

            var result = ((SerializableError)((ObjectResult)_controller.Register(user).Result).Value).Values;
            var actual = result.Select(value => (string[])value).FirstOrDefault()[0];

            Assert.AreEqual(expected, actual);
        }

        [TestCase("")]
        [TestCase(" ")]
        [TestCase(null)]
        public void Register_Should_return_message_field_is_required_if_first_name_is_null_or_empy(string firstName) {
            var expected = "The FirstName field is required.";
            _controller.ModelState.AddModelError("FirstName", expected);
            var user = GetUser(firstName: firstName);

            var result = ((SerializableError)((ObjectResult)_controller.Register(user).Result).Value).Values;
            var actual = result.Select(value => (string[])value).FirstOrDefault()[0];

            Assert.AreEqual(expected, actual);
        }

        [TestCase("st")]
        [TestCase("g")]
        public void Register_Should_return_message_error_if_first_name_is_shorter(string firstName) {
            var expected = "FirstName must be minimum 3 characters";
            _controller.ModelState.AddModelError("FirstName", expected);
            var user = GetUser(firstName: firstName);

            var result = ((SerializableError)((ObjectResult)_controller.Register(user).Result).Value).Values;
            var actual = result.Select(value => (string[])value).FirstOrDefault()[0];

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Register_Should_return_message_error_if_first_name_is_longer() {
            var expected = "FirstName must be maximum 100 characters";
            _controller.ModelState.AddModelError("FirstName", expected);
            var user = GetUser(firstName: "unmonheahsjkahdaio9yadaodjpaisdadaudoiaosidunmonheahsjkahdaio9yadaodjpaisdadaudoiaosidouqwahskasçpsd");

            var result = ((SerializableError)((ObjectResult)_controller.Register(user).Result).Value).Values;
            var actual = result.Select(value => (string[])value).FirstOrDefault()[0];

            Assert.AreEqual(expected, actual);
        }

        [TestCase("")]
        [TestCase(" ")]
        [TestCase(null)]
        public void Register_Should_return_message_field_is_required_if_last_name_is_null_or_empy(string lastName) {
            var expected = "The LastName field is required.";
            _controller.ModelState.AddModelError("LastName", expected);
            var user = GetUser(lastName: lastName);

            var result = ((SerializableError)((ObjectResult)_controller.Register(user).Result).Value).Values;
            var actual = result.Select(value => (string[])value).FirstOrDefault()[0];

            Assert.AreEqual(expected, actual);
        }

        [TestCase("st")]
        [TestCase("g")]
        public void Register_Should_return_message_error_if_last_name_is_shorter(string lastName) {
            var expected = "LastName must be minimum 3 characters.";
            _controller.ModelState.AddModelError("LastName", expected);
            var user = GetUser(lastName: lastName);

            var result = ((SerializableError)((ObjectResult)_controller.Register(user).Result).Value).Values;
            var actual = result.Select(value => (string[])value).FirstOrDefault()[0];

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Register_Should_return_message_error_if_last_name_is_longer() {
            var expected = "LastName must be maximum 100 characters.";
            _controller.ModelState.AddModelError("LastName", expected);
            var user = GetUser(lastName: "unmonheahsjkahdaio9yadaodjpaisdadaudoiaosidunmonheahsjkahdaio9yadaodjpaisdadaudoiaosidouqwahskasçpsd");

            var result = ((SerializableError)((ObjectResult)_controller.Register(user).Result).Value).Values;
            var actual = result.Select(value => (string[])value).FirstOrDefault()[0];

            Assert.AreEqual(expected, actual);
        }

        [TestCase(-1)]
        [TestCase(0)]
        [TestCase(null)]
        public void Register_Should_return_message_field_is_required_if_role_is_invalid(Role role) {
            var expected = "The Role field is required.";
            _controller.ModelState.AddModelError("Role", expected);
            var user = GetUser(role: role);

            var result = ((SerializableError)((ObjectResult)_controller.Register(user).Result).Value).Values;
            var actual = result.Select(value => (string[])value).FirstOrDefault()[0];

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Register_Should_return_message_error_if_cpf_cnpj_is_shorter() {
            var expected = "CpfCnpj must be minimum 11 characters.";
            _controller.ModelState.AddModelError("CpfCnpj", expected);
            var user = GetUser();
            user.CpfCnpj = "1234567891";

            var result = ((SerializableError)((ObjectResult)_controller.Register(user).Result).Value).Values;
            var actual = result.Select(value => (string[])value).FirstOrDefault()[0];

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Register_Should_return_message_error_if_cpf_cnpj_is_longer() {
            var expected = "CpfCnpj must be maximum 14 characters.";
            _controller.ModelState.AddModelError("CpfCnpj", expected);
            var user = GetUser();
            user.CpfCnpj = "123456789101215";

            var result = ((SerializableError)((ObjectResult)_controller.Register(user).Result).Value).Values;
            var actual = result.Select(value => (string[])value).FirstOrDefault()[0];

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Register_Should_return_status_code_500_if_error() {
            _userServiceRepositoryMock.Setup(user => user.Insert(It.IsAny<UserEntity>())).Throws(new Exception());
            var expected = (int)HttpStatusCode.InternalServerError;

            var actual = ((ObjectResult)_controller.Register(new User()).Result).StatusCode;

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Put_Should_return_status_code_200_if_updated_with_success() {
            var user = GetUser();
            var statusCodeExpected = (int)HttpStatusCode.OK;
            var guidId = Guid.NewGuid();
            SetupRepositoryGetUser(guidId);
            var userEntity = GetDefaultUserEntity(guidId);
            var expected = new UserEntity {
                Id = userEntity.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = user.Role
            };
            _userServiceRepositoryMock.Setup(u => u.GetUserById(guidId)).ReturnsAsync(userEntity);
            _autoMapperMock.Setup(map => map.Map(user, userEntity)).Returns(expected);

            var actual = ((ObjectResult)_controller.Put(guidId, user).Result);

            Assert.AreEqual(statusCodeExpected, actual.StatusCode);
            _userServiceRepositoryMock.Verify(put => put.Update(It.IsAny<UserEntity>()), Times.Once());
            _userServiceRepositoryMock.Verify(put => put.Update(expected));
        }

        [Test]
        public void Put_Should_return_status_code_400_if_model_state_is_invalid() {
            _controller.ModelState.AddModelError("Error", "Error");
            var statusCodeExpected = (int)HttpStatusCode.BadRequest;
            var guidId = Guid.NewGuid();

            var actual = ((ObjectResult)_controller.Put(guidId, new User()).Result);

            Assert.AreEqual(statusCodeExpected, actual.StatusCode);
        }

        [Test]
        public void Put_Should_return_status_code_404_if_user_dont_exist() {
            var guidId = Guid.NewGuid();
            _userServiceRepositoryMock.Setup(user => user.GetUserById(It.IsAny<Guid>())).ReturnsAsync((UserEntity)null);
            var expected = (int)HttpStatusCode.NotFound;

            var actual = ((ObjectResult)_controller.Put(guidId, new User()).Result).StatusCode;

            Assert.AreEqual(expected, actual);
        }

        [TestCase("")]
        [TestCase(" ")]
        [TestCase(null)]
        public void Put_Should_return_message_field_is_required_if_mail_is_null_or_empy(string email) {
            var expected = "The Email field is required.";
            _controller.ModelState.AddModelError("Email", expected);
            var user = GetUser(email: email);
            var guidId = Guid.NewGuid();
            SetupRepositoryGetUser(guidId);

            var result = ((SerializableError)((ObjectResult)_controller.Put(guidId, user).Result).Value).Values;
            var actual = result.Select(value => (string[])value).FirstOrDefault()[0];

            Assert.AreEqual(expected, actual);
        }

        [TestCase("email@")]
        [TestCase("@email")]
        [TestCase("email.com")]
        public void Put_Should_return_error_message_if_mail_is_invalid(string email) {
            var expected = "The Email field is not a valid e-mail address.";
            _controller.ModelState.AddModelError("Email", expected);
            var user = GetUser(email: email);
            var guidId = Guid.NewGuid();
            SetupRepositoryGetUser(guidId);

            var result = ((SerializableError)((ObjectResult)_controller.Put(guidId, user).Result).Value).Values;
            var actual = result.Select(value => (string[])value).FirstOrDefault()[0];

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Put_Should_return_error_message_if_mail_is_invalid() {
            var expected = "Email must be a maximum 100 characters.";
            _controller.ModelState.AddModelError("Email", expected);
            var user = GetUser(email: "thisisabigemailmostbigofworld@thisisabigemailmostbigofworldthisisabigemailmostbigofworldthisisabigemail");
            var guidId = Guid.NewGuid();
            SetupRepositoryGetUser(guidId);

            var result = ((SerializableError)((ObjectResult)_controller.Put(guidId, user).Result).Value).Values;
            var actual = result.Select(value => (string[])value).FirstOrDefault()[0];

            Assert.AreEqual(expected, actual);
        }

        [TestCase("")]
        [TestCase(" ")]
        [TestCase(null)]
        public void Put_Should_return_message_field_is_required_if_first_name_is_null_or_empy(string firstName) {
            var expected = "The FirstName field is required.";
            _controller.ModelState.AddModelError("FirstName", expected);
            var user = GetUser(firstName: firstName);
            var guidId = Guid.NewGuid();
            SetupRepositoryGetUser(guidId);

            var result = ((SerializableError)((ObjectResult)_controller.Put(guidId, user).Result).Value).Values;
            var actual = result.Select(value => (string[])value).FirstOrDefault()[0];

            Assert.AreEqual(expected, actual);
        }

        [TestCase("st")]
        [TestCase("g")]
        public void Put_Should_return_message_error_if_first_name_is_shorter(string firstName) {
            var expected = "FirstName must be minimum 3 characters";
            _controller.ModelState.AddModelError("FirstName", expected);
            var user = GetUser(firstName: firstName);
            var guidId = Guid.NewGuid();
            SetupRepositoryGetUser(guidId);

            var result = ((SerializableError)((ObjectResult)_controller.Put(guidId, user).Result).Value).Values;
            var actual = result.Select(value => (string[])value).FirstOrDefault()[0];

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Put_Should_return_message_error_if_first_name_is_longer() {
            var expected = "FirstName must be maximum 100 characters";
            _controller.ModelState.AddModelError("FirstName", expected);
            var user = GetUser(firstName: "unmonheahsjkahdaio9yadaodjpaisdadaudoiaosidunmonheahsjkahdaio9yadaodjpaisdadaudoiaosidouqwahskasçpsd");
            var guidId = Guid.NewGuid();
            SetupRepositoryGetUser(guidId);

            var result = ((SerializableError)((ObjectResult)_controller.Put(guidId, user).Result).Value).Values;
            var actual = result.Select(value => (string[])value).FirstOrDefault()[0];

            Assert.AreEqual(expected, actual);
        }

        [TestCase("")]
        [TestCase(" ")]
        [TestCase(null)]
        public void Put_Should_return_message_field_is_required_if_last_name_is_null_or_empy(string lastName) {
            var expected = "The LastName field is required.";
            _controller.ModelState.AddModelError("LastName", expected);
            var user = GetUser(lastName: lastName);
            var guidId = Guid.NewGuid();
            SetupRepositoryGetUser(guidId);

            var result = ((SerializableError)((ObjectResult)_controller.Put(guidId, user).Result).Value).Values;
            var actual = result.Select(value => (string[])value).FirstOrDefault()[0];

            Assert.AreEqual(expected, actual);
        }

        [TestCase("st")]
        [TestCase("g")]
        public void Put_Should_return_message_error_if_last_name_is_shorter(string lastName) {
            var expected = "LastName must be minimum 3 characters.";
            _controller.ModelState.AddModelError("LastName", expected);
            var user = GetUser(lastName: lastName);
            var guidId = Guid.NewGuid();
            SetupRepositoryGetUser(guidId);

            var result = ((SerializableError)((ObjectResult)_controller.Put(guidId, user).Result).Value).Values;
            var actual = result.Select(value => (string[])value).FirstOrDefault()[0];

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Put_Should_return_message_error_if_last_name_is_longer() {
            var expected = "LastName must be maximum 100 characters.";
            _controller.ModelState.AddModelError("LastName", expected);
            var user = GetUser(lastName: "unmonheahsjkahdaio9yadaodjpaisdadaudoiaosidunmonheahsjkahdaio9yadaodjpaisdadaudoiaosidouqwahskasçpsd");
            var guidId = Guid.NewGuid();
            SetupRepositoryGetUser(guidId);

            var result = ((SerializableError)((ObjectResult)_controller.Put(guidId, user).Result).Value).Values;
            var actual = result.Select(value => (string[])value).FirstOrDefault()[0];

            Assert.AreEqual(expected, actual);
        }

        [TestCase(-1)]
        [TestCase(0)]
        [TestCase(null)]
        public void Put_Should_return_message_field_is_required_if_role_is_invalid(Role role) {
            var expected = "The Role field is required.";
            _controller.ModelState.AddModelError("Role", expected);
            var user = GetUser(role: role);
            var guidId = Guid.NewGuid();
            SetupRepositoryGetUser(guidId);

            var result = ((SerializableError)((ObjectResult)_controller.Put(guidId, user).Result).Value).Values;
            var actual = result.Select(value => (string[])value).FirstOrDefault()[0];

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Put_Should_return_message_error_if_cpf_cnpj_is_shorter() {
            var expected = "CpfCnpj must be minimum 11 characters.";
            _controller.ModelState.AddModelError("CpfCnpj", expected);
            var user = GetUser();
            user.CpfCnpj = "1234567891";
            var guidId = Guid.NewGuid();
            SetupRepositoryGetUser(guidId);

            var result = ((SerializableError)((ObjectResult)_controller.Put(guidId, user).Result).Value).Values;
            var actual = result.Select(value => (string[])value).FirstOrDefault()[0];

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Put_Should_return_message_error_if_cpf_cnpj_is_longer() {
            var expected = "CpfCnpj must be maximum 14 characters.";
            _controller.ModelState.AddModelError("CpfCnpj", expected);
            var user = GetUser();
            user.CpfCnpj = "123456789101215";
            var guidId = Guid.NewGuid();
            SetupRepositoryGetUser(guidId);

            var result = ((SerializableError)((ObjectResult)_controller.Put(guidId, user).Result).Value).Values;
            var actual = result.Select(value => (string[])value).FirstOrDefault()[0];

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Put_Should_return_status_code_500_if_error() {
            var guidId = Guid.NewGuid();
            SetupRepositoryGetUser(guidId);
            _userServiceRepositoryMock.Setup(user => user.Update(It.IsAny<UserEntity>())).Throws(new Exception());
            var expected = (int)HttpStatusCode.InternalServerError;

            var actual = ((ObjectResult)_controller.Put(guidId, new User()).Result).StatusCode;

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Delete_Should_return_status_code_200_if_user_removed_with_success() {
            var guidId = Guid.NewGuid(); ;
            var userEntity = GetDefaultUserEntity(guidId);
            _userServiceRepositoryMock.Setup(user => user.GetUserById(guidId)).ReturnsAsync(userEntity);
            _userServiceRepositoryMock.Setup(user => user.Delete(userEntity)).Returns(Task.FromResult(0));
            var expected = (int)HttpStatusCode.OK;

            var actual = ((ObjectResult)_controller.Delete(guidId).Result).StatusCode;

            Assert.AreEqual(expected, actual);
            _userServiceRepositoryMock.Verify(del => del.Delete(It.IsAny<UserEntity>()), Times.Once);
            _userServiceRepositoryMock.Verify(del => del.Delete(userEntity));
        }

        [Test]
        public void Delete_Should_return_status_code_404_if_user_dont_exist() {
            var guidId = Guid.NewGuid();
            _userServiceRepositoryMock.Setup(user => user.GetUserById(It.IsAny<Guid>())).ReturnsAsync((UserEntity)null);
            var expected = (int)HttpStatusCode.NotFound;

            var actual = ((ObjectResult)_controller.Delete(guidId).Result).StatusCode;

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Delete_Should_return_status_code_500_if_error() {
            var guidId = Guid.NewGuid();
            _userServiceRepositoryMock.Setup(user => user.GetUserById(It.IsAny<Guid>())).ReturnsAsync(new UserEntity());
            _userServiceRepositoryMock.Setup(user => user.Delete(It.IsAny<UserEntity>())).Throws(new Exception());
            var expected = (int)HttpStatusCode.InternalServerError;

            var actual = ((ObjectResult)_controller.Delete(guidId).Result).StatusCode;

            Assert.AreEqual(expected, actual);
        }

        private void SetupRepositoryGetUser(Guid guidId) {
            var userEntity = GetDefaultUserEntity(guidId);
            _userServiceRepositoryMock.Setup(user => user.GetUserById(guidId)).ReturnsAsync(userEntity);
        }

        private static User GetUser(string firstName = "first", string lastName = "last", string email = "email@email.com", Role role = Role.User) {
            return new User {
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                Role = role
            };
        }

        private List<User> GetExpectedUsers() {
            return new List<User> {
                new User {
                    FirstName = "Test 1",
                    LastName = "Test 1",
                    Email="test1@test.com",
                    Role = Role.User
                },
                new User {
                    FirstName = "Test 2",
                    LastName = "Test 2",
                    Email="test2@test.com",
                    Role = Role.Guest
                }
            };
        }

        private List<UserEntity> GetUsersEntityMock() {
            return new List<UserEntity> {
                new UserEntity {
                    Id= new Guid("00000000-1111-1234-2222-333333334444"),
                    FirstName = "Test 1",
                    LastName = "Test 1",
                    Email="test1@test.com",
                    Role = Role.User
                },
                new UserEntity {
                    Id= new Guid("00000000-1111-1234-2222-888888887777"),
                    FirstName = "Test 2",
                    LastName = "Test 2",
                    Email="test2@test.com",
                    Role = Role.Guest
                }
            };
        }

        private static UserEntity GetDefaultUserEntity(Guid guid) {
            return new UserEntity {
                Id = guid,
                FirstName = "Firt Name",
                LastName = "Last Name",
                Email = "default@test.com",
                Role = Role.User
            };
        }
    }
}
