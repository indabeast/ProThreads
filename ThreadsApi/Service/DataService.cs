using Microsoft.EntityFrameworkCore;
using shared;
using ThreadsApi.Data;
using Thread = shared.Thread;

namespace ThreadsApi.Service;

public class DataService
{
    private readonly ThreadsContext _context;

    public DataService(ThreadsContext context)
    {
        _context = context;
    }

    // Fetch all threads (limited to 50 newest threads)
    public async Task<List<Thread>> GetThreadsAsync()
    {
        return await _context.Threads
            .OrderByDescending(t => t.CreatedAt)
            .Take(50)
            .Include(t => t.Comments) // Explicitly include comments
            .ThenInclude(c => c.Votes) // Explicitly include votes for comments
            .Include(t => t.Votes) // Explicitly include votes for threads
            .ToListAsync();
    }


    // Fetch a specific thread by ID
    public async Task<Thread> GetThreadByIdAsync(int threadId)
    {
        return await _context.Threads
            .Include(t => t.Comments)
            .ThenInclude(c => c.Votes)
            .Include(t => t.Votes)
            .FirstOrDefaultAsync(t => t.Id == threadId);
    }


    // Create a new thread
    public async Task CreateThreadAsync(Thread thread)
    {
        thread.CreatedAt = DateTime.UtcNow;
        _context.Threads.Add(thread);
        await _context.SaveChangesAsync();
    }

    // Add a comment to a specific thread
    public async Task AddCommentAsync(int threadId, Comment comment)
    {
        comment.ThreadId = threadId;
        comment.CreatedAt = DateTime.UtcNow;
        _context.Comments.Add(comment);
        await _context.SaveChangesAsync();
    }
    
    // Add or update a vote for a thread
    public async Task VoteOnThreadAsync(int threadId, bool isUpvote)
    {
        var existingVote = await _context.Votes
            .FirstOrDefaultAsync(v => v.ThreadId == threadId && v.CommentId == 0); // Assuming CommentId == 0 means it's a thread vote

        if (existingVote != null)
        {
            // Update the existing vote
            existingVote.IsUpvote = isUpvote;
        }
        else
        {
            // Add a new vote
            _context.Votes.Add(new Vote
            {
                ThreadId = threadId,
                IsUpvote = isUpvote,
                CommentId = 0 // 0 means it's for the thread
            });
        }
        await _context.SaveChangesAsync();
    }

    // Add or update a vote for a comment
    public async Task VoteOnCommentAsync(int commentId, bool isUpvote)
    {
        var existingVote = await _context.Votes
            .FirstOrDefaultAsync(v => v.CommentId == commentId);

        if (existingVote != null)
        {
            // Update the existing vote
            existingVote.IsUpvote = isUpvote;
        }
        else
        {
            // Add a new vote
            _context.Votes.Add(new Vote
            {
                CommentId = commentId,
                IsUpvote = isUpvote,
                ThreadId = 0 // 0 means it's for the comment
            });
        }
        await _context.SaveChangesAsync();
    }

    public void SeedData()
    {
        if (!_context.Threads.Any())
        {
            Console.WriteLine("No threads found, seeding data...");

            // Create threads
            var threads = new List<Thread>
            {
                new Thread
                {
                    Title = "First Thread",
                    Content = "This is the first thread content.",
                    AuthorName = "John Doe",
                    CreatedAt = DateTime.UtcNow,
                    Upvotes = 5,
                    Downvotes = 3
                },
                new Thread
                {
                    Title = "Second Thread",
                    Content = "This is the second thread content.",
                    AuthorName = "Jane Smith",
                    CreatedAt = DateTime.UtcNow,
                    Upvotes = 6,
                    Downvotes = 3
                }
            };

            _context.Threads.AddRange(threads);
            _context.SaveChanges();

            // Create comments
            var comments = new List<Comment>
            {
                new Comment
                {
                    Text = "First comment on first thread",
                    AuthorName = "Alice",
                    CreatedAt = DateTime.UtcNow,
                    ThreadId = threads[0].Id,
                    Upvotes = 2,
                    Downvotes = 1
                },
                new Comment
                {
                    Text = "Second comment on first thread",
                    AuthorName = "Bob",
                    CreatedAt = DateTime.UtcNow,
                    ThreadId = threads[0].Id,
                    Upvotes = 3,
                    Downvotes = 2
                },
                new Comment
                {
                    Text = "First comment on second thread",
                    AuthorName = "Charlie",
                    CreatedAt = DateTime.UtcNow,
                    ThreadId = threads[1].Id,
                    Upvotes = 1,
                    Downvotes = 0
                }
            };

            _context.Comments.AddRange(comments);
            _context.SaveChanges();

            // Create votes
            var votes = new List<Vote>
            {
                new Vote
                {
                    ThreadId = threads[0].Id,
                    IsUpvote = true
                },
                new Vote
                {
                    ThreadId = threads[1].Id,
                    IsUpvote = false
                },
                new Vote
                {
                    CommentId = comments[0].Id,
                    IsUpvote = true
                },
                new Vote
                {
                    CommentId = comments[1].Id,
                    IsUpvote = false
                }
            };

            _context.Votes.AddRange(votes);
            _context.SaveChanges();

            Console.WriteLine("Data seeding completed.");
        }
    }
}