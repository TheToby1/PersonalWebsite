using System.Collections.Generic;

namespace PersonalWebsite.Shared.CV
{
    /// <summary>
    /// Represents a section in a CV which contains entries
    /// This could even be the top level CV
    /// </summary>
    public class CVSection
    {
        public string? Title { get; set; }
        public string? SubTitle { get; set; }
        public string? Description { get; set; }
        public string? ImagePath { get; set; }
        public HashSet<CVSection> SubSections { get; set; } = new HashSet<CVSection>();
    }
}
