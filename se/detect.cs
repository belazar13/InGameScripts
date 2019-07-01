IMyCameraBlock Camera;
IMyTextPanel LCD;


//Конструктор скрипта
// ------------------------------------------

public Program()
{
    //Находим блоки
    Camera = GridTerminalSystem.GetBlockWithName("Camera") as IMyCameraBlock;
    Camera.EnableRaycast = true;
    LCD = GridTerminalSystem.GetBlockWithName("LCD") as IMyTextPanel;
}

//Главная функция
// ------------------------------------------

public void Main(string argument, UpdateType updateSource)
{
    //Разбор аргументов. Вызов функций raycast и расчета точки сброса.
    if (argument == "Detect") {
        Detect();
    }
}

void Detect()
{
    MyDetectedEntityInfo DetectedObject = Camera.Raycast(10000, 0, 0);
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
