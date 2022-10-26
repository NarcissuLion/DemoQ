_ENV = _G._ENV_ADVANTURE

local Event = {}
Event.__index = Event

function Event:OnInit(buffName, targets, belongSkill)
    assert(targets ~= nil and #targets > 0, self:DumpErrorFormat("targets invalidate."))
    self.buffName = buffName
    self.targets = targets
    self.belongSkill = belongSkill
end

function Event:OnDispose()
    self.buffName = nil
    self.targets = nil
    self.belongSkill = nil
end 

function Event:OnEnter()
    -- add buff
    for _,target in ipairs(self.targets) do
        Buff.Create(self.buffName, target, self.belongSkill)
    end
end

return Event