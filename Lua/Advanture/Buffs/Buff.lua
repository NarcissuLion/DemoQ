_ENV = _G._ENV_ADVANTURE

local Buff = {}
Buff.__index = Buff

Buff.icon = nil             -- buff图标，nil不显示图标
Buff.priority = 0           -- buff执行优先级
Buff.round = 0              -- 持续回合数

function Buff.Create(buffName, owner)
    local BUFF = require('Advanture.Buffs.'..buffName)
    assert(BUFF ~= nil, 'Error! buff not exists: '..buffName)
    setmetatable(BUFF, Buff)

    local copy = {}
    setmetatable(copy, BUFF)
    copy:Init(owner)
    return copy
end

function Buff:Init(owner)
    if self.OnInit ~= nil then self:OnInit() end
    self:Add(owner)
end
function Buff:Dispose()
    self.owner = nil
    if self.OnDispose ~= nil then self:OnDispose() end
end
function Buff:Add(owner)
    self.owner = owner
    self.owner:AddBuff(self)
    if self.OnAdd ~= nil then self:OnAdd() end
end
function Buff:Remove()
    if self.OnRemove ~= nil then self:OnRemove() end
    self.owner:RemoveBuff(self)
    self:Dispose()
end

-- 基于回合的四个逻辑入口，可能返回action
function Buff:EnterRound()
    if self.OnEnterRound ~= nil then return self:OnEnterRound() end
    return nil
end
function Buff:EnterActor()
    if self.OnEnterActor ~= nil then return self:OnEnterActor() end
    return nil
end
function Buff:ExitActor()
    if self.OnExitActor ~= nil then return self:OnExitActor() end
    return nil
end
function Buff:ExitRound()
    if self.OnExitRound ~= nil then return self:OnExitRound() end
    return nil
end

-- 影响一次hit的两个逻辑入口
function Buff:Hit(hit)
    if self.OnHit ~= nil then self:OnHit(hit) end
end
function Buff:BeHit(hit)
    if self.OnBeHit ~= nil then self:OnBeHit(hit) end
end

return Buff