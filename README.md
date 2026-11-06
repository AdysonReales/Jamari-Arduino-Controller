# Jamari's Arduino Controller

A Computer Architecture project connecting a custom Arduino controller to a Unity application. The prototype explores affordable, customizable game interaction through serial communication.

![Arduino controller prototype](docs/images/controller.jpg)

## About the project

Built at Mapúa University, the controller uses an Arduino Uno R3, six push buttons, a dual-axis joystick, and a potentiometer for steering. The two main buttons and four additional buttons provide inputs that vary between games. Arduino reads the controls and sends their values over USB serial for Unity to interpret.

![Serial input demonstration](docs/images/serial-monitor.png)

## Games

### Crane game

Move the crane with the joystick and use the buttons to drop or grab the ball.

![Crane controls](docs/images/crane-controls.png)
![Crane game](docs/images/crane-game.png)

### Memory sequence

Watch the sequence and repeat it using the four buttons.

![Memory controls](docs/images/memory-controls.png)
![Memory game](docs/images/memory-game.png)

### Parking game

Use the buttons to move forward and backward and the analog controls to steer.

![Parking controls](docs/images/parking-controls.png)
![Parking game](docs/images/parking-game.png)

## Opening the project

1. Clone this repository and add its folder to Unity Hub.
2. Open it with Unity **6000.0.54f1** and allow Unity to import the assets.
3. Connect the Arduino controller over USB. Hardware input requires the appropriate Arduino firmware and serial port configuration; check the project scripts for the expected port and baud rate.

## Team

Adyson Reales (leader), Matteo De Leon, John Kerby Gadiano, Richard Imus, Adrian Muros, Karl Evlan Robasto, Jared Saflor, and Darrelle Yuri Soriano.
