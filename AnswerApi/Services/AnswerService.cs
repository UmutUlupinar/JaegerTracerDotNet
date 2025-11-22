using System.Diagnostics;
using OpenTelemetry.Trace;
using AnswerApi.Models;

namespace AnswerApi.Services;

public class AnswerService : IAnswerService
{
    private readonly ILogger<AnswerService> _logger;
    private static readonly ActivitySource ActivitySource = new("AnswerService");

    public AnswerService(ILogger<AnswerService> logger)
    {
        _logger = logger;
    }

    public async Task<AnswerResponse> CreateAnswerAsync(CreateAnswerRequest request, CancellationToken cancellationToken = default)
    {
        using var activity = ActivitySource.StartActivity("CreateAnswer");
        activity?.SetTag("question.id", request.QuestionId);
        activity?.SetTag("answer.text", request.AnswerText);

        try
        {
            _logger.LogInformation("Creating answer for question {QuestionId}", request.QuestionId);
            
            // Simulate some processing
            await Task.Delay(150, cancellationToken);
            
            var answer = new AnswerResponse(
                Id: Guid.NewGuid().ToString(),
                QuestionId: request.QuestionId,
                AnswerText: request.AnswerText,
                CreatedAt: DateTime.UtcNow
            );

            activity?.SetTag("answer.id", answer.Id);
            _logger.LogInformation("Answer created with ID: {Id} for question {QuestionId}", answer.Id, request.QuestionId);
            
            return answer;
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            _logger.LogError(ex, "Failed to create answer for question {QuestionId}", request.QuestionId);
            throw;
        }
    }

    public async Task<AnswerResponse?> GetAnswerAsync(string id, CancellationToken cancellationToken = default)
    {
        using var activity = ActivitySource.StartActivity("GetAnswer");
        activity?.SetTag("answer.id", id);

        try
        {
            _logger.LogInformation("Retrieving answer with ID: {Id}", id);
            
            // Simulate some processing
            await Task.Delay(50, cancellationToken);
            
            var answer = new AnswerResponse(
                Id: id,
                QuestionId: "sample-question-id",
                AnswerText: "Sample answer",
                CreatedAt: DateTime.UtcNow.AddMinutes(-30)
            );

            _logger.LogInformation("Answer retrieved: {Id}", id);
            return answer;
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            _logger.LogError(ex, "Failed to get answer {Id}", id);
            throw;
        }
    }

    public async Task<AnswerResponse?> GetAnswerByQuestionIdAsync(string questionId, CancellationToken cancellationToken = default)
    {
        using var activity = ActivitySource.StartActivity("GetAnswerByQuestionId");
        activity?.SetTag("question.id", questionId);

        try
        {
            _logger.LogInformation("Retrieving answer for question {QuestionId}", questionId);
            
            // Simulate some processing
            await Task.Delay(75, cancellationToken);
            
            var answer = new AnswerResponse(
                Id: Guid.NewGuid().ToString(),
                QuestionId: questionId,
                AnswerText: "Answer for the question",
                CreatedAt: DateTime.UtcNow.AddMinutes(-15)
            );

            _logger.LogInformation("Answer retrieved for question {QuestionId}", questionId);
            return answer;
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            _logger.LogError(ex, "Failed to get answer for question {QuestionId}", questionId);
            throw;
        }
    }
}

