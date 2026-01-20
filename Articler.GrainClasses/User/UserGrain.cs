using Articler.AppDomain.Constants;
using Articler.AppDomain.Factories.Auth;
using Articler.AppDomain.Models.Auth;
using Articler.AppDomain.Models.Project;
using Articler.GrainInterfaces.Project;
using Articler.GrainInterfaces.User;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Logging;

namespace Articler.GrainClasses.User
{
    public class UserGrain(
        ILogger<UserGrain> logger,
        [PersistentState(OrleansConstants.UserStateName, OrleansConstants.AdoStorageProviderName)]
            IPersistentState<UserGrainState> userState,
        [PersistentState(OrleansConstants.UserProjectsStateName, OrleansConstants.AdoStorageProviderName)]
            IPersistentState<UserProjectsGrainState> projectsState
        ) : Grain, IUserGrain
    {
        private readonly ILogger<UserGrain> _logger = logger;

        private readonly IPersistentState<UserGrainState> _userState = userState;
        private readonly IPersistentState<UserProjectsGrainState> _projectsState = projectsState;

        private async Task<IUserProfile> CreateNewUser(string email, string firstName, string lastName)
        {
            _logger.LogInformation("UserGrain::CreateNewUser: create new user. " +
                "Email={email}, FirstName={firstName}, LastName={lastName}",
                email, firstName, lastName);

            _userState.State.Status = UserProfileStatus.Known;

            _userState.State.UserEmail = email;
            _userState.State.FirstName = firstName;
            _userState.State.LastName = lastName;

            _userState.State.AccountType = AccountType.Trial;
            _userState.State.CreatedDate = DateTime.UtcNow;

            await _userState.WriteStateAsync();

            return UserProfileFactory.CreateUserProfile(email, firstName, lastName, AccountType.Trial);
        }

        public async Task<IUserProfile> Authenticate(
            string email,
            string firstName, string lastName)
        {
            var grainId = this.GetPrimaryKeyString();
            _logger.LogInformation("UserGrain::Authenticate: start authenticate user. " +
                "GrainId={grainId} Email={email}, FirstName={firstName}, LastName={lastName}",
                grainId, email, firstName, lastName);

            var user = (_userState.State.Status) switch
            {
                UserProfileStatus.Unknown => await CreateNewUser(email, firstName, lastName),
                UserProfileStatus.Known => UserProfileFactory.CreateUserProfile(
                    _userState.State.UserEmail,
                    _userState.State.FirstName,
                    _userState.State.LastName,
                    _userState.State.AccountType,
                    _userState.State.CreatedDate
                ),
                _ => throw new NotImplementedException(),
            };

            _logger.LogInformation("UserGrain::Authenticate: return user. " +
                "Email={email}, FirstName={firstName}, AccountType={accountType}",
                user.Email, user.FirstName, user.AccountType);

            return user;
        }

        public Task<IUserProfile> GetUser()
        {
            var grainId = this.GetPrimaryKeyString();
            _logger.LogInformation("UserGrain::GetUser: start get user. GrainId={grainId}", grainId);

            var user = UserProfileFactory.CreateUserProfile(
                _userState.State.UserEmail,
                _userState.State.FirstName,
                _userState.State.LastName,
                _userState.State.AccountType,
                _userState.State.CreatedDate
            );

            _logger.LogInformation("UserGrain::GetUser: return user. " +
                "GrainId={grainId}, Email={email}, FirstName={firstName}, AccountType={accountType}",
                grainId, user.Email, user.FirstName, user.AccountType);
            return Task.FromResult(user);
        }

        public async Task<IProject> CreateProject(string title, string description, ProjectLanguage language)
        {
            var grainId = this.GetPrimaryKeyString();
            _logger.LogInformation("UserGrain::AddProject: start create new project. " +
                "GrainId={grainId}, Title={title}", grainId, title);

            var projectId = Guid.NewGuid();
            var projectGrain = GrainFactory.GetGrain<IProjectGrain>(projectId, grainId, null);
            var project = await projectGrain.Create(title, description, language);

            _logger.LogInformation("UserGrain::AddProject: created new project. " +
                "ProjectId={projectId}, State={state}, Title={title}",
                project.Id, project.State, project.Title);

            _projectsState.State
                .ProjectIds
                .Add(projectId);
            await _projectsState.WriteStateAsync();
            _logger.LogInformation("UserGrain::AddProject: saved projectId to state. " +
                "GrainId={grainId}, ProjectId={projectId}", grainId, project.Id);

            return project;
        }

        public async Task<IProject> RemoveProject(Guid projectId)
        {
            var grainId = this.GetPrimaryKeyString();
            _logger.LogInformation("UserGrain::RemoveProject: start remove user project. " +
                "GrainId={grainId}, ProjectId={projectId}", grainId, projectId);

            try
            {
                var projectGrain = GrainFactory.GetGrain<IProjectGrain>(projectId, grainId);
                var project = await projectGrain.Remove();
                _logger.LogInformation("UserGrain::RemoveProject: removed project grain. " +
                    "GrainId={grainId} ProjectId={projectId} ProjectTitle={projectTitle}",
                    grainId, project.Id, project.Title);

                _projectsState.State
                    .ProjectIds
                    .Remove(projectId);
                await _projectsState.WriteStateAsync();

                _logger.LogInformation("UserGrain::RemoveProject: successfully removed project. " +
                    "GrainId={grainId} ProjectId={projectId} ProjectTitle={projectTitle}",
                    grainId, project.Id, project.Title);
                return project;
            }
            catch (Exception ex)
            {
                _logger.LogCritical("UserGrain::RemoveProject: exception raised. " +
                    "GrainId={grainId}, ProjectId={projectId}. Message: {exMessage}", 
                    grainId, projectId, ex.Message);
                throw;
            }
        }

        public async Task<IEnumerable<IProject>> GetProjects()
        {
            var grainId = this.GetPrimaryKeyString();
            _logger.LogInformation("UserGrain::GetProjects: start get user projects. " +
                "GrainId={grainId}", grainId);
            
            var projectIds = _projectsState.State.ProjectIds;
            if (projectIds.Any())
            {
                _logger.LogInformation("UserGrain::GetProjects: start get {projectCount} projects. " +
                    "GrainId={grainId}", projectIds.Count, grainId);
                
                var tasks = projectIds
                    .Select(pId => GrainFactory.GetGrain<IProjectGrain>(pId, grainId, null).Get());
                var projects = await Task.WhenAll(tasks);

                _logger.LogInformation("UserGrain::GetProjects: got {projectCount} projects. " +
                    "GrainId={grainId}", projects.Count(), grainId);
                return projects;
            }
            else
            {
                _logger.LogInformation("UserGrain::GetProjects: user project list is empty. " +
                    "GrainId={grainId}", grainId);
                return [];
            }
        }

        public async Task<IProject?> GetProjectById(string projectId)
        {
            var grainId = this.GetPrimaryKeyString();
            _logger.LogInformation("UserGrain::GetProjectById: start get project meta by Id. " +
                "GrainId={grainId}, ProjectId={projectId}", grainId, projectId);

            if(!Guid.TryParse(projectId, out var prjId))
            {
                _logger.LogError("UserGrain::GetProjectById: can\'t parse project id. " +
                    "GrainId={grainId} ProjectId={projectId}", grainId, projectId);
                return null;
            }

            if (_projectsState.State.ProjectIds.Contains(prjId))
            {
                var projectGrain = GrainFactory.GetGrain<IProjectGrain>(prjId, grainId, null);
                var project = await projectGrain.Get();

                _logger.LogInformation("UserGrain::GetProjectById: got user project. " +
                    "GrainId={grainId} ProjectId={projectId} ProjectTitle={projectTitle} ProjectState={projectState}",
                    grainId, project.Id, project.Title, project.State);
                return project;
            }

            _logger.LogInformation("UserGrain::GetProjectById: project is not found. " +
                "GrainId={grainId}, ProjectId={projectId}", grainId, projectId);
            return null;
        }

        public async Task<IUserProfile> UpdateUser(string firstName, string lastName)
        {
            var grainId = this.GetPrimaryKeyString();
            _logger.LogInformation("UserGrain::UpdateUser: start update user info. " +
                "GrainId={grainId}, FirstName={firstName}, LastName={lastName}", grainId, firstName, lastName);

            if (_userState.State.Status == UserProfileStatus.Unknown)
            {
                _logger.LogCritical("UserGrain::UpdateUser: user is not found. " +
                    "GrainId={grainId}", grainId);
                throw new Exception("User is not found");
            }

            if (string.IsNullOrEmpty(firstName))
            {
                _logger.LogCritical("UserGrain::UpdateUser: user first name is empty. " +
                    "GrainId={grainId} FirstName={firstName}", grainId, firstName);
                throw new Exception("First name is empty");
            }

            _userState.State.FirstName = firstName;
            _userState.State.LastName = lastName;
            await _userState.WriteStateAsync();

            var user = UserProfileFactory.CreateUserProfile(
                _userState.State.UserEmail,
                _userState.State.FirstName,
                _userState.State.LastName,
                _userState.State.AccountType,
                _userState.State.CreatedDate
            );

            _logger.LogInformation("UserGrain::UpdateUser: return user. " +
                "GrainId={grainId}, Email={email}, FirstName={firstName}, AccountType={accountType}",
                grainId, user.Email, user.FirstName, user.AccountType);
            return user;
        }
    }
}
