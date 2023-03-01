using BugHunterBugTrackerZD.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Net.Mail;

namespace BugHunterBugTrackerZD.Data
{
    public class ApplicationDbContext : IdentityDbContext<BTUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Project> Projects { get; set; } = default!;
        public virtual DbSet<BTUser> Members { get; set; } = default!;
        public virtual DbSet<Invite> Invites { get; set; } = default!;
        public virtual DbSet<Ticket> Tickets { get; set; } = default!;
        public virtual DbSet<TicketComment> Comments { get; set; } = default!;
        public virtual DbSet<TicketHistory> Histories { get; set; } = default!;
        public virtual DbSet<Attachment> Attachments { get; set; } = default!;

    }
}