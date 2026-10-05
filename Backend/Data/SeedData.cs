using CRMPro.Api.Models;
using CRMPro.Api;

namespace CRMPro.Api.Data;

public static class SeedData
{
    public static void Initialize(AppDbContext db)
    {
        if (!db.Users.Any())
        {
            db.Users.AddRange(
                new User { Name = "CRM Admin", Email = "admin@crmpro.com", Password = PasswordHasher.Hash("123456"), Role = "Admin", IsActive = true },
                new User { Name = "Sales Employee", Email = "sales@crmpro.com", Password = PasswordHasher.Hash("123456"), Role = "Sales", IsActive = true },
                new User { Name = "Sales Manager", Email = "manager@crmpro.com", Password = PasswordHasher.Hash("123456"), Role = "Sales", IsActive = true }
            );
        }

        if (!db.Customers.Any())
        {
            db.Customers.AddRange(
                new Customer { Name = "Ahmed Hassan", Company = "Hassan Trading", Email = "ahmed@example.com", Phone = "01000000001", Status = "Active" },
                new Customer { Name = "Mona Ali", Company = "Mona Solutions", Email = "mona@example.com", Phone = "01000000002", Status = "Active" },
                new Customer { Name = "Omar Samir", Company = "Samir Group", Email = "omar@example.com", Phone = "01000000003", Status = "Prospect" },
                new Customer { Name = "Nour Adel", Company = "Nour Retail", Email = "nour@example.com", Phone = "01000000006", Status = "Active" }
            );
        }

        if (!db.Leads.Any())
        {
            db.Leads.AddRange(
                new Lead { Name = "Khaled Mostafa", Company = "KM Tech", Email = "khaled@example.com", Phone = "01000000004", Source = "Website", Status = "New", EstimatedValue = 18000 },
                new Lead { Name = "Sara Adel", Company = "Adel Stores", Email = "sara@example.com", Phone = "01000000005", Source = "Referral", Status = "Qualified", EstimatedValue = 32000 },
                new Lead { Name = "Youssef Nabil", Company = "YN Logistics", Email = "youssef@example.com", Phone = "01000000007", Source = "Social Media", Status = "Contacted", EstimatedValue = 21000 }
            );
        }

        if (!db.Deals.Any())
        {
            db.Deals.AddRange(
                new Deal { Title = "E-commerce Website", CustomerName = "Ahmed Hassan", Value = 25000, Stage = "Negotiation" },
                new Deal { Title = "Support Contract", CustomerName = "Mona Ali", Value = 12000, Stage = "Won" },
                new Deal { Title = "Management System", CustomerName = "Omar Samir", Value = 38000, Stage = "Proposal" },
                new Deal { Title = "Mobile App", CustomerName = "Nour Adel", Value = 29000, Stage = "New" }
            );
        }

        if (!db.Activities.Any())
        {
            db.Activities.AddRange(
                new Activity { Type = "Call", Subject = "Follow up with Ahmed Hassan", RelatedTo = "Ahmed Hassan", DueDate = DateTime.UtcNow.AddDays(1), Status = "Pending" },
                new Activity { Type = "Meeting", Subject = "Product demonstration", RelatedTo = "Mona Ali", DueDate = DateTime.UtcNow.AddDays(2), Status = "Pending" },
                new Activity { Type = "Email", Subject = "Send proposal", RelatedTo = "Omar Samir", DueDate = DateTime.UtcNow.AddDays(3), Status = "Pending" }
            );
        }

        if (!db.Notes.Any())
        {
            db.Notes.AddRange(
                new Note { Title = "Sales Process", Content = "Follow leads, qualify opportunities, prepare proposals and close deals." },
                new Note { Title = "CRMPro Demo", Content = "Portfolio demonstration data." }
            );
        }

        if (!db.Notifications.Any())
        {
            db.Notifications.AddRange(
                new Notification { Title = "Welcome to CRMPro", Message = "Your CRM workspace is ready.", Type = "Success" },
                new Notification { Title = "Follow-up due", Message = "Ahmed Hassan follow-up is due tomorrow.", Type = "Reminder" },
                new Notification { Title = "Pipeline update", Message = "Management System is currently in Proposal.", Type = "Info" }
            );
        }

        if (!db.AuditLogs.Any())
        {
            db.AuditLogs.AddRange(
                new AuditLog { Actor = "CRM Admin", Action = "Created", Entity = "Customer", EntityId = 1 },
                new AuditLog { Actor = "CRM Admin", Action = "Updated", Entity = "Deal", EntityId = 3 },
                new AuditLog { Actor = "Sales Employee", Action = "Created", Entity = "Lead", EntityId = 2 }
            );
        }

        db.SaveChanges();
    }
}
