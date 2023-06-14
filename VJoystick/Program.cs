using Spectre.Console;
using VJoystick;

try
{
    var gamepadManager = new GamepadManager();
    var vbarUdpReceiver = new VbarUdpReceiver(gamepadManager);

    if (gamepadManager.CheckVJoyInstallation() == false)
    {
        Console.ReadLine();
        return;
    }

    AnsiConsole.Status()
        .AutoRefresh(false)
        .Spinner(Spinner.Known.Default)
        .Start("[yellow]Initializing...[/]", ctx => { gamepadManager.Initialize(); });

    vbarUdpReceiver.Run();
}
catch (Exception e)
{
    AnsiConsole.WriteException(e);
    Console.ReadLine();
}