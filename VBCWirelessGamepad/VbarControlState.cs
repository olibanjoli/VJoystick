namespace VBCWirelessGamepad;

public class VbarControlState
{
    public int Pitch { get; set; } = 0;
    public int Tail { get; set; } = 0;
    public int Ail { get; set; } = 0;
    public int Elev { get; set; } = 0;

    public int Switches { get; set; }
    public bool MotorOff { get; set; }
    public bool MotorOn { get; set; }
    public bool MotorIdle { get; set; }

    public bool Bank1 { get; set; }
    public bool Bank2 { get; set; }
    public bool Bank3 { get; set; }

    public bool Buddy { get; set; }
    public bool Master { get; set; }

    public bool Option1A { get; set; }
    public bool Option1B { get; set; }
    public bool Option1Middle { get; set; }

    public bool Option2A { get; set; }
    public bool Option2B { get; set; }
    public bool Option2Middle { get; set; }

    public bool Option3A { get; set; }
    public bool Option3B { get; set; }
    public bool Option3Middle { get; set; }

    public bool Option4A { get; set; }
    public bool Option4B { get; set; }
    public bool Option4Middle { get; set; }
}