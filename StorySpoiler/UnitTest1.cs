using NUnit.Framework;
using RestSharp;
using RestSharp.Authenticators;
using StorySpoiler.Models;
using System;
using System.Net;
using System.Reflection;
using System.Text.Json;


namespace StorySpoiler
{
    [TestFixture]
    [NonParallelizable]
    public class StorySpoiler
    {
        RestClient client;
        private static string lastCreatedStoryId;
        private const string baseUrl = "https://d3s5nxhwblsjbi.cloudfront.net";

        [OneTimeSetUp]
        public void Setup()
        {
            string token = getJwtToken("iavorkata11", "1234567");

            var options = new RestClientOptions(baseUrl)
            {
                Authenticator = new JwtAuthenticator(token)
            };

            client = new RestClient(options);
        }

        private string getJwtToken(string username, string password)
        {
            var loginClient = new RestClient(baseUrl);
            var request = new RestRequest("/api/User/Authentication ", Method.Post);

            request.AddJsonBody(new { username, password });

            var response = loginClient.Execute(request);

            var json = JsonSerializer.Deserialize<JsonElement>(response.Content);

            return json.GetProperty("accessToken").GetString();
        }

        //Tests

        [Order(1)]
        [Test]
        public void CreateStoryWtihRequiredFields_ShouldReturnCreated()
        {
            var story = new StoryDTO()
            {
                Title = "Final Created Story",
                Description = "description for the first created story",
                Url = ""
            };

            var request = new RestRequest("/api/Story/Create", Method.Post);
            request.AddJsonBody(story);

            var response = client.Execute(request);

            var json = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);
            lastCreatedStoryId = json.StoryId;

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
            Assert.That(json.Msg, Is.EqualTo("Successfully created!"));
            Assert.That(json.StoryId, Is.Not.Null.Or.Empty);

        }

        [Order(2)]
        [Test]
        public void EditStoryThatYouCreated_ShouldBeSuccessfully()
        {
            
            var storyEdit = new StoryDTO()
            {
                Title = "Final Created Story --- Edited",
                Description = "description for the second created story is edited",
                Url = ""
            };

            var request = new RestRequest($"/api/Story/Edit/{lastCreatedStoryId}", Method.Put);
            request.AddJsonBody(storyEdit);

            var response = client.Execute(request);
            var json = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(json.Msg, Is.EqualTo("Successfully edited"));
        }

        [Order(3)]
        [Test]
        public void GetAllStorySpoilers_ShouldBeSuccessfully()
        {
            var request = new RestRequest("/api/Story/All", Method.Get);
            

            var response = client.Execute(request);
            var json = JsonSerializer.Deserialize<List<ApiResponseDTO>>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(json.Count, Is.GreaterThan(0));
            Assert.That(json, Is.Not.Empty.Or.Null);
            Console.WriteLine($"Number of stories returned: {json.Count}");

        }

        [Order(4)]
        [Test]
        public void DeleteStorySpoiler_ShouldBeDeleted()
        {
            var request = new RestRequest($"/api/Story/Delete/{lastCreatedStoryId}", Method.Delete);


            var response = client.Execute(request);
            var json = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(json.Msg, Is.EqualTo("Deleted successfully!"));
        }

        [Order(5)]
        [Test]
        public void TryToCreateStoryWitohoutRequiredFields_ShouldBeNotCreated()
        {
            var story = new StoryDTO()
            {
                Title = "",
                Description = "",
                Url = ""
            };

            var request = new RestRequest("/api/Story/Create", Method.Post);
            request.AddJsonBody(story);

            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [Order(6)]
        [Test]
        public void EditNonExistingStorySpoiler_ShouldBeNotEdited()
        {
            string fakeId = "23232";

            var storyEdit = new StoryDTO()
            {
                Title = "Third Created Story --- Edited",
                Description = "description for the second created story is edited",
                Url = ""
            };

            var request = new RestRequest($"/api/Story/Edit/{fakeId}", Method.Put);
            request.AddJsonBody(storyEdit);

            var response = client.Execute(request);
            var json = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
            Assert.That(json.Msg, Is.EqualTo("No spoilers..."));
        }

        [Order(7)]
        [Test]
        public void DeletNonExistingStorySpoiler_ShouldBeNNotDeleted()
        {
            string fakeId = "23232";

            var request = new RestRequest($"/api/Story/Delete/{fakeId}", Method.Delete);

            var response = client.Execute(request);
            var json = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(json.Msg, Is.EqualTo("Unable to delete this story spoiler!"));
        }

        [OneTimeTearDown]
        public void TearDown()
        { 
            client?.Dispose();
        }
        
    }
}