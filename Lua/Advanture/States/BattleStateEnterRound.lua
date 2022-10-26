_ENV = _G._ENV_ADVANTURE

local Notifier = require('Framework.Notifier')

-- 战斗进入新回合
local BattleStateEnterRound = {}
local FSMachine = require 'Framework.FSMachine'
BattleStateEnterRound.__index = BattleStateEnterRound
setmetatable(BattleStateEnterRound, FSMachine.State)

function BattleStateEnterRound.Create()
    local copy = {}
    setmetatable(copy, BattleStateEnterRound)
    copy:Init()
    return copy
end

function BattleStateEnterRound:OnEnter()
    battleInst.currRound = battleInst.currRound + 1
    Notifier.Dispatch("_Battle_New_Round", battleInst.currRound)

    -- 生成排序过的行动角色列表
    battleInst.sortedActionActors = {}
    for _,actor in pairs(battleInst.grids) do
        if actor.hp > 0 then
            local alreadyIn = false
            local inserted = false
            for i,inlistActor in ipairs(battleInst.sortedActionActors) do
                if actor == inlistActor then
                    alreadyIn = true
                    break
                end
                if actor.context.class > inlistActor.context.class or -- 阶层优先
                   actor.context.class == inlistActor.context.class and actor.spd > inlistActor.spd or    -- 其次速度
                   actor.context.class == inlistActor.context.class and actor.spd == inlistActor.spd and actor.camp == 1 then  -- 最后攻防先动
                    table.insert(battleInst.sortedActionActors, i, actor)
                    inserted = true
                    break
                end
            end
            if not alreadyIn and not inserted then 
                table.insert(battleInst.sortedActionActors, actor)
            end
        end
    end
    battleInst.currActionActorIdx = 1  -- 重置回合行动角色索引

    -- 更新技能CD
    for _,actor in ipairs(battleInst.sortedActionActors) do
        for _,skill in ipairs(actor.skills) do
            skill.cd = skill.cd - 1
        end
    end

    self.loopActorIdx = 1
    self.loopBuffIdx = 1
    self.waitingForActionDone = false
    self.allLogicDone = false
end

function BattleStateEnterRound:OnUpdate(deltaTime)
    if not self.allLogicDone then
        if self.waitingForActionDone then -- 等待buff产生的action执行完毕
            if TestEverybodyInList(battleInst.advanture.actors, CheckActorState_IDLE_or_DEAD, nil, 1) then
                self.waitingForActionDone = false -- action执行完毕，继续buff逻辑遍历
                return
            end
        else -- 遍历执行所有buff的EnterRound逻辑
            for i=self.loopActorIdx,#battleInst.sortedActionActors do
                local actor = battleInst.sortedActionActors[i]
                for j=self.loopBuffIdx,#actor.buffs do
                    local action = actor.buffs[j]:EnterRound()
                    if action ~= nil then
                        -- buff逻辑产生了action，中断遍历，记录一下遍历索引
                        self.loopActorIdx = i
                        self.loopBuffIdx = j+1
                        self.waitingForActionDone = true
                        actor.fsm:Switch("ACTION", action)
                        return
                    end
                end
                self.loopBuffIdx = 1 -- 这里要重置一下
            end
            -- 标记逻辑执行完毕
            self.allLogicDone = true
        end
    else  -- 所有buff逻辑执行完毕，退出状态
        battleInst.fsm:Switch("DOROUND")
    end
end

function BattleStateEnterRound:OnExit()
    self.exitTimer = nil
end

return BattleStateEnterRound