_ENV = _G._ENV_ADVANTURE


local Action = {}
Action.__index = Action

function Action:InitEvents()
    self:AddEvent(0, "EventPlayAnim", "animName")
    self:AddEvent(0.25, "EventDoDamage", self.skill, self.targets)
end

return Action