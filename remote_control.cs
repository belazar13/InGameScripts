IMyCameraBlock Camera;
IMyTextPanel LCD;
IMyTextPanel LCD2;
//IMyRadioAntenna Antenna;

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
    LCD2 = GridTerminalSystem.GetBlockWithName("LCD2") as IMyTextPanel;
    //Antenna = GridTerminalSystem.GetBlockWithName("ShipAntenna") as IMyRadioAntenna;

    IGC.RegisterBroadcastListener(tagChannel);
}

//Главная функция
// ------------------------------------------

public void Main(string argument, UpdateType updateSource)
{
    ProcessMessages();

    string[] msg = argument.Split(';');

    if (msg.Length == 1)
    {
        switch (argument)
        {
            case "detect":
                Detect();
                break;
            case "prepare":
                Prepare();
                break;
        }
    }
    else if (msg.Length == 2 && msg[0] == "start")
    {
        Start(msg[1]);
    }
}

void ProcessMessages()
{
    List<IMyBroadcastListener> listeners = new List<IMyBroadcastListener>();
    IGC.GetBroadcastListeners(listeners);

    foreach (var listener in listeners)
    {
        while (listener.HasPendingMessage)
        {
            MyIGCMessage message = listener.AcceptMessage();
            string messagetext = message.Data.ToString();
            string messagetag = message.Tag;

            string[] msg = messagetext.Split(';');
            switch (msg[0])
            {
                case "TorpedoStatus":
                    if (LCD2 != null)
                    {
                        LCD2.WriteText(msg[1] + " OK\n", true);
                    }
                    break;
            }
        }
    }
}
void Start(string torpedoID)
{
    string msg = "Start;"+torpedoID;
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
    LCD2.WriteText("Torpedo status\n", false);
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
