_ENV = _G._ENV_ADVANTURE

-- 探险中遭遇事件
local AdvantureStateEncounter = {}
local FSMachine = require 'Framework.FSMachine'
AdvantureStateEncounter.__index = AdvantureStateEncounter
setmetatable(AdvantureStateEncounter, FSMachine.State)

function AdvantureStateEncounter.Create()
    local copy = {}
    setmetatable(copy, AdvantureStateEncounter)
    copy:Init()
    return copy
end

function AdvantureStateEncounter:OnEnter()
    local wave = advantureInst:GetCurrWave()
    local func = self["PrepareEvent_"..wave.context.type]
    assert(func ~= nil, 'Error! type of wave unsupported: '..wave.context.type)
    func(self, wave)
end

-- 准备遭遇战斗事件
function AdvantureStateEncounter:PrepareEvent_Battle(wave)
    battleContext = {}  -- 构造battleContext
    wave.context.battle = battleContext

    battleContext.grids = {}    -- 基于战场格子的容器，存放actor id
    for coord=0,BATTLE_FIELD_GRIDS-1 do -- 战场内格子坐标从0开始
        battleContext.grids[coord] = 0        -- 先全部初始化成空格子
    end
    BATTLE_WORLD_OFFSET_X = wave.context.posx + 8 -- 整个战场偏移于探险世界的坐标，事件起始位置往后错几米
    
    -- 创建所有敌人实例
    for _,enemy_c in ipairs(wave.context.enemies) do
        advantureInst:CreateActor(enemy_c)
    end
    -- 将所有角色注册进battleContext中
    for id,actor in pairs(advantureInst.actors) do
        local coord = actor.context.coord
        actor:SetSortOrder(coord) -- 进战斗前根据从左到右排下显示层次
        battleContext.grids[coord] = id
        for i=2,actor.context.cfgTbl.size do -- 大体型怪物占多格
            battleContext.grids[coord+i] = id
        end
        local worldPos = BattleCoord2WorldPos(coord, actor.context.cfgTbl.size)
        if actor.context.camp == 1 then
            actor.fsm:Switch("RUN", worldPos, 1) -- 我方角色跑步就位
        else
            actor:SetPosition(worldPos)  -- 敌方角色直接站好
        end
    end
    
    -- 镜头对准战场中间
    local battleFieldCenter = Vector3(BattleCenterWorldX(), GROUND_Y, 0)
    virtualCameraTarget:DOMove(battleFieldCenter, 1):OnComplete(function()
        self.battle = require('Advanture.Battle').Create(battleContext, advantureInst)
    end)
end

-- 准备遇到宝箱事件
function AdvantureStateEncounter:PrepareEvent_Chest()
end

function AdvantureStateEncounter:OnUpdate(deltaTime)
    if self.battle ~= nil then
        self.battle:Update(deltaTime)
    end
end

function AdvantureStateEncounter:OnExit()
    if self.battle ~= nil then
        self.battle:Dispose()
        self.battle = nil
    end
    advantureInst.currWaveSeq = advantureInst.currWaveSeq + 1
end

return AdvantureStateEncounter