_ENV = _G._ENV_ADVANTURE

-- 角色探险行走状态
local ActorStateWalk = {}
local FSMachine = require 'Framework.FSMachine'
ActorStateWalk.__index = ActorStateWalk
setmetatable(ActorStateWalk, FSMachine.State)

function ActorStateWalk.Create()
    local copy = {}
    setmetatable(copy, ActorStateWalk)
    copy:Init()
    return copy
end

local MAX_SPEED = 4
local ACCELERATE = 4

function ActorStateWalk:OnEnter()
    self.speed = 0
end

function ActorStateWalk:OnUpdate(deltaTime)
    if self.speed < MAX_SPEED then
        self.speed = math.min(MAX_SPEED, self.speed + ACCELERATE * deltaTime)
    end
    self.owner:SetVelocity(self.speed, 0)
end

-- function ActorStateWalk:OnExit()
-- end

return ActorStateWalk