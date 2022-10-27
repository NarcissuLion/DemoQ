_ENV = _G._ENV_ADVANTURE

local ActorStateDying = {}
local FSMachine = require 'Framework.FSMachine'
ActorStateDying.__index = ActorStateDying
setmetatable(ActorStateDying, FSMachine.State)

function ActorStateDying.Create()
    local copy = {}
    setmetatable(copy, ActorStateDying)
    copy:Init()
    return copy
end

function ActorStateDying:OnEnter()
    self.owner:ClearAllBuffs()
    self.owner.renderer:SetHpBarVisible(false)
    self.owner.renderer.spriteRenderer:DOColor(Color(1, 1, 1, 0), 0.5):OnComplete(function()
        self.owner.fsm:Switch("DEAD")
    end)
end

return ActorStateDying