using SignalMenu.Managers.DiscordRPC.RPC.Payload;

namespace SignalMenu.Managers.DiscordRPC.RPC.Commands
{
    internal interface ICommand
	{
		IPayload PreparePayload(long nonce);
	}
}
