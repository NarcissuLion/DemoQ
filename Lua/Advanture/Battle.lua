_ENV = _G._ENV_ADVANTURE

local Battle = {}
Battle.__index = Battle

local Notifier = require 'Framework.Notifier'

function Battle.Create(context, advanture)
    assert(battleInst == nil, 'Error! duplicate battle instance.')
    battleInst = {}
    setmetatable(battleInst, Battle)
    battleInst:Init(context, advanture)
    return battleInst
end

function Battle:Init(context, advanture)
    self.context = context
    self.advanture = advanture

    self.grids = {}  -- 战斗实例中的grids里存放actor实例，方便查找
    for coord,actorID in pairs(self.context.grids) do
        self.grids[coord] = advanture.actors[actorID]
    end

    -- 一些战斗中的状态 -----------------------------
    self.currRound = 0  -- 当前回合数
    self.sortedActionActors = nil -- 当前回合的待行动角色(速度排序)
    self.currActionActorIdx = 1  -- 当前回合的待行动角色索引
    ------------------------------------------------

    require('UI.BattleUI').Create() -- Demo先不接UI系统了，放在这里但是不要直接调用里面方法，还是通过消息通信

    self.fsm = require('Framework.FSMachine').Create(self)
    self.fsm:Add("START", require('Advanture.States.BattleStateStart').Create())
    self.fsm:Add("END", require('Advanture.States.BattleStateEnd').Create())
    self.fsm:Add("ENTERROUND", require('Advanture.States.BattleStateEnterRound').Create())
    self.fsm:Add("DOROUND", require('Advanture.States.BattleStateDoRound').Create())
    self.fsm:Add("EXITROUND", require('Advanture.States.BattleStateExitRound').Create())
    self.fsm:Add("ENTERACTOR", require('Advanture.States.BattleStateEnterActor').Create())
    self.fsm:Add("USERINPUT", require('Advanture.States.BattleStateUserInput').Create())
    self.fsm:Add("ACTORAI", require('Advanture.States.BattleStateActorAI').Create())
    self.fsm:Add("DOACTOR", require('Advanture.States.BattleStateDoActor').Create())
    self.fsm:Add("EXITACTOR", require('Advanture.States.BattleStateExitActor').Create())
    -- self.fsm.debug = true
    self.fsm:Switch("START")
end

function Battle:Dispose()
    self.grids = nil
    self.fsm:Dispose()
    self.fsm = nil
    battleInst = nil
end

function Battle:Update(deltaTime)
    self.fsm:Update(deltaTime)
end

function Battle:ChangeActorCoord(actor, newCoord)
    for coord,actorInGrid in pairs(self.grids) do
        if actor == actorInGrid then
            self.grids[coord] = nil
        end
    end
    for i=0,actor.context.cfgTbl.size-1 do
        self.grids[newCoord+i] = actor
    end
end

-- 清扫一下战场grids信息
function Battle:RefreshGrids()
    -- 移除尸体
    for coord,actor in pairs(self.grids) do
        if actor ~= nil and actor.hp <= 0 then
            self.grids[coord] = nil
        end
    end
end

-- 查询战斗结束状态，Demo先写死判断全部阵亡
function Battle:CheckDone()
    local aliveNum = {0, 0}
    for _,actor in pairs(self.advanture.actors) do
        if actor.hp > 0 then aliveNum[actor.context.camp] = aliveNum[actor.context.camp] + 1 end
    end
    local done = aliveNum[1] == 0 or aliveNum[2] == 0
    local win = aliveNum[2] == 0
    return done, win
end

return Battle