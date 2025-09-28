namespace Polls.Common
{
    public class RankedPoll : Poll
    {
        private Dictionary<Person, Dictionary<Guid, int>> Votes = new Dictionary<Person, Dictionary<Guid, int>>();

        public RankedPoll(string title) : base(title) { }

        //Перевизначений метод
        public override void Vote(Person person, Guid optionId)
        {
            BlockInvalidOption(optionId);

            if (Votes.ContainsKey(person))
            {
                if (Votes[person].ContainsKey(optionId))
                {
                    throw new ArgumentException("Одну опцію можливо обрати тільки раз!");
                }

                Votes[person].Add(optionId, Options.Count - Votes[person].Count);
            }
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
            AnnounceFinish();
            return result;
        }
    }
}
