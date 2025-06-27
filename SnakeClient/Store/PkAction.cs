using System.Net.WebSockets;

namespace SnakeClient.Store
{
    
    public class UpdateStatusAction
    {
        public string Status { get; }

        public UpdateStatusAction(string status)
        {
            Status = status;
        }
    }
    
    public class UpdateSocketAction
    {
        public ClientWebSocket Socket { get; }

        public UpdateSocketAction(ClientWebSocket socket)
        {
            Socket = socket;
        }
    }

    public class UpdateOpponentAction
    {
        public string OpponentName { get; }
        public string OpponentPhoto {  get; }

        public UpdateOpponentAction(string opponentName, string opponentPhoto)
        {
            OpponentName = opponentName;
            OpponentPhoto = opponentPhoto;
        }
    }
    public class UpdateMeAction
    {
        public string Me { get; }
        public UpdateMeAction(char me)
        {
            if(me == 'A')
            {
                Me = "蓝";
            } 
            else
            {
                Me = "红";
            }
        }
    }
    public class UpdateGameAction
    {
        public UpdateGameAction()
        {

        }
    }
    public class UpdateGameObjectAction
    {
        public UpdateGameObjectAction()
        {

        }
    }
    public class UpdateLoserAction
    {
        public string Loser { get; }
        public UpdateLoserAction(string loser)
        {
            Loser = loser;
        }
    }
    public class UpdateGoToAction
    {
        public bool GoToPlayground { get; }
        public UpdateGoToAction(bool goToPlayground)
        {
            GoToPlayground = goToPlayground;
        }
    }
}
