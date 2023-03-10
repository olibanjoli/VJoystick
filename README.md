# VBC Wireless Gamepad

The VBC Wireless Gamepad application enables wireless control of a generic Windows game controller using a VBarControl Touch, allowing it to be used with a wide range of flight simulators and games. While some simulators natively support the protocol developed by Mikado, they remain in the minority. The VBC Wireless Gamepad fills this gap by providing a versatile solution that can be used with any simulator or game that supports Windows joysticks.

## Installation

1. Build the solution or download release from [here](https://vbc-wireless-gamepad.vercel.app/docs/installation/)
2. Run `VBCWirelessGamepad.exe`  
   ![Startup](./web/docs/installation/startup.gif)
3. Start IP UDP Simulator App von your VBCt  
   ![IP UDP Simulator App](./web/docs/installation/ip_udp_simulator.png)
4. Activate **Send UDP Control**  
   ![Send UDP Control](./web/docs/installation/udp_active.png)
5. VBCWirelessGamepad will pickup all the inputs from your VBC and forward it to the Windows gamecontroller. ![Connected](./web/docs/installation/vbc_connected.gif)
6. Setup the game controller in your favorite simulator / game and have fun.
