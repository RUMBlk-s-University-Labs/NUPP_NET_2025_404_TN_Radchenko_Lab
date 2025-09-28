using System.ComponentModel.Design;
using System.Xml.Linq;

namespace Polls.Common
{
    public static class PollTracker //Статичний клас
    {
        private static readonly GenericCrudService<Poll> polls; //Статичне поле

        public delegate void PollCreatedHandler(Poll poll); //Делегат
        public static event PollCreatedHandler? PollCreated; //Подія

        public delegate void PollStartedHandler(Guid id); //Делегат
        public static event PollStartedHandler? PollStarted; //Подія

        public delegate void PollFinishedHandler(Poll poll); //Делегат
        public static event PollFinishedHandler? PollFinished; //Подія

        //Статичний конструктор
        static PollTracker()
        {
            polls = new GenericCrudService<Poll>();
        }

        //Статичний метод
        public static Poll Read(Guid id)
        {
            return polls.Read(id);
        }
        //Статичний метод
        public static IEnumerable<Poll> ReadAll()
        {
            return polls.ReadAll();
        }
        //Статичний метод
        public static void RegisterPoll(Poll poll)
        {
            polls.Create(poll);
            PollCreated?.Invoke(poll);
        }
        //Статичний метод
        public static void UnregisterPoll(Poll poll)
        {
            polls.Remove(poll);
            PollFinished?.Invoke(poll);
        }
        //Статичний метод
        public static void PollStartedInvoke(Poll poll)
        {
            PollStarted?.Invoke(poll.Id);
        }
        //Метод розширення
        public static int PollOptionsLength(Poll poll)
        {
            return poll.GetOptions().Count();
        }
        //Статичний метод
        public static void Save(string FilePath)
        {
            polls.Save(FilePath);
        }
        //Статичний метод
        public static void Load(string FilePath)
        {
            polls.Load(FilePath);
        }
    }
}
