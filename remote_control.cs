IMyCameraBlock Camera;
IMyTextPanel LCD;
IMyRadioAntenna Antenna;

MyDetectedEntityInfo DetectedObject;

string tagChannel = "ch1";

//Конструктор скрипта
// ------------------------------------------

public Program()
{
    Runtime.UpdateFrequency = UpdateFrequency.Update100;

    Camera = GridTerminalSystem.GetBlockWithName("Camera") as IMyCameraBlock;
    Camera.EnableRaycast = true;
    LCD = GridTerminalSystem.GetBlockWithName("LCD") as IMyTextPanel;
    Antenna = GridTerminalSystem.GetBlockWithName("ShipAntenna") as IMyRadioAntenna;

    IGC.RegisterBroadcastListener(tagChannel);
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

    LCD.WriteText(argument + "\n", true);
    LCD.WriteText(DateTime.Now.ToString(), true);
}

void Start()
{
    string msg = "Start;";
    IGC.SendBroadcastMessage(tagChannel, msg, TransmissionDistance.TransmissionDistanceMax);
}

void Prepare()
{
    string msg = "TorpedoLock;";
    msg += DetectedObject.Position.X.ToString() + ";";
    msg += DetectedObject.Position.Y.ToString() + ";";
    msg += DetectedObject.Position.Z.ToString() + ";";

    //Antenna.TransmitMessage(msg, MyTransmitTarget.Owned);
    IGC.SendBroadcastMessage(tagChannel, msg, TransmissionDistance.TransmissionDistanceMax);
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
