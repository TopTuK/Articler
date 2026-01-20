using Articler.AppDomain.Constants;
using Articler.AppDomain.Factories.Project;
using Articler.AppDomain.Models.Chat;
using Articler.AppDomain.Models.Documents;
using Articler.AppDomain.Models.Project;
using Articler.GrainInterfaces.Chat;
using Articler.GrainInterfaces.Document;
using Articler.GrainInterfaces.Project;
using Microsoft.Extensions.Logging;
using Orleans.Runtime;
using static System.Net.Mime.MediaTypeNames;

namespace Articler.GrainClasses.Project
{
    public class ProjectGrain(
        ILogger<ProjectGrain> logger,
        [PersistentState(OrleansConstants.ProjectStateName, OrleansConstants.AdoStorageProviderName)]
            IPersistentState<ProjectGrainState> projectState,
        [PersistentState(OrleansConstants.ProjectTextStateName, OrleansConstants.AdoStorageProviderName)]
            IPersistentState<ProjectTextGrainState> projectTextState,
        [PersistentState(OrleansConstants.ProjectDocumentStateName, OrleansConstants.AdoStorageProviderName)]
            IPersistentState<ProjectDocumentGrainState> projectDocumentState
        ) : Grain, IProjectGrain
    {
        private readonly ILogger<ProjectGrain> _logger = logger;

        private readonly IPersistentState<ProjectGrainState> _projectState = projectState;
        private readonly IPersistentState<ProjectTextGrainState> _projectTextState = projectTextState;
        private readonly IPersistentState<ProjectDocumentGrainState> _projectDocumentState = projectDocumentState;

        public async Task<IProject> Create(string title, string description)
        {
            var grainId = this.GetPrimaryKey(out var userId);
            _logger.LogInformation("ProjectGrain::Create: start create project. " +
                "GrainId={grainId}, Title={title}", grainId, title);

            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogError("ProjectGrain::Create: userId is null or empty. " +
                    "GrainId={grainId}", grainId);
                throw new ArgumentNullException(nameof(userId));
            }

            if (_projectState.State.Status != ProjectGrainStatus.Unknown)
            {
                _logger.LogError("ProjectGrain::Create: project already has been created. " +
                    "GrainId={grainId}", grainId);
                throw new InvalidOperationException("Project already has been created");
            }

            var chatGrain = GrainFactory.GetGrain<IChatAgentGrain>(grainId, userId);
            await chatGrain.CreateChatAgent(title, description);
            _logger.LogInformation("ProjectGrain::Create: created chat agent. " +
                "GrainId={grainId}", grainId);

            _projectState.State.Status = ProjectGrainStatus.InProgress;
            _projectState.State.ProjectId = grainId;
            _projectState.State.ProjetState = ProjectState.Unpublished;
            _projectState.State.Title = title;
            _projectState.State.Description = description;
            _projectState.State.CreatedDate = DateTime.UtcNow;
            await _projectState.WriteStateAsync();

            var project = ProjectFactory.CreateProject(
                _projectState.State.ProjectId,
                _projectState.State.ProjetState,
                _projectState.State.Title,
                _projectState.State.Description,
                _projectState.State.CreatedDate
            );
            _logger.LogInformation("ProjectGrain::Create: return new project. " +
                "ProjectId={projectId}, Title={title}, CreatedDate={createdDate}",
                project.Id, project.Title, project.CreatedDate
            );

            return project;
        }

        public async Task<IProject> Get()
        {
            var grainId = this.GetPrimaryKey(out var userId);
            _logger.LogInformation("ProjectGrain::GetProject: start get project. " +
                "GrainId={grainId}", grainId);

            await CheckProjectState(userId);

            var project = ProjectFactory.CreateProject(
                _projectState.State.ProjectId,
                _projectState.State.ProjetState,
                _projectState.State.Title,
                _projectState.State.Description,
                _projectState.State.CreatedDate
            );

            _logger.LogInformation("ProjectGrain::GetProject: return project. " +
                "GrainId={grainId},ProjectId={projectId}, Title={title}, CreatedDate={createdDate}",
                grainId, project.Id, project.Title, project.CreatedDate
            );
            return project;
        }

        private async Task CheckProjectState(string? userId)
        {
            if (userId == null)
            {
                _logger.LogError("ProjectGrain::CheckProjectState: userId is null");
                throw new InvalidOperationException("UserId is null");
            }

            if (_projectState.State.Status == ProjectGrainStatus.Unknown)
            {
                _logger.LogError("ProjectGrain::CheckProjectState: project state is invalid. " +
                    "State={projectState}", _projectState.State.Status);

                await _projectState.ClearStateAsync();
                await _projectTextState.ClearStateAsync();

                throw new InvalidOperationException("Project is not created");
            }
        }

        public async Task<IProjectText> GetProjectText()
        {
            var grainId = this.GetPrimaryKey(out var userId);

            await CheckProjectState(userId);

            var text = _projectTextState.State.Text;
            _logger.LogInformation("ProjectGrain::GetProjectText: return project text. " +
                "GrainId={grainId}, TextCount={textCount}", grainId, text.Length);

            return ProjectTextFactory.CreateProjectText(text);
        }

        public async Task<IProjectText> SetProjectText(string text)
        {
            var grainId = this.GetPrimaryKey(out var userId);

            await CheckProjectState(userId);

            var currentText = _projectTextState.State.Text;
            _projectTextState.State.Text = text;
            await _projectTextState.WriteStateAsync();

            _logger.LogInformation("ProjectGrain::SetProjectText: update project text. " +
                "GrainId={grainId} Old TextLength={oldTextLength} Current TextLength={newTextLength}",
                grainId, currentText.Length, _projectTextState.State.Text.Length);
            return ProjectTextFactory.CreateProjectText(_projectTextState.State.Text);
        }

        public async Task<IEnumerable<IDocument>> GetDocuments()
        {
            var grainId = this.GetPrimaryKey(out var userId);
            _logger.LogInformation("ProjectGrain::GetDocuments: start get documents. " +
                "GrainId={grainId}, UserId={userId}",
                grainId, userId);

            await CheckProjectState(userId);

            var documents = _projectDocumentState.State.Documents;
            _logger.LogInformation("ProjectGrain::GetDocuments: return project documents. " +
                "GrainId={grainId}, UserId={userId}, DocumentCount={documentCount}",
                grainId, userId, documents.Count);
            return documents;
        }

        public async Task<IDocument?> RemoveDocument(Guid documentId)
        {
            var grainId = this.GetPrimaryKey(out var userId);
            _logger.LogInformation("ProjectGrain::RemoveDocument: start remove document. " +
                "GrainId={grainId}, UserId={userId}, DocumentId={documentId}",
                grainId, userId, documentId);

            await CheckProjectState(userId);

            var document = _projectDocumentState.State
                .Documents
                .FirstOrDefault(d => d.Id == documentId);
            if (document == null)
            {
                _logger.LogError("ProjectGrain::RemoveDocument: can\'t find project document. " +
                    "GrainId={grainId}, UserId={userId}, DocumentId={documentId}",
                    grainId, userId, documentId);
                return null;
            }

            try
            {
                var documentGrain = GrainFactory.GetGrain<IDocumentStorageGrain>(grainId, userId!);
                var removedDocument = await documentGrain.RemoveDocument(document);

                _projectDocumentState.State.Documents.Remove(document);
                await _projectDocumentState.WriteStateAsync();

                _logger.LogInformation("ProjectGrain::RemoveDocument: successfully removed document. " +
                    "GrainId={grainId}, UserId={userId}, DocumentId={documentId} DocumentTitle={documentTitle}",
                    grainId, userId, removedDocument.Id, removedDocument.Title);
                return removedDocument;
            }
            catch (Exception ex)
            {
                _logger.LogCritical("ProjectGrain::RemoveDocument: exception raised. " +
                    "GrainId={grainId}, UserId={userId}, DocumentId={documentId} Message: {exMessage}",
                    grainId, userId, documentId, ex.Message);
                throw;
            }
        }

        public async Task<IDocument> AddTextDocument(string title, string text)
        {
            var grainId = this.GetPrimaryKey(out var userId);
            _logger.LogInformation("ProjectGrain::AddTextDocument: add text data source to project. " +
                "GrainId={grainId}, UserId={userId} Title={title} TextLength={textLength}",
                grainId, userId, title, text.Length);

            await CheckProjectState(userId);

            try
            {
                _logger.LogInformation("ProjectGrain::AddTextDocument: call DocumentStorageGrain to store user\'s text");
                var documentGrain = GrainFactory.GetGrain<IDocumentStorageGrain>(grainId, userId!);
                var document = await documentGrain.AddTextDocument(title, text);

                _logger.LogInformation("ProjectGrain::AddTextDocument: stored document to vector storage. " +
                    "GrainId={grainId}, UserId={userId} DocumentId={documentId}, DocumentType={documentType}, DocumentTitle={documentTitle}",
                    grainId, userId, document.Id, document.DocumentType, document.Title);
                _projectDocumentState
                    .State
                    .Documents
                    .Add(document);
                await _projectDocumentState.WriteStateAsync();

                _logger.LogInformation("ProjectGrain::AddTextDocument: writed new document to state. " +
                    "GrainId={grainId}, UserId={userId} DocumentId={documentId}, DocumentType={documentType}, DocumentTitle={documentTitle}",
                    grainId, userId, document.Id, document.DocumentType, document.Title);
                return document;
            }
            catch (Exception ex)
            {
                _logger.LogCritical("ProjectGrain::AddTextDocument: exception raised. " +
                    "GrainId={grainId} UserId={userId}. Message: {exMsg}", grainId, userId, ex.Message);
                throw;
            }
        }

        public async Task<IDocument> AddPdfDocument(string title, string url)
        {
            var grainId = this.GetPrimaryKey(out var userId);
            _logger.LogInformation("ProjectGrain::AddPdfDocument: add text data source to project. " +
                "GrainId={grainId}, UserId={userId} Title={title} Url={pdfUrl}",
                grainId, userId, title, url);

            await CheckProjectState(userId);

            try
            {
                _logger.LogInformation("ProjectGrain::AddPdfDocument: call DocumentStorageGrain to store user\'s PDF");
                var documentGrain = GrainFactory.GetGrain<IDocumentStorageGrain>(grainId, userId!);
                var document = await documentGrain.AddPdfDocument(title, url);

                _logger.LogInformation("ProjectGrain::AddPdfDocument: stored document to vector storage. " +
                    "GrainId={grainId}, UserId={userId} DocumentId={documentId}, DocumentType={documentType}, DocumentTitle={documentTitle}",
                    grainId, userId, document.Id, document.DocumentType, document.Title);
                _projectDocumentState
                    .State
                    .Documents
                    .Add(document);
                await _projectDocumentState.WriteStateAsync();

                _logger.LogInformation("ProjectGrain::AddPdfDocument: writed new document to state. " +
                    "GrainId={grainId}, UserId={userId} DocumentId={documentId}, DocumentType={documentType}, DocumentTitle={documentTitle}",
                    grainId, userId, document.Id, document.DocumentType, document.Title);
                return document;
            }
            catch (Exception ex)
            {
                _logger.LogCritical("ProjectGrain::AddPdfDocument: exception raised. " +
                    "GrainId={grainId} UserId={userId}. Message: {exMsg}", grainId, userId, ex.Message);
                throw;
            }
        }

        public async Task<IProject> Remove()
        {
            var grainId = this.GetPrimaryKey(out var userId);
            _logger.LogInformation("ProjectGrain::RemoveProject: start removing project. " +
                "Grainid={grainId} ProjectId={userId}", grainId, userId);

            await CheckProjectState(userId);

            // Remove project documents
            var documents = _projectDocumentState.State.Documents;
            if (documents.Any())
            {
                var documentGrain = GrainFactory.GetGrain<IDocumentStorageGrain>(grainId, userId!);
                foreach (var document in documents)
                {
                    var removedDocument = await documentGrain.RemoveDocument(document);
                    _logger.LogInformation("ProjectGrain::RemoveProject: removed document. " +
                        "DocumentId={documentId} DocumentTitle={documentTitle}",
                        document.Id, document.Title);
                }
            }
            await _projectDocumentState.ClearStateAsync();

            // Remove chat agent grain
            var chatAgentGrain = GrainFactory.GetGrain<IChatAgentGrain>(grainId, userId!);
            await chatAgentGrain.DeleteChatAgent();

            // Remove project text
            _logger.LogInformation("ProjectGrain::RemoveProject: removing project text state. " +
                "Grainid={grainId} ProjectId={userId}", grainId, userId);
            await _projectTextState.ClearStateAsync();
            
            _logger.LogInformation("ProjectGrain::RemoveProject: removing project state. " +
                "Grainid={grainId} ProjectId={userId}", grainId, userId);
            var project = ProjectFactory.CreateProject(
                _projectState.State.ProjectId,
                _projectState.State.ProjetState,
                _projectState.State.Title,
                _projectState.State.Description,
                _projectState.State.CreatedDate
            );
            await _projectState.ClearStateAsync();

            return project;
        }
    }
}
