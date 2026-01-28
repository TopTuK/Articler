using Articler.AppDomain.Models.Documents;
using Articler.AppDomain.Models.Project;
using Articler.AppDomain.Models.Token;
using Articler.GrainInterfaces.Project;
using Articler.GrainInterfaces.User;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Concurrency;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Articler.GrainClasses.Project
{
    [StatelessWorker]
    public class ProjectOperatorGrain(Logger<ProjectOperatorGrain> logger) : Grain, IProjectOperatorGrain
    {
        private readonly ILogger<ProjectOperatorGrain> _logger = logger;

        public async Task<IProject?> GetProject(string userId, string projectId)
        {
            _logger.LogInformation("ProjectOperatorGrain::GetProject: start get project. " +
                "UserId={userId} ProjectId={projectId}", userId, projectId);

            var userGrain = GrainFactory.GetGrain<IUserGrain>(userId);
            var project = await userGrain.GetProjectById(projectId);

            _logger.LogInformation("ProjectOperatorGrain::GetProject: return project. " +
                "UserId={userId} ProjectId={projectId}", userId, project?.Id);
            return project;
        }

        public Task<ICalculateTokenResult<IDocument>> AddProjectTextDocumentAsync(string userId, string projectId, string title, string text)
        {
            throw new NotImplementedException();
        }

        public Task<ICalculateTokenResult<IDocument>> AddProjectPdfDocumentAsync(string userId, string projectId, string title, string url)
        {
            throw new NotImplementedException();
        }
    }
}
