_ENV = _G._ENV_ADVANTURE

local Event = {}
Event.__index = Event

function Event:OnInit(skill, targets, powerFactor)
    assert(skill ~= nil, self:DumpErrorFormat("skill invalidate."))
    assert(targets ~= nil and #targets > 0, self:DumpErrorFormat("targets invalidate."))
    self.skill = skill
    self.targets = targets
    self.powerFactor = powerFactor ~= nil and powerFactor or 1
end

function Event:OnDispose()
    self.skill = nil
    self.targets = nil
    self.powerFactor = 1
end

function Event:OnEnter()
    local hit = {}  -- 产生一次hit
    hit.caster = self.caster
    hit.value = CommonUtils.SafeMultiply(-self.skill.cfgTbl.power, self.powerFactor)  -- hit数值, 负为伤害，正为治疗
    hit.isCrit = false -- 是否暴击
    hit.effect = nil -- 受击特效
    hit.sound = nil -- 受击音效

    for _,target in ipairs(self.targets) do
        target.fsm:Switch("HIT", hit)
    end
end

return Event