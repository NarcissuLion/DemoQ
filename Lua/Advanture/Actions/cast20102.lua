_ENV = _G._ENV_ADVANTURE

local Action = {}
Action.__index = Action

function Action:InitEvents()
    local monsters = {}
    for coord=7,9 do
        if battleInst.grids[coord] ~= nil then
            table.insert(monsters, battleInst.grids[coord])
        end
    end

    local powerFactor = 1 + #monsters
    if powerFactor == 1 then
        self:AddEvent(0, "EventPlayAnim", "animName")
        self:AddEvent(0.25, "EventDoDamage", self.skill, self.targets)
    else
        self:AddEvent(0, "EventPlayAnim", "animName")
        self:AddEvent(0.25, "EventKillActors", monsters)
        self:AddEvent(1, "EventPlayAnim", "animName")
        self:AddEvent(1.25, "EventDoDamage", self.skill, self.targets, powerFactor)
    end

end

return Action