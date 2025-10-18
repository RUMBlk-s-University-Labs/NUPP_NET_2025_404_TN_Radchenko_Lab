using System;
using System.Collections.Concurrent;
using Polls.Common;

class Program
{
    static async Task Main()
    {
        //Відстежування подій
        PollTracker.PollCreated += poll =>
        {
            Console.WriteLine($"Створено нове опитування: [{poll.Id}]{poll.Title}");
        };

        PollTracker.PollStarted += async id =>
        {
            var poll = await PollTracker.Read(id);
            Console.WriteLine($"Опитування [{id}]{poll.Title} розпочалося"); //Використання CRUD Read
        };

        //Вивід результатів завершених опитувань
        PollTracker.PollFinished += poll =>
        {
            Console.WriteLine($"Опитування [{poll.Id}] закінчилося");
            Console.WriteLine("Результати опитування:");
            var options = poll.GetOptions();
            foreach (var result in poll.PrevResult())
            {
                var option = options.First(o => o.Id == result.Key);
                Console.WriteLine($"[{result.Key}]{option.Name}: {result.Value}");
            }
        };

        var persons = new ConcurrentBag<Person>();
        Parallel.For(0, 1000, i =>
        {
            persons.Add(Person.CreateNew());
        });

        var rand = new Random();
        FinishedPollProcessor.StartProcessing();
        Parallel.For(0, 1000, i =>
        {
            var poll = Poll.CreateNew<RankedPoll>();
        });

        PollTracker.Save();

        var polls = await PollTracker.ReadAll();

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
    }
}