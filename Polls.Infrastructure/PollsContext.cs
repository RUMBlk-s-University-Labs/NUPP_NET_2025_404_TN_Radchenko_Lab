using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Polls.Infrastructure.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace Polls.Infrastructure
{
    public class PollsContext : IdentityDbContext<PersonModel, IdentityRole<Guid>, Guid>
    {
        public DbSet<PersonModel> Persons { get; set; }
        public DbSet<OptionModel> Options { get; set; }
        public DbSet<PollModel> Polls { get; set; }
        public DbSet<SingleVotePollModel> SingleVotePoll { get; set; }
        public DbSet<RankedPollModel> RankedPoll { get; set; }
        public DbSet<VoteModel> Votes { get; set; }

        public PollsContext(DbContextOptions<PollsContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<PollModel>().ToTable("Polls");
            modelBuilder.Entity<SingleVotePollModel>().ToTable("SingleVotePolls");
            modelBuilder.Entity<RankedPollModel>().ToTable("RankedPolls");

            // Багато-до-багатьох
            modelBuilder.Entity<PollModel>()
                .HasMany(p => p.Options)
                .WithMany(o => o.Polls);

            // Один-до-багатьох
            modelBuilder.Entity<VoteModel>()
                .HasOne(v => v.Person)
                .WithMany(p => p.Votes)
                .HasForeignKey(v => v.PersonId);

            // Один-до-одного
            modelBuilder.Entity<PollModel>()
                .HasOne(p => p.Status)
                .WithOne(s => s.Poll)
                .HasForeignKey<PollStatusModel>(s => s.PollId);

            // Один-до-одного
            modelBuilder.Entity<PollModel>()
                .HasOne(p => p.Iteration)
                .WithOne(i => i.Poll)
                .HasForeignKey<IterationModel>(i => i.PollId);

            // Один-до-багатьох
            modelBuilder.Entity<VoteModel>()
                .HasOne(v => v.Iteration)
                .WithMany(i => i.Votes)
                .HasForeignKey(i => i.IterationId);
        }
    }
}