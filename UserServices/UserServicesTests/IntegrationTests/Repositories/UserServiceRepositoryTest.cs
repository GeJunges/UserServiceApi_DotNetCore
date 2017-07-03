using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UserServices.Entities;
using UserServices.Repositories;
using UserServicesDotNetCore.Entities;
using UserServicesDotNetCore.Helpers;

namespace UserServicesTests.IntegrationTests.Repositories {
    [TestFixture]
    public class UserServiceRepositoryTest {

        private AppDbContext _context;
        private UserServiceRepository _repository;

        [SetUp]
        public void SetUp() {

            var serviceProvider = new ServiceCollection()
            .AddEntityFrameworkSqlServer()
            .BuildServiceProvider();

            var builder = new DbContextOptionsBuilder<AppDbContext>();
            builder.UseSqlServer($"Server=(localdb)\\mssqllocaldb;Database=UserServiceTest;Trusted_Connection=True;MultipleActiveResultSets=true")
                   .UseInternalServiceProvider(serviceProvider);
            _context = new AppDbContext(builder.Options);
            _context.Database.Migrate();

            _repository = new UserServiceRepository(_context);
        }

        [Test]
        public async Task GetUsers_Should_return_a_list_of_users() {
            var expected = await InsertUsersToTests();

            var actual = await _repository.GetUsers();

            CollectionAssert.AreEquivalent(expected, actual);
        }

        [Test]
        public async Task GetUsers_Should_return_a_empty_list_if_no_users_entered() {
            var actual = await _repository.GetUsers();

            Assert.IsEmpty(actual);
        }

        [Test]
        public async Task GetUser_Should_return_a_correct_users_by_id() {
            var users = await InsertUsersToTests();
            var expected = users.First();

            var actual = await _repository.GetUser(expected.Id);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public async Task GetUser_Should_return_null_if_id_no_match_a_user() {
            await InsertUsersToTests();
            var id = Guid.NewGuid();

            var actual = await _repository.GetUser(id);

            Assert.IsNull(actual);
        }

        [Test]
        public async Task Register_Should_insert_a_user() {
            var expected = CreateUser();

            await _repository.Insert(expected);
            var actual = await _repository.GetUser(expected.Id);

            Assert.AreEqual(expected, actual);
        }

        [TestCaseSource(nameof(GetUserRequiredFields))]
        public void Register_Should_throw_an_Exception_if_required_field_has_no_value(string firstName, string lastName, string email) {
            var user = new UserEntity {
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                Role = Role.User
            }; ;
            var expected = "An error occurred while updating the entries. See the inner exception for details.";

            var actual = Assert.ThrowsAsync<DbUpdateException>(async () => await _repository.Insert(user));

            Assert.AreEqual(expected, actual.Message);
        }

        [TestCaseSource(nameof(GetUserUniqueFields))]
        public async Task Register_Should_throw_an_Exception_if_unique_field_unrespected(string cpfCnpj1, string cpfCnpj2, string email1, string email2) {
            var user1 = CreateUser();
            user1.Email = email1;
            user1.CpfCnpj = cpfCnpj1;
            await _repository.Insert(user1);
            var user2 = CreateUser();
            user2.Email = email2;
            user2.CpfCnpj = cpfCnpj2;
            var expected = "An error occurred while updating the entries. See the inner exception for details.";

            var actual = Assert.ThrowsAsync<DbUpdateException>(async () => await _repository.Insert(user2));

            Assert.AreEqual(expected, actual.Message);
        }

        [Test]
        public async Task Update_Should_update_a_user() {
            var expected = CreateUser();
            await _repository.Insert(expected);
            expected.FirstName = "Edited";
            expected.LastName = "Updated";

            await _repository.Update(expected);
            var actual = await _repository.GetUser(expected.Id);

            Assert.AreEqual(expected, actual);
        }

        [TestCaseSource(nameof(GetUserRequiredFields))]
        public async Task Update_Should_throw_an_Exception_if_required_field_has_no_value(string firstName, string lastName, string email) {
            var user = CreateUser();
            await _repository.Insert(user);
            user.FirstName = firstName;
            user.LastName = lastName;
            user.Email = email;
            var expected = "An error occurred while updating the entries. See the inner exception for details.";

            var actual = Assert.ThrowsAsync<DbUpdateException>(async () => await _repository.Update(user));

            Assert.AreEqual(expected, actual.Message);
        }

        [TestCaseSource(nameof(GetUserUniqueFields))]
        public async Task Update_Should_throw_an_Exception_if_unique_field_unrespected(string cpfCnpj1, string cpfCnpj2, string email1, string email2) {
            var user1 = CreateUser();
            await _repository.Insert(user1);
            var user2 = CreateUser();
            user2.Email = email2;
            user2.CpfCnpj = cpfCnpj2;
            await _repository.Insert(user2);
            user1.Email = email1;
            user1.CpfCnpj = cpfCnpj1;
            var expected = "An error occurred while updating the entries. See the inner exception for details.";

            var actual = Assert.ThrowsAsync<DbUpdateException>(async () => await _repository.Update(user1));

            Assert.AreEqual(expected, actual.Message);
        }

        [Test]
        public async Task Delete_Should_remove_a_user_by_id() {
            var users = InsertUsersToTests();
            var userToRemove = users.Result.Last();

            await _repository.Delete(userToRemove);
            var actual = await _repository.GetUser(userToRemove.Id);

            Assert.IsNull(actual);
        }

        private static List<string[]> GetUserUniqueFields() {
            return new List<string[]> {
                new []{ "36328181426", "36328181426", "email@email.com", "email2@email.com"},
                new []{ "83422428000150", "83422428000150", "email@email.com", "email2@email.com"},
                new []{ "36328181426", "83422428000150", "same@email.com", "same@email.com"},
             };
        }

        private static List<string[]> GetUserRequiredFields() {
            return new List<string[]> {
                new []{null, "Last Name", "email@email.com"},
                new []{"First Name", null, "email@email.com"},
                new []{"First Name", "Last Name", null},
             };
        }

        private static UserEntity CreateUser() {
            return new UserEntity {
                FirstName = "Test 1",
                LastName = "Test 2",
                Email = "email@email.com",
                Role = Role.User
            };
        }

        private async Task<List<UserEntity>> InsertUsersToTests() {
            var user1 =
            new UserEntity {
                FirstName = "User 1",
                LastName = "User 1",
                Email = "test@email.com",
                Role = Role.User
            };
            var user2 =
            new UserEntity {
                FirstName = "User 2",
                LastName = "User 2",
                Email = "test2@email.com",
                Role = Role.Guest
            };

            await _repository.Insert(user1);
            await _repository.Insert(user2);

            return new List<UserEntity> { user1, user2 };
        }

        [TearDown]
        public void TearDown() {
            _context.Database.ExecuteSqlCommand("Truncate table Users");
        }
    }
}
