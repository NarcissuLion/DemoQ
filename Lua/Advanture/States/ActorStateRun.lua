_ENV = _G._ENV_ADVANTURE

-- 角色探险奔跑状态，除了动画和速度区别，这个状态需要一个目标地点，到达后自动回到IDLE状态
local ActorStateRun = {}
local FSMachine = require 'Framework.FSMachine'
ActorStateRun.__index = ActorStateRun
setmetatable(ActorStateRun, FSMachine.State)

function ActorStateRun.Create()
    local copy = {}
    setmetatable(copy, ActorStateRun)
    copy:Init()
    return copy
end

function ActorStateRun:OnEnter(dstPos, duration, action)
    local tweenSetter = function(pos) self.owner:SetPosition(pos) end
    local tweenGetter = function() return self.owner:GetPosition() end
    
    self.owner.renderer:TurnFace(dstPos.x - self.owner:GetPosition().x)
    DOTween.To(tweenGetter, tweenSetter, dstPos, duration):SetEase(CS.DG.Tweening.Ease.InOutQuad):OnComplete(function()
        if action == nil then
            self.owner.fsm:Switch("IDLE") 
        else
            self.owner.fsm:Switch("ACTION", action)
        end
    end)
end

return ActorStateRun