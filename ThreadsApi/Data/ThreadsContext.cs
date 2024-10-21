using System.Collections.Generic;
using System.Reflection.Emit;
using Microsoft.EntityFrameworkCore;
using shared;
using Thread = shared.Thread;

namespace ThreadsApi.Data;

public class ThreadsContext : DbContext

{
    
    public DbSet<Thread> Threads { get; set; } // Use shared.Thread
    public DbSet<Comment> Comments { get; set; } // Use shared.Comment
    public DbSet<Vote> Votes { get; set; } // Use shared.Vote

    public ThreadsContext(DbContextOptions<ThreadsContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure the many-to-one relationship between Vote and Thread
        modelBuilder.Entity<Vote>()
            .HasOne(v => v.Thread)
            .WithMany(t => t.Votes)
            .HasForeignKey(v => v.ThreadId)
            .OnDelete(DeleteBehavior.Restrict); // Or DeleteBehavior.Cascade if needed

        // Configure the many-to-one relationship between Vote and Comment
        modelBuilder.Entity<Vote>()
            .HasOne(v => v.Comment)
            .WithMany(c => c.Votes)
            .HasForeignKey(v => v.CommentId)
            .OnDelete(DeleteBehavior.Restrict); // Or DeleteBehavior.Cascade if needed
    }

}