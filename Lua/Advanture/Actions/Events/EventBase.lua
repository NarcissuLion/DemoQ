_ENV = _G._ENV_ADVANTURE

local EventBase = {}
EventBase.__index = EventBase

function EventBase:Init(name, action, time, caster, ...)
    self.name = name
    self.action = action
    self.enterTime = time
    self.caster = caster
    self.state = 0   -- 事件状态 0 等待  1 执行中  2 完成
    self.timer = 0
    self.autoExitTime = 0  -- 自动结束时间，默认是单帧事件
    if self.OnInit ~= nil then self:OnInit(...) end
end
function EventBase:Dispose()
    self.action = nil
    self.caster = nil
    self.state = 0
    if self.OnDispose ~= nil then self:OnDispose() end
end

function EventBase:Enter()
    self.state = 1
    if self.OnEnter ~= nil then self:OnEnter() end
end

function EventBase:Update(deltaTime)
    self.timer = self.timer + deltaTime
    if self.OnUpdate ~= nil then self:OnUpdate(deltaTime) end
    if self.timer >= self.autoExitTime then
        self.state = 2
    end
end

function EventBase:DumpErrorFormat(message)
    return string.format("Error! %s:%s[%f] -> %s", self.action.name, self.name, self.enterTime, message)
end

return EventBase