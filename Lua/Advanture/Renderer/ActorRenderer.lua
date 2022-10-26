local ActorRenderer = {}
ActorRenderer.__index = ActorRenderer

function ActorRenderer.Create(id, prefab)
    local copy = {}
    setmetatable(copy, ActorRenderer)
    copy:Init(id, prefab)
    return copy
end

function ActorRenderer:Init(id, prefab)
    self.id = id
    self.rawAsset = Resources.Load("Actors/"..prefab)
    self.gameObject = GameObject.Instantiate(self.rawAsset, WORLD_ROOT)
    self.gameObject.name = "actor_"..id
    self.transform = self.gameObject.transform
    self.spriteRenderer = self.gameObject:GetComponentInChildren(typeof(SpriteRenderer))
    self.spriteRenderer.flipX = false
    self.eventTrigger = self.gameObject:GetComponentInChildren(typeof(CS.SpriteRendererEventTrigger))
end

function ActorRenderer:Dispose()
    self.eventTrigger.pointClickCallback = nil -- 先清理event回调，否则如果将来是资源回池的话会有问题
    GameObject.Destroy(self.gameObject) -- Demo先直接销毁掉
    self.gameObject = nil
    self.rawAsset = nil
    self.transform = nil
    self.spriteRenderer = nil
    self.eventTrigger = nil
end

function ActorRenderer:SetPosition(position)
    self.transform.localPosition = position
end

-- 规范所有角色资源默认超右
function ActorRenderer:TurnFace(dir)
    self.spriteRenderer.flipX = dir < 0
end

function ActorRenderer:SetSortOrder(order)
    self.spriteRenderer.sortingOrder = order
end

function ActorRenderer:AddPointClick(callback)
    if self.eventTrigger.pointClickCallback == nil then self.eventTrigger.pointClickCallback = callback
    else self.eventTrigger.pointClickCallback = self.eventTrigger.pointClickCallback + callback end
end

function ActorRenderer:RemovePointClick(callback)
    if self.eventTrigger.pointClickCallback == nil then return end
    self.eventTrigger.pointClickCallback = self.eventTrigger.pointClickCallback - callback
end

function ActorRenderer:Clone()
    local clone = {}
    clone.gameObject = GameObject.Instantiate(self.rawAsset, WORLD_ROOT)
    clone.gameObject.name = self.gameObject.name.."_clone"
    clone.transform = clone.gameObject.transform
    clone.spriteRenderer = clone.gameObject:GetComponentInChildren(typeof(SpriteRenderer))
    clone.spriteRenderer.flipX = false
end

return ActorRenderer