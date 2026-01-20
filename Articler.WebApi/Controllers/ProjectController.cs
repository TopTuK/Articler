using Articler.WebApi.Models;
using Articler.WebApi.Services.Project;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using static Articler.AppDomain.Factories.Project.ProjectFactory;

namespace Articler.WebApi.Controllers
{
    [Route("[controller]/[action]")]
    [ApiController]
    public class ProjectController(
            ILogger<ProjectController> logger,
            IProjectService projectService
        ) : ControllerBase
    {
        private readonly ILogger<ProjectController> _logger = logger;
        private readonly IProjectService _projectService = projectService;

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateProjectRequest request)
        {
            var userId = HttpContext.Items["UserId"]!.ToString()!;
            _logger.LogInformation("ProjectController::Create: start create project. " +
                "UserId={userId}, Title={title}", userId, request.Title);

            if (string.IsNullOrWhiteSpace(request.Title))
            {
                _logger.LogWarning("ProjectController::Create: title is empty or null. " +
                    "UserId={userId}", userId);
                return BadRequest("Title is required");
            }

            try
            {
                var project = await _projectService.CreateProjectAsync(
                    userId, 
                    request.Title, 
                    request.Description ?? string.Empty,
                    request.Language);
                _logger.LogInformation("ProjectController::Create: project created successfully. " +
                    "UserId={userId}, ProjectId={projectId}", userId, project.Id);

                return new JsonResult(project);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ProjectController::Create: exception raised. " +
                    "UserId={userId}, Msg={exMsg}", userId, ex.Message);
                return BadRequest("Failed to create project");
            }
        }

        [Authorize]
        public async Task<IActionResult> GetProjects()
        {
            var userId = HttpContext.Items["UserId"]!.ToString()!;
            _logger.LogInformation("ProjectController::GetProjects: start get user projects. " +
                "UserId={userId}", userId);

            try
            {
                var userProjects = await _projectService.GetUserProjectsAsync(userId);
                _logger.LogInformation("ProjectController::GetProjects: return user projects. " +
                    "ProjectsCount={projectsCount}", userProjects.Count());
                return new JsonResult(userProjects);
            }
            catch (Exception ex)
            {
                _logger.LogCritical("ProjectController::GetProjects: exception raised. " +
                    "Msg={exMsg}", ex.Message);
                return BadRequest("Exception raised");
            }
            
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetProject(string projectId)
        {
            var userId = HttpContext.Items["UserId"]!.ToString()!;
            _logger.LogInformation("ProjectController::GetProject: start get user project. " +
                "UserId={userId}, ProjectId={projectId}", userId, projectId);

            try
            {
                var project = await _projectService.GetUserProjectAsync(userId, projectId);

                if (project == null)
                {
                    _logger.LogInformation("ProjectController::GetProject: project is not found. " +
                        "UserId={userId}, ProjectId={projectId}", userId, projectId);
                    return NotFound();
                }

                _logger.LogInformation("ProjectController::GetProject: return user project. " +
                    "UserId={userId}, ProjectId={projectId}, ProjectTitle={projectTitle}, State={projectState}",
                    userId, project.Id, project.Title, project.State);
                return new JsonResult(project);
            }
            catch (Exception ex)
            {
                _logger.LogCritical("ProjectController::GetProject: exception raised. " +
                    "Msg={exMsg}", ex.Message);
                return BadRequest("Exception raised");
            }
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> DeleteProject([FromBody] DeleteProjectRequest request)
        {
            var userId = HttpContext.Items["UserId"]!.ToString()!;
            _logger.LogInformation("ProjectController::DeleteProject: start delete user project. " +
                "UserId={userId}, Request=[{request}]", userId, request);

            try
            {
                var project = await _projectService.DeleteUserProjectAsync(userId, request.ProjectId);

                if (project == null)
                {
                    _logger.LogWarning("ProjectController::DeleteProject: user project is not found. " +
                        "UserId={userId}, Request=[{request}]", userId, request);
                    return NotFound();
                }

                _logger.LogInformation("ProjectController::DeleteProject: successfully deleted user project. " +
                    "UserId={userId}, ProjectId={projectId}, ProjetTitle={projectTitle}",
                    userId, project.Id, project.Title);
                return new JsonResult(project);
            }
            catch (Exception ex)
            {
                _logger.LogCritical("ProjectController::DeleteProject: exception raised. " +
                    "UserId={userId}, Request=[{request}] Message={exMessage}", userId, request, ex.Message);
                return BadRequest("Exception raised");
            }
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetProjectText(string projectId)
        {
            var userId = HttpContext.Items["UserId"]!.ToString()!;
            _logger.LogInformation("ProjectController::GetProjectText: start get user project. " +
                "UserId={userId}, ProjectId={projectId}", userId, projectId);

            try
            {
                var projectText = await _projectService.GetProjectTextAsync(userId, projectId);

                if (projectText == null)
                {
                    _logger.LogError("ProjectController::GetProjectText: project text is null. " +
                        "UserId={userId}, ProjectId={projectId}", userId, projectId);
                    return NotFound();
                }

                _logger.LogInformation("ProjectController::GetProjectText: return project text. " +
                    "UserId={userId} ProjectId={projectId} Textlength={textLength}", userId, projectId, projectText.Text.Length);
                return new JsonResult(projectText);
            }
            catch (Exception ex)
            {
                _logger.LogCritical("ProjectController::GetProject: exception raised. " +
                    "UserId={userId} Msg={exMsg}", userId, ex.Message);
                return BadRequest("Exception raised");
            }
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> UpdateProjectText([FromBody] UpdateProjectTextRequest request)
        {
            var userId = HttpContext.Items["UserId"]!.ToString()!;
            _logger.LogInformation("ProjectController::UpdateProjectText: start update project text. " +
                "UserId={userId}, Request={projectId}", userId, request);

            try
            {
                if (string.IsNullOrEmpty(request.ProjectId))
                {
                    _logger.LogCritical("ProjectController::UpdateProjectText: ProjectId is Empty. " +
                        "UserId={userId}", userId);
                    return BadRequest("ProjectId is Empty");
                }

                var projectText = await _projectService.SetProjectTextAsync(userId,
                    request.ProjectId, request.Text);
                if (projectText == null)
                {
                    _logger.LogError("ProjectController::UpdateProjectText: new project text is null. " +
                        "UserId={userId}, ProjectId={projectId}", userId, request.ProjectId);
                    return BadRequest("New project text is null.");
                }

                _logger.LogInformation("ProjectController::UpdateProjectText: saved new project text. " +
                    "UserId={userId}, ProjectId={projectId}", userId, request.ProjectId);
                return new JsonResult(projectText);
            }
            catch (Exception ex)
            {
                _logger.LogCritical("ProjectController::UpdateProjectText: exception raised. " +
                    "UserId={userId} Msg={exMsg}", userId, ex.Message);
                return BadRequest("Exception raised");
            }
            
        }
    }
}
