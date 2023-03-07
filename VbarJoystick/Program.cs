// See https://aka.ms/new-console-template for more information

using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Spectre.Console;
using VbarJoystick;
using vJoyInterfaceWrap;


UdpClient? udpClient = null;
const int joystickId = 1;

var joystick = new vJoy();

var iReport = new vJoy.JoystickState
{
    bDevice = joystickId,
};

long maxX = 0;
long maxY = 0;
long maxZ = 0;
long maxRX = 0;
long maxRY = 0;
long maxRZ = 0;
long maxSL0 = 0;
long maxSL1 = 0;

var pitch = 0;
var tail = 0;
var ail = 0;
var elev = 0;

int switches = 0;
var motorOff = false;
var motorOn = false;
var motorIdle = false;

var bank1 = false;
var bank2 = false;
var bank3 = false;

var buddy = false;
var master = false;

var option1A = false;
var option1B = false;
var option1Middle = false;

var option2A = false;
var option2B = false;
var option2Middle = false;

var option3A = false;
var option3B = false;
var option3Middle = !option3A && !option3A;
        
var option4A = false;
var option4B = false;
var option4Middle = !option4A && !option4A;

var sendData = new byte[10];
byte[] receiveBytes;

string GetBitsAsString(int number)
{
    const int IntSize = sizeof(int) * 8; // Number of bits in an int

    // Create a char array to hold the bits, initialize to '0'
    char[] bits = new char[IntSize];
    for (int i = 0; i < IntSize; i++)
    {
        bits[i] = '0';
    }

    // Convert the number to binary and fill in the char array
    int index = IntSize - 1;
    while (number > 0 && index >= 0)
    {
        bits[index] = (number & 1) == 1 ? '1' : '0';
        number >>= 1;
        index--;
    }

    // Convert the char array to a string and return
    return new string(bits);
}

var gamepadManager = new GamepadManager();
var vbarUdpReceiver = new VbarUdpReceiver(gamepadManager);

AnsiConsole.Status()
    .AutoRefresh(true)
    .Spinner(Spinner.Known.Default)
    .Start("[yellow]Initializing...[/]", ctx =>
    {
        gamepadManager.Initialize();
        
  
        // AnsiConsole.MarkupLine("[grey]starting udp client[/]");
        //
        // udpClient = new UdpClient(1025);
        //
        // //AnsiConsole.MarkupLine("waiting for connection from TX...");
        //
        // AnsiConsole.MarkupLine("[green3_1]Ready, please start [dodgerblue1]IP UDP Simulator[/] App on VBar Transmitter[/]");
        // ctx.Status("waiting for connection from VBar TX...");
        //
        // var remoteIpEndPoint = new IPEndPoint(IPAddress.Any, 1025);
        //
        // receiveBytes = udpClient.Receive(ref remoteIpEndPoint);
        //
        // var rule = new Rule($"Connected to TX [blue]Oli[/] [blue dim]1023023[/] (10.0.0.201)");
        // AnsiConsole.Write(rule);

        ctx.Status("[green]Initialized[/]");
    });


AnsiConsole.WriteLine("foo");

vbarUdpReceiver.Run();

var table = new Table()
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

AnsiConsole.Live(table)
    .AutoClear(false)
    .Overflow(VerticalOverflow.Ellipsis)
    .Cropping(VerticalOverflowCropping.Top)
    .Start(context =>
    {
        var remoteIpEndPoint = new IPEndPoint(IPAddress.Any, 1025);

        while (true)
        {
            try
            {
                receiveBytes = udpClient.Receive(ref remoteIpEndPoint);

                var id = receiveBytes[0] & 0xFF | (receiveBytes[1] & 0xFF) << 8;
                var version = receiveBytes[2] & 0xFF;
                var command = receiveBytes[3] & 0xFF;

                if ((id != 0x8c51) || (version != 1))
                {
                    continue;
                }
                
                switch (command)
                {
                    case 1: // plain control data
                        switches = receiveBytes[4] & 0xFF | (receiveBytes[5] & 0xFF) << 8 |
                                       (receiveBytes[6] & 0xFF) << 16
                                       | (receiveBytes[7] & 0xFF) << 24;
                        //Console.WriteLine(Convert.ToString(switches, 2));

                        motorOff = (switches & 0b_0000_0000_0000_0000_0000_0000_0000_0001) != 0;
                        motorOn = (switches & 0b_0000_0000_0000_0000_0000_0000_0000_0010) != 0;
                        motorIdle = !motorOff && !motorOn;

                        bank1 = (switches & 0b_0000_0000_0000_0000_0000_0000_0000_0100) != 0;
                        bank3 = (switches & 0b_0000_0000_0000_0000_0000_0000_0000_1000) != 0;
                        bank2 = !bank1 && !bank3;

                        buddy = (switches & 0b_0000_0000_0000_0000_0000_0000_0001_0000) != 0;
                        master = !buddy;

                        option1A = (switches & 0b_0000_0000_0000_0000_0000_0000_0100_0000) != 0;
                        option1B = (switches & 0b_0000_0000_0000_0000_0000_0000_1000_0000) != 0;
                        option1Middle = !option1A && !option1A;

                        option2A = (switches & 0b_0000_0000_0000_0000_0000_0001_0000_0000) != 0;
                        option2B = (switches & 0b_0000_0000_0000_0000_0000_0010_0000_0000) != 0;
                        option2Middle = !option2A && !option2A;

                        option3A = (switches & 0b_0000_0000_0000_0000_0000_0100_0000_0000) != 0;
                        option3B = (switches & 0b_0000_0000_0000_0000_0000_1000_0000_0000) != 0;
                        option3Middle = !option3A && !option3A;

                        option4A = (switches & 0b_0000_0000_0000_0000_0001_0000_0000_0000) != 0;
                        option4B = (switches & 0b_0000_0000_0000_0000_0010_0000_0000_0000) != 0;
                        option4Middle = !option4A && !option4A;

                        var buttonState = 0;

                        if (motorOff)
                            buttonState |= (1 << 0);

                        if (motorIdle)
                            buttonState |= (1 << 1);

                        if (motorOn)
                            buttonState |= (1 << 2);

                        if (bank1)
                            buttonState |= (1 << 3);

                        if (bank2)
                            buttonState |= (1 << 4);

                        if (bank3)
                            buttonState |= (1 << 5);

                        if (buddy)
                            buttonState |= (1 << 6);

                        if (master)
                            buttonState |= (1 << 7);

                        if (option1A)
                            buttonState |= (1 << 8);

                        if (option1Middle)
                            buttonState |= (1 << 9);

                        if (option1B)
                            buttonState |= (1 << 10);

                        if (option2A)
                            buttonState |= (1 << 11);

                        if (option2Middle)
                            buttonState |= (1 << 12);

                        if (option2B)
                            buttonState |= (1 << 13);

                        if (option3A)
                            buttonState |= (1 << 14);

                        if (option3Middle)
                            buttonState |= (1 << 15);

                        if (option3B)
                            buttonState |= (1 << 16);

                        iReport.Buttons = (uint)buttonState;

                        // main channels center value is 2048
                        ail = receiveBytes[8] & 0xFF | (receiveBytes[9] & 0xFF) << 8;
                        elev = receiveBytes[10] & 0xFF | (receiveBytes[11] & 0xFF) << 8;
                        tail = receiveBytes[12] & 0xFF | (receiveBytes[13] & 0xFF) << 8;
                        pitch = receiveBytes[14] & 0xFF | (receiveBytes[15] & 0xFF) << 8;

                        // aux channels center value is 2048
                        var pot1 = receiveBytes[16] & 0xFF | (receiveBytes[17] & 0xFF) << 8;
                        var pot2 = receiveBytes[18] & 0xFF | (receiveBytes[19] & 0xFF) << 8;
                        var trim1 = receiveBytes[20] & 0xFF | (receiveBytes[21] & 0xFF) << 8;
                        var trim2 = receiveBytes[22] & 0xFF | (receiveBytes[23] & 0xFF) << 8;
                        var trim3 = receiveBytes[24] & 0xFF | (receiveBytes[25] & 0xFF) << 8;
                        var trim4 = receiveBytes[26] & 0xFF | (receiveBytes[27] & 0xFF) << 8;

                        iReport.AxisX = Mikado.MapRange(ail, maxX);
                        iReport.AxisY = Mikado.MapRange(elev, maxY);
                        iReport.AxisZ = Mikado.MapRange(tail, maxZ);
                        iReport.AxisXRot = Mikado.MapRange(pitch, maxRX);

                        iReport.AxisYRot = Mikado.MapRange(pot1, maxRY);
                        iReport.AxisZRot = Mikado.MapRange(pot2, maxRZ);

                        joystick.UpdateVJD(joystickId, ref iReport);

                        break;

                    case 2: // transmitter name packet
                        var serial = receiveBytes[4] & 0xFF | (receiveBytes[5] & 0xFF) << 8 |
                                     (receiveBytes[6] & 0xFF) << 16 |
                                     (receiveBytes[7] & 0xFF) << 24;

                        var name = Encoding.UTF8.GetString(
                            receiveBytes[new Range(9, new Index(receiveBytes[8] + 9, fromEnd: false))]);
                        //Console.WriteLine($"name: {name} serial: {serial:x8}");

                        sendData[0] = 0x51;
                        sendData[1] = 0x8c;
                        sendData[2] = 1; // Version 1
                        sendData[3] = 3; // command for answer My ID
                        sendData[4] = 4; // ID of Simulator

                        udpClient.Send(sendData, remoteIpEndPoint.Address.ToString(), 1026);

                        // live update takes less than a ms and should therefore finish before the next packet arrives
                        table
                            .UpdateCell(0, 1, pitch.ToString())
                            .UpdateCell(1, 1, tail.ToString())
                            .UpdateCell(2, 1, elev.ToString())
                            .UpdateCell(3, 1, ail.ToString())
                            .UpdateCell(4, 1, motorOff ? "off" : motorIdle ? "Idle" : "On")
                            .UpdateCell(5, 1, bank1 ? "1" : bank2 ? "2" : "3" )
                            .UpdateCell(6, 1, option1A ? "A" : option1B ? "B" : "Middle")
                            .UpdateCell(7, 1, option2A ? "A" : option2B ? "B" : "Middle")
                            .UpdateCell(8, 1, option3A ? "A" : option3B ? "B" : "Middle")
                            .UpdateCell(9, 1, option4A ? "A" : option4B ? "B" : "Middle")
                            .UpdateCell(10, 1, master ? "master" : "buddy");

                        context.Refresh();
                        break;

                    default:
                        Console.WriteLine("unknown command");
                        break;
                }
            }
            catch (Exception e)
            {
                AnsiConsole.WriteException(e);
            }
        }
    });