_ENV = _G._ENV_ADVANTURE

-- 探险中队伍行进状态
local AdvantureStateWalk = {}
local FSMachine = require 'Framework.FSMachine'
AdvantureStateWalk.__index = AdvantureStateWalk
setmetatable(AdvantureStateWalk, FSMachine.State)

function AdvantureStateWalk.Create()
    local copy = {}
    setmetatable(copy, AdvantureStateWalk)
    copy:Init()
    return copy
end

function AdvantureStateWalk:OnEnter()
    -- 设置终点位置
    self.goalPosX = advantureInst:GetCurrWave().context.posx
    -- 依次让角色进入Walk状态
    require('Framework.Timer').Create(0.1, #advantureInst.actors, self.ActorFirstStep, self):Play()
end

function AdvantureStateWalk:ActorFirstStep(counter)
    advantureInst.actors[counter].fsm:Switch("WALK")
end

function AdvantureStateWalk:OnUpdate(deltaTime)
    -- 监测抵达终点
    local vcameraTargetPos = advantureInst.actors[1]:GetPosition()
    virtualCameraTarget.position = vcameraTargetPos
    if vcameraTargetPos.x >= self.goalPosX then
        advantureInst.fsm:Switch("SUPRISE")
    end
end

function AdvantureStateWalk:OnExit()
end

return AdvantureStateWalk