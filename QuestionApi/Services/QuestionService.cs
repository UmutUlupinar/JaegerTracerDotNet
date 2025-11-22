using System.Diagnostics;
using OpenTelemetry.Trace;
using QuestionApi.Clients;
using QuestionApi.Models;

namespace QuestionApi.Services;

public class QuestionService : IQuestionService
{
    private readonly IAnswerApiClient _answerApiClient;
    private readonly ILogger<QuestionService> _logger;
    private static readonly ActivitySource ActivitySource = new("QuestionService");

    public QuestionService(IAnswerApiClient answerApiClient, ILogger<QuestionService> logger)
    {
        _answerApiClient = answerApiClient;
        _logger = logger;
    }

    public async Task<QuestionResponse> CreateQuestionAsync(QuestionRequest request, CancellationToken cancellationToken = default)
    {
        using var activity = ActivitySource.StartActivity("CreateQuestion");
        activity?.SetTag("question.text", request.Text);

        try
        {
            _logger.LogInformation("Creating question: {Text}", request.Text);
            
            // Simulate some processing
            await Task.Delay(100, cancellationToken);
            
            var question = new QuestionResponse(
                Id: Guid.NewGuid().ToString(),
                Text: request.Text,
                CreatedAt: DateTime.UtcNow
            );

            _logger.LogInformation("Question created with ID: {Id}", question.Id);
            return question;
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            _logger.LogError(ex, "Failed to create question");
            throw;
        }
    }

    public async Task<QuestionResponse?> GetQuestionAsync(string id, CancellationToken cancellationToken = default)
    {
        using var activity = ActivitySource.StartActivity("GetQuestion");
        activity?.SetTag("question.id", id);

        try
        {
            _logger.LogInformation("Retrieving question with ID: {Id}", id);
            
            // Simulate some processing
            await Task.Delay(50, cancellationToken);
            
            var question = new QuestionResponse(
                Id: id,
                Text: "Sample question",
                CreatedAt: DateTime.UtcNow.AddHours(-1)
            );

            _logger.LogInformation("Question retrieved: {Id}", id);
            return question;
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            _logger.LogError(ex, "Failed to get question {Id}", id);
            throw;
        }
    }

    public async Task<QuestionResponse> ProcessQuestionWithAnswerAsync(QuestionRequest request, CancellationToken cancellationToken = default)
    {
        using var activity = ActivitySource.StartActivity("ProcessQuestionWithAnswer");
        activity?.SetTag("question.text", request.Text);

        try
        {
            _logger.LogInformation("Processing question with answer: {Text}", request.Text);
            
            // Create question
            var question = await CreateQuestionAsync(request, cancellationToken);
            activity?.SetTag("question.id", question.Id);

            // Get answer from AnswerApi
            var answerRequest = new CreateAnswerRequest(question.Id, "This is an automated answer");
            var answer = await _answerApiClient.CreateAnswerAsync(answerRequest, cancellationToken);
            
            if (answer != null)
            {
                activity?.SetTag("answer.id", answer.Id);
                _logger.LogInformation("Question processed with answer. Question: {QuestionId}, Answer: {AnswerId}", question.Id, answer.Id);
            }

            return question;
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            _logger.LogError(ex, "Failed to process question with answer");
            throw;
        }
    }
}

