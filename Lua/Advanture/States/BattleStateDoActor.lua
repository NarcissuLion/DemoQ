_ENV = _G._ENV_ADVANTURE

-- 角色执行回合行动
local BattleStateDoActor = {}
local FSMachine = require 'Framework.FSMachine'
BattleStateDoActor.__index = BattleStateDoActor
setmetatable(BattleStateDoActor, FSMachine.State)

function BattleStateDoActor.Create()
    local copy = {}
    setmetatable(copy, BattleStateDoActor)
    copy:Init()
    return copy
end

function BattleStateDoActor:OnExit()
    self.actor = nil
end

function BattleStateDoActor:OnEnter(actor, skill, targets, newCoord)
    self.actor = actor
    local action = Action.Create(skill.cfgTbl.castAction, actor, skill, targets)
    if newCoord ~= nil then
        battleInst:ChangeActorCoord(actor, newCoord)
        actor.fsm:Switch("RUN", BattleCoord2WorldPos(newCoord, actor.context.cfgTbl.size), 0.5, action)
    else
        actor.fsm:Switch("ACTION", action)
    end
end

function BattleStateDoActor:OnUpdate(deltaTime)
    if TestEverybodyInList(battleInst.advanture.actors, CheckActorState_IDLE_or_DEAD, nil, 1) then
        battleInst.fsm:Switch('EXITACTOR', self.actor)
    end
end

return BattleStateDoActor