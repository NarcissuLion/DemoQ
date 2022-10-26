_ENV = _G._ENV_ADVANTURE

local Wave = {}
Wave.__index = Wave

function Wave.Create(idx, context)
    local copy = {}
    setmetatable(copy, Wave)
    copy:Init(idx, context)
    return copy
end

function Wave:Init(idx, context)
    self.idx = idx
    self.context = context
end

function Wave:Dispose()
    self.context = nil
end

return Wave