using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using OpenTelemetry.Trace;
using AnswerApi.Models;
using AnswerApi.Services;
using Shared.Exceptions;

namespace AnswerApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AnswerController : ControllerBase
{
    private readonly IAnswerService _answerService;
    private readonly ILogger<AnswerController> _logger;
    private static readonly ActivitySource ActivitySource = new("AnswerController");

    public AnswerController(IAnswerService answerService, ILogger<AnswerController> logger)
    {
        _answerService = answerService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult<AnswerResponse>> CreateAnswer([FromBody] CreateAnswerRequest request, CancellationToken cancellationToken = default)
    {
        using var activity = ActivitySource.StartActivity("CreateAnswerEndpoint");
        activity?.SetTag("http.route", "/api/answer");
        activity?.SetTag("http.method", "POST");
        activity?.SetTag("question.id", request.QuestionId);

        try
        {
            _logger.LogInformation("Received request to create answer for question {QuestionId}", request.QuestionId);
            var result = await _answerService.CreateAnswerAsync(request, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            _logger.LogError(ex, "Error creating answer");
            throw new ApiException("Failed to create answer", 500, "ANSWER_CREATE_ERROR");
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AnswerResponse>> GetAnswer(string id, CancellationToken cancellationToken = default)
    {
        using var activity = ActivitySource.StartActivity("GetAnswerEndpoint");
        activity?.SetTag("http.route", "/api/answer/{id}");
        activity?.SetTag("http.method", "GET");
        activity?.SetTag("answer.id", id);

        try
        {
            _logger.LogInformation("Received request to get answer {Id}", id);
            var result = await _answerService.GetAnswerAsync(id, cancellationToken);
            
            if (result == null)
            {
                throw new ApiException($"Answer with id {id} not found", 404, "ANSWER_NOT_FOUND");
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
            _logger.LogError(ex, "Error getting answer {Id}", id);
            throw new ApiException("Failed to get answer", 500, "ANSWER_GET_ERROR");
        }
    }

    [HttpGet("question/{questionId}")]
    public async Task<ActionResult<AnswerResponse>> GetAnswerByQuestionId(string questionId, CancellationToken cancellationToken = default)
    {
        using var activity = ActivitySource.StartActivity("GetAnswerByQuestionIdEndpoint");
        activity?.SetTag("http.route", "/api/answer/question/{questionId}");
        activity?.SetTag("http.method", "GET");
        activity?.SetTag("question.id", questionId);

        try
        {
            _logger.LogInformation("Received request to get answer for question {QuestionId}", questionId);
            var result = await _answerService.GetAnswerByQuestionIdAsync(questionId, cancellationToken);
            
            if (result == null)
            {
                throw new ApiException($"Answer for question {questionId} not found", 404, "ANSWER_NOT_FOUND");
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
            _logger.LogError(ex, "Error getting answer for question {QuestionId}", questionId);
            throw new ApiException("Failed to get answer for question", 500, "ANSWER_GET_ERROR");
        }
    }
}

