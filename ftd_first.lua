Time=0
dT=0
TargetPos=Vector3(0,0,0)
TargetVelocity=Vector3(0,0,0)

function InteceptPoint(TargetPos, TargetVel, TorpedoPos, TorpedoVel)
  cntMax=10
  cnt=0
  InterceptPoint=TargetPos
  InterceptError=1000
  while ((InterceptError>1) and (cnt<cntMax)) do
    InterceptVector = InterceptPoint-TorpedoPos
    InterceptTime = InterceptVector.magnitude / TorpedoVel
    NewInterceptPoint = TargetPos + TargetVel*InterceptTime
    InterceptError = (NewInterceptPoint-InterceptPoint).magnitude
    InterceptPoint = NewInterceptPoint
    cnt=cnt+1
  end
  return InterceptPoint
  --return cnt
end

function Update(I)

   --считаем дельту Т--
   dT = I:GetGameTime() - Time
   Time =I:GetGameTime()
   LogStr = "dT: ".. dT

  
  -- считаем скорость цели
   TrgInfo = I:GetTargetInfo(0, 0)
   dS = TrgInfo.Position - TargetPos
   TargetPos = TrgInfo.Position
   AimPos=TrgInfo.AimPointPosition
 
   TargetVelocity = TargetVelocity*0.99 + (dS/dT)*0.01
   --HorVelMag = math.sqrt(TargetVelocity.x^2 + TargetVelocity.z^2)
   --I:LogToHud(HorVelMag)
  --Управляем торпедами

  for iTrans=0, I:GetLuaTransceiverCount()-1 do
    for iMissile=0, I:GetLuaControlledMissileCount(iTrans)-1 do
      
      MissileInfo = I:GetLuaControlledMissileInfo(iTrans,iMissile)
      InterceptPos = InteceptPoint(AimPos, TargetVelocity, MissileInfo.Position, 0.9*MissileInfo.Velocity.magnitude)
      
      TrgNorm = (InterceptPos - MissileInfo.Position).normalized
      TrgNorm = TrgNorm*100
      TrgNorm = MissileInfo.Position + TrgNorm

      InterceptRange = InterceptPos - MissileInfo.Position
      distance = math.sqrt(InterceptRange.x^2+InterceptRange.z^2+InterceptRange.y^2);

      if distance>200 then
        I:SetLuaControlledMissileAimPoint(iTrans,iMissile,TrgNorm.x,50,TrgNorm.z)
      else 
        I:SetLuaControlledMissileAimPoint(iTrans,iMissile,InterceptPos.x,0,InterceptPos.z)
        if distance < 8 then
          I:DetonateLuaControlledMissile(iTrans,iMissile) --если цель в радиусе 5м - подрыв (он на самом деле еще ближе произойдет)
        end
      end
    end
  end
   
end