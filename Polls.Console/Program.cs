using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Polls.Common;
using Polls.Infrastructure;
using Polls.Infrastructure.Repositories;

namespace Polls.ConsoleApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            var connectionString = configuration.GetConnectionString("DefaultConnection");

            var optionsBuilder = new DbContextOptionsBuilder<PollsContext>();
            optionsBuilder.UseNpgsql(connectionString);
            
            using (var context = new PollsContext(optionsBuilder.Options))
            {
                Console.WriteLine("Застосування міграції...");
                await context.Database.MigrateAsync();
                Console.WriteLine("Міграція застосована.");

                var personRepo = new PersonRepository(context);
                var optionRepo = new OptionRepository(context);
                
                var pollRepo = new PollRepository(context, personRepo);
                var singleVotePollRepo = new SingleVotePollRepository(context, personRepo);
                var rankedPollRepo = new RankedPollRepository(context, personRepo);

                var personService = new GenericCrudServiceAsync<Person>(personRepo);
                var optionService = new GenericCrudServiceAsync<Option>(optionRepo);
                var rankedPollService = new GenericCrudServiceAsync<RankedPoll>(rankedPollRepo);
                
                var rand = new Random();
                List<Person> persons;
                HashSet<Option> options;
                List<RankedPoll> polls;

                Console.WriteLine("Створення опцій...");
                var optTasks = Enumerable.Range(0, 50).Select(i => 
                    optionService.CreateAsync(Option.CreateNew())
                );
                await Task.WhenAll(optTasks);
                options = (await optionService.ReadAllAsync()).ToHashSet();
                Console.WriteLine($"Завантажено {options.Count()} опцій.");

                Console.WriteLine("Створення користувачів...");
                var personTasks = Enumerable.Range(0, 50).Select(i => 
                    personService.CreateAsync(Person.CreateNew())
                );
                await Task.WhenAll(personTasks);
                persons = (await personService.ReadAllAsync()).ToList();
                Console.WriteLine($"Завантажено {persons.Count()} користувачів.");

                FinishedPollProcessor.StartProcessing();
                Console.WriteLine("Створення 10 RankedPolls...");
                polls = new List<RankedPoll>();
                for (int i = 0; i < 100; i++)
                {
                    var poll = Poll.CreateNew<RankedPoll>();
                    foreach (Option option in options)
                    {
                        poll.AddOption(option);
                    }
                    await rankedPollService.CreateAsync(poll);
                    polls.Add(poll);
                }
                polls = (await rankedPollService.ReadAllAsync()).ToList();
                Console.WriteLine("Опитування створено.");

                Console.WriteLine($"Запуск голосування для {polls.Count()} опитувань...");
                Parallel.ForEach(polls, poll =>
                {
                    poll.Start();
                    var options = poll.GetOptions();
                    Parallel.For(0, rand.Next(100, 500), j =>
                    {
                        try { poll.Vote(persons.ElementAt(rand.Next(0, persons.Count)), options.ElementAt(rand.Next(0, options.Count)).Id); } catch { }
                    });
                    poll.Finish();
                });
                FinishedPollProcessor.PrintStatistics();

                Console.WriteLine("Збереження результатів...");
                var updateTasks = polls.Select(poll => rankedPollService.UpdateAsync(poll));
                await Task.WhenAll(updateTasks);

                Console.WriteLine("Результати збережено.");
            }
        }
    }
}

