_ENV = _G._ENV_ADVANTURE

local Action = {}
Action.__index = Action

function Action:InitEvents()
    self:AddEvent(0, "EventPlayAnim", "animName")
    local target = self.targets[1] -- 技能只有单目标
    -- 左右各一个溅射目标，先不考虑大体型
    local nearbyActors = {}
    local targetLeft = FindNearbyActor(target, -1)
    local targetRight = FindNearbyActor(target, 1)
    if targetLeft ~= nil and targetLeft ~= target and targetLeft.context.camp == target.context.camp then table.insert(nearbyActors, targetLeft) end
    if targetRight ~= nil and targetRight ~= target and targetRight.context.camp == target.context.camp then table.insert(nearbyActors, targetRight) end

    self:AddEvent(0.25, "EventDoDamage", self.skill, self.targets)
    if #nearbyActors > 0 then
        self:AddEvent(0.5, "EventDoDamage", self.skill, nearbyActors)
    end
end

return Action