local ObjectPool = {}
ObjectPool.__index = ObjectPool

function ObjectPool.Create(ObjType)
    assert(ObjType ~= nil, "Error! ObjectPool -> ObjType is nil.")
    local copy = {}
    setmetatable(copy, ObjectPool)
    copy:Init(ObjType)
    return copy
end

function ObjectPool:Init(ObjType)
    self.pool = {}
    self.objType = ObjType
end

function ObjectPool:Get(...)
    local obj
    if #self.pool > 0 then
        obj = self.pool[1]
        table.remove(self.pool, 1)
    else
        obj = self.objType.Create()
    end
    obj:OnGet(...)
    return obj
end

function ObjectPool:Release(obj, ...)
    assert(obj ~= nil, "Error! ObjectPool -> release nil.")
    obj:OnRelease(...)
    table.insert(self.pool, obj)
end

function ObjectPool:Dispose()
    for _,obj in ipairs(self.pool) do
        obj:Dispose()
    end
    self.pool = nil
end

ObjectPool.Object = {}
ObjectPool.Object.__index = ObjectPool.Object

function ObjectPool.Object.Create()
    local copy = {}
    setmetatable(copy, ObjectPool.Object)
    copy:Init()
    return copy
end

function ObjectPool.Object:Init()
end

function ObjectPool.Object:Dispose()
end

function ObjectPool.Object:OnGet(...)
end

function ObjectPool.Object:OnRelease(...)
end

return ObjectPool