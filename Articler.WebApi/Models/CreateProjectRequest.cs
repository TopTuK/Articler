using Articler.AppDomain.Models.Project;

namespace Articler.WebApi.Models
{
    public class CreateProjectRequest
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public ProjectLanguage Language { get; set; } = ProjectLanguage.English;

        public override string ToString()
        {
            return $"Title={Title} Description={Description} Language={Language}";
        }
    }
}

