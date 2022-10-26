_ENV = _G._ENV_ADVANTURE

local Action = {}
Action.__index = Action

function Action:InitEvents(delay, actorContext)
    self:AddEvent(delay, "EventCreateActor", actorContext)
end

return Action