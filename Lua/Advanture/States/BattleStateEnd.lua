_ENV = _G._ENV_ADVANTURE

local Notifier = require 'Framework.Notifier'

-- 战斗结束状态
local BattleStateEnd = {}
local FSMachine = require 'Framework.FSMachine'
BattleStateEnd.__index = BattleStateEnd
setmetatable(BattleStateEnd, FSMachine.State)

function BattleStateEnd.Create()
    local copy = {}
    setmetatable(copy, BattleStateEnd)
    copy:Init()
    return copy
end

function BattleStateEnd:OnEnter(win)
    Notifier.Dispatch("_Battle_End")
    local advanture = battleInst.advanture
    if win then -- 回到探险
        for i=#advanture.actors,1,-1 do
            local actor = advanture.actors[i]
            if actor.context.camp == 2 then  -- 移除所有敌方角色
                table.remove(advanture.actors, i)
            else
                -- todo 我方死了的角色复活 ??
                actor:ClearAllBuffs()
                actor.renderer:SetHpBarVisible(false)
            end
        end
        advanture.fsm:Switch("LINEUP")
    else
        -- todo lose
    end
end

return BattleStateEnd