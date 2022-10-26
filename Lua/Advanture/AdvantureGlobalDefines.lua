_G._ENV_ADVANTURE = {} -- 这里面的全局定义不会写入_G中，所有探索玩法相关的脚本都可以通过使用这个环境获取这些定义
setmetatable(_ENV_ADVANTURE, {__index = _G})
_ENV = _ENV_ADVANTURE

Action = require 'Advanture.Actions.Action'
Buff = require 'Advanture.Buffs.Buff'

-- 探索中的一些常量
GROUND_Y = -1.5     -- 地面在画面中Y轴的高度
BATTLE_FIELD_WIDTH = 17    -- 战斗场地X轴世界宽度(米)
BATTLE_FIELD_GRIDS = 10    -- 战斗场地格子数
SINGLE_GRID_WIDTH = BATTLE_FIELD_WIDTH / BATTLE_FIELD_GRIDS  -- 单格宽度

-- 探索中需要赋值的全局状态
WORLD_ROOT = nil  -- todo 先扔这，可能有问题
BATTLE_WORLD_OFFSET_X = 0 -- 当前战斗区域基于探险世界的x轴偏移，探险中会赋值

--## 探索环境下的单例 -----------------
advantureInst = nil  -- 探索实例
battleInst = nil    -- 战斗实例
virtualCamera = nil -- 探索世界虚拟相机实例
virtualCameraTarget = nil -- 探索世界虚拟相机跟随目标
--------------------------------------

-- 下面定义一些探险和战斗的通用计算方法，复杂函数放在这里也可以让OOP逻辑里更关注状态和结构，代码更简短清晰一些

--## 位置计算 ------------------------
-- 战斗中地格坐标转换世界坐标，注意：这里coord永远是按最左边的格子作为size的起点
function BattleCoord2WorldPos(coord, size)
    local worldX = SINGLE_GRID_WIDTH * (coord + 0.5) + BATTLE_WORLD_OFFSET_X
    local sizeOffsetX = (size - 1) * 0.5 * SINGLE_GRID_WIDTH
    return Vector3(worldX + sizeOffsetX, GROUND_Y, 0)
end

-- 战场中间的世界坐标X
function BattleCenterWorldX()
    return BATTLE_FIELD_WIDTH * 0.5 + BATTLE_WORLD_OFFSET_X
end

-- 战斗中相机聚焦于某个坐标时的世界坐标X
function BattleCameraFocusWorldXByActor(actor)
    local standCoords = GetActorStandingCoords(actor)
    return BattleCameraFocusWorldXByCoord(standCoords[1])
end

-- 战斗中相机聚焦于某个坐标时的世界坐标X
function BattleCameraFocusWorldXByCoord(coord)
    local scale = 0.1
    return -(BATTLE_FIELD_GRIDS * 0.5 - coord) * SINGLE_GRID_WIDTH * scale + BattleCenterWorldX()
end

-- 获取角色当前站位的格子(大体型占多个)
function GetActorStandingCoords(actor)
    assert(actor ~= nil, 'Error! GetActorStandingCoords -> actor is nil.')
    local coords = {}
    for coord,actorInGrid in pairs(battleInst.grids) do
        if actor == actorInGrid then
            table.insert(coords, coord)
        end
    end
    assert(#coords > 0, 'Error! actor\'s standing coords empty: '..actor.id..'[hp:'..actor.hp.."]")
    return coords
end

--## 技能施放逻辑 ----------------------------------------

-- 获取当前角色所有可移动到的站位
function GetAllPossibleStandCoords(actor)
    local casterStandingCoords = GetActorStandingCoords(actor) -- 角色初始位置
    local allPossibleStands = { casterStandingCoords } -- 所有角色可能的站位，先把角色当前站位加进去
    local minCoord = casterStandingCoords[1]
    local maxCoord = casterStandingCoords[#casterStandingCoords]
    -- 往前探
    for coord=minCoord-1,0,-1 do
        if battleInst.grids[coord] ~= nil then break end -- 有人就停止
        local standCoords = {coord}  -- 产生一个站位
        for s=1,actor.context.cfgTbl.size-1 do
            table.insert(standCoords, coord+s)   -- 向左找，所以体型坐标向右占
        end
        table.insert(allPossibleStands, standCoords)
    end
    -- 往后探
    for coord=maxCoord+1,BATTLE_FIELD_GRIDS-1 do
        if battleInst.grids[coord] ~= nil then break end -- 有人就停止
        local standCoords = {coord}  -- 产生一个站位
        for s=1,actor.context.cfgTbl.size-1 do
            table.insert(standCoords, 1, coord-s)   -- 向右找，所以体型坐标向左占
        end
        table.insert(allPossibleStands, standCoords)
    end
    return allPossibleStands
end

-- 获取当前角色施放技能的所有可行方案
function GetAllPlansForCastSkill(caster, skill)
    assert(caster ~= nil, "Error! GetAllPlansForCastSkill -> caster is nil.")
    assert(skill ~= nil, "Error! GetAllPlansForCastSkill -> skill is nil.")
    local allPossibleStands = GetAllPossibleStandCoords(caster) -- 所有角色可能的站位，先把角色当前站位加进去
    local allPlans = {}
    local anyPlanHasTaunt = false
    for i,standCoords in ipairs(allPossibleStands) do  -- 为每个可能站位分析目标
        local plan = {}
        plan.needMove = i > 1  -- 第一个站位是当前角色站位
        plan.standCoords = standCoords
        plan.hasTaunt = false  -- 计划书中是否有嘲讽目标
        plan.targets = {}
        local minCoord = standCoords[1]
        local maxCoord = standCoords[#standCoords]
        -- 往前探，从最左边+最小范围开始，到最左边+最大范围结束
        local coordFrom = math.max(0, minCoord - skill.cfgTbl.rangeMin)
        local coordTo = math.max(0, minCoord - skill.cfgTbl.rangeMax)
        for coord=coordFrom,coordTo,-1 do
            local target = battleInst.grids[coord]
            if target ~= nil then  -- 格子有人
                if not plan.hasTaunt or skill.cfgTbl.isAOE or target:CheckSpState("taunt") then -- 如果计划中已经包含嘲讽目标，而且不是AOE技能，则不用再可考非嘲讽目标
                    local dist = minCoord - coord
                    if dist >= skill.cfgTbl.rangeMin and dist <= skill.cfgTbl.rangeMax then -- 目标距离在技能范围内
                        local sameCamp = target.context.camp == caster.context.camp
                        if sameCamp and skill.cfgTbl.campFilter == 1 or not sameCamp and skill.cfgTbl.campFilter == 2 then -- 目标阵营符合技能配置
                            if skill.cfgTbl.campFilter == 2 and target:CheckSpState("taunt") then -- 如果技能是对敌方释放的，才考虑嘲讽问题
                                if not plan.hasTaunt and not skill.cfgTbl.isAOE then  -- 如果这是第一个发现的嘲讽目标，且不是AOE技能，清除掉之前的目标
                                    plan.targets = {} 
                                end
                                plan.hasTaunt = true
                            end
                            -- print(i.."-LEFT:"..target.id)
                            table.insert(plan.targets, target)
                        end
                    end
                end
            end
        end
        -- 往后探
        local coordFrom = math.min(BATTLE_FIELD_GRIDS-1, maxCoord + skill.cfgTbl.rangeMin)
        local coordTo = math.min(BATTLE_FIELD_GRIDS-1, maxCoord + skill.cfgTbl.rangeMax)
        for coord=maxCoord+skill.cfgTbl.rangeMin,BATTLE_FIELD_GRIDS-1 do
            local target = battleInst.grids[coord]
            if target ~= nil then  -- 格子有人
                if not plan.hasTaunt or skill.cfgTbl.isAOE or target:CheckSpState("taunt") then -- 如果计划中已经包含嘲讽目标，而且不是AOE技能，则不用再可考非嘲讽目标
                    local dist = coord - maxCoord
                    if dist >= skill.cfgTbl.rangeMin and dist <= skill.cfgTbl.rangeMax then -- 目标距离在技能范围内
                        local sameCamp = target.context.camp == caster.context.camp
                        if sameCamp and skill.cfgTbl.campFilter == 1 or not sameCamp and skill.cfgTbl.campFilter == 2 then -- 目标阵营符合技能配置
                            if skill.cfgTbl.campFilter == 2 and target:CheckSpState("taunt") then -- 如果技能是对敌方释放的，才考虑嘲讽问题
                                if not plan.hasTaunt and not skill.cfgTbl.isAOE then  -- 如果这是第一个发现的嘲讽目标，且不是AOE技能，清除掉之前的目标
                                    plan.targets = {} 
                                end
                                plan.hasTaunt = true
                            end
                            -- print(i.."-LEFT:"..target.id)
                            table.insert(plan.targets, target)
                        end
                    end
                end
            end
        end
        if #plan.targets > 0 then
            if plan.hasTaunt and not anyPlanHasTaunt then allPlans = {} end -- 如果这是第一个包含嘲讽目标的计划，清除掉之前的计划
            anyPlanHasTaunt = anyPlanHasTaunt or plan.hasTaunt   -- 标记所有计划中存在嘲讽目标
            if not anyPlanHasTaunt or plan.hasTaunt then  -- 如果之前的计划包含了嘲讽目标，则后面的计划书只要也包含嘲讽目标的
                -- 如果计划里目标和之前某个计划完全一样，则抛弃这个计划
                local hasAllSameTargetsInPrePlan = false
                for _,prePlan in ipairs(allPlans) do
                    if CommonUtils.CompareTwoLists(plan.targets, prePlan.targets) then
                        hasAllSameTargetsInPrePlan = true
                        break
                    end
                end
                if not hasAllSameTargetsInPrePlan then table.insert(allPlans, plan) end
            end
        end
    end

    return allPlans
end

-- 基于角色寻找相邻的人
function FindNearbyActor(baseActor, coordOffset)
    local standCoords = GetActorStandingCoords(baseActor)
    -- 先不考虑大体型
    local baseCoord = standCoords[1]
    return battleInst.grids[baseCoord + coordOffset]
end
---------------------------------------------------------

--## 效果表现 --------------------------------------------
-- 临时播特效用的
function PlayEffect(resName, position, lastTime)
    local gameObject = GameObject.Instantiate(Resources.Load("Effects/"..resName), position, Quaternion.identity, WORLD_ROOT)
    if lastTime ~= nil and lastTime > 0 then
        local autoDestroy = gameObject:GetComponent("autoDestroy")
        if autoDestroy == nil then autoDestroy = gameObject:AddComponent(typeof(CS.AutoDestroy)) end
        autoDestroy.delay = lastTime
    end
    return gameObject
end
function DrawBezierCurve(resName, startPosition, endPosition)
    local gameObject = GameObject.Instantiate(Resources.Load("Effects/"..resName), Vector3.zero, Quaternion.identity, WORLD_ROOT)
    local bezierRenderer = gameObject:GetComponent(typeof(CS.BezierLineRenderer))
    local ctrlPosition = startPosition + (endPosition - startPosition) * 0.5 + Vector3(0, 3, 0)
    bezierRenderer:DrawQuadraticCurve(20, startPosition, ctrlPosition, endPosition)
    return gameObject
end
-----------------------------------------------------------

--## 其他通用方法 ------------------------------------------
-- 遍历判断条件
function TestEverybodyInList(list, testfunc, tester, mode) -- mode 1全部满足, 2任意满足
    local result = mode == 1
    for _,body in ipairs(list) do
        local b
        if tester == nil then b = testfunc(body)
        else b = testfunc(tester, body) end

        if mode == 1 then result = result and b
        else result = result or b end

        -- 短路一下，节省计算
        if mode == 1 and result == false then return false end
        if mode == 2 and result == true then return true end
    end
    return result
end
function CheckActorState_IDLE_or_DEAD(actor)
    return actor.fsm.currStateName == "IDLE" or actor.fsm.currStateName == "DEAD"
end
-----------------------------------------------------------

--## debug方法 -------------------------------------
function __Debug_DumpBattleGrids()
    local message = ''
    for coord,actorInGrid in pairs(battleInst.grids) do
        message = message .. string.format("[%d:%s]", coord, actorInGrid ~= nil and tostring(actorInGrid.id) or 'nil')
    end
    print(message)
end