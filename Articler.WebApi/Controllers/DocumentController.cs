using Articler.WebApi.Models;
using Articler.WebApi.Services.DataSource;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using static Articler.AppDomain.Factories.Project.ProjectFactory;

namespace Articler.WebApi.Controllers
{
    [Route("[controller]/[action]")]
    [ApiController]
    public class DocumentController(
            ILogger<DocumentController> logger,
            IDocumentService dataSourceService
        ) : ControllerBase
    {
        private readonly ILogger<DocumentController> _logger = logger;
        private readonly IDocumentService _documentService = dataSourceService;

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetProjectDocuments(string projectId)
        {
            var userId = HttpContext.Items["UserId"]!.ToString()!;
            _logger.LogInformation("DocumentController::GetProjectDocuments: start get project documents. " +
                "UserId={userId} ProjectId={projectId}", userId, projectId);

            try
            {
                var documents = await _documentService.GetProjectDocumentsAsync(userId, projectId);

                if (documents == null)
                {
                    _logger.LogError("DocumentController::GetProjectDocuments: can\'t get project documents. " +
                        "UserId={userId} ProjectId={projectId}", userId, projectId);
                    return NotFound();
                }

                _logger.LogInformation("DocumentController::GetProjectDocuments: return project documents. " +
                    "UserId={userId} ProjectId={projectId} DocumentsCount={documentsCount}",
                    userId, projectId, documents.Count());
                return new JsonResult(documents);
            }
            catch (Exception ex)
            {
                _logger.LogCritical("DocumentController::GetProjectDocuments: exception raised. " +
                    "UserId={userId} ProjectId={projectId} Message: {exMsg}", userId, projectId, ex.Message);
                return BadRequest();
            }
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> RemoveDocument(RemoveDocumentRequest request)
        {
            var userId = HttpContext.Items["UserId"]!.ToString()!;
            _logger.LogInformation("DocumentController::RemoveDocument: start remove project document. " +
                "UserId={userId} Request=[{request}]", userId, request);

            try
            {
                var document = await _documentService.RemoveProjectDocumentAsync(
                    userId, request.ProjectId, request.DocumentId);

                if (document == null)
                {
                    _logger.LogError("DocumentController::RemoveDocument: can\'t find document. " +
                        "UserId={userId} ProjectId={projectId} DocumentId={documentId}",
                        userId, request.ProjectId, request.DocumentId);
                    return NotFound();
                }

                _logger.LogInformation("DocumentController::RemoveDocument: sucsessfully removed document. " +
                    "UserId={userId}, ProjectId={projetid}, DocumentId={documentId}, DocumentTitle={documentTitle}",
                    userId, request.ProjectId, document.Id, document.Title);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogCritical("DocumentController::RemoveDocument: exception raised. " +
                    "UserId={userId} ProjectId={projectId} Message: {exMsg}", userId, request.ProjectId, ex.Message);
                return BadRequest();
            }
            throw new NotImplementedException();
        }


        [Authorize]
        [HttpPost]
        public async Task<IActionResult> AddTextDocument(TextDataSourceRequest request)
        {
            var userId = HttpContext.Items["UserId"]!.ToString()!;
            _logger.LogInformation("DocumentController::AddTextDocument: received text data source. " +
                "UserId={userId}, Request=[{request}]", userId, request);

            try
            {
                var document = await _documentService.AddProjectTextDocumentAsync(userId, request.ProjectId, 
                    request.Title, request.Text);

                if (document == null)
                {
                    _logger.LogError("DataSourceController::AddTextDocument: can\'t add text document." +
                        "UserId={userId}, Request=[{request}]", userId, request);
                    return BadRequest();
                }

                _logger.LogInformation("DataSourceController::AddTextDocument: sucessfully added text document. " +
                    "UserId={userId}, ProjectId={projectId} DocumentId={documentId} DocumentTitle={documentTitle}",
                    userId, request.ProjectId, document.Id, document.Title);
                return new JsonResult(document);
            }
            catch (Exception ex)
            {
                _logger.LogCritical("DataSourceController::AddTextDocument: exception raised. " +
                    "Message={exMsg}", ex.Message);
                return BadRequest("Exception raised");
            }
        }
    }
}
