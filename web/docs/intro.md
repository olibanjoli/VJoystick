---
sidebar_position: 1
---

# Intro

The VBC Wireless Gamepad application enables wireless control of a generic Windows game controller using a VBarControl Touch, allowing it to be used with a wide range of flight simulators and games. While some simulators natively support the protocol developed by Mikado, they remain in the minority. The VBC Wireless Gamepad fills this gap by providing a versatile solution that can be used with any simulator or game that supports Windows joysticks.

### Limitations

It's important to note that Direct Input, the input API used by this application, has a maximum support for only 8 axes. As a result, not all the axes on the VBarControl Touch are available for use with the software. The following axes are currently available:

- Pitch
- Tail
- Ailerons
- Elevator
- Pot1
- Pot2
- Trim1
- Trim2 (currently not working)

Technically, it is possible to use two game controllers controlled by one VBarControl Touch to access all the axes. However, for most simulator applications, 8 axes should be sufficient to provide a satisfying flying/training experience. Trim3/Trim4 are currently not available. For buttons there are no limitations, all buttons of the VBCt are mapped.
