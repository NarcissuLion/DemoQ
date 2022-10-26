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
    battleInst.fsm:Switch("ENTERROUND")
end

return BattleStateStart