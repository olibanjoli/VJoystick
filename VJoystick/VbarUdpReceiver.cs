﻿using System.Net;
using System.Net.Sockets;
using System.Text;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace VJoystick;

public class VbarUdpReceiver
{
    private readonly GamepadManager _gamepadManager;
    private UdpClient? _udpClient;

    private byte[] _sendData = new byte[10];
    private VbarControlState _state;
    private IPEndPoint? _remoteIpEndPoint;
    private int _txSerial;
    private string _txName = string.Empty;
    private Table? _table;

    public VbarUdpReceiver(GamepadManager gamepadManager)
    {
        _gamepadManager = gamepadManager;
        _state = new VbarControlState();

        _sendData[0] = 0x51;
        _sendData[1] = 0x8c;
        _sendData[2] = 1; // Version 1
        _sendData[3] = 3; // command for answer My ID
        _sendData[4] = 4; // ID of Simulator
    }

    public void Run()
    {
        var receiveBytes = Array.Empty<byte>();

        AnsiConsole.Status()
            .AutoRefresh(true)
            .Spinner(Spinner.Known.Default)
            .Start("[grey]starting udp client[/]", ctx =>
            {
                _udpClient = new UdpClient(1025);

                _remoteIpEndPoint = new IPEndPoint(IPAddress.Any, 1025);

                ctx.Status = "[yellow]waiting for connection from VBar TX...[/]";

                receiveBytes = _udpClient.Receive(ref _remoteIpEndPoint);
            });

        HandleMessage(receiveBytes);

        _table = new Table()
            .Border(TableBorder.Rounded)
            .AddColumn("Channel")
            .AddColumn("Value")
            .AddRow("Pitch", "0")
            .AddRow("Tail", "0")
            .AddRow("Elevator", "0")
            .AddRow("Aileron", "0")
            .AddRow("Motor")
            .AddRow("Bank")
            .AddRow("Option 1")
            .AddRow("Option 2")
            .AddRow("Option 3")
            .AddRow("Option 4")
            .AddRow("Buddy");

        var rows = new Rows(new List<IRenderable>
        {
            new Rule(
                $"Connected to TX [blue]{_txName}[/] [blue3_1]{_txSerial:x8}[/] ({_remoteIpEndPoint?.Address})"),
            _table,
            new Markup(Environment.NewLine +
                       "[lime]Ready![/] Your VBarControl is now controlling the Game Controller [yellow]vJoy Device[/]" +
                       Environment.NewLine),
            new Rule(
                $"by Oliver Zürcher")
            {
                Justification = Justify.Right
            },
        });

        AnsiConsole.Live(rows)
            .AutoClear(false)
            .Overflow(VerticalOverflow.Ellipsis)
            .Cropping(VerticalOverflowCropping.Top)
            .Start(context =>
            {
                while (true)
                {
                    receiveBytes = _udpClient!.Receive(ref _remoteIpEndPoint);
                    HandleMessage(receiveBytes);

                    context.Refresh();
                }
            });
    }

    private void HandleMessage(byte[] receiveBytes)
    {
        var id = receiveBytes[0] & 0xFF | (receiveBytes[1] & 0xFF) << 8;
        var version = receiveBytes[2] & 0xFF;
        var command = receiveBytes[3] & 0xFF;

        if ((id != 0x8c51) || (version != 1))
        {
            return;
        }

        switch (command)
        {
            case 1: // plain control data
                HandleControlData(receiveBytes);
                break;

            case 2: // transmitter name packet
                HandleTransmitterNamePacket(receiveBytes);
                break;

            default:
                Console.WriteLine("unknown command");
                break;
        }
    }

    private void HandleControlData(IReadOnlyList<byte> receiveBytes)
    {
        _state.Switches = receiveBytes[4] & 0xFF | (receiveBytes[5] & 0xFF) << 8 |
                          (receiveBytes[6] & 0xFF) << 16
                          | (receiveBytes[7] & 0xFF) << 24;

        _state.MotorOff = (_state.Switches & 0b_0000_0000_0000_0000_0000_0000_0000_0001) != 0;
        _state.MotorOn = (_state.Switches & 0b_0000_0000_0000_0000_0000_0000_0000_0010) != 0;
        _state.MotorIdle = _state is { MotorOff: false, MotorOn: false };

        _state.Bank1 = (_state.Switches & 0b_0000_0000_0000_0000_0000_0000_0000_0100) != 0;
        _state.Bank3 = (_state.Switches & 0b_0000_0000_0000_0000_0000_0000_0000_1000) != 0;
        _state.Bank2 = _state is { Bank1: false, Bank3: false };

        _state.Buddy = (_state.Switches & 0b_0000_0000_0000_0000_0000_0000_0001_0000) != 0;
        _state.Master = !_state.Buddy;

        _state.Option1A = (_state.Switches & 0b_0000_0000_0000_0000_0000_0000_0100_0000) != 0;
        _state.Option1B = (_state.Switches & 0b_0000_0000_0000_0000_0000_0000_1000_0000) != 0;
        _state.Option1Middle = _state is { Option1A: false, Option1B: false };

        _state.Option2A = (_state.Switches & 0b_0000_0000_0000_0000_0000_0001_0000_0000) != 0;
        _state.Option2B = (_state.Switches & 0b_0000_0000_0000_0000_0000_0010_0000_0000) != 0;
        _state.Option2Middle = _state is { Option2A: false, Option2B: false };

        _state.Option3A = (_state.Switches & 0b_0000_0000_0000_0000_0000_0100_0000_0000) != 0;
        _state.Option3B = (_state.Switches & 0b_0000_0000_0000_0000_0000_1000_0000_0000) != 0;
        _state.Option3Middle = _state is { Option3A: false, Option3B: false };

        _state.Option4A = (_state.Switches & 0b_0000_0000_0000_0000_0001_0000_0000_0000) != 0;
        _state.Option4B = (_state.Switches & 0b_0000_0000_0000_0000_0010_0000_0000_0000) != 0;
        _state.Option4Middle = _state is { Option4A: false, Option4B: false };

        // main channels center value is 2048
        _state.Ail = receiveBytes[8] & 0xFF | (receiveBytes[9] & 0xFF) << 8;
        _state.Elev = receiveBytes[10] & 0xFF | (receiveBytes[11] & 0xFF) << 8;
        _state.Tail = receiveBytes[12] & 0xFF | (receiveBytes[13] & 0xFF) << 8;
        _state.Pitch = receiveBytes[14] & 0xFF | (receiveBytes[15] & 0xFF) << 8;

        // aux channels center value is 2048
        _state.Pot1 = receiveBytes[16] & 0xFF | (receiveBytes[17] & 0xFF) << 8;
        _state.Pot2 = receiveBytes[18] & 0xFF | (receiveBytes[19] & 0xFF) << 8;
        _state.Trim1 = receiveBytes[20] & 0xFF | (receiveBytes[21] & 0xFF) << 8;
        _state.Trim2 = receiveBytes[22] & 0xFF | (receiveBytes[23] & 0xFF) << 8;
        _state.Trim3 = receiveBytes[24] & 0xFF | (receiveBytes[25] & 0xFF) << 8;
        _state.Trim4 = receiveBytes[26] & 0xFF | (receiveBytes[27] & 0xFF) << 8;

        _gamepadManager.ApplyToGamepad(_state);

        // live update takes less than a ms and should therefore
        // finish before the next packet arrives
        UpdateTable();
    }

    private void HandleTransmitterNamePacket(byte[] receiveBytes)
    {
        _txSerial = receiveBytes[4] & 0xFF | (receiveBytes[5] & 0xFF) << 8 |
                    (receiveBytes[6] & 0xFF) << 16 |
                    (receiveBytes[7] & 0xFF) << 24;

        _txName = Encoding.UTF8.GetString(
            receiveBytes[new Range(9, new Index(receiveBytes[8] + 9, fromEnd: false))]);

        _udpClient!.Send(_sendData, _remoteIpEndPoint!.Address.ToString(), 1026);
    }

    private void UpdateTable()
    {
        const string a = "[blue]A[/]";
        const string b = "[deeppink2]B[/]";
        const string middle = "[yellow]Middle[/]";

        if (_table != null)
        {
            _table
                .UpdateCell(0, 1, _state.Pitch.ToString())
                .UpdateCell(1, 1, _state.Tail.ToString())
                .UpdateCell(2, 1, _state.Elev.ToString())
                .UpdateCell(3, 1, _state.Ail.ToString())
                .UpdateCell(4, 1,
                    _state.MotorOff ? "[red]off[/]" : _state.MotorIdle ? "[yellow]Idle[/]" : "[lime]On[/]")
                .UpdateCell(5, 1, _state.Bank1 ? "1" : _state.Bank2 ? "2" : "3")
                .UpdateCell(6, 1, _state.Option1A ? a : _state.Option1B ? b : middle)
                .UpdateCell(7, 1, _state.Option2A ? a : _state.Option2B ? b : middle)
                .UpdateCell(8, 1, _state.Option3A ? a : _state.Option3B ? b : middle)
                .UpdateCell(9, 1, _state.Option4A ? a : _state.Option4B ? b : middle)
                .UpdateCell(10, 1, _state.Master ? "master" : "buddy");
        }
    }
}