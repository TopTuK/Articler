using Articler.AppDomain.Models.Project;

namespace Articler.WebApi.Services.Project
{
    public interface IProjectService
    {
        Task<IEnumerable<IProject>> GetUserProjectsAsync(string userId);
        Task<IProject> CreateProjectAsync(string userId, string title, string description, ProjectLanguage language);
        Task<IProject?> GetUserProjectAsync(string userId, string projectId);
        Task<IProject?> DeleteUserProjectAsync(string userId, string projectId);

        Task<IProjectText?> GetProjectTextAsync(string userId, string projectId);
        Task<IProjectText?> SetProjectTextAsync(string userId, string projectId, string text);
    }
}
