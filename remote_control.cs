IMyCameraBlock Camera;
IMyTextPanel LCD;
IMyRadioAntenna Antenna;

MyDetectedEntityInfo DetectedObject;

//Конструктор скрипта
// ------------------------------------------

public Program()
{
    //Находим блоки
    Camera = GridTerminalSystem.GetBlockWithName("Camera") as IMyCameraBlock;
    Camera.EnableRaycast = true;
    LCD = GridTerminalSystem.GetBlockWithName("LCD") as IMyTextPanel;
    Antenna = GridTerminalSystem.GetBlockWithName("ShipAntenna") as IMyRadioAntenna;
}

//Главная функция
// ------------------------------------------

public void Main(string argument, UpdateType updateSource)
{
    switch (argument)
    {
        case "detect":
            Detect();
            break;
        case "prepare":
            Prepare();
            break;
        case "start":
            Start();
            break;
    }
}

void Start()
{
    string msg = "Start;";
    Antenna.TransmitMessage(msg, MyTransmitTarget.Owned);
}

void Prepare()
{
    string msg = "TorpedoLock;";
    msg += DetectedObject.Position.X.ToString() + ";";
    msg += DetectedObject.Position.Y.ToString() + ";";
    msg += DetectedObject.Position.Z.ToString() + ";";

    Antenna.TransmitMessage(msg, MyTransmitTarget.Owned);
    LCD.WriteText(msg, false);
}

void Detect()
{
    DetectedObject = Camera.Raycast(10000, 0, 0);
    LCD.WriteText("Обнаружено: \n", false);
    LCD.WriteText("Объект: " + DetectedObject.Name + "\n", true);
    LCD.WriteText("Координаты: \n", true);
    LCD.WriteText("     X: " + DetectedObject.Position.X + "\n", true);
    LCD.WriteText("     Y: " + DetectedObject.Position.Y + "\n", true);
    LCD.WriteText("     Z: " + DetectedObject.Position.Z + "\n", true);

    string GPS = "\nGPS:" + DetectedObject.Name + ":" + DetectedObject.Position.X + ":"
                            + DetectedObject.Position.Y + ":"
                            + DetectedObject.Position.Z + ":";
    LCD.WriteText(GPS, true);
}
