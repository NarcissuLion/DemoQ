_ENV = _G._ENV_ADVANTURE

-- 战斗离开回合
local BattleStateExitRound = {}
local FSMachine = require 'Framework.FSMachine'
BattleStateExitRound.__index = BattleStateExitRound
setmetatable(BattleStateExitRound, FSMachine.State)

function BattleStateExitRound.Create()
    local copy = {}
    setmetatable(copy, BattleStateExitRound)
    copy:Init()
    return copy
end

function BattleStateExitRound:OnEnter()
    self.loopActorIdx = 1
    self.loopBuffIdx = 1
    self.waitingForActionDone = false
    self.allLogicDone = false
end

function BattleStateExitRound:OnUpdate(deltaTime)
    if not self.allLogicDone then -- buff进入回合逻辑还没执行完
        if self.waitingForActionDone then -- 等待buff产生的action执行完毕
            if TestEverybodyInList(battleInst.advanture.actors, CheckActorState_IDLE_or_DEAD, nil, 1) then
                self.waitingForActionDone = false -- action执行完毕，继续buff逻辑遍历
                return
            end
        else -- 遍历执行所有buff的ExitRound逻辑
            for i=self.loopActorIdx,#battleInst.sortedActionActors do
                local actor = battleInst.sortedActionActors[i]
                self.loopBuffIdx = 1 -- 这里要重置一下
                for j=self.loopBuffIdx,#actor.buffs do
                    local action = actor.buffs[j]:ExitRound()
                    if action ~= nil then
                        -- buff逻辑产生了action，中断遍历，记录一下遍历索引
                        self.loopActorIdx = i
                        self.loopBuffIdx = j
                        self.waitingForActionDone = true
                        actor.fsm:Switch("ACTION", action)
                        return
                    end
                end
            end
            -- 标记逻辑执行完毕
            self.allLogicDone = true
        end
    else
        -- 执行完回合结束逻辑后，更新Buff回合并清除
        for _,actor in ipairs(battleInst.sortedActionActors) do
            actor:RefreshBuffs()
        end
        battleInst.fsm:Switch("ENTERROUND")
    end
end

return BattleStateExitRound