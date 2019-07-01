antiShipFrame = 0
aaFrame = 1

Time=0
dT=0
TargetPos=Vector3(0,0,0)
TargetVelocity=Vector3(0,0,0)

function Update(I)
   updateAA(I)
   updateAS(I)
end

function updateAA(I)
    --MW = I:GetMissileWarning(0, 0) --Так это будет ПРО
    MW = I:GetTargetInfo(aaFrame, 0) --а так - ПВО

    -- здесь просто управляем установкой на вертушке
    AMD = I:GetWeaponInfo(0)
    if (not MW.Valid) then
        do return end
    end 

    MDir = MW.Position - AMD.GlobalPosition
    --I:AimWeaponInDirection(0,MDir.x,MDir.y,MDir.z,0)
    if (MDir.y < 20) then
        I:LogToHud("Not air target")
        do return end
        I:LogToHud("Not air target 2")
    end

    --I:AimWeaponInDirection(0,MDir.x,MDir.y,MDir.z,0)
    if (MDir.magnitude < 2500) then
        I:FireWeapon(aaFrame, 0)
    end

    -- цикл по запущенным ракетам
    for iTrans=0, I:GetLuaTransceiverCount()-1 do
        for iMissile=0, I:GetLuaControlledMissileCount(iTrans)-1 do
            Rocket = I:GetLuaControlledMissileInfo(iTrans,iMissile)
            RDir = MW.Position - Rocket.Position
            --считаем вектор перехвата
            InerceptDir = FindInterceptVector(Rocket.Position, Rocket.Velocity.magnitude, MW.Position, MW.Velocity)
            
            if (RDir.magnitude<300) then    
                InerceptDir = -ReflectK(Rocket.Velocity, InerceptDir, 5) --отражаем скорость ракеты от желаемого направления. гасим инерцию
            end 
            
            AimVec = InerceptDir + Rocket.Position   
            I:SetLuaControlledMissileAimPoint(iTrans,iMissile,AimVec.x,AimVec.y,AimVec.z)
            
            if (RDir.magnitude<05) then
                I:DetonateLuaControlledMissile(iTrans,iMissile) --если цель в радиусе 5м - подрыв (он на самом деле еще ближе произойдет)
            end
        end
    end
        
end

-- расчет направления на точку упреждения по скорости цели
function FindInterceptVector(shotOrigin, shotSpeed, targetOrigin, targetVel)
  dirToTarget = (targetOrigin - shotOrigin).normalized --направление от снаряда на цель
  targetVelOrth = dirToTarget * Vector3.Dot(targetVel, dirToTarget) --ортогональная скорость цели
  targetVelTang = Vector3.ProjectOnPlane(targetVel, dirToTarget) --тангенциальная скорость цели

  shotVelTang = targetVelTang --уравниваем тангенциальную скорость снаряда со скоростью цели
  shotVelSpeed = shotVelTang.magnitude --получаем ее длину

  if (shotVelSpeed > shotSpeed) then  
      -- требуемая тангенциальная скорость выше полной скорости снаряда
      -- попадание невозможно
      return targetVel.normalized * shotSpeed 
  else
      -- иначе считаем тот "остаток" скорости снаряда, который мы можем направить в сторону цели.
      shotSpeedOrth = math.sqrt(shotSpeed * shotSpeed - shotVelSpeed * shotVelSpeed)
      shotVelOrth = dirToTarget * shotSpeedOrth --вектор ортогональной скорости снаряда
      return shotVelOrth + shotVelTang --суммируем орт. и танг. скорости снаряда и получаем требуемый 
      -- для попадания вектор скорости снаряда. Он же вектор перехвата.
  end  
end
-- отражение с коэффициентом
function ReflectK(Vec, Dir, K)
  return Vector3.ProjectOnPlane(Vec, Dir.normalized) *K - Vec
end

-- Anti ship


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

function updateAS(I)
   --считаем дельту Т--
   dT = I:GetGameTime() - Time
   Time =I:GetGameTime()
   LogStr = "dT: ".. dT
  
  -- считаем скорость цели
   TrgInfo = I:GetTargetInfo(antiShipFrame, 0)

    AMD = I:GetWeaponInfo(0)
    MDir = TrgInfo.Position - AMD.GlobalPosition
    --I:AimWeaponInDirection(0,MDir.x,MDir.y,MDir.z,0)
    if (MDir.y > 20) then
        I:LogToHud("Not naval target")
        return
    end

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
      if math.sqrt(InterceptRange.x^2+InterceptRange.z^2)>200 then
        I:SetLuaControlledMissileAimPoint(iTrans,iMissile,TrgNorm.x,-25,TrgNorm.z)
      else 
        I:SetLuaControlledMissileAimPoint(iTrans,iMissile,InterceptPos.x,0,InterceptPos.z)
      end
    end
  end
end