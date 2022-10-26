_ENV = _G._ENV_ADVANTURE

local Actor = {}
Actor.__index = Actor

local ActorRenderer = require 'Advanture.Renderer.ActorRenderer'
local Notifier = require 'Framework.Notifier'

function Actor.Create(id, context)
    local copy = {}
    setmetatable(copy, Actor)
    copy:Init(id, context)
    return copy
end

function Actor:Init(id, context)
    self.id = id
    self.context = context
    self.context.cfgTbl = ConfigCenter.Find("actor", context.cfgID)
    self.hp = self.context.cfgTbl.maxHP
    self.spd = self.context.cfgTbl.speed

    self.buffs = {}
    self.spState = {  -- 特殊状态  key:状态名  value:计数
        taunt = 0,  -- 嘲讽
        faint = 0,  -- 眩晕, demo 暂时用不着
    }

    -- skills
    self.skills = {}
    for _,skillCfgID in ipairs(self.context.cfgTbl.skills) do
        local skill = { owner=self, cfgTbl=ConfigCenter.Find("skill", skillCfgID) }
        table.insert(self.skills, skill)
    end

    self.position = Vector3.zero
    self.renderer = ActorRenderer.Create(self.id, self.context.cfgTbl.prefab)
    self.renderer:AddPointClick(function() Notifier.Dispatch("_Advanture_Click_Actor", self) end)

    self.fsm = require('Framework.FSMachine').Create(self)
    self.fsm:Add("IDLE", require('Advanture.States.ActorStateIdle').Create())
    self.fsm:Add("WALK", require('Advanture.States.ActorStateWalk').Create())
    self.fsm:Add("RUN", require('Advanture.States.ActorStateRun').Create())
    self.fsm:Add("DYING", require('Advanture.States.ActorStateDying').Create())
    self.fsm:Add("DEAD", require('Advanture.States.ActorStateDead').Create())
    self.fsm:Add("ACTION", require('Advanture.States.ActorStateAction').Create())
    self.fsm:Add("HIT", require('Advanture.States.ActorStateHit').Create())
    self.fsm:Switch("IDLE")
end

function Actor:Dispose()
    self.id = nil
    self.cfg = nil
    self.skills = nil
    self.buffs = nil
    self.renderer:Dispose()
    self.renderer = nil
    self.fsm:Dispose()
    self.fsm = nil
end

function Actor:Update(deltaTime)
    self.fsm:Update(deltaTime)
    if self.velocity ~= nil and self.velocity ~= Vector3.zero then
        self:SetPosition(self.position + self.velocity * deltaTime)
    end
end

-- Buff相关 ------------------------------------
function Actor:AddBuff(buff)
    table.insert(self.buffs, buff)
    buff.restRound = buff.round
end
function Actor:RemoveBuff(buff)
    for i=#self.buffs,1,-1 do
        if buff == self.buffs[i] then
            table.remove(self.buffs, i)
            break
        end
    end
end
-- 回合结束时刷新buff持续时间
function Actor:RefreshBuffs()
    for i=#self.buffs,1,-1 do
        self.buffs[i].restRound = self.buffs[i].restRound - 1
        if self.buffs[i].restRound == 0 then
            self.buffs[i]:Remove()
        end
    end
end
------------------------------------------------

-- 特殊状态 -------------------------------------
function Actor:CheckSpState(spStateName)
    return self.spState[spStateName] > 0
end
function Actor:AddSpState(spStateName)
    self.spState[spStateName] = self.spState[spStateName] + 1
    if self.spState[spStateName] == 1 then
        -- todo 状态从0到1转变
    end
end
function Actor:RemoveSpState(spStateName)
    if self.spState[spStateName] == 0 then return end
    self.spState[spStateName] = self.spState[spStateName] - 1
    if self.spState[spStateName] == 0 then
        -- todo 状态从1到0转变
    end
end
-------------------------------------------------

-- 位置速度 --------------------------------------
function Actor:GetPosition()
    return self.position;
end
function Actor:SetPosition(position)
    self.position = position
    self.renderer:SetPosition(self.position)
end
function Actor:SetVelocity(vx, vy)
    self.velocity = Vector3(vx, vy, 0)
    self.renderer:TurnFace(vx)
end
-------------------------------------------------

-- 显示相关 --------------------------------------
function Actor:SetSortOrder(order)
    self.renderer:SetSortOrder(order)
end
-- 创建一个虚影
function Actor:CreateShade(alpha, sortingOrder)
    local shade = ActorRenderer.Create(self.id, self.context.cfgTbl.prefab)
    shade.spriteRenderer.color = Color(1, 1, 1, alpha)
    shade:SetSortOrder(sortingOrder)
    return shade
end
--------------------------------------------------

return Actor