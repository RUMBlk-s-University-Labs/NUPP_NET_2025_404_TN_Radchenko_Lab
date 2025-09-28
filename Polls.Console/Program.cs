using System;
using Polls.Common;

class Program
{
    static void Main()
    {
        //Відстежування подій
        PollTracker.PollCreated += poll =>
        {
            Console.WriteLine($"Створено нове опитування: [{poll.Id}]{poll.Title}");
        };

        PollTracker.PollStarted += id =>
        {
            Console.WriteLine($"Опитування [{id}]{PollTracker.Read(id)} розпочалося"); //Використання CRUD Read
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

        var AA1 = new SingleVotePoll("Опитування АА1"); //Створення нового опитування | CRUD Create через виклик методу PollTracker.RegisterPoll() в конструкторі
        var Option1 = new Option("Йоа"); //Створення опцій
        var Option2 = new Option("LJFD");

        AA1
            .AddOption(Option1) //прикріплення опцій
            .AddOption(Option2)
            .Start(); //Розпочаток опитування

        var Person1 = new Person("Іван"); //Створення нових персон
        var Person2 = new Person("Петя");
        var Person3 = new Person("Вася");

        AA1.Vote(Person1, Option1.Id); //Голосування новими персонами
        AA1.Vote(Person2, Option2.Id);
        AA1.Vote(Person3, Option1.Id);

        Console.WriteLine("Список опитувань:"); 
        foreach (var poll in PollTracker.ReadAll()) //CRUD ReadAll
        {
            Console.WriteLine($"[{poll.Id}]{poll.Title}");
        }

        PollTracker.Save("polls.json"); //CRUD Save
        AA1.Finish(); //CRUD Remove через виклик PollTracker.UnregisterPoll() в даному методі
        PollTracker.Load("polls.json"); //CRUD Load

        Console.WriteLine("Список опитувань після відновлення:"); //Опитування після закінчення виключаються з PollTracker
        foreach (var poll in PollTracker.ReadAll()) //CRUD ReadAll
        {
            Console.WriteLine($"[{poll.Id}]{poll.Title}");
        }
    }
}