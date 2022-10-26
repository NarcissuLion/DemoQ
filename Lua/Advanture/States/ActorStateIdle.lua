_ENV = _G._ENV_ADVANTURE

-- 角色待机状态，清除速度并播放待机动画
local ActorStateIdle = {}
local FSMachine = require 'Framework.FSMachine'
ActorStateIdle.__index = ActorStateIdle
setmetatable(ActorStateIdle, FSMachine.State)

function ActorStateIdle.Create()
    local copy = {}
    setmetatable(copy, ActorStateIdle)
    copy:Init()
    return copy
end

function ActorStateIdle:OnEnter()
    self.owner:SetVelocity(0, 0)
end

return ActorStateIdle