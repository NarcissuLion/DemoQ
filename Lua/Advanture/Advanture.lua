require 'Advanture.AdvantureGlobalDefines'

_ENV = _G._ENV_ADVANTURE

local Advanture = {}
Advanture.__index = Advanture

local __AUTO_INC_ID = 1

function Advanture.Create(context)
    assert(advantureInst == nil, 'Error! duplicate advanture instance.')
    advantureInst = {}
    setmetatable(advantureInst, Advanture)
    advantureInst:Init(context)
    return advantureInst
end

function Advanture:Init(context)
    self.context = context

    self.actors = {}   -- 全体角色，包括敌我
    -- 创建角色实例
    for _, heroContext in ipairs(context.heros) do
        self:CreateActor(heroContext)
    end

    self.waves = {}  -- 所有波次实例
    for i, waveContext in ipairs(context.waves) do
        self:CreateWave(i, waveContext)
    end
    self.currWaveSeq = 1
    
    -- 虚拟相机
    virtualCamera = GameObject.Find("World/VCamera"):GetComponent("CinemachineVirtualCamera")
    virtualCameraTarget = GameObject.Find("World/VCameraTarget").transform

    -- 全局常量初始化
    WORLD_ROOT = GameObject.Find("World").transform
    BATTLE_WORLD_OFFSET_X = 0

    -- 探险状态机
    self.fsm = require('Framework.FSMachine').Create(self)
    self.fsm:Add("START", require('Advanture.States.AdvantureStateStart').Create())
    self.fsm:Add("LINEUP", require('Advanture.States.AdvantureStateLineUp').Create())
    self.fsm:Add("WALK", require('Advanture.States.AdvantureStateWalk').Create())
    self.fsm:Add("SUPRISE", require('Advanture.States.AdvantureStateSuprise').Create())
    self.fsm:Add("ENCOUNTER", require('Advanture.States.AdvantureStateEncounter').Create())
    self.fsm:Switch("START")
end

function Advanture:Dispose()
    for _,actor in ipairs(self.actors) do
        actor:Dispose()
    end
    self.actors = nil
    self.fsm:Dispose()
    self.fsm = nil
    self.context = nil
    advantureInst = nil
    virtualCamera = nil
    virtualCameraTarget = nil
end

function Advanture:Update(deltaTime)
    self.fsm:Update(deltaTime)
    for _,actor in ipairs(self.actors) do
        actor:Update(deltaTime)
    end
end

function Advanture:CreateActor(context)
    local actor = require('Advanture.Actor').Create(__AUTO_INC_ID, context)
    self.actors[actor.id] = actor
    __AUTO_INC_ID = __AUTO_INC_ID + 1
    return actor
end

function Advanture:CreateWave(idx, context)
    local wave = require('Advanture.Wave').Create(idx, context)
    table.insert(self.waves, wave)
end

function Advanture:GetCurrWave()
    return self.waves[self.currWaveSeq]
end

return Advanture