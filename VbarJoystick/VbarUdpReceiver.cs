using System.Net;
using System.Net.Sockets;
using System.Text;
using Spectre.Console;

namespace VbarJoystick;

public class VbarUdpReceiver
{
    private readonly GamepadManager _gamepadManager;
    private UdpClient? _udpClient;

    private byte[] _sendData = new byte[10];
    private VbarControlState _state;
    private IPEndPoint? _remoteIpEndPoint;

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
        _udpClient = new UdpClient(1025);

        AnsiConsole.Status()
            .AutoRefresh(true)
            .Spinner(Spinner.Known.Default)
            .Start("[yellow]waiting for connection from VBar TX...[/]", ctx =>
            {
                _remoteIpEndPoint = new IPEndPoint(IPAddress.Any, 1025);

                var receiveBytes = _udpClient.Receive(ref _remoteIpEndPoint);
                
                HandleMessage(receiveBytes);

                var rule = new Rule($"Connected to TX [blue]Oli[/] [blue dim]1023023[/] (10.0.0.201)");
                AnsiConsole.Write(rule);
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
        _state.Option1Middle = !_state.Option1A && !_state.Option1A;

        _state.Option2A = (_state.Switches & 0b_0000_0000_0000_0000_0000_0001_0000_0000) != 0;
        _state.Option2B = (_state.Switches & 0b_0000_0000_0000_0000_0000_0010_0000_0000) != 0;
        _state.Option2Middle = !_state.Option2A && !_state.Option2A;

        _state.Option3A = (_state.Switches & 0b_0000_0000_0000_0000_0000_0100_0000_0000) != 0;
        _state.Option3B = (_state.Switches & 0b_0000_0000_0000_0000_0000_1000_0000_0000) != 0;
        _state.Option3Middle = !_state.Option3A && !_state.Option3A;

        _state.Option4A = (_state.Switches & 0b_0000_0000_0000_0000_0001_0000_0000_0000) != 0;
        _state.Option4B = (_state.Switches & 0b_0000_0000_0000_0000_0010_0000_0000_0000) != 0;
        _state.Option4Middle = !_state.Option4A && !_state.Option4A;

        // main channels center value is 2048
        _state.Ail = receiveBytes[8] & 0xFF | (receiveBytes[9] & 0xFF) << 8;
        _state.Elev = receiveBytes[10] & 0xFF | (receiveBytes[11] & 0xFF) << 8;
        _state.Tail = receiveBytes[12] & 0xFF | (receiveBytes[13] & 0xFF) << 8;
        _state.Pitch = receiveBytes[14] & 0xFF | (receiveBytes[15] & 0xFF) << 8;

        // aux channels center value is 2048
        var pot1 = receiveBytes[16] & 0xFF | (receiveBytes[17] & 0xFF) << 8;
        var pot2 = receiveBytes[18] & 0xFF | (receiveBytes[19] & 0xFF) << 8;
        var trim1 = receiveBytes[20] & 0xFF | (receiveBytes[21] & 0xFF) << 8;
        var trim2 = receiveBytes[22] & 0xFF | (receiveBytes[23] & 0xFF) << 8;
        var trim3 = receiveBytes[24] & 0xFF | (receiveBytes[25] & 0xFF) << 8;
        var trim4 = receiveBytes[26] & 0xFF | (receiveBytes[27] & 0xFF) << 8;

        _gamepadManager.ApplyToGamepad(_state);
    }

    private void HandleTransmitterNamePacket(byte[] receiveBytes)
    {
        var serial = receiveBytes[4] & 0xFF | (receiveBytes[5] & 0xFF) << 8 |
                     (receiveBytes[6] & 0xFF) << 16 |
                     (receiveBytes[7] & 0xFF) << 24;

        var name = Encoding.UTF8.GetString(
            receiveBytes[new Range(9, new Index(receiveBytes[8] + 9, fromEnd: false))]);
        //Console.WriteLine($"name: {name} serial: {serial:x8}");


        _udpClient!.Send(_sendData, _remoteIpEndPoint.Address.ToString(), 1026);

        // live update takes less than a ms and should therefore finish before the next packet arrives
        // table
        //     .UpdateCell(0, 1, pitch.ToString())
        //     .UpdateCell(1, 1, tail.ToString())
        //     .UpdateCell(2, 1, elev.ToString())
        //     .UpdateCell(3, 1, ail.ToString())
        //     .UpdateCell(4, 1, motorOff ? "off" : motorIdle ? "Idle" : "On")
        //     .UpdateCell(5, 1, bank1 ? "1" : bank2 ? "2" : "3" )
        //     .UpdateCell(6, 1, _state.Option1A ? "A" : _state.Option1B ? "B" : "Middle")
        //     .UpdateCell(7, 1, _state.Option2A ? "A" : _state.Option2B ? "B" : "Middle")
        //     .UpdateCell(8, 1, _state.Option3A ? "A" : _state.Option3B ? "B" : "Middle")
        //     .UpdateCell(9, 1, option4A ? "A" : option4B ? "B" : "Middle")
        //     .UpdateCell(10, 1, master ? "master" : "buddy");
        //
        // context.Refresh();
    }
}