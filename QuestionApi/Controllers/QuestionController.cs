using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using OpenTelemetry.Trace;
using QuestionApi.Models;
using QuestionApi.Services;
using Shared.Exceptions;

namespace QuestionApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class QuestionController : ControllerBase
{
    private readonly IQuestionService _questionService;
    private readonly ILogger<QuestionController> _logger;
    private static readonly ActivitySource ActivitySource = new("QuestionController");

    public QuestionController(IQuestionService questionService, ILogger<QuestionController> logger)
    {
        _questionService = questionService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult<QuestionResponse>> CreateQuestion([FromBody] QuestionRequest request, CancellationToken cancellationToken = default)
    {
        using var activity = ActivitySource.StartActivity("CreateQuestionEndpoint");
        activity?.SetTag("http.route", "/api/question");
        activity?.SetTag("http.method", "POST");

        try
        {
            _logger.LogInformation("Received request to create question");
            var result = await _questionService.CreateQuestionAsync(request, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            _logger.LogError(ex, "Error creating question");
            throw new ApiException("Failed to create question", 500, "QUESTION_CREATE_ERROR");
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<QuestionResponse>> GetQuestion(string id, CancellationToken cancellationToken = default)
    {
        using var activity = ActivitySource.StartActivity("GetQuestionEndpoint");
        activity?.SetTag("http.route", "/api/question/{id}");
        activity?.SetTag("http.method", "GET");
        activity?.SetTag("question.id", id);

        try
        {
            _logger.LogInformation("Received request to get question {Id}", id);
            var result = await _questionService.GetQuestionAsync(id, cancellationToken);
            
            if (result == null)
            {
                throw new ApiException($"Question with id {id} not found", 404, "QUESTION_NOT_FOUND");
            }

            return Ok(result);
        }
        catch (ApiException)
        {
            throw;
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            _logger.LogError(ex, "Error getting question {Id}", id);
            throw new ApiException("Failed to get question", 500, "QUESTION_GET_ERROR");
        }
    }

    [HttpPost("process-with-answer")]
    public async Task<ActionResult<QuestionResponse>> ProcessQuestionWithAnswer([FromBody] QuestionRequest request, CancellationToken cancellationToken = default)
    {
        using var activity = ActivitySource.StartActivity("ProcessQuestionWithAnswerEndpoint");
        activity?.SetTag("http.route", "/api/question/process-with-answer");
        activity?.SetTag("http.method", "POST");

        try
        {
            _logger.LogInformation("Received request to process question with answer");
            var result = await _questionService.ProcessQuestionWithAnswerAsync(request, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            _logger.LogError(ex, "Error processing question with answer");
            throw new ApiException("Failed to process question with answer", 500, "QUESTION_PROCESS_ERROR");
        }
    }
}

