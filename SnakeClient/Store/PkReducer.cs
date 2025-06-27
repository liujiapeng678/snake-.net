using Fluxor;

namespace SnakeClient.Store
{
    public static class PkReducer
    {
        [ReducerMethod]
        public static PkState ReduceUpdateOpponentAction(PkState state, UpdateOpponentAction action)
        {
            return new PkState(
                state.Status,
                state.Socket,
                state.Me,
                action.OpponentName,
                action.OpponentPhoto,
                state.Map,
                state.AId,
                state.ASx,
                state.ASy,
                state.BId,
                state.BSx,
                state.BSy,
                state.Game,
                state.Loser,
                state.SelectedBot,
                state.SelectedBotRating,
                state.GoToPlayground
            );
        }
        [ReducerMethod]
        public static PkState ReduceUpdateStatusAction(PkState state, UpdateStatusAction action)
        {
            return new PkState(
                action.Status,
                state.Socket,
                state.Me,
                state.OpponentName,
                state.OpponentPhoto,
                state.Map,
                state.AId,
                state.ASx,
                state.ASy,
                state.BId,
                state.BSx,
                state.BSy,
                state.Game,
                state.Loser,
                state.SelectedBot,
                state.SelectedBotRating,
                state.GoToPlayground
            );
        }
        [ReducerMethod]
        public static PkState ReduceUpdateGoToAction(PkState state, UpdateGoToAction action)
        {
            return new PkState(
                state.Status,
                state.Socket,
                state.Me,
                state.OpponentName,
                state.OpponentPhoto,
                state.Map,
                state.AId,
                state.ASx,
                state.ASy,
                state.BId,
                state.BSx,
                state.BSy,
                state.Game,
                state.Loser,
                state.SelectedBot,
                state.SelectedBotRating,
                action.GoToPlayground
            );
        }
    }
}
