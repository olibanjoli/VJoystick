using System.Diagnostics;
using Spectre.Console;

namespace VBCWirelessGamepad;

public class GamepadManager
{
    private const int JoystickId = 1;

    private vJoy _joystick;
    private vJoy.JoystickState _joystickState;
    private GamepadAxisMaxValues? _ranges;

    public GamepadManager()
    {
        _joystick = new vJoy();

        _joystickState = new vJoy.JoystickState
        {
            bDevice = JoystickId,
        };
    }

    public bool Initialize()
    {
        if (CheckJoystickState() == false)
            return false;

        if (AcquireJoystick() == false)
            return false;

        if (VerifyJoystickConfig() == false)
            return false;

        // CheckVersionMatch();

        return true;
    }

    public bool CheckVJoyInstallation()
    {
        var enabled = _joystick.vJoyEnabled();

        if (enabled == false)
        {
            AnsiConsole.MarkupLine("vJoy Device driver is not installed yet. vJoy Device driver is required to run this software.");
            AnsiConsole.WriteLine();
            
            if (AnsiConsole.Confirm("Do you want to install vJoy Device driver?") == false)
            {
                AnsiConsole.WriteLine();
                AnsiConsole.MarkupLine("Alright, hopefully another time. cya");
                AnsiConsole.WriteLine();
                return false;
            }

            AnsiConsole.Status()
                .AutoRefresh(true)
                .Spinner(Spinner.Known.Default)
                .Start("[yellow]Waiting for installer to finish...[/]", ctx =>
                {
                    var setup = Process.Start("./vJoySetup.exe", "/SILENT");

                    setup.WaitForExit();
                });

            return true;
        }

        return true;
    }

    private bool CheckJoystickState()
    {
        AnsiConsole.MarkupLine("[grey]checking Joystick #1 state [/]");

        var joystickStatus = _joystick.GetVJDStatus(JoystickId);

        if (joystickStatus != VjdStat.VJD_STAT_FREE)
        {
            AnsiConsole.MarkupLine("[red]error: [/] Joystick #1 is not free.");
            return false;
        }

        return true;
    }

    private bool AcquireJoystick()
    {
        AnsiConsole.MarkupLine("[grey]acquiring joystick [/]");

        if (_joystick.AcquireVJD(JoystickId) == false)
        {
            AnsiConsole.MarkupLine("[red]error: [/] failed to acquire vJoy device #1");
            return false;
        }

        return true;
    }

    private bool VerifyJoystickConfig()
    {
        AnsiConsole.MarkupLine("[grey]reading joystick config[/]");

        // TODO: validate axes & button count
        var AxisX = _joystick.GetVJDAxisExist(JoystickId, HID_USAGES.HID_USAGE_X);
        var AxisY = _joystick.GetVJDAxisExist(JoystickId, HID_USAGES.HID_USAGE_Y);
        var AxisZ = _joystick.GetVJDAxisExist(JoystickId, HID_USAGES.HID_USAGE_Z);
        var AxisRX = _joystick.GetVJDAxisExist(JoystickId, HID_USAGES.HID_USAGE_RX);
        var AxisRY = _joystick.GetVJDAxisExist(JoystickId, HID_USAGES.HID_USAGE_RY);
        var AxisRZ = _joystick.GetVJDAxisExist(JoystickId, HID_USAGES.HID_USAGE_RZ);

        var buttonCount = _joystick.GetVJDButtonNumber(JoystickId);

        long maxX = 0;
        long maxY = 0;
        long maxZ = 0;
        long maxRX = 0;
        long maxRY = 0;
        long maxRZ = 0;
        long maxSL0 = 0;
        long maxSL1 = 0;

        _joystick.GetVJDAxisMax(JoystickId, HID_USAGES.HID_USAGE_X, ref maxX);
        _joystick.GetVJDAxisMax(JoystickId, HID_USAGES.HID_USAGE_Y, ref maxY);
        _joystick.GetVJDAxisMax(JoystickId, HID_USAGES.HID_USAGE_Z, ref maxZ);
        _joystick.GetVJDAxisMax(JoystickId, HID_USAGES.HID_USAGE_RX, ref maxRX);
        _joystick.GetVJDAxisMax(JoystickId, HID_USAGES.HID_USAGE_RY, ref maxRY);
        _joystick.GetVJDAxisMax(JoystickId, HID_USAGES.HID_USAGE_RZ, ref maxRZ);
        _joystick.GetVJDAxisMax(JoystickId, HID_USAGES.HID_USAGE_SL0, ref maxSL0);
        _joystick.GetVJDAxisMax(JoystickId, HID_USAGES.HID_USAGE_SL1, ref maxSL1);

        _ranges = new GamepadAxisMaxValues
        {
            X = maxX,
            Y = maxY,
            Z = maxZ,
            RX = maxRX,
            RY = maxRY,
            RZ = maxRZ,
            SL0 = maxSL0,
            SL1 = maxSL1,
        };

        return true;
    }

    private void CheckVersionMatch()
    {
        uint dllVer = 0, drvVer = 0;
        var match = _joystick.DriverMatch(ref dllVer, ref drvVer);

        if (match)
            AnsiConsole.MarkupLine("[grey]Version of Driver Matches DLL Version ({0:X})[/]\n", dllVer);
        else
            AnsiConsole.MarkupLine("[grey]Version of Driver ({0:X}) does NOT match DLL Version ({1:X})[/]\n", drvVer,
                dllVer);
    }

    public void ApplyToGamepad(VbarControlState state)
    {
        var buttonState = 0;

        if (state.MotorOff)
            buttonState |= (1 << 0);

        if (state.MotorIdle)
            buttonState |= (1 << 1);

        if (state.MotorOn)
            buttonState |= (1 << 2);

        if (state.Bank1)
            buttonState |= (1 << 3);

        if (state.Bank2)
            buttonState |= (1 << 4);

        if (state.Bank3)
            buttonState |= (1 << 5);

        if (state.Buddy)
            buttonState |= (1 << 6);

        if (state.Master)
            buttonState |= (1 << 7);

        if (state.Option1A)
            buttonState |= (1 << 8);

        if (state.Option1Middle)
            buttonState |= (1 << 9);

        if (state.Option1B)
            buttonState |= (1 << 10);

        if (state.Option2A)
            buttonState |= (1 << 11);

        if (state.Option2Middle)
            buttonState |= (1 << 12);

        if (state.Option2B)
            buttonState |= (1 << 13);

        if (state.Option3A)
            buttonState |= (1 << 14);

        if (state.Option3Middle)
            buttonState |= (1 << 15);

        if (state.Option3B)
            buttonState |= (1 << 16);

        _joystickState.Buttons = (uint)buttonState;

        _joystickState.AxisX = Mikado.MapRange(state.Ail, _ranges!.X);
        _joystickState.AxisY = Mikado.MapRange(state.Elev, _ranges.Y);
        _joystickState.AxisZ = Mikado.MapRange(state.Tail, _ranges.Z);
        _joystickState.AxisXRot = Mikado.MapRange(state.Pitch, _ranges.RX);

        // TODO: add missing controls
        // _iReport.AxisYRot = Mikado.MapRange(pot1, maxRY);
        // _iReport.AxisZRot = Mikado.MapRange(pot2, maxRZ);

        _joystick.UpdateVJD(JoystickId, ref _joystickState);
    }
}

public class GamepadAxisMaxValues
{
    public long X { get; set; }
    public long Y { get; set; }
    public long Z { get; set; }
    public long RX { get; set; }
    public long RY { get; set; }
    public long RZ { get; set; }
    public long SL0 { get; set; }
    public long SL1 { get; set; }
}