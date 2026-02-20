namespace VikingJamGame.Models.GameEvents.Commands;

public interface IEventCommand
{
    void Execute(PlayerInfo playerInfo, GameResources gameResources);
}
