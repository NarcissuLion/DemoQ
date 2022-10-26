_ENV = _G._ENV_ADVANTURE

local Event = {}
Event.__index = Event

function Event:OnInit(animName)
    assert(animName ~= nil, self:DumpErrorFormat("animName invalidate."))
    self.animName = animName 
    self.autoExitTime = 0.5   -- 蹦跶一下0.5秒, 以后从动画文件中读取
end

function Event:OnDispose()
    self.animName = nil
end

function Event:OnEnter()
    -- play anim，先蹦跶一下意思意思
    self.caster.renderer.transform:DOMove(self.caster.renderer.transform.position + Vector3(0, 0.2, 0), 0.1):SetLoops(2, CS.DG.Tweening.LoopType.Yoyo)
end

return Event