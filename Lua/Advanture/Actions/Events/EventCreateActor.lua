_ENV = _G._ENV_ADVANTURE

local EventBase = require 'Advanture.Actions.Events.EventBase'
local Event = {}
Event.__index = Event
setmetatable(Event, EventBase)

function Event:OnInit(actorContext)
    assert(actorContext ~= nil, self:DumpErrorFormat("actorContext invalidate."))
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
    for _,skill in ipairs(actor.skills) do
        skill.cd = skill.cfgTbl.startCD+1
    end
    actor:SetPosition(BattleCoord2WorldPos(self.actorContext.coord, actor.context.cfgTbl.size))
    actor.renderer:SetHpBarVisible(true)
    actor.renderer:SyncHpBar(actor.hp / actor.context.cfgTbl.maxHP)
end

return Event