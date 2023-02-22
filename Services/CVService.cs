using Microsoft.JSInterop;
using System.Text.Json;

namespace Personal_Website;

public interface ICVService
{
    Task<CVSection> GetCVAsync();
    Task SaveCVAsync(IJSRuntime js, CVSection cv);
}

public sealed class JsonCVService : ICVService
{
    private HttpClient HttpClient { get; set; }
    private ILogger<JsonCVService> Logger { get; set; }

    public JsonCVService(HttpClient httpClient, ILogger<JsonCVService> logger)
    {
        HttpClient = httpClient;
        Logger = logger;
    }

    public async Task<CVSection> GetCVAsync()
    {
        try
        {
            var response = await HttpClient.GetAsync($"staticData/{typeof(CVSection).Name}.json");
            response.EnsureSuccessStatusCode();
            var data = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<CVSection>(data, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            if (result == null)
            {
                // Log the error
                Logger.LogError("Error fetching CV data: Result of JsonSerializer.Deserialize was null");
                return new CVSection();
            }
            return result;
        }
        catch (Exception ex)
        {
            // Log the error
            Logger.LogError("Error fetching CV data: " + ex.Message);
            // Return a default CV object or throw an exception, depending on your requirements
            return new CVSection();
        }
    }

    public async Task SaveCVAsync(IJSRuntime js, CVSection cv)
    {
        throw new NotImplementedException();
    }
}

public sealed class FixedCVService : ICVService
{
    private ILogger<JsonCVService> Logger { get; set; }

    public FixedCVService(ILogger<JsonCVService> logger)
    {
        Logger = logger;
    }

    public async Task<CVSection> GetCVAsync()
    {
        // TODO: Build a fixed CV for testing
        var cv = new CVSection();
        cv.Title = "Tobias Burns";
        cv.SubTitle = "Software Engineer";
        // TODO: grab a random image at some point for testing
        cv.ImagePath = "staticData/profile.jpg";
        foreach (string s in new List<string> { "Education", "Work Experience", "Awards and Funding" })
        {
            var cvSection = new CVSection();
            cvSection.Title = s;

            for (int i = 0; i < 3; i++)
            {
                var cvEntry = new CVSection();
                cvEntry.Title = "Susquehanna International Group";
                cvEntry.SubTitle = "Software Engineer | Apr 2020 - Jul 2022";
                cvEntry.Description = "Given complete ownership/control of a high volume desktop trading tool.\r\n" +
                    "Designed, developed and supported business critical code using C# and the .NET framework.\r\n" +
                    "Interviewed and mentored intern software engineers, helping them to develop their skills toward becoming full time hires.";

                cvSection.SubSections.Add(cvEntry);
            }

            cv.SubSections.Add(cvSection);
        }
        return await Task.FromResult(cv);
    }

    public async Task SaveCVAsync(IJSRuntime js, CVSection cv)
    {
        try
        {
            var result = JsonSerializer.SerializeToUtf8Bytes(cv, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                WriteIndented =true,
            });
            if (result == null)
            {
                Logger.LogError("Error saving CV data: Result of JsonSerializer.SerializeToUtf8Bytes was null");
            }

            await js.InvokeAsync<object>(
            "saveAsFile",
                "cv.json",
                Convert.ToBase64String(result));
        }
        catch (Exception ex)
        {
            // Log the error
            Logger.LogError("Error saving CV data: " + ex.Message);
        }
    }
}