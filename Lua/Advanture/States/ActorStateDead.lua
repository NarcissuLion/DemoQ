_ENV = _G._ENV_ADVANTURE

local ActorStateDead = {}
local FSMachine = require 'Framework.FSMachine'
ActorStateDead.__index = ActorStateDead
setmetatable(ActorStateDead, FSMachine.State)

function ActorStateDead.Create()
    local copy = {}
    setmetatable(copy, ActorStateDead)
    copy:Init()
    return copy
end

function ActorStateDead:OnEnter()
end

return ActorStateDead