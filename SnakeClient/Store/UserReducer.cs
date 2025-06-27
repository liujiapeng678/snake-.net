using Fluxor;

namespace SnakeClient.Store
{
    public static class UserReducers
    {
        [ReducerMethod]
        public static UserState ReduceUpdateUserAction(UserState state, UpdateUserAction action)
        {
            return new UserState(
                action.Id,
                action.Username,
                action.Photo,
                state.Token,
                action.IsLogin
            );
        }
    }
}
