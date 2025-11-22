using AnswerApi.Models;

namespace AnswerApi.Services;

public interface IAnswerService
{
    Task<AnswerResponse> CreateAnswerAsync(CreateAnswerRequest request, CancellationToken cancellationToken = default);
    Task<AnswerResponse?> GetAnswerAsync(string id, CancellationToken cancellationToken = default);
    Task<AnswerResponse?> GetAnswerByQuestionIdAsync(string questionId, CancellationToken cancellationToken = default);
}

