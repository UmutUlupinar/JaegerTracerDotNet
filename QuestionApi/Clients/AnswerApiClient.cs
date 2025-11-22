using System.Diagnostics;
using OpenTelemetry.Trace;
using QuestionApi.Models;
using Shared.Http;

namespace QuestionApi.Clients;

public class AnswerApiClient : IAnswerApiClient
{
    private readonly IHttpClientService _httpClient;
    private readonly ILogger<AnswerApiClient> _logger;
    private static readonly ActivitySource ActivitySource = new("AnswerApiClient");

    public AnswerApiClient(IHttpClientService httpClient, ILogger<AnswerApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<AnswerResponse?> GetAnswerAsync(string questionId, CancellationToken cancellationToken = default)
    {
        using var activity = ActivitySource.StartActivity("GetAnswer");
        activity?.SetTag("question.id", questionId);

        try
        {
            _logger.LogInformation("Requesting answer for question {QuestionId}", questionId);
            var response = await _httpClient.GetAsync<AnswerResponse>($"/api/answer/{questionId}", cancellationToken);
            _logger.LogInformation("Answer retrieved for question {QuestionId}", questionId);
            return response;
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            _logger.LogError(ex, "Failed to get answer for question {QuestionId}", questionId);
            throw;
        }
    }

    public async Task<AnswerResponse?> CreateAnswerAsync(CreateAnswerRequest request, CancellationToken cancellationToken = default)
    {
        using var activity = ActivitySource.StartActivity("CreateAnswer");
        activity?.SetTag("question.id", request.QuestionId);

        try
        {
            _logger.LogInformation("Creating answer for question {QuestionId}", request.QuestionId);
            var response = await _httpClient.PostAsync<CreateAnswerRequest, AnswerResponse>("/api/answer", request, cancellationToken);
            _logger.LogInformation("Answer created for question {QuestionId}", request.QuestionId);
            return response;
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            _logger.LogError(ex, "Failed to create answer for question {QuestionId}", request.QuestionId);
            throw;
        }
    }
}

