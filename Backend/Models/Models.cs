namespace CRMPro.Api.Models;

public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public string Password { get; set; } = "";
    public string Role { get; set; } = "Sales";
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Company { get; set; } = "";
    public string Email { get; set; } = "";
    public string Phone { get; set; } = "";
    public string Status { get; set; } = "Active";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class Lead
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Company { get; set; } = "";
    public string Email { get; set; } = "";
    public string Phone { get; set; } = "";
    public string Source { get; set; } = "Website";
    public string Status { get; set; } = "New";
    public decimal EstimatedValue { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class Deal
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string CustomerName { get; set; } = "";
    public decimal Value { get; set; }
    public string Stage { get; set; } = "New";
    public DateTime ExpectedCloseDate { get; set; } = DateTime.UtcNow.AddDays(30);
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class Activity
{
    public int Id { get; set; }
    public string Type { get; set; } = "Call";
    public string Subject { get; set; } = "";
    public string RelatedTo { get; set; } = "";
    public DateTime DueDate { get; set; } = DateTime.UtcNow.AddDays(1);
    public string Status { get; set; } = "Pending";
}

public class Note
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string Content { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class Notification
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string Message { get; set; } = "";
    public string Type { get; set; } = "Info";
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class AuditLog
{
    public int Id { get; set; }
    public string Actor { get; set; } = "";
    public string Action { get; set; } = "";
    public string Entity { get; set; } = "";
    public int? EntityId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
