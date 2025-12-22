using System.Collections.Concurrent;
using System.Text.Json.Serialization;

namespace Polls.Common
{
    public class Poll : IIdentifiable
    {
        public Guid Id { get; } = Guid.NewGuid();
        public string Title { get; protected set; }
        [JsonInclude]
        protected HashSet<Option> Options = new HashSet<Option>();
        public bool IsOngoing { get; private set; } = false;
        [JsonInclude]
        protected Dictionary<Guid, int> prevResult = new();

        public Poll() { Title = "ToBeSetted"; }

        public Poll(Guid id, string title, HashSet<Option> options, bool isOngoing, Dictionary<Guid, int> prev_result)
        {
            Id = id;
            Title = title;
            Options = options;
            IsOngoing = isOngoing;
            prevResult = prev_result;

            if (IsOngoing) {
                PollConcurrencyLimiter.Enter();
            }
        }

        //Конструктор
        public Poll(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                throw new ArgumentException("Заголовок не може бути порожнім");
            }
            Title = title;

            //PollTracker.RegisterPoll(this);
        }

        //Метод
        private void BlockIfOngoing()
        {
            if (IsOngoing)
            {
                throw new Exception("Неможливо редагувати розпочате опитування!");
            }
        }

        //Метод
        public HashSet<Option> GetOptions()
        {
            return Options;
        }

        //Метод
        public Poll AddOption(Option option)
        {
            BlockIfOngoing();
            Options.Add(option);
            return this;
        }
        //Метод
        public Poll RemoveOption(Option option)
        {
            BlockIfOngoing();
            Options.Remove(option);
            return this;
        }
        //Метод
        public Poll RemoveOptionByIndex(Guid id)
        {
            BlockIfOngoing();
            Options.RemoveWhere(o => o.Id == id);
            return this;
        }

        //Метод
        public Poll Start()
        {
            if (Options.Count == 0)
            {
                throw new Exception("Не можливо розпочати процес голосування без опцій");
            }
            if (IsOngoing)
            {
                throw new Exception("Дане опитування вже розпочате!");
            }
            PollConcurrencyLimiter.Enter();
            IsOngoing = true;
            //PollTracker.PollStartedInvoke(this);
            return this;
        }

        //Метод
        protected void AnnounceFinish()
        {
            IsOngoing = false;
            //PollTracker.UnregisterPoll(this);
            PollConcurrencyLimiter.Exit();
            FinishedPollProcessor.AddPollToQueue(this);
        }
        //Метод
        public virtual Dictionary<Guid, int> Finish()
        {
            AnnounceFinish();
            return new Dictionary<Guid, int>();
        }
        //Метод
        public Dictionary<Guid, int> PrevResult()
        {
            return prevResult;
        }
        //Віртуальний метод
        public virtual void Vote(Person person, Guid optionId) { }
        //Метод
        public void BlockInvalidOption(Guid optionId)
        {
            if (!Options.Contains(new Option(optionId)))
            {
                throw new ArgumentException("Вибрано невірну опцію!");
            }
        }

        public static T CreateNew<T>() where T : Poll, new()
        {
            var poll = new T();
            poll.Title = RandomNames.RandomString.Get(16);
            int num = new Random().Next(6);
            if (num <= 2) num += 3;
            foreach (int i in Enumerable.Range(2, num))
            {
                poll.AddOption(Option.CreateNew());
            }
            //PollTracker.RegisterPoll(poll);
            return poll;
        }

    }

    public static class PollConcurrencyLimiter
    {
        private static readonly Semaphore _semaphore = new Semaphore(5, 5);

        public static void Enter()
        {
            _semaphore.WaitOne();
        }

        public static void Exit()
        {
            _semaphore.Release();
        }
    }

    public static class FinishedPollProcessor
    {
        private static readonly AutoResetEvent _signal = new AutoResetEvent(false);
        private static readonly ConcurrentQueue<Poll> _finishedPolls = new ConcurrentQueue<Poll>();
        private static CancellationTokenSource _cts = new CancellationTokenSource();
        private static readonly ConcurrentBag<int> _allVoteCounts = new ConcurrentBag<int>();

        public static void StartProcessing()
        {
            Task.Run(() =>
            {
                while (!_cts.Token.IsCancellationRequested)
                {
                    _signal.WaitOne();
                    while (_finishedPolls.TryDequeue(out var poll))
                    {
                        var results = poll.PrevResult();
                        if (results != null)
                        {
                            foreach (var num in results.Values)
                            {
                                _allVoteCounts.Add(num);
                            }
                        }
                        //PollTracker.UnregisterPoll(poll);
                    }
                    }
            });
        }

        public static void AddPollToQueue(Poll poll)
        {
            _finishedPolls.Enqueue(poll);
            _signal.Set();
        }

        public static void StopProcessing()
        {
            _cts.Cancel();
            _signal.Set();
        }

        public static void PrintStatistics()
        {
            Console.WriteLine("Статистика по всім голосуванням:");
            if (!_allVoteCounts.Any())
            {
                Console.WriteLine("Кількість голосів: 0");
                return;
            }

            var min = _allVoteCounts.Min();
            var max = _allVoteCounts.Max();
            var avg = _allVoteCounts.Average();

            Console.WriteLine($"Мінімальна кількість голосів за опцію: {min}");
            Console.WriteLine($"Максимальна кількість голосів за опцію: {max}");
            Console.WriteLine($"Середня кількість голосів за опцію: {avg:F2}");
        }
    }
}