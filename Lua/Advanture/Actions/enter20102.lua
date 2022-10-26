_ENV = _G._ENV_ADVANTURE

local Action = {}
Action.__index = Action

function Action:InitEvents()
    self:AddEvent(0, "EventAddBuff", "demo_spawn_monster", {self.caster}, self.skill)
end

return Action