_ENV = _G._ENV_ADVANTURE

-- 角色进入战斗回合
local BattleStateExitActor = {}
local FSMachine = require 'Framework.FSMachine'
BattleStateExitActor.__index = BattleStateExitActor
setmetatable(BattleStateExitActor, FSMachine.State)

function BattleStateExitActor.Create()
    local copy = {}
    setmetatable(copy, BattleStateExitActor)
    copy:Init()
    return copy
end

function BattleStateExitActor:OnExit()
    self.actor = nil
end

function BattleStateExitActor:OnEnter(actor)
    self.actor = actor
    -- 清理下战场
    battleInst:RefreshGrids()

    self.loopBuffIdx = 1
    self.waitingForActionDone = false
    self.allLogicDone = false
end

function BattleStateExitActor:OnUpdate(deltaTime)
    if not self.allLogicDone then -- buff进入回合逻辑还没执行完
        if self.waitingForActionDone then -- 等待buff产生的action执行完毕
            if TestEverybodyInList(battleInst.advanture.actors, CheckActorState_IDLE_or_DEAD, nil, 1) then
                self.waitingForActionDone = false -- action执行完毕，继续buff逻辑遍历
                return
            end
        else -- 遍历执行所有buff的ExitRound逻辑
            self.loopBuffIdx = 1 -- 这里要重置一下
            for j=self.loopBuffIdx,#self.actor.buffs do
                local action = self.actor.buffs[j]:ExitActor()
                if action ~= nil then
                    -- buff逻辑产生了action，中断遍历，记录一下遍历索引
                    self.loopBuffIdx = j
                    self.waitingForActionDone = true
                    self.actor.fsm:Switch("ACTION", action)
                    return
                end
            end
            -- 标记逻辑执行完毕
            self.allLogicDone = true
        end
    else
           -- 判断战斗是否结束
        local done,win = battleInst:CheckDone()
        if done then
            battleInst.fsm:Switch("END", win)
        else
            battleInst.fsm:Switch("DOROUND")
        end
    end
end

return BattleStateExitActor