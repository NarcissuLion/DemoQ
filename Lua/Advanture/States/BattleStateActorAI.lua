_ENV = _G._ENV_ADVANTURE

local Notifier = require 'Framework.Notifier'

-- 角色等待AI输入指令
local BattleStateActorAI = {}
local FSMachine = require 'Framework.FSMachine'
BattleStateActorAI.__index = BattleStateActorAI
setmetatable(BattleStateActorAI, FSMachine.State)

function BattleStateActorAI.Create()
    local copy = {}
    setmetatable(copy, BattleStateActorAI)
    copy:Init()
    return copy
end

function BattleStateActorAI:OnExit()
    self.actor = nil
end

function BattleStateActorAI:OnEnter(actor)
    assert(actor ~= nil, "Error! BattleStateActorAI.OnEnter -> actor is nil.")
    self.actor = actor
    PlayEffect("ef_thinking", actor:GetPosition() + Vector3(0, 2.5, 0), 0.5)
    require('Framework.Timer').Create(0.5, 1, self.MakeDecision, self):Play() -- 假装思考1秒中
end

function BattleStateActorAI:MakeDecision()
    local sortedSkills = {}
    for _,skill in ipairs(self.actor.skills) do
        if skill.cd <= 0 then  -- 判断cd
            local inserted = false
            for i,sortedSkill in ipairs(sortedSkills) do
                if skill.cfgTbl.priority > sortedSkill.cfgTbl.priority then  -- 根据技能优先级排序
                    table.insert(sortedSkills, i, skill)
                    inserted = true
                    break
                end
            end
            if not inserted then table.insert(sortedSkills, skill) end
        end
    end

    if #sortedSkills > 0 then
        for i,skill in ipairs(sortedSkills) do
            if self.actor == nil then print("WTFF") end
            local allPlans = GetAllPlansForCastSkill(self.actor, skill)  -- 获取该技能施放计划书和目标查找表
            if #allPlans > 0 then  -- 如果当前技能没有可用计划书，就跳过到下一个技能
                local plan = allPlans[1] -- 这里涉及AI策略，暂时先只选第一个方案书
                local targets = skill.cfgTbl.isAOE and plan.targets or { plan.targets[math.random(1, #plan.targets)] }  -- 非AOE的随机选个目标
                skill.cd = skill.cfgTbl.intervalCD+1
                local newCoord = plan.needMove and plan.standCoords[1] or nil
                battleInst.fsm:Switch("DOACTOR", self.actor, skill, targets, newCoord)  -- 带着计划书进入行动环节
                return -- 状态在逻辑中Switch时，一定要保证中断后面逻辑，不然很容易产生难查的bug
            end
        end
    else
        battleInst.fsm:Switch("EXITACTOR", self.actor) -- 没有可用技能，直接跳过阶段
        return
    end
end

return BattleStateActorAI