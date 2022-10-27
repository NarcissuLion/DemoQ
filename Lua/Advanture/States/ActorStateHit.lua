_ENV = _G._ENV_ADVANTURE

local ActorStateHit = {}
local FSMachine = require 'Framework.FSMachine'
ActorStateHit.__index = ActorStateHit
setmetatable(ActorStateHit, FSMachine.State)

local Notifier = require('Framework.Notifier')

function ActorStateHit.Create()
    local copy = {}
    setmetatable(copy, ActorStateHit)
    copy:Init()
    return copy
end

function ActorStateHit:OnEnter(hit)
    -- 施法者的所有buff过一遍Hit函数
    for _,buff in ipairs(hit.caster.buffs) do
        buff:Hit(hit)
    end
    -- 目标者的所有buff过一遍BeHit函数
    for _,buff in ipairs(self.owner.buffs) do
        buff:BeHit(hit)
    end

    -- 这里处理hit响应逻辑
    local hurt = hit.value < 0
    local heal = hit.value > 0
    self.owner.hp = math.max(0, self.owner.hp + hit.value)
    self.owner.hp = math.min(self.owner.hp, self.owner.context.cfgTbl.maxHP)
    self.owner.renderer:SyncHpBar(self.owner.hp / self.owner.context.cfgTbl.maxHP)

    -- 播放hit特效
    if hit.effect ~= nil then
    end
    -- 播放飘字
    PlayFloatText(hurt and hit.value or ('+'..hit.value), hurt and Color.red or Color.green, self.owner:GetPosition() + Vector3(0, 1.5, 0))

    if hurt then
        -- 可以通知飘字什么的
        self.owner.renderer.spriteRenderer:DOColor(Color.red, 0.1):SetLoops(2, CS.DG.Tweening.LoopType.Yoyo)
        self.owner.renderer.transform:DOScale(Vector3(0.9, 1.1, 1), 0.1):SetLoops(2, CS.DG.Tweening.LoopType.Yoyo):OnComplete(function()
            self:Leave()
        end)
    end
    
    if heal then
        self.owner.renderer.spriteRenderer:DOColor(Color.green, 0.1):SetLoops(2, CS.DG.Tweening.LoopType.Yoyo):OnComplete(function()
            self:Leave()
        end)
    end
end

function ActorStateHit:Leave()
    if self.owner.hp > 0 then
        self.owner.fsm:Switch("IDLE")
    else
        self.owner.fsm:Switch("DYING")
    end
end

return ActorStateHit