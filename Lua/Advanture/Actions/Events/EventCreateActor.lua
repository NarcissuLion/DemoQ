_ENV = _G._ENV_ADVANTURE

local EventBase = require 'Advanture.Actions.Events.EventBase'
local Event = {}
Event.__index = Event
setmetatable(Event, EventBase)

function Event:OnInit(actorContext)
    self.actorContext = actorContext
end

function Event:OnDispose()
    self.actorContext = nil
end 

function Event:OnEnter()
    local actor = advantureInst:CreateActor(self.actorContext)
    for i=0,actor.context.cfgTbl.size-1 do
        battleInst.grids[self.actorContext.coord + i] = actor
    end
end

return Event