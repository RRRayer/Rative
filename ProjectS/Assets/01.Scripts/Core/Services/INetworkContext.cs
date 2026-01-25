namespace PS.Core.Services
{
    public interface INetworkContext
    {
        bool IsConnected { get; }
        bool IsHost { get; }
        int LocalPlayerId { get; }
    }
}
