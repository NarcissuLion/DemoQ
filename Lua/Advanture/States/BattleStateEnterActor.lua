_ENV = _G._ENV_ADVANTURE

-- 角色进入战斗回合
local BattleStateEnterActor = {}
local FSMachine = require 'Framework.FSMachine'
BattleStateEnterActor.__index = BattleStateEnterActor
setmetatable(BattleStateEnterActor, FSMachine.State)

function BattleStateEnterActor.Create()
    local copy = {}
    setmetatable(copy, BattleStateEnterActor)
    copy:Init()
    return copy
end

function BattleStateEnterActor:OnExit()
    self.actor = nil
end

function BattleStateEnterActor:OnEnter(actor)
    self.actor = actor
    -- 角色行动前镜头偏移一下
    local camPosX = BattleCameraFocusWorldXByActor(actor)
    local battleFieldCenter = Vector3(camPosX, GROUND_Y, 0)
    virtualCameraTarget:DOMove(battleFieldCenter, 0.5)

    self.loopBuffIdx = 1
    self.waitingForActionDone = false
    self.allLogicDone = false
end

function BattleStateEnterActor:OnUpdate(deltaTime)
    if not self.allLogicDone then -- buff进入回合逻辑还没执行完
        if self.waitingForActionDone then -- 等待buff产生的action执行完毕
            if TestEverybodyInList(battleInst.advanture.actors, CheckActorState_IDLE_or_DEAD, nil, 1) then
                self.waitingForActionDone = false -- action执行完毕，继续buff逻辑遍历
                return
            end
        else -- 遍历执行所有buff的ExitRound逻辑
            self.loopBuffIdx = 1 -- 这里要重置一下
            for j=self.loopBuffIdx,#self.actor.buffs do
                local action = self.actor.buffs[j]:EnterActor()
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
        local ai = self.actor.context.camp == 2 -- Demo目前只有敌方走AI
        if ai then battleInst.fsm:Switch("ACTORAI", self.actor)
        else battleInst.fsm:Switch("USERINPUT", self.actor) end
    end
end

return BattleStateEnterActor