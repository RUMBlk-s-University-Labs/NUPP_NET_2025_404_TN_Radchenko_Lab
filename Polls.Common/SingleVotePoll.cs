using System.Collections.Concurrent;

namespace Polls.Common
{
    public class SingleVotePoll : Poll
    {
        private ConcurrentDictionary<Person, Guid> Votes = new ConcurrentDictionary<Person, Guid>();

        public SingleVotePoll(string title) : base(title) { }
        public SingleVotePoll(Guid id, string title, HashSet<Option> options, bool isOngoing, Dictionary<Guid, int> prev_result, Dictionary<Person, Guid> votes)
            : base(id, title, options, isOngoing, prev_result)
        {
            Votes = new ConcurrentDictionary<Person, Guid>(votes);
        }
        //Перевизначений метод
        public override void Vote(Person person, Guid optionId)
        {
            BlockInvalidOption(optionId);

            try
            {
                Votes.TryAdd(person, optionId);
            }
            catch
            {
                throw new ArgumentException("В даному опитування можна зробити вибір тільки один раз!");
            }
        }
        //Перевизначений метод
        public override Dictionary<Guid, int> Finish()
        {
            if (!IsOngoing) { throw new ArgumentException("Опитування не розпочате!"); }
            var result = new Dictionary<Guid, int>();

            foreach (var Option in GetOptions())
            {
                result[Option.Id] = 0;
            }

            foreach (var vote in Votes.Values)
            {
                result[vote] += 1;
            }

            prevResult = result;
            AnnounceFinish();
            return result;
        }

        public ConcurrentDictionary<Person, Guid> GetVotes()
        {
            return Votes;
        }
    }
}
