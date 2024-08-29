using System.Text.Json;
using Microsoft.JSInterop;

namespace PersonalWebsite.Shared.CV
{
    public static class CVServiceExtensions
    {
        private static readonly JsonSerializerOptions _jsonSerializerOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = true,
        };

        public static async Task SaveCVAsync(this BaseCVService thisCVService, IJSRuntime js, CVSection cv)
        {
            try
            {
                var result = JsonSerializer.SerializeToUtf8Bytes(cv, _jsonSerializerOptions);
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
                thisCVService.Logger.LogError("{LoggerString}:{}", thisCVService.LoggerString, $"Error saving CV data: {ex.Message}");
            }
        }
    }
}
