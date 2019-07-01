function Update(I)
   I:LogToHud("Targets: " .. I:GetNumberOfTargets(0)) 
   --MW = I:GetMissileWarning(0, 0) --Так это будет ПРО
   MW = I:GetTargetInfo(0, 0) --а так - ПВО
   
   -- здесь просто управляем установкой на вертушке
   AMD = I:GetWeaponInfo(0)
   if (MW.Valid) then
     MDir = MW.Position - AMD.GlobalPosition
     I:AimWeaponInDirection(0,MDir.x,MDir.y,MDir.z,0)

     -- цикл по запущенным ракетам
     for iTrans=0, I:GetLuaTransceiverCount()-1 do
       for iMissile=0, I:GetLuaControlledMissileCount(iTrans)-1 do
         Rocket = I:GetLuaControlledMissileInfo(iTrans,iMissile)
         RDir = MW.Position - Rocket.Position
		 --считаем вектор перехвата
         InerceptDir = FindInterceptVector(Rocket.Position, Rocket.Velocity.magnitude, 
                        MW.Position, MW.Velocity)
         if (RDir.magnitude<300) then    
            InerceptDir = -ReflectK(Rocket.Velocity, InerceptDir, 5) --отражаем скорость ракеты от желаемого направления. гасим инерцию
         end 
         AimVec = InerceptDir + Rocket.Position   
         I:SetLuaControlledMissileAimPoint(iTrans,iMissile,AimVec.x,AimVec.y+5,AimVec.z)
         if (RDir.magnitude<5) then
           I:DetonateLuaControlledMissile(iTrans,iMissile) --если цель в радиусе 5м - подрыв (он на самом деле еще ближе произойдет)
         end
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
  return Vector3.ProjectOnPlane(Vec, Dir.normalized)
*K - Vec
end