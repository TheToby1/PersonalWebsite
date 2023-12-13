using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace PersonalWebsite.Shared.CV
{
    public interface ICVService
    {
        Task<CVSection> GetCVAsync();
    }

    public abstract class BaseCVService : ICVService
    {
        // ToDo: This should be protected and the extension service in client should log some other way
        public ILogger Logger { get; set; }
        public readonly string LoggerString = "ICVService: ";

        protected BaseCVService(ILogger logger)
        {
            Logger = logger;
        }

        public abstract Task<CVSection> GetCVAsync();
    }

    public sealed class JsonCVService : BaseCVService
    {
        private HttpClient HttpClient { get; set; }
        private readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions()
        {
            PropertyNameCaseInsensitive = true
        };

        public JsonCVService(HttpClient httpClient, ILogger<JsonCVService> logger) : base(logger)
        {
            HttpClient = httpClient;
        }

        public override async Task<CVSection> GetCVAsync()
        {
            try
            {
                var response = await HttpClient.GetAsync($"staticData/{typeof(CVSection).Name}.json");
                response.EnsureSuccessStatusCode();
                var data = await response.Content.ReadAsStringAsync();

                CVSection? result = JsonSerializer.Deserialize<CVSection>(data, _jsonSerializerOptions);
                if (result == null)
                {
                    // Log the error
                    Logger.LogError("{LoggerString}:{}", LoggerString, "Error fetching CV data: Result of JsonSerializer.Deserialize was null");
                    return new CVSection();
                }
                return result;
            }
            catch (Exception ex)
            {
                // Log the error
                Logger.LogError("{LoggerString}:{}", LoggerString, $"Error fetching CV data: {ex.Message}");
                // Return a default CV object or throw an exception, depending on your requirements
                return new CVSection();
            }
        }
    }

    public sealed class StubCVService : BaseCVService
    {
        public StubCVService(ILogger<StubCVService> logger) : base(logger) { }

        public override async Task<CVSection> GetCVAsync()
        {
            // TODO: Build a fixed CV for testing
            var cv = new CVSection
            {
                Title = "Tobias Burns",
                SubTitle = "Software Engineer",
                // TODO: grab a random image at some point for testing
                ImagePath = "staticData/profile.jpg"
            };
            foreach (string s in new List<string> { "Education", "Work Experience", "Awards and Funding" })
            {
                var cvSection = new CVSection
                {
                    Title = s
                };

                for (int i = 0; i < 3; i++)
                {
                    var cvEntry = new CVSection
                    {
                        Title = "Susquehanna International Group",
                        SubTitle = "Software Engineer | Apr 2020 - Jul 2022",
                        Description = "Given complete ownership/control of a high volume desktop trading tool.\r\n" +
                        "Designed, developed and supported business critical code using C# and the .NET framework.\r\n" +
                        "Interviewed and mentored intern software engineers, helping them to develop their skills toward becoming full time hires."
                    };

                    cvSection.SubSections.Add(cvEntry);
                }

                cv.SubSections.Add(cvSection);
            }
            return await Task.FromResult(cv);
        }
    }
}