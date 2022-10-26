_ENV = _G._ENV_ADVANTURE

local Buff = {}
Buff.__index = Buff

Buff.round = 99999

function Buff:OnExitRound()
    local standCoords = battleInst:GetActorStandingCoords(self.owner)
    local maxCoord = standCoords[#standCoords]
    -- 向右找一个空格
    for coord=maxCoord+1,BATTLE_FIELD_GRIDS-1 do
        if battleInst.grids[coord] == nil then
            local context = {}
            context.coord = coord
            context.cfgID = 202
            context.camp = 2
            context.class = 0
            return Action.Create("simple_spawn_actor", context)
        end
    end
    return nil
end

return Buff