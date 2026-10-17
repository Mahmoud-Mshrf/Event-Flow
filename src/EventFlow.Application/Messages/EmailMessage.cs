namespace EventFlow.Application.Messages;

public sealed record EmailMessage(
    string To,
    string Subject,
    string Body,
    bool IsHtml);