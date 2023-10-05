using Microsoft.JSInterop;
using PersonalWebsite.Shared;
using System.Text.Json;

namespace PersonalWebsite.Client.Shared
{
    public static class CVServiceExtensions
    {
        public static async Task SaveCVAsync(this BaseCVService thisCVService, IJSRuntime js, CVSection cv)
        {
            try
            {
                var result = JsonSerializer.SerializeToUtf8Bytes(cv, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    WriteIndented = true,
                });
                if (result == null)
                {
                    thisCVService.Logger.LogError("Error saving CV data: Result of JsonSerializer.SerializeToUtf8Bytes was null");
                    return;
                }

                await js.InvokeAsync<object>(
                "saveAsFile",
                    "cv.json",
                    Convert.ToBase64String(result));
            }
            catch (Exception ex)
            {
                // Log the error
                thisCVService.Logger.LogError(thisCVService.LoggerString, "Error saving CV data: " + ex.Message);
            }
        }
    }
}
