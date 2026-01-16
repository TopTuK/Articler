using Articler.AppDomain.Models.Project;
using Articler.GrainInterfaces.Project;
using Articler.GrainInterfaces.User;
using System.Security.Cryptography;

namespace Articler.WebApi.Services.Project
{
    public class ProjectService(
        ILogger<ProjectService> logger,
        IClusterClient clusterClient
        ) : IProjectService
    {
        private readonly ILogger<ProjectService> _logger = logger;
        private readonly IClusterClient _clusterClient = clusterClient;

        public async Task<IProject> CreateProjectAsync(string userId, string title, string description)
        {
            _logger.LogInformation("ProjectService::CreateProjectAsync: start create user project. " +
                "UserId={userId}, Title={title}, Description={description}",
                userId, title, description);

            try
            {
                var userGrain = _clusterClient.GetGrain<IUserGrain>(userId);
                var project = await userGrain.CreateProject(title, description);

                _logger.LogInformation("ProjectService::CreateProjectAsync: create user project. " +
                    "UserId={userId}, ProjectId={projectId} ProjectTitle={projectTitle}, ProjectState={projectState}",
                    userId, project.Id, project.Title, project.State);
                return project;
            }
            catch (Exception ex)
            {
                _logger.LogCritical("ProjectService::CreateProjectAsync: exception raised. Msg={exMsg}",
                    ex.Message);
                throw;
            }
        }

        public async Task<IEnumerable<IProject>> GetUserProjectsAsync(string userId)
        {
            _logger.LogInformation("ProjectService::GetUserProjectsAsync: start get projects for user. " +
                "UserId={userId}", userId);

            var userGrain = _clusterClient.GetGrain<IUserGrain>(userId);
            var projects = await userGrain.GetProjects();

            _logger.LogInformation("ProjectService::GetUserProjectsAsync: return user proejects. " +
                "UserId={userId} ProjectsCount={projectsCount}", userId, projects.Count());
            return projects;
        }

        public async Task<IProject?> GetUserProjectAsync(string userId, string projectId)
        {
            _logger.LogInformation("ProjectService::GetUserProjectAsync: start get user project. " +
                "UserId={userId} ProjectId={projectId}", userId, projectId);

            try
            {
                var userGrain = _clusterClient.GetGrain<IUserGrain>(userId);
                var project = await userGrain.GetProjectById(projectId);

                if (project != null)
                {
                    _logger.LogInformation("ProjectService::GetUserProjectAsync: return user project. " +
                        "UserId={userId} ProjectId={projectId} ProjectTitle={projectTitle} ProjectState={projectState}",
                        userId, project.Id, project.Title, project.State);
                }
                else
                {
                    _logger.LogWarning("ProjectService::GetUserProjectAsync:");
                }

                return project;
            }
            catch (Exception ex)
            {
                _logger.LogError("ProjectService::GetUserProjectAsync: exception raised. Message: {exMsg}",
                    ex.Message);
                return null;
            }
        }

        public async Task<IProjectText?> GetProjectTextAsync(string userId, string projectId)
        {
            _logger.LogInformation("ProjectService::GetUserProjectAsync: start get user project. " +
                "UserId={userId} ProjectId={projectId}", userId, projectId);

            try
            {
                if (Guid.TryParse(projectId, out var grainId))
                {
                    var projectGrain = _clusterClient.GetGrain<IProjectGrain>(grainId, userId, null);
                    var projectText = await projectGrain.GetProjectText();

                    _logger.LogInformation("ProjectService::GetProjectTextAsync: got project text. " +
                        "UserId={userId} ProjectId={projectId} Textlength={textLength}",
                        userId, projectId, projectText.Text.Length);
                    return projectText;
                }

                _logger.LogError("ProjectService::GetProjectTextAsync: can\'t parse projectId. " +
                    "UserId={userId} ProjectId={projectId}", userId, projectId);
                return null;

            }
            catch (Exception ex)
            {
                _logger.LogCritical("ProjectService::GetProjectTextAsync: exception raised. " +
                    "UserId={userId} Message={exMsg}", userId, ex.Message);
                return null;
            }
        }

        public async Task<IProjectText?> SetProjectTextAsync(string userId, string projectId, string text)
        {
            _logger.LogInformation("ProjectService::SetProjectTextAsync: start get user project. " +
                "UserId={userId} ProjectId={projectId} TextLength={textLength}", userId, projectId, text.Length);

            try
            {
                if (Guid.TryParse(projectId, out var grainId))
                {
                    var projectGrain = _clusterClient.GetGrain<IProjectGrain>(grainId, userId, null);
                    var projectText = await projectGrain.SetProjectText(text);

                    _logger.LogInformation("ProjectService::SetProjectTextAsync: set new project text. " +
                        "UserId={userId} ProjectId={projectId} Textlength={textLength}",
                        userId, projectId, projectText.Text.Length);
                    return projectText;
                }

                _logger.LogError("ProjectService::SetProjectTextAsync: can\'t parse projectId. " +
                    "UserId={userId} ProjectId={projectId}", userId, projectId);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogCritical("ProjectService::SetProjectTextAsync: exception raised. " +
                    "UserId={userId} Message={exMsg}", userId, ex.Message);
                return null;
            }
        }

        public async Task<IProject?> DeleteUserProjectAsync(string userId, string projectId)
        {
            _logger.LogInformation("ProjectService::DeleteUserProjectAsync: start delete user project. " +
                "UserId={userId} ProjectId={projectId}", userId, projectId);

            try
            {
                var userGrain = _clusterClient.GetGrain<IUserGrain>(userId);
                var project = await userGrain.GetProjectById(projectId);

                if (project == null)
                {
                    _logger.LogError("ProjectService::DeleteUserProjectAsync: user project is not found. " +
                        "UserId={userId} ProjectId={projectId}", userId, projectId);
                    return null;
                }

                var removedProject = await userGrain.RemoveProject(project.Id);
                _logger.LogInformation("ProjectService::DeleteUserProjectAsync: successfully removed user project. " +
                    "UserId={userId} ProjectId={projectId} ProjectTitle={projectTitle}",
                    userId, removedProject.Id, removedProject.Title);
                return removedProject;
            }
            catch (Exception ex)
            {
                _logger.LogCritical("ProjectService::DeleteUserProjectAsync: exception raised. " +
                    "UserId={userId} ProjectId={projectId}. Message={exMsg}", userId, projectId, ex.Message);
                throw;
            }
        }
    }
}
