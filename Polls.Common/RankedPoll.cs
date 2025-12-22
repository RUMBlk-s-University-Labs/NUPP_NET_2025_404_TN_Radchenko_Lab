using System.Collections.Concurrent;
using System.Runtime.InteropServices.Swift;

namespace Polls.Common
{
    public class RankedPoll : Poll
    {
        private ConcurrentDictionary<Person, ConcurrentDictionary<Guid, int>> Votes = new ConcurrentDictionary<Person, ConcurrentDictionary<Guid, int>>();
        public RankedPoll() {}
        public RankedPoll(string title) : base(title) { }
        public RankedPoll(Guid id, string title, HashSet<Option> options, bool isOngoing, Dictionary<Guid, int> prev_result, Dictionary<Person, Dictionary<Guid, int>> votes)
            : base(id, title, options, isOngoing, prev_result) { }

        //Перевизначений метод
        public override void Vote(Person person, Guid optionId)
        {
            BlockInvalidOption(optionId);

            if (!Votes.ContainsKey(person))
            {
                Votes.TryAdd(person, new ConcurrentDictionary<Guid, int>());
            }

            if (Votes[person].ContainsKey(optionId))
            {
                throw new ArgumentException("Одну опцію можливо обрати тільки раз!");
            }

            Votes[person].TryAdd(optionId, Options.Count - Votes[person].Count);
        }

        //Перевизначений метод
        public override Dictionary<Guid, int> Finish()
        {
            var result = new Dictionary<Guid, int>();

            foreach (var Option in GetOptions())
            {
                result[Option.Id] = 0;
            }

            foreach (var vote in Votes.Values)
            {
                foreach (var rank in vote)
                {
                    result[rank.Key] += rank.Value;
                }
            }
            prevResult = result;
            base.Finish();
            return result;
        }

        public ConcurrentDictionary<Person, ConcurrentDictionary<Guid, int>> GetVotes()
        {
            return Votes;
        }
    }
}
