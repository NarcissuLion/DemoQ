_ENV = _G._ENV_ADVANTURE

local EventBase = require 'Advanture.Actions.Events.EventBase'
local Event = {}
Event.__index = Event
setmetatable(Event, EventBase)

function Event:OnInit(skill, targets)
    self.skill = skill
    self.targets = targets
end

function Event:OnDispose()
    self.skill = nil
    self.targets = nil
end

function Event:OnEnter()
    local hit = {}  -- 产生一次hit
    hit.caster = self.caster
    hit.value = self.skill.cfgTbl.power  -- hit数值, 负为伤害，正为治疗
    hit.isCrit = false -- 是否暴击
    hit.effect = nil -- 受击特效
    hit.sound = nil -- 受击音效

    for _,target in ipairs(self.targets) do
        target.fsm:Switch("HIT", hit)
    end
end

return Event