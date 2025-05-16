using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using GoRestTest.Models;
using Newtonsoft.Json;
using NUnit.Framework;

namespace GoRestTest.Tests
{
    [TestFixture]
    public class UsersTests : TestBase
    {
        private const string UsersEndpoint = "public/v2/users";

        [Test]
        public async Task GetAllUsers_ShouldReturnList()
        {
            // Act
            var response = await Client.GetAsync(UsersEndpoint);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            string responseData = await response.Content.ReadAsStringAsync();
            var users = JsonConvert.DeserializeObject<List<UserResponse>>(responseData);
            users.Should().NotBeNull();
            users.Count.Should().BeGreaterThan(0);
        }

        [Test]
        public async Task CreateUser_ShouldReturn201Created()
        {
            // Arrange
            var newUser = new
            {
                name = "Test User",
                gender = "male",
                email = $"test.user.{Guid.NewGuid()}@example.com", // Динамичен email
                status = "active"
            };
            var json = JsonConvert.SerializeObject(newUser);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await Client.PostAsync(UsersEndpoint, content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            string responseData = await response.Content.ReadAsStringAsync();
            var createdUser = JsonConvert.DeserializeObject<UserResponse>(responseData);
            createdUser.Should().NotBeNull();
            createdUser.Email.Should().Be(newUser.email);
        }

        [Test]
        public async Task GetUserById_ShouldReturnUser()
        {
            // Arrange: Създаваме нов потребител
            var newUser = new
            {
                name = "Test User",
                gender = "female",
                email = $"test.user.{Guid.NewGuid()}@example.com",
                status = "active"
            };
            var json = JsonConvert.SerializeObject(newUser);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var createResponse = await Client.PostAsync(UsersEndpoint, content);
            createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
            string createResponseData = await createResponse.Content.ReadAsStringAsync();
            var createdUser = JsonConvert.DeserializeObject<UserResponse>(createResponseData);
            createdUser.Should().NotBeNull();

            // Act: Вземаме създадения потребител по id
            var response = await Client.GetAsync($"{UsersEndpoint}/{createdUser.Id}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            string responseData = await response.Content.ReadAsStringAsync();
            var user = JsonConvert.DeserializeObject<UserResponse>(responseData);
            user.Should().NotBeNull();
            user.Id.Should().Be(createdUser.Id);
        }

        [Test]
        public async Task UpdateUser_ShouldReturnUpdatedUser()
        {
            // Arrange: Създаваме потребител
            var newUser = new
            {
                name = "User to Update",
                gender = "male",
                email = $"update.user.{Guid.NewGuid()}@example.com",
                status = "active"
            };
            var jsonCreate = JsonConvert.SerializeObject(newUser);
            var createContent = new StringContent(jsonCreate, Encoding.UTF8, "application/json");
            var createResponse = await Client.PostAsync(UsersEndpoint, createContent);
            createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
            string createResponseData = await createResponse.Content.ReadAsStringAsync();
            var createdUser = JsonConvert.DeserializeObject<UserResponse>(createResponseData);
            createdUser.Should().NotBeNull();

            // Act: Актуализираме потребителя (PATCH)
            var updateData = new
            {
                name = "Updated Name"
            };
            var jsonUpdate = JsonConvert.SerializeObject(updateData);
            var updateContent = new StringContent(jsonUpdate, Encoding.UTF8, "application/json");

            // HttpClient няма вградена PATCH, използваме SendAsync
            var request = new HttpRequestMessage(new HttpMethod("PATCH"), $"{UsersEndpoint}/{createdUser.Id}")
            {
                Content = updateContent
            };
            var updateResponse = await Client.SendAsync(request);
            updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            // Assert: Проверяваме обновените данни
            var getResponse = await Client.GetAsync($"{UsersEndpoint}/{createdUser.Id}");
            getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            string getResponseData = await getResponse.Content.ReadAsStringAsync();
            var updatedUser = JsonConvert.DeserializeObject<UserResponse>(getResponseData);
            updatedUser.Should().NotBeNull();
            updatedUser.Name.Should().Be(updateData.name);
        }

        [Test]
        public async Task DeleteUser_ShouldRemoveUser()
        {
            // Arrange: Създаваме потребител за изтриване
            var newUser = new
            {
                name = "User to Delete",
                gender = "female",
                email = $"delete.user.{Guid.NewGuid()}@example.com",
                status = "active"
            };
            var jsonCreate = JsonConvert.SerializeObject(newUser);
            var createContent = new StringContent(jsonCreate, Encoding.UTF8, "application/json");
            var createResponse = await Client.PostAsync(UsersEndpoint, createContent);
            createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
            string createResponseData = await createResponse.Content.ReadAsStringAsync();
            var createdUser = JsonConvert.DeserializeObject<UserResponse>(createResponseData);
            createdUser.Should().NotBeNull();

            // Act: Изтриваме потребителя
            var deleteResponse = await Client.DeleteAsync($"{UsersEndpoint}/{createdUser.Id}");
            deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

            // Assert: Опитът за вземане на изтрит потребител трябва да върне 404
            var getResponse = await Client.GetAsync($"{UsersEndpoint}/{createdUser.Id}");
            getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
    }
}
