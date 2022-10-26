_ENV = _G._ENV_ADVANTURE

local Buff = {}
Buff.__index = Buff

Buff.round = 99999

function Buff:OnExitRound()
    local standCoords = GetActorStandingCoords(self.owner)
    local maxCoord = standCoords[#standCoords]
    -- 向右找一个空格
    for coord=maxCoord+1,BATTLE_FIELD_GRIDS-1 do
        if battleInst.grids[coord] == nil then
            local actorContext = {}
            actorContext.coord = coord
            actorContext.cfgID = 202
            actorContext.camp = 2
            actorContext.class = 0
            return Action.Create("simple_spawn_actor", self.owner, nil, nil, 0.25, actorContext)
        end
    end
    return nil
end

return Buff