using Articler.WebApi.Models;
using Articler.WebApi.Services.DataSource;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;

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
        }


        [Authorize]
        [HttpPost]
        public async Task<IActionResult> AddTextDocument(TextDataSourceRequest request)
        {
            var userId = HttpContext.Items["UserId"]!.ToString()!;
            _logger.LogInformation("DocumentController::AddTextDocument: received text data source. " +
                "UserId={userId}, Request=[{request}]", userId, request);

            var documentResult = await _documentService.AddProjectTextDocumentAsync(userId, request.ProjectId,
                request.Title, request.Text);
            if (documentResult.Status != AppDomain.Models.Token.CalculateTokenStatus.Success)
            {
                IActionResult result = null!;
                
                switch (documentResult.Status)
                {
                    case AppDomain.Models.Token.CalculateTokenStatus.NoTokens:
                    case AppDomain.Models.Token.CalculateTokenStatus.NotEnoughTokens:
                        _logger.LogInformation("DocumentController::AddTextDocument: not enough tokens for request. " +
                            "UserId={userId} Status={tokenStatus}", userId, documentResult.Status);
                        result = StatusCode(402, "Not enogh tokens");
                        break;
                    case AppDomain.Models.Token.CalculateTokenStatus.InternalError:
                        _logger.LogInformation("DocumentController::AddTextDocument: internal error. " +
                            "UserId={userId} Status={tokenStatus}", userId, documentResult.Status);
                        result = NotFound("Internal error");
                        break;
                    case AppDomain.Models.Token.CalculateTokenStatus.ExceptionRaised:
                        _logger.LogInformation("DocumentController::AddTextDocument: exception raised. " +
                            "UserId={userId} Status={tokenStatus}", userId, documentResult.Status);
                        result = BadRequest("Exception raised");
                        break;
                    case AppDomain.Models.Token.CalculateTokenStatus.Success:
                    default:
                        return BadRequest("Make compiler happy");
                }
                return result;
            }

            var document = documentResult.Result!;
            _logger.LogInformation("DataSourceController::AddTextDocument: sucessfully added text document. " +
                "UserId={userId}, ProjectId={projectId} DocumentId={documentId} DocumentTitle={documentTitle}",
                userId, request.ProjectId, document.Id, document.Title);
            return new JsonResult(document);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> AddPdfDocument(PdfDataSourceDocumentRequest request)
        {
            var userId = HttpContext.Items["UserId"]!.ToString()!;
            _logger.LogInformation("DocumentController::AddPdfDocument: received text data source. " +
                "UserId={userId}, Request=[{request}]", userId, request);

            var documentResult = await _documentService.AddProjectPdfDocumentAsync(userId, request.ProjectId,
                request.Title, request.PdfUrl);
            if (documentResult.Status != AppDomain.Models.Token.CalculateTokenStatus.Success)
            {
                IActionResult result = null!;

                switch (documentResult.Status)
                {
                    case AppDomain.Models.Token.CalculateTokenStatus.NoTokens:
                    case AppDomain.Models.Token.CalculateTokenStatus.NotEnoughTokens:
                        _logger.LogInformation("DocumentController::AddPdfDocument: not enough tokens for request. " +
                            "UserId={userId} Status={tokenStatus}", userId, documentResult.Status);
                        result = StatusCode(402, "Not enogh tokens");
                        break;
                    case AppDomain.Models.Token.CalculateTokenStatus.InternalError:
                        _logger.LogInformation("DocumentController::AddPdfDocument: internal error. " +
                            "UserId={userId} Status={tokenStatus}", userId, documentResult.Status);
                        result = NotFound("Internal error");
                        break;
                    case AppDomain.Models.Token.CalculateTokenStatus.ExceptionRaised:
                        _logger.LogInformation("DocumentController::AddPdfDocument: exception raised. " +
                            "UserId={userId} Status={tokenStatus}", userId, documentResult.Status);
                        result = BadRequest("Exception raised");
                        break;
                    case AppDomain.Models.Token.CalculateTokenStatus.Success:
                    default:
                        return BadRequest("Make compiler happy");
                }
                return result;
            }

            var document = documentResult.Result!;
            _logger.LogInformation("DataSourceController::AddPdfDocument: sucessfully added PDF document. " +
                "UserId={userId}, ProjectId={projectId} DocumentId={documentId} DocumentTitle={documentTitle}",
                userId, request.ProjectId, document.Id, document.Title);
            return new JsonResult(document);
        }
    }
}
