namespace EmployeeManagement.Api.Messaging;

public record TaskAssignedEvent(int TaskId, string TaskTitle, int ProjectId, int AssignedToId, string AssignedToEmail, int AssignedById, DateTime OccurredAtUtc);

public record TaskCompletedEvent(int TaskId, string TaskTitle, int ProjectId, int CompletedById, DateTime OccurredAtUtc);

public record TaskCreatedEvent(int TaskId, string TaskTitle, int ProjectId, int CreatedById, DateTime OccurredAtUtc);

public record TaskRemovedEvent(int TaskId, int ProjectId, int RemovedById, DateTime OccurredAtUtc);

public record ProjectMemberAddedEvent(int ProjectId, int EmployeeId, int AddedById, DateTime OccurredAtUtc);

public record ProjectMemberRemovedEvent(int ProjectId, int EmployeeId, int RemovedById, DateTime OccurredAtUtc);

public record ProjectRemovedEvent(int ProjectId, string ProjectName, int RemovedById, DateTime OccurredAtUtc);

public record UserCreatedEvent(int UserId, string Email, string Role, int CreatedById, DateTime OccurredAtUtc);

public record UserRemovedEvent(int UserId, int RemovedById, DateTime OccurredAtUtc);
