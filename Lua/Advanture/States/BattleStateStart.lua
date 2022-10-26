_ENV = _G._ENV_ADVANTURE

local Notifier = require('Framework.Notifier')

-- 战斗开始状态
local BattleStateStart = {}
local FSMachine = require 'Framework.FSMachine'
BattleStateStart.__index = BattleStateStart
setmetatable(BattleStateStart, FSMachine.State)

function BattleStateStart.Create()
    local copy = {}
    setmetatable(copy, BattleStateStart)
    copy:Init()
    return copy
end

function BattleStateStart:OnEnter()
    Notifier.Dispatch("_Battle_Start")
    -- 战斗开始重置下技能初始CD
    for _,actor in ipairs(battleInst.advanture.actors) do
        for _,skill in ipairs(actor.skills) do
            skill.cd = skill.cfgTbl.startCD+1
        end
    end

    self.loopActorIdx = 1
    self.loopSkillIdx = 1
    self.waitingForActionDone = false
    self.allLogicDone = false
end

function BattleStateStart:OnUpdate(deltaTime)
    if not self.allLogicDone then
        if self.waitingForActionDone then -- 等待开场技能action执行完毕
            if TestEverybodyInList(battleInst.advanture.actors, CheckActorState_IDLE_or_DEAD, nil, 1) then
                self.waitingForActionDone = false -- action执行完毕，继续逻辑遍历
                return
            end
        else -- 遍历执行所有skill的enterAction逻辑
            for i=self.loopActorIdx,#advantureInst.actors do
                local actor = advantureInst.actors[i]
                for j=self.loopSkillIdx,#actor.skills do
                    local enterActionName = actor.skills[j].cfgTbl.enterAction
                    if enterActionName ~= nil then
                        -- 技能有enterAction，中断遍历，记录一下遍历索引
                        self.loopActorIdx = i
                        self.loopSkillIdx = j+1
                        self.waitingForActionDone = true
                        -- 开场技action不用传目标，技能action中自己确定
                        actor.fsm:Switch("ACTION", Action.Create(enterActionName, actor, actor.skills[j]))
                        return
                    end
                end
                self.loopSkillIdx = 1 -- 这里要重置一下
            end
            -- 标记逻辑执行完毕
            self.allLogicDone = true
        end
    else  -- 所有buff逻辑执行完毕，退出状态
        battleInst.fsm:Switch("ENTERROUND")
    end
end

return BattleStateStart