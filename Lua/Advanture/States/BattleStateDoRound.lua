_ENV = _G._ENV_ADVANTURE

local Notifier = require('Framework.Notifier')

-- 战斗执行回合
local BattleStateDoRound = {}
local FSMachine = require 'Framework.FSMachine'
BattleStateDoRound.__index = BattleStateDoRound
setmetatable(BattleStateDoRound, FSMachine.State)

function BattleStateDoRound.Create()
    local copy = {}
    setmetatable(copy, BattleStateDoRound)
    copy:Init()
    return copy
end

function BattleStateDoRound:OnEnter()
    local actor = nil
    for i=battleInst.currActionActorIdx,#battleInst.sortedActionActors do
        if battleInst.sortedActionActors[i].hp > 0 then
            actor = battleInst.sortedActionActors[i]
            battleInst.currActionActorIdx = i
            break
        end
    end

    -- 刷新行动顺序条
    Notifier.Dispatch("_Battle_Action_Sort", battleInst.sortedActionActors , battleInst.currActionActorIdx)

    if actor == nil then -- 全动完了
        battleInst.fsm:Switch("EXITROUND")
    else
        battleInst.fsm:Switch("ENTERACTOR", actor)
    end
end

function BattleStateDoRound:OnExit()
    battleInst.currActionActorIdx = battleInst.currActionActorIdx + 1  -- 行动角色索引后移
end

return BattleStateDoRound