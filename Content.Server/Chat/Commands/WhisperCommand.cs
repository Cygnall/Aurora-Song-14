using Content.Server.Chat.Systems;
using Content.Shared.Administration;
using Robust.Shared.Console;
using Robust.Shared.Enums;

namespace Content.Server.Chat.Commands
{
    [AnyCommand]
    internal sealed class WhisperCommand : LocalizedEntityCommands
    {
        [Dependency] private readonly ChatSystem _chatSystem = default!;
        public override string Command => "whisper";

        public override void Execute(IConsoleShell shell, string argStr, string[] args)
        {
            if (shell.Player is not { } player)
            {
                shell.WriteError(Loc.GetString("shell-cannot-run-command-from-server"));
                return;
            }

            if (player.Status != SessionStatus.InGame)
                return;

            if (player.AttachedEntity is not {} playerEntity)
            {
                shell.WriteError(Loc.GetString($"shell-must-be-attached-to-entity"));
                return;
            }

            if (args.Length < 1)
                return;

            var message = string.Join(" ", args).Trim();
            if (string.IsNullOrEmpty(message))
                return;

            IoCManager.Resolve<IEntitySystemManager>().GetEntitySystem<ChatSystem>()
                // ChatTransmitRange.Normal < ChatTransmitRange.NoGhosts | Should hide whispers from ghosts | Aurora
                .TrySendInGameICMessage(playerEntity, message, InGameICChatType.Whisper, ChatTransmitRange.NoGhosts, false, shell, player);
        }
    }
}
