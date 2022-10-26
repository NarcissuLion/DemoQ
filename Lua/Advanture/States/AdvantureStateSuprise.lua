_ENV = _G._ENV_ADVANTURE

-- 探险中队伍行进到下一个事件时
local AdvantureStateSuprise = {}
local FSMachine = require 'Framework.FSMachine'
AdvantureStateSuprise.__index = AdvantureStateSuprise
setmetatable(AdvantureStateSuprise, FSMachine.State)

function AdvantureStateSuprise.Create()
    local copy = {}
    setmetatable(copy, AdvantureStateSuprise)
    copy:Init()
    return copy
end

function AdvantureStateSuprise:OnEnter()
    self.delayTime = nil
    require('Framework.Timer').Create(0.2, #advantureInst.actors, self.ActorSuprise, self):Play()
end

function AdvantureStateSuprise:ActorSuprise(counter)
    local actor = advantureInst.actors[counter]
    actor.fsm:Switch("IDLE")
    if counter == 1 then
        PlayEffect("ef_suprise", actor:GetPosition() + Vector3(0, 2.5, 0), 0.75)
    else
        PlayEffect("ef_sweat", actor:GetPosition() + Vector3(0, 2, 0), 0.4)
        actor.renderer.transform:DOScale(Vector3(0.9, 1.1, 1), 0.1):SetLoops(2, CS.DG.Tweening.LoopType.Yoyo)
    end
end

function AdvantureStateSuprise:OnUpdate(deltaTime)
    if self.delayTime == nil then
        if TestEverybodyInList(advantureInst.actors, self.CheckActorState, self, 1) then
            self.delayTime = 0.5 -- 所有人状态都OK后，启动延迟
        end
    else
        if self.delayTime > 0 then
            self.delayTime = self.delayTime - deltaTime
            if self.delayTime <= 0 then
                advantureInst.fsm:Switch("ENCOUNTER")
            end
        end
    end
end

function AdvantureStateSuprise:CheckActorState(actor)
    return actor.fsm.currStateName == "IDLE"
end

function AdvantureStateSuprise:OnExit()
    self.delayTime = nil
end

return AdvantureStateSuprise