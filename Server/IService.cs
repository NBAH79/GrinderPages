using Grinder;

namespace Server;


   public abstract class IService
    {
        public IManager manager {get;set;}
        public abstract void Register(Listener listener);
    }
