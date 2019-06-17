IMyRemoteControl RemCon;
IMyTextPanel LCD;

bool Stop = true;
float GyroMult = 15f;
int Tick;

//Vector3D TestVector = new Vector3D(61174.3561, 373.8672, 86.4822);
Vector3D TestVector;// = new Vector3D(61411.5216, 128.8047, -249.9993);

public Program()
{
    if (RemCon == null)
        RemCon = GridTerminalSystem.GetBlockWithName("RemCon") as IMyRemoteControl;

    LCD = GridTerminalSystem.GetBlockWithName("LCD") as IMyTextPanel;

    Runtime.UpdateFrequency = UpdateFrequency.Update1;
}

public void Main(string argument, UpdateType updateSource)
{
    if (updateSource == UpdateType.Antenna)
    {
        string[] msg = argument.Split(';');

        switch (msg[0])
        {
            case "TorpedoLock":
                TestVector = new Vector3D(Convert.ToDouble(msg[1]), Convert.ToDouble(msg[2]), Convert.ToDouble(msg[3]));
                
                if (LCD != null)
                {
                    LCD.WriteText("X" + TestVector.X + "\n", true);
                    LCD.WriteText("Y" + TestVector.Y + "\n", true);
                    LCD.WriteText("Z" + TestVector.Z + "\n", true);
                }
                break;
            case "Start":
                start();
                break;
        }
    }

    if (argument == "testlcd")
    {
        LCD.WriteText("OKOKOK", false);
    }

    if (Tick++ < 120)
    {
        return;
    }

    if (Stop)
    {
        SetGyroOverride(false, new Vector3D(0, 0, 0));
    }
    else
    {
        SetGyroOverride(true, GetNavAngles(TestVector) * GyroMult);
    }
}

Vector3D GetNavAngles(Vector3D Target)
{
    Vector3D V3Dcenter = RemCon.GetPosition();
    Vector3D V3Dfow = RemCon.WorldMatrix.Forward;
    Vector3D V3Dup = RemCon.WorldMatrix.Up;
    Vector3D V3Dleft = RemCon.WorldMatrix.Left;

    Vector3D TargetNorm = Vector3D.Normalize(Target - V3Dcenter);

    double TargetPitch = Math.Acos(Vector3D.Dot(V3Dup, Vector3D.Normalize(Vector3D.Reject(TargetNorm, V3Dleft)))) - Math.PI / 2;
    double TargetYaw = Math.Acos(Vector3D.Dot(V3Dleft, Vector3D.Normalize(Vector3D.Reject(TargetNorm, V3Dup)))) - Math.PI / 2;
    double TargetRoll = Math.Acos(Vector3D.Dot(V3Dleft, Vector3D.Normalize(Vector3D.Reject(-RemCon.GetNaturalGravity(), V3Dfow)))) - Math.PI / 2;

    return new Vector3D(TargetYaw, -TargetPitch, TargetRoll);
}

void SetGyroOverride(bool OverrideOnOff, Vector3D settings, float Power = 1f)
{
    var Gyros = new List<IMyTerminalBlock>();
    GridTerminalSystem.SearchBlocksOfName("Gyro", Gyros);

    for (int i = 0; i < Gyros.Count; i++)
    {
        IMyGyro Gyro = Gyros[i] as IMyGyro;
        if (Gyro != null)
        {
            Gyro.GyroOverride = OverrideOnOff;
            Gyro.SetValue("Power", Power);
            Gyro.SetValue("Yaw", Convert.ToSingle(settings.GetDim(0)));
            Gyro.SetValue("Pitch", Convert.ToSingle(settings.GetDim(1)));
            Gyro.SetValue("Roll", Convert.ToSingle(settings.GetDim(2)));
        }
    }
}

public void start()
{
    Stop = false;

    var Engines = new List<IMyTerminalBlock>();
    GridTerminalSystem.SearchBlocksOfName("MT", Engines);

    foreach (var engine in Engines)
    {
        engine.ApplyAction("OnOff_On");
    }

    //(GridTerminalSystem.GetBlockWithName("Link") as IMyTerminalBlock).ApplyAction("OnOff_Off");
    //(GridTerminalSystem.GetBlockWithName("SMOKE") as IMyTerminalBlock).ApplyAction("OnOff_On");
    Tick = 0;
}

public void stop()
{
    Stop = true;
}
