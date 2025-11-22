using QuestionApi.Models;

namespace QuestionApi.Services;

public interface IQuestionService
{
    Task<QuestionResponse> CreateQuestionAsync(QuestionRequest request, CancellationToken cancellationToken = default);
    Task<QuestionResponse?> GetQuestionAsync(string id, CancellationToken cancellationToken = default);
    Task<QuestionResponse> ProcessQuestionWithAnswerAsync(QuestionRequest request, CancellationToken cancellationToken = default);
}

