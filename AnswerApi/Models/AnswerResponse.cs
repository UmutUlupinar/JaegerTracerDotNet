namespace AnswerApi.Models;

public record AnswerResponse(string Id, string QuestionId, string AnswerText, DateTime CreatedAt);

