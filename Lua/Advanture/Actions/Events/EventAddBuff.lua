_ENV = _G._ENV_ADVANTURE

local Event = {}
Event.__index = Event

function Event:OnInit(buffName, targets, belongSkill, floatText)
    assert(targets ~= nil and #targets > 0, self:DumpErrorFormat("targets invalidate."))
    self.buffName = buffName
    self.targets = targets
    self.belongSkill = belongSkill
    self.floatText = floatText
end

function Event:OnDispose()
    self.buffName = nil
    self.targets = nil
    self.belongSkill = nil
    self.floatText = nil
end 

function Event:OnEnter()
    -- add buff
    for _,target in ipairs(self.targets) do
        Buff.Create(self.buffName, target, self.belongSkill)
        if self.floatText ~= nil then
            PlayFloatText(self.floatText, Color(0, 1, 1, 1), target:GetPosition() + Vector3(0, 1.5, 0))
        end
    end
end

return Event