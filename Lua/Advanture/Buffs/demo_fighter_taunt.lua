_ENV = _G._ENV_ADVANTURE

local Buff = {}
Buff.__index = Buff

Buff.icon = "Sprites/SpriteAssets/Icon/buff_taunt"
Buff.priority = 0
Buff.round = 2

function Buff:OnAdd()
    self.owner:AddSpState("taunt")
end

function Buff:OnRemove()
    self.owner:RemoveSpState("taunt")
end

return Buff