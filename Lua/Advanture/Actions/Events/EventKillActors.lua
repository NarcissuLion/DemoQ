_ENV = _G._ENV_ADVANTURE

local Event = {}
Event.__index = Event

function Event:OnInit(targets)
    assert(targets ~= nil and #targets > 0, self:DumpErrorFormat("targets invalidate."))
    self.targets = targets
end

function Event:OnDispose()
    self.targets = nil
end

function Event:OnEnter()
    for _,target in ipairs(self.targets) do
        target.hp = 0
        target.fsm:Switch("DYING")
    end
end

return Event