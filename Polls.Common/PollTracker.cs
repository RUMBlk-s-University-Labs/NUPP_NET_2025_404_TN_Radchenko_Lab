using System.ComponentModel.Design;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Polls.Common
{
    public static class PollTracker //Статичний клас
    {
        private static readonly GenericCrudServiceAsync<Poll> polls; //Статичне поле

        public delegate void PollCreatedHandler(Poll poll); //Делегат
        public static event PollCreatedHandler? PollCreated; //Подія

        public delegate void PollStartedHandler(Guid id); //Делегат
        public static event PollStartedHandler? PollStarted; //Подія

        public delegate void PollFinishedHandler(Poll poll); //Делегат
        public static event PollFinishedHandler? PollFinished; //Подія

        //Статичний конструктор
        static PollTracker()
        {
            polls = new GenericCrudServiceAsync<Poll>();
        }

        //Статичний метод
        public async static Task<Poll> Read(Guid id)
        {
            return await polls.ReadAsync(id);
        }
        //Статичний метод
        public static async Task<IEnumerable<Poll>> ReadAll()
        {
            return await polls.ReadAllAsync();
        }
        //Статичний метод
        public static void RegisterPoll(Poll poll)
        {
            polls.CreateAsync(poll);
            PollCreated?.Invoke(poll);
        }
        //Статичний метод
        public static void UnregisterPoll(Poll poll)
        {
            polls.RemoveAsync(poll);
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
        public static void Save()
        {
            polls.SaveAsync();
        }
        //Статичний метод
        /*public static void Load(string FilePath)
        {
            polls.Load(FilePath);
        }*/
    }
}
