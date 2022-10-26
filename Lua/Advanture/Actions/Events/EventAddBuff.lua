_ENV = _G._ENV_ADVANTURE

local Event = {}
Event.__index = Event

function Event:OnInit(buffName, targets)
    self.buffName = buffName
    self.targets = targets
end

function Event:OnDispose()
    self.buffName = nil
    self.targets = nil
end 

function Event:OnEnter()
    -- add buff
    for _,target in ipairs(self.targets) do
        Buff.Create(self.buffName, target)
    end
end

return Event