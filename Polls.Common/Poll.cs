using System.Text.Json.Serialization;

namespace Polls.Common
{
    public class Poll: IIdentifiable
    {
        public Guid Id { get; } = Guid.NewGuid();
        public string Title { get; protected set; }
        [JsonInclude]
        protected HashSet<Option> Options = new HashSet<Option>();
        public bool IsOngoing { get; private set; } = false;
        [JsonInclude]
        protected Dictionary<Guid, int> prevResult = new();

        //Конструктор
        public Poll(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                throw new ArgumentException("Заголовок не може бути порожнім");
            }
            Title = title;

            PollTracker.RegisterPoll(this);
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
            IsOngoing = true;
            PollTracker.PollStartedInvoke(this);
            return this;
        }

        //Метод
        protected void AnnounceFinish()
        {
            IsOngoing = false;
            PollTracker.UnregisterPoll(this);
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

    }
}