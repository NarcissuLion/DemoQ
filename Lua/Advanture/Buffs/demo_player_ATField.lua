_ENV = _G._ENV_ADVANTURE

local Buff = {}
Buff.__index = Buff

Buff.icon = "Sprites/SpriteAssets/Icon/buff_ATField"
Buff.priority = -999        -- 这里写的很小为了保证逻辑最后执行
Buff.round = 1

function Buff:OnBeHit(hit)
    if hit.value < 0 and self.owner.hp + hit.value <= 0 then  -- 是致死伤害
        hit.value = -self.owner.hp + 1  -- 锁1点血
    end
end

return Buff