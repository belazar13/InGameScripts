IMyTimerBlock Timer;
IMyRemoteControl RemCon;
int TickCount;
int Clock = 1;
bool Stop;
float GyroMult = 5f;

IMyTextPanel TP;

Vector3D TestVector = new Vector3D(61180, 343, 56);

public Program()
{
    if (Timer == null)
        Timer = GridTerminalSystem.GetBlockWithName("Timer") as IMyTimerBlock;

    if (RemCon == null)
        RemCon = GridTerminalSystem.GetBlockWithName("RemCon") as IMyRemoteControl;

    if (TP == null)
        TP = GridTerminalSystem.GetBlockWithName("LCD") as IMyTextPanel;
    
    Runtime.UpdateFrequency = UpdateFrequency.Update1;


    
    IMyLargeGatlingTurret turret = GridTerminalSystem.GetBlockWithName("SMOKE") as IMyLargeGatlingTurret;
    if (turret != null)
    {
        turret.ApplyAction("ShootOnce");
    }

}

public void Main(string argument, UpdateType updateSource)
{
    TickCount++;

    TP.WriteText(TickCount+"\n", true);

    switch (argument)
    {
        case "start":
            start();
            break;
        case "stop":
            stop();
            break;
    }


    if (TickCount % Clock == 0 && TickCount > 0)
    {
        //SetGyroOverride(true, GetNavAngles(TestVector)*GyroMult);
    }

    SetGyroOverride(true, GetNavAngles(TestVector)*GyroMult);

    if (!Stop)
    {
        //Timer.ApplyAction("TriggerNow");
    }
    else
    {
        SetGyroOverride(false, new Vector3D(0,0,0));
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

    TP.WriteText(
        Math.Round(TargetYaw, 5) + "\n"+
        Math.Round(TargetPitch, 5) + "\n"+
        Math.Round(TargetRoll, 5) + "\n"
    );

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
            //if ((!Gyro.GyroOverride && OverrideOnOff) || (Gyro.GyroOverride && !OverrideOnOff))
            if (true)
            {
                //Gyro.ApplyAction("Override");
                Gyro.GyroOverride = OverrideOnOff;
                Gyro.SetValue("Power", Power);
                Gyro.SetValue("Yaw", Convert.ToSingle(settings.GetDim(0)));
                Gyro.SetValue("Pitch", Convert.ToSingle(settings.GetDim(1)));
                Gyro.SetValue("Roll", Convert.ToSingle(settings.GetDim(2)));
                
                TP.WriteText(DateTime.Now.ToString(), true);
            }
        }
    }
}

public void start()
{
    Stop = false;
    //(GridTerminalSystem.GetBlockWithName("MainTruster") as IMyTerminalBlock).ApplyAction("OnOff_On");
}

public void stop()
{
    Stop = true;
}
