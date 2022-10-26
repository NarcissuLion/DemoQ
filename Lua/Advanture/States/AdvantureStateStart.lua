_ENV = _G._ENV_ADVANTURE

-- 探险开始
local AdvantureStateStart = {}
local FSMachine = require 'Framework.FSMachine'
AdvantureStateStart.__index = AdvantureStateStart
setmetatable(AdvantureStateStart, FSMachine.State)

function AdvantureStateStart.Create()
    local copy = {}
    setmetatable(copy, AdvantureStateStart)
    copy:Init()
    return copy
end

function AdvantureStateStart:OnEnter()
    -- 暂时没什么事干，初始化一下队伍位置
    local startOffset = -16
    for _,actor in ipairs(advantureInst.actors) do
        actor:SetPosition(Vector3(startOffset, GROUND_Y, 0))
    end
    virtualCameraTarget.position = Vector3(0, GROUND_Y, 0)

    advantureInst.fsm:Switch("LINEUP")
end

function AdvantureStateStart:OnUpdate()
end

return AdvantureStateStart