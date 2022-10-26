_ENV = _G._ENV_ADVANTURE

local ActorStateAction = {}
local FSMachine = require 'Framework.FSMachine'
ActorStateAction.__index = ActorStateAction
setmetatable(ActorStateAction, FSMachine.State)

function ActorStateAction.Create()
    local copy = {}
    setmetatable(copy, ActorStateAction)
    copy:Init()
    return copy
end

function ActorStateAction:OnEnter(action)
    self.action = action
end

function ActorStateAction:OnUpdate(deltaTime)
    self.action:Update(deltaTime)
    if self.action.done then
        self.owner.fsm:Switch("IDLE")
    end
end

function ActorStateAction:OnExit()
    self.action:Dispose()
    self.action = nil
end

return ActorStateAction