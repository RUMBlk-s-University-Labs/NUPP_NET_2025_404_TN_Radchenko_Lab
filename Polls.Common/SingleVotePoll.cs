namespace Polls.Common
{
    public class SingleVotePoll : Poll
    {
        private Dictionary<Person, Guid> Votes = new Dictionary<Person, Guid>();

        public SingleVotePoll(string title) : base(title) { }
        //Перевизначений метод
        public override void Vote(Person person, Guid optionId)
        {
            BlockInvalidOption(optionId);

            try
            {
                Votes.Add(person, optionId);
            }
            catch
            {
                throw new ArgumentException("В даному опитування можна зробити вибір тільки один раз!");
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
                result[vote] += 1;
            }

            prevResult = result;
            AnnounceFinish();
            return result;
        }
    }
}
