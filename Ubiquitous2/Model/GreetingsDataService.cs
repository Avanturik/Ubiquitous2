using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.ServiceLocation;

namespace UB.Model
{
    public class GreetingsDataService : IGreetingsDataService
    {
        IChatDataService chatDataService;
        List<Greeting> greetingsQueue = new List<Greeting>();
        List<Greeting> sentGreetings = new List<Greeting>();
        private object lockQueue = new object();

        public GreetingsDataService()
        {
            chatDataService = ServiceLocator.Current.GetInstance<IChatDataService>();
            foreach( var chat in chatDataService.Chats )
            {
                if( chat is IFollowersProvider )
                {
                    (chat as IFollowersProvider).AddFollower = (user) => {
                        lock( greetingsQueue)
                            greetingsQueue.Add(new Greeting("Thank you for following me!", user.NickName, user.ChatName));
                    };
                }
            }
        }

        public void GetGreetings(Action<Greeting> callback)
        {
            if( callback != null && greetingsQueue.Count > 0)
            {       
                lock( greetingsQueue )
                {
                    greetingsQueue.RemoveAll( greeting => sentGreetings.Any(g =>  g.ChatName == greeting.ChatName &&
                        g.Message == greeting.Message &&
                        g.Title == greeting.Title ));

                    if( greetingsQueue.Count > 0 )
                    {
                        sentGreetings.Add(greetingsQueue.First());
                        callback(greetingsQueue.First());
                    }
                }
            }
        }
    }
}
