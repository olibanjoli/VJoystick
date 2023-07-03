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

    var initialized = false;

    AnsiConsole.Status()
        .AutoRefresh(false)
        .Spinner(Spinner.Known.Default)
        .Start("[yellow]Initializing...[/]", ctx => { initialized = gamepadManager.Initialize(); });

    if (initialized)
    {
        vbarUdpReceiver.Run();
    }
}
catch (Exception e)
{
    AnsiConsole.WriteException(e);
    Console.ReadLine();
}