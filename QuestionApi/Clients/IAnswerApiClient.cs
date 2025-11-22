using QuestionApi.Models;

namespace QuestionApi.Clients;

public interface IAnswerApiClient
{
    Task<AnswerResponse?> GetAnswerAsync(string questionId, CancellationToken cancellationToken = default);
    Task<AnswerResponse?> CreateAnswerAsync(CreateAnswerRequest request, CancellationToken cancellationToken = default);
}

