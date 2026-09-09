namespace ProductApp.Domain.Common;

public enum RecordStatus { Draft = 1, Active = 2, Inactive = 3, Archived = 4, Validated = 5 }
public enum UserStatus { Active = 1, Inactive = 2, Suspended = 3, Deleted = 4 }
public enum ResourceType { RawMaterial = 1, Machine = 2, Equipment = 3, Labor = 4, Energy = 5, Consumable = 6, Packaging = 7 }
public enum AvailabilityStatus { Available = 1, LowStock = 2, Unavailable = 3, ToOrder = 4 }
public enum ExperimentResult { Success = 1, PartialSuccess = 2, Failed = 3, Cancelled = 4 }
public enum MarketStudyStatus { Draft = 1, InProgress = 2, Completed = 3, Validated = 4 }
public enum OptimizationPriority { Low = 1, Medium = 2, High = 3, Critical = 4 }
public enum OptimizationStatus { New = 1, InReview = 2, InProgress = 3, Resolved = 4, Rejected = 5 }
public enum NotificationType { Information = 1, Success = 2, Warning = 3, Error = 4 }
public enum AuditLevel { Information = 1, Warning = 2, Error = 3, Critical = 4 }
